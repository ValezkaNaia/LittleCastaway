using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour, IDropHandler
{
    public int indexDoSlot; // Definido no Inspector (0 para o primeiro, 1 para o segundo, etc.)

    [Header("Componentes de UI Internos")]
    public Image imagemIcon;
    public Image imagemFundo; 

    // Este método é disparado automaticamente pelo Unity quando soltas um arrasto em cima deste slot
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // CORREÇÃO: Procura o componente no pai do Ícone arrastado!
            InventorySlot slotOrigem = eventData.pointerDrag.GetComponentInParent<InventorySlot>();

            if (slotOrigem != null && slotOrigem.GetItem() != null)
            {
                ItemData itemArrastado = slotOrigem.GetItem();

                // Envia o item para o slot correspondente na Hotbar
                if (HotbarManager.instance != null)
                {
                    HotbarManager.instance.DefinirItemNoSlot(indexDoSlot, itemArrastado);
                    Debug.Log($"[UI Hotbar] Item {itemArrastado.itemName} inserido com sucesso no slot {indexDoSlot}!");
                }
            }
            else
            {
                Debug.LogWarning("[UI Hotbar] O objeto arrastado não continha um InventorySlot válido no pai.");
            }
        }
    }
}