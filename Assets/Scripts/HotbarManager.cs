using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class HotbarManager : MonoBehaviour
{
    [Header("UI da Hotbar")]
    public GameObject hotbarPanel;
    public List<HotbarSlotUI> slotsUI = new List<HotbarSlotUI>();

    public Color corSelecao = new Color(0.96f, 0.78f, 0.26f, 1f);
    public Color corNormal = new Color(0.13f, 0.13f, 0.13f, 0.75f);

    [Header("UI de Avisos")]
    public TextMeshProUGUI textoConsumoUI; 

    [Header("Mão do Jogador")]
    public Transform handTransform;

    private List<ItemData> hotbarItems = new List<ItemData>();
    private int slotSelecionado = 0;
    private GameObject modeloEquipado = null;

    public static HotbarManager instance;

    void Awake()
    {
        instance = this;
        for (int i = 0; i < slotsUI.Count; i++)
            hotbarItems.Add(null);
    }

    void Start()
    {
        if (hotbarPanel != null) hotbarPanel.SetActive(true);
        AtualizarUI();
        DestaqueSelecionado();
        VerificarPromptConsumo();
    }

    void Update()
    {
        LerInput();

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            TentarConsumirItem();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TentarColocarObjetoNoChao();
        }
    }

    void LerInput()
    {
        for (int i = 0; i < slotsUI.Count && i < 9; i++)
        {
            Key chaveNumérica = Key.Digit1 + i;
            if (Keyboard.current[chaveNumérica].wasPressedThisFrame)
            {
                SelecionarSlot(i);
                return;
            }
        }

        Vector2 scroll = Mouse.current.scroll.ReadValue();
        if (scroll.y > 0f)
            SelecionarSlot((slotSelecionado - 1 + slotsUI.Count) % slotsUI.Count);
        else if (scroll.y < 0f)
            SelecionarSlot((slotSelecionado + 1) % slotsUI.Count);
    }

    public void AdicionarItem(ItemData item)
    {
        if (item == null) return;

        for (int i = 0; i < hotbarItems.Count; i++)
        {
            if (hotbarItems[i] == item)
            {
                AtualizarSlot(i);
                return;
            }
        }

        for (int i = 0; i < hotbarItems.Count; i++)
        {
            if (hotbarItems[i] == null)
            {
                DefinirItemNoSlot(i, item);
                return;
            }
        }

        Debug.LogWarning("Hotbar cheia! O item foi apenas para o inventário geral.");
    }

    public void SelecionarSlot(int index)
    {
        if (index < 0 || index >= slotsUI.Count) return;
        slotSelecionado = index;
        DestaqueSelecionado();
        EquiparItem(hotbarItems[slotSelecionado]);
    }

    public void DefinirItemNoSlot(int index, ItemData item)
    {
        if (index < 0 || index >= hotbarItems.Count) return;

        if (item != null)
        {
            for (int i = 0; i < hotbarItems.Count; i++)
            {
                if (hotbarItems[i] == item && i != index)
                {
                    hotbarItems[i] = null;
                    AtualizarSlot(i);
                    
                    if (i == slotSelecionado)
                    {
                        EquiparItem(null);
                    }
                }
            }
        }

        hotbarItems[index] = item;
        AtualizarSlot(index);

        if (index == slotSelecionado)
            EquiparItem(item);
    }

    void TentarConsumirItem()
    {
        ItemData item = GetItemSelecionado();
        if (item == null || !item.isConsumable) return;

        InventoryManager invManager = Object.FindFirstObjectByType<InventoryManager>();

        if (SurvivalManager.instance != null)
        {
            // 1. Aplica os efeitos de sobrevivência
            SurvivalManager.instance.ReceberNutricao(item.pontosRestauracao, item.tipoConsumivel);
            
            // 2. Remove apenas uma unidade do Inventário Geral
            if (invManager != null)
            {
                invManager.RemoveItem(item);
            }

            // CORREÇÃO VISUAL: Verifica se o inventário geral ainda tem este item guardado
            VerificarSeItemAindaExisteNoInventario(slotSelecionado, item, invManager);
            
            // 4. Força a atualização visual do inventário geral
            if (Object.FindFirstObjectByType<InventoryUI>() != null)
            {
                Object.FindFirstObjectByType<InventoryUI>().AtualizarUI();
            }
        }
    }

    void VerificarPromptConsumo()
    {
        if (textoConsumoUI == null) return;

        ItemData itemAtivo = GetItemSelecionado();

        if (itemAtivo != null && itemAtivo.isConsumable)
        {
            if (itemAtivo.tipoConsumivel == ItemData.TipoConsumivel.Comida)
                textoConsumoUI.text = "Pressiona [C] para Comer";
            else if (itemAtivo.tipoConsumivel == ItemData.TipoConsumivel.Agua)
                textoConsumoUI.text = "Pressiona [C] para Beber";
            
            textoConsumoUI.gameObject.SetActive(true);
        }
        else
        {
            textoConsumoUI.gameObject.SetActive(false);
        }
    }

    void EquiparItem(ItemData item)
    {
        if (modeloEquipado != null) Destroy(modeloEquipado);

        VerificarPromptConsumo();

        if (item == null || item.prefabModel == null || handTransform == null) return;

        modeloEquipado = Instantiate(item.prefabModel, handTransform);
        modeloEquipado.transform.localPosition = item.holdOffset;
        modeloEquipado.transform.localRotation = Quaternion.Euler(item.holdRotation);
        modeloEquipado.transform.localScale = item.holdScale == Vector3.zero ? Vector3.one : item.holdScale;

        foreach (var col in modeloEquipado.GetComponentsInChildren<Collider>()) col.enabled = false;
        var io = modeloEquipado.GetComponent<ItemObject>();
        if (io != null) Destroy(io);
    }

    void TentarColocarObjetoNoChao()
    {
        ItemData itemAtivo = GetItemSelecionado();
        if (itemAtivo == null || !itemAtivo.isPlaceable || itemAtivo.prefabModel == null) return;

        Camera cameraPrincipal = Camera.main;
        if (cameraPrincipal == null) return;

        Ray raio = cameraPrincipal.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        float distanciaColocacao = 5f; 

        if (Physics.Raycast(raio, out hit, distanciaColocacao))
        {
            Quaternion rotacaoAlinhada = Quaternion.FromToRotation(Vector3.up, hit.normal);
            GameObject novoObjetoMundo = Instantiate(itemAtivo.prefabModel, hit.point, rotacaoAlinhada);

            foreach (var col in novoObjetoMundo.GetComponentsInChildren<Collider>()) col.enabled = true;
            
            InventoryManager invManager = Object.FindFirstObjectByType<InventoryManager>();
            if (invManager != null)
            {
                invManager.RemoveItem(itemAtivo);
            }

            // CORREÇÃO VISUAL: Verifica se ainda restam mais objetos deste tipo
            VerificarSeItemAindaExisteNoInventario(slotSelecionado, itemAtivo, invManager);
            
            if (Object.FindFirstObjectByType<InventoryUI>() != null)
            {
                Object.FindFirstObjectByType<InventoryUI>().AtualizarUI();
            }

            Debug.Log($"[Sucesso] {itemAtivo.itemName} colocado no mundo!");
        }
    }

    // ADICIONADO: Função que remove da mão/slot apenas quando o estoque real for 0!
    public void RemoverItemGasto(ItemData item)
    {
        if (item == null) return;

        for (int i = 0; i < hotbarItems.Count; i++)
        {
            if (hotbarItems[i] == item)
            {
                InventoryManager invManager = Object.FindFirstObjectByType<InventoryManager>();
                VerificarSeItemAindaExisteNoInventario(i, item, invManager);
                return;
            }
        }
    }

    // NOVA FUNÇÃO AUXILIAR DE SEGURANÇA CONTRA LIMPEZA DE STOCK
    // NOVA FUNÇÃO AUXILIAR DE SEGURANÇA CONTRA LIMPEZA DE STOCK (Versão Corrigida)
    private void VerificarSeItemAindaExisteNoInventario(int slotIndex, ItemData item, InventoryManager invManager)
    {
        bool aindaTemNoInventario = false;

        if (invManager != null && invManager.items != null)
        {
            // Percorre a lista de ItemAcumulado para ver se o item ainda lá está escondido
            foreach (ItemAcumulado slotLogico in invManager.items)
            {
                // Se encontrar o item e a quantidade dele for maior que zero
                if (slotLogico.item == item && slotLogico.quantidade > 0)
                {
                    aindaTemNoInventario = true;
                    break; // Já encontrámos, podemos parar a busca
                }
            }
        }

        // Se NÃO houver mais nenhum stock real nas malas do jogador, limpa o slot da hotbar
        if (!aindaTemNoInventario)
        {
            hotbarItems[slotIndex] = null;
            AtualizarSlot(slotIndex);

            if (slotIndex == slotSelecionado)
            {
                EquiparItem(null);
            }
        }
        else
        {
            // Se ainda houver stock no inventário, mantém o atalho vivo!
            AtualizarSlot(slotIndex);
        }
    }

    void AtualizarUI()
    {
        for (int i = 0; i < slotsUI.Count; i++) AtualizarSlot(i);
    }

    void AtualizarSlot(int index)
    {
        if (index < 0 || index >= slotsUI.Count || slotsUI[index] == null) return;

        ItemData item = hotbarItems[index];
        HotbarSlotUI slotUI = slotsUI[index];

        if (item != null && item.itemIcon != null)
        {
            slotUI.imagemIcon.sprite = item.itemIcon;
            slotUI.imagemIcon.enabled = true;
        }
        else
        {
            slotUI.imagemIcon.enabled = false;
        }

        if (index == slotSelecionado)
        {
            VerificarPromptConsumo();
        }
    }

    void DestaqueSelecionado()
    {
        for (int i = 0; i < slotsUI.Count; i++)
        {
            if (slotsUI[i] == null || slotsUI[i].imagemFundo == null) continue;
            slotsUI[i].imagemFundo.color = (i == slotSelecionado) ? corSelecao : corNormal;
            slotsUI[i].transform.localScale = (i == slotSelecionado) ? new Vector3(1.1f, 1.1f, 1f) : Vector3.one;
        }
    }

    public ItemData GetItemSelecionado() => (slotSelecionado < hotbarItems.Count) ? hotbarItems[slotSelecionado] : null;
    public FerramentaAtaque GetFerramentaAtiva() => modeloEquipado != null ? modeloEquipado.GetComponent<FerramentaAtaque>() : null;
}