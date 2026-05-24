using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Garante que a Input System está mapeada
using TMPro;

public class FogueiraUI : MonoBehaviour
{
    [Header("Slots de Ligação")]
    public FogueiraSlotUI slotEntrada;
    public FogueiraSlotUI slotSaida;

    [Header("Botões e Textos")]
    public Button botaoCozinhar;     // O teu ButtonCozinhar (Cook)
    public Button botaoAcender;      // O novo botão para pôr lenha
    public TextMeshProUGUI textoBotaoAcender; // Texto de feedback do botão de acender
    public TextMeshProUGUI textoTempoFogueira; // Texto para mostrar os segundos a descer

    private Fogueira fogueiraAtual;
    private InventoryManager inventoryManager;

    void Awake()
    {
        inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
        
        // Configura as funções dos botões ao clicar
        if (botaoCozinhar != null) botaoCozinhar.onClick.AddListener(CozinharAlimento);
        if (botaoAcender != null) botaoAcender.onClick.AddListener(TentarAcenderFogueiraUI);
    }

    // Chamado pelo teu sistema de interação quando abres a fogueira
    public void InicializarInterface(Fogueira fogueiraLogica)
    {
        fogueiraAtual = fogueiraLogica;
        AtualizarEstadoDaUI();

        // Liberta o rato para o jogador conseguir clicar!
        AtivarRato(true);
    }
    // Função auxiliar para controlar o estado do cursor
    private void AtivarRato(bool ativar)
    {
        if (ativar)
        {
            Cursor.lockState = CursorLockMode.None; // Liberta o rato do centro da tela
            Cursor.visible = true;                  // Torna o cursor visível
            
            // DICA EXTRA: Se tiveres um script de movimentação/câmara no teu Player (ex: PlayerLook),
            // deves desativá-lo aqui para a câmara não mexer enquanto usas o menu:
            // Object.FindFirstObjectByType<PlayerLook>().enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Prende o rato no centro novamente
            Cursor.visible = false;                   // Esconde o cursor
            
            // Reativa a câmara do jogador:
            // Object.FindFirstObjectByType<PlayerLook>().enabled = true;
        }
    }

    void Update()
    {
        // SEGURANÇA: Se a fogueira atual sumir (ex: destruída), fecha a interface
        if (fogueiraAtual == null)
        {
            AtivarRato(false); // Garante que o rato prende se a fogueira sumir
            gameObject.SetActive(false);
            return;
        }

        // Se o jogador carregar em ESC para fechar a fogueira
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Deixamos a fogueira tratar do fecho e do reset do rato centralizado
            fogueiraAtual.FecharInterfaceFogueira();
            return;
        }

        // CONTROLADOR DO TEMPO EM REAL-TIME
        if (fogueiraAtual.GetEstaAcesa())
        {
            if (textoTempoFogueira != null)
            {
                int tempoInt = Mathf.CeilToInt(fogueiraAtual.GetTempoRestante());
                textoTempoFogueira.text = $"Fogo Ativo: {tempoInt}s";
            }
            if (textoBotaoAcender != null) textoBotaoAcender.text = "Fogueira Acesa!";
            if (botaoAcender != null) botaoAcender.interactable = false;
        }
        else
        {
            if (textoTempoFogueira != null) textoTempoFogueira.text = "Fogueira Apagada";
            VerificarMadeirasNoInventario();
        }
    }

    void VerificarMadeirasNoInventario()
    {
        if (inventoryManager == null) return;

        int totalMadeiras = 0;
        foreach (var slot in inventoryManager.items)
        {
            if (slot.item != null && slot.item.isCombustivel)
            {
                totalMadeiras += slot.quantidade;
            }
        }

        if (textoBotaoAcender != null)
        {
            if (totalMadeiras >= 3)
            {
                textoBotaoAcender.text = "Acender Fogueira (Usa 3 Madeiras)";
                if (botaoAcender != null) botaoAcender.interactable = true;
            }
            else
            {
                textoBotaoAcender.text = $"Falta Madeira ({totalMadeiras}/3)";
                if (botaoAcender != null) botaoAcender.interactable = false;
            }
        }
    }

    void TentarAcenderFogueiraUI()
    {
        if (fogueiraAtual == null || inventoryManager == null) return;

        int madeirasParaRemover = 3;
        
        for (int i = inventoryManager.items.Count - 1; i >= 0; i--)
        {
            var slot = inventoryManager.items[i];
            if (slot.item != null && slot.item.isCombustivel)
            {
                if (slot.quantidade >= madeirasParaRemover)
                {
                    slot.quantidade -= madeirasParaRemover;
                    madeirasParaRemover = 0;
                }
                else
                {
                    madeirasParaRemover -= slot.quantidade;
                    slot.quantidade = 0;
                }

                if (slot.quantidade <= 0) inventoryManager.items.RemoveAt(i);
                if (madeirasParaRemover == 0) break;
            }
        }

        fogueiraAtual.TentarAcender(3);
        
        if (Object.FindFirstObjectByType<InventoryUI>() != null)
            Object.FindFirstObjectByType<InventoryUI>().AtualizarUI();

        AtualizarEstadoDaUI();
    }

    void CozinharAlimento()
    {
        if (fogueiraAtual == null || !fogueiraAtual.GetEstaAcesa()) return;
        if (slotEntrada == null || slotEntrada.GetItem() == null) return;

        ItemData alimentoCru = slotEntrada.GetItem();

        if (alimentoCru.isCru && alimentoCru.itemCozinhado != null)
        {
            ItemData alimentoPronto = alimentoCru.itemCozinhado;


            slotEntrada.DefinirItem(null);
            slotSaida.DefinirItem(alimentoPronto);

            Debug.Log($"[Culinária] {alimentoCru.itemName} transformado em {alimentoPronto.itemName}!");
        }
    }

    public void AtualizarEstadoDaUI()
    {
        if (fogueiraAtual == null) return;

        bool podeCozinhar = fogueiraAtual.GetEstaAcesa() && 
                            slotEntrada.GetItem() != null && 
                            slotEntrada.GetItem().isCru;

        if (botaoCozinhar != null) botaoCozinhar.interactable = podeCozinhar;
    }
}
