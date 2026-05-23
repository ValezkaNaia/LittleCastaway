using UnityEngine;
using UnityEngine.UI;
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
    public TextMeshProUGUI textoTempoFogueira; // Texto opcional para mostrar os 60 segundos a descer

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
    }

    void Update()
    {
        if (fogueiraAtual != null && fogueiraAtual.gameObject.activeSelf)
        {
            // Atualiza o relógio dos 60 segundos na tela caso ela esteja acesa
            if (fogueiraAtual.GetEstaAcesa())
            {
                if (textoTempoFogueira != null)
                {
                    textoTempoFogueira.text = $"Fogo Ativo: {Mathf.CeilToInt(fogueiraAtual.tempoAcesaMax)}s";
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
    }

    void VerificarMadeirasNoInventario()
    {
        if (inventoryManager == null) return;

        // Procura quantas madeiras o jogador tem no total do inventário
        int totalMadeiras = 0;
        foreach (var slot in inventoryManager.items)
        {
            if (slot.item != null && slot.item.isCombustivel)
            {
                totalMadeiras += slot.quantidade;
            }
        }
        // Atualiza o texto e o estado do botão de acender com base na quantidade de madeira encontrada
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

        // Conta e consome as 3 madeiras
        int madeirasParaRemover = 3;
        
        // Cria uma lista temporária para evitar erros de modificação durante o loop
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

        // Avisa o objeto físico no mundo para ativar as partículas de fogo
        fogueiraAtual.TentarAcender(3);
        
        // Força a atualização visual do inventário geral
        if (Object.FindFirstObjectByType<InventoryUI>() != null)
            Object.FindFirstObjectByType<InventoryUI>().AtualizarUI();

        AtualizarEstadoDaUI();
    }

    void CozinharAlimento()
    {
        if (fogueiraAtual == null || !fogueiraAtual.GetEstaAcesa()) return;
        if (slotEntrada == null || slotEntrada.GetItem() == null) return;

        ItemData alimentoCru = slotEntrada.GetItem();

        // Validação de segurança: Só cozinha se for cru e se tiver um output configurado no ItemData
        if (alimentoCru.isCru && alimentoCru.itemCozinhado != null)
        {
            ItemData alimentoPronto = alimentoCru.itemCozinhado;

            // Transforma o slot de entrada em vazio e gera a carne cozinhada na saída
            slotEntrada.DefinirItem(null);
            slotSaida.DefinirItem(alimentoPronto);

            Debug.Log($"[Culinária] {alimentoCru.itemName} transformado em {alimentoPronto.itemName}!");
        }
    }

    public void AtualizarEstadoDaUI()
    {
        if (fogueiraAtual == null) return;

        // O botão Cook só fica clicável se a fogueira estiver acesa e se houver um item válido na entrada
        bool podeCozinhar = fogueiraAtual.GetEstaAcesa() && 
                            slotEntrada.GetItem() != null && 
                            slotEntrada.GetItem().isCru;

        if (botaoCozinhar != null) botaoCozinhar.interactable = podeCozinhar;
    }
}
