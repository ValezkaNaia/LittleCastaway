using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gere a barra de itens (hotbar) na parte inferior do ecrã.
/// Permite selecionar um slot com teclas numéricas ou scroll do rato
/// e "equipar" o item na mão do jogador (instancia o modelo 3D).
/// </summary>
public class HotbarManager : MonoBehaviour
{
    // --- Referências de UI ----------------------------------------------------
    [Header("UI da Hotbar")]
    [Tooltip("Arrasta o HotBarPanel aqui — o script garante que fica sempre visível.")]
    public GameObject hotbarPanel;

    [Tooltip("Lista dos GameObjects de cada slot na hierarquia do Canvas (da esquerda para a direita).")]
    public List<GameObject> slots = new List<GameObject>();

    [Tooltip("Nome da cor usada para destacar o slot selecionado (ex: #F5C842).")]
    public Color corSelecao = new Color(0.96f, 0.78f, 0.26f, 1f);

    [Tooltip("Cor normal dos slots não selecionados.")]
    public Color corNormal = new Color(0.13f, 0.13f, 0.13f, 0.75f);

    // --- Referência à mão do jogador ------------------------------------------
    [Header("Mão do Jogador")]
    [Tooltip("Transform vazio filho da câmara onde o modelo 3D do item será colocado.")]
    public Transform handTransform;

    // --- Estado interno -------------------------------------------------------
    private List<ItemData> hotbarItems = new List<ItemData>(); // ItemData em cada slot (null = vazio)
    private int slotSelecionado = 0;
    private GameObject modeloEquipado = null; // modelo 3D atualmente na mão

    // Singleton simples para acesso externo (InventoryManager, etc.)
    public static HotbarManager instance;

    // -------------------------------------------------------------------------
    void Awake()
    {
        instance = this;

        // Inicializa a lista de itens com nulls (todos os slots vazios)
        for (int i = 0; i < slots.Count; i++)
            hotbarItems.Add(null);
    }

    void Start()
    {
        // Garante que a barra aparece logo no início
        if (hotbarPanel != null) hotbarPanel.SetActive(true);

        AtualizarUI();
        DestaqueSelecionado();
    }

    void Update()
    {
        // A hotbar tem de estar SEMPRE visível — nunca pode ser escondida
        if (hotbarPanel != null && !hotbarPanel.activeSelf)
            hotbarPanel.SetActive(true);

        LerInput();
    }

    // =========================================================================
    // Input
    // =========================================================================
    void LerInput()
    {
        // Teclas numéricas 1..9 (e 0 para o décimo slot se existir)
        for (int i = 0; i < slots.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelecionarSlot(i);
                return;
            }
        }
        // Tecla 0 → último slot (slot 10)
        if (slots.Count >= 10 && Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelecionarSlot(9);
            return;
        }

        // Scroll do rato
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SelecionarSlot((slotSelecionado - 1 + slots.Count) % slots.Count);
        }
        else if (scroll < 0f)
        {
            SelecionarSlot((slotSelecionado + 1) % slots.Count);
        }
    }

    // =========================================================================
    // Seleção de slot
    // =========================================================================
    public void SelecionarSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        slotSelecionado = index;
        DestaqueSelecionado();
        EquiparItem(hotbarItems[slotSelecionado]);
    }

    // =========================================================================
    // Adicionar item ao primeiro slot livre
    // =========================================================================
    /// <summary>
    /// Chamado pelo InventoryManager quando o jogador apanha um item.
    /// Coloca o item no primeiro slot vazio da hotbar.
    /// </summary>
    public void AdicionarItem(ItemData item)
    {
        // Tenta empilhar se for stackable e o item já existir
        if (item.isStackable)
        {
            for (int i = 0; i < hotbarItems.Count; i++)
            {
                if (hotbarItems[i] != null && hotbarItems[i] == item)
                {
                    // Slot já tem este item → só atualiza a UI (stack futuro)
                    AtualizarUI();
                    return;
                }
            }
        }

        // Coloca no primeiro slot vazio
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            if (hotbarItems[i] == null)
            {
                hotbarItems[i] = item;
                AtualizarSlot(i);

                // Se é o slot atualmente selecionado, equipa logo
                if (i == slotSelecionado)
                    EquiparItem(item);

                return;
            }
        }

        Debug.LogWarning("Hotbar cheia! Não foi possível adicionar: " + item.itemName);
    }

    // =========================================================================
    // Equipar modelo na mão
    // =========================================================================
    void EquiparItem(ItemData item)
    {
        // Destroi o modelo anterior
        if (modeloEquipado != null)
        {
            Destroy(modeloEquipado);
            modeloEquipado = null;
        }

        if (item == null || item.prefabModel == null || handTransform == null)
            return;

        // Instancia o modelo 3D como filho da mão
        modeloEquipado = Instantiate(item.prefabModel, handTransform);
        modeloEquipado.transform.localPosition = item.holdOffset;
        modeloEquipado.transform.localRotation = Quaternion.Euler(item.holdRotation);
        modeloEquipado.transform.localScale    = item.holdScale == Vector3.zero
                                                  ? Vector3.one
                                                  : item.holdScale;

        // Remove colisores para não interagir com o mundo enquanto está na mão
        // E remove LODGroups que escondem modelos quando a escala é muito pequena
        var lod = modeloEquipado.GetComponent<LODGroup>();
        if (lod != null) Destroy(lod);

        foreach (var t in modeloEquipado.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = LayerMask.NameToLayer("Default");
            var col = t.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        // Remove o componente ItemObject se existir
        var ioComp = modeloEquipado.GetComponent<ItemObject>();
        if (ioComp != null) Destroy(ioComp);

        Debug.Log($"[Hotbar] Equipado na mão: {item.itemName} (Escala Global: {modeloEquipado.transform.lossyScale.x})");
    }

    // =========================================================================
    // UI
    // =========================================================================
    void AtualizarUI()
    {
        for (int i = 0; i < slots.Count; i++)
            AtualizarSlot(i);
    }

    void AtualizarSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        GameObject slot = slots[index];
        if (slot == null) return;

        ItemData item = (index < hotbarItems.Count) ? hotbarItems[index] : null;

        // Ícone: procura imagem filha chamada "Icon" (ou a primeira Image que não seja o fundo)
        Transform iconTransform = slot.transform.Find("Icon");
        if (iconTransform != null)
        {
            var img = iconTransform.GetComponent<Image>();
            if (img != null)
            {
                img.enabled = (item != null && item.itemIcon != null);
                if (item != null && item.itemIcon != null)
                    img.sprite = item.itemIcon;
            }
            iconTransform.gameObject.SetActive(item != null);
        }

        // Texto de nome (opcional, filho chamado "ItemName")
        Transform nameTransform = slot.transform.Find("ItemName");
        if (nameTransform != null)
        {
            var tmp = nameTransform.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = (item != null) ? item.itemName : "";
        }
    }

    void DestaqueSelecionado()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;

            var bg = slots[i].GetComponent<Image>();
            if (bg == null) continue;

            bg.color = (i == slotSelecionado) ? corSelecao : corNormal;

            // Escala ligeiramente o slot selecionado para feedback visual
            slots[i].transform.localScale = (i == slotSelecionado)
                ? new Vector3(1.12f, 1.12f, 1f)
                : Vector3.one;
        }
    }

    // =========================================================================
    // Acessores públicos
    // =========================================================================
    public ItemData GetItemSelecionado() => (slotSelecionado < hotbarItems.Count)
                                              ? hotbarItems[slotSelecionado]
                                              : null;
}
