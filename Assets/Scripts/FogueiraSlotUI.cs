using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // OBRIGATÓRIO: Permite usar o sistema de Drag & Drop

// Adicionámos ', IDropHandler' para o Unity saber que este slot aceita itens largados nele
public class FogueiraSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public enum TipoSlotFogueira { Entrada, Saida }
    public TipoSlotFogueira tipoDeSlot;

    [Header("Componentes Visuais")]
    public Image imagemIcon;

    private ItemData itemNoSlot;
    private FogueiraUI fogueiraUI;

    void Awake()
    {
        // Encontra o componente pai para podermos atualizar os botões (como o Cook)
        fogueiraUI = GetComponentInParent<FogueiraUI>();
    }

    public void DefinirItem(ItemData novoItem)
    {
        itemNoSlot = novoItem;
        if (imagemIcon != null)
        {
            if (novoItem != null && novoItem.itemIcon != null)
            {
                imagemIcon.sprite = novoItem.itemIcon;
                imagemIcon.enabled = true;
            }
            else
            {
                imagemIcon.enabled = false;
            }
        }

        // Sempre que o item deste slot muda, avisa a UI principal para validar o botão "Cook"
        if (fogueiraUI != null)
        {
            fogueiraUI.AtualizarEstadoDaUI();
        }
    }

    public ItemData GetItem() => itemNoSlot;

    // =================================================================
    // MECÂNICA DE SOLTAR O ITEM (IDropHandler)
    // =================================================================
    public void OnDrop(PointerEventData eventData)
    {
        // 1. REGRA: Se este for o slot de SAÍDA, o jogador NÃO pode arrastar itens para aqui
        if (tipoDeSlot == TipoSlotFogueira.Saida) return;

        // 2. DETEÇÃO: Verifica se o objeto que o rato está a arrastar tem o script de slot do inventário
        // ATENÇÃO: Se o teu script do inventário se chamar 'InventorySlot', mantém como está.
        // Se se chamar 'SlotUI' ou outro nome, substitui 'InventorySlot' pelo nome exato dele!
        InventorySlot slotArrastado = eventData.pointerDrag?.GetComponent<InventorySlot>();

        if (slotArrastado != null && slotArrastado.GetItem() != null)
        {
            ItemData itemAlvo = slotArrastado.GetItem();

            // 3. VALIDAÇÃO: Só aceita o item se ele for Cru e se tiver um output de cozinhado configurado
            if (itemAlvo.isCru && itemAlvo.itemCozinhado != null)
            {
                // Define o item na fogueira
                DefinirItem(itemAlvo);

                // OPCIONAL: Se quiseres que o item SUMA do inventário ao ser posto na fogueira,
                // podes chamar uma função do teu slot para o limpar, por exemplo:
                // slotArrastado.DefineItem(null, 0); 
                
                Debug.Log($"[Fogueira] {itemAlvo.itemName} colocado no slot de entrada!");
            }
            else
            {
                Debug.LogWarning("[Fogueira] Este item não pode ser cozinhado!");
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Se for o slot de saída e tiver lá uma carne pronta...
        if (tipoDeSlot == TipoSlotFogueira.Saida && itemNoSlot != null)
        {
            InventoryManager inventory = Object.FindFirstObjectByType<InventoryManager>();
            InventoryUI inventoryUI = Object.FindFirstObjectByType<InventoryUI>();

            if (inventory != null)
            {
                // Adiciona diretamente ao inventário geral do jogador
                inventory.items.Add(new ItemAcumulado(itemNoSlot, 1)); 
                
                // Limpa o slot da fogueira
                DefinirItem(null);

                // Atualiza o painel visual do inventário para mostrar a nova carne
                if (inventoryUI != null) inventoryUI.AtualizarUI();
                
                Debug.Log($"[Recolha] {itemNoSlot.itemName} movido para o inventário!");
            }
        }
    }
}