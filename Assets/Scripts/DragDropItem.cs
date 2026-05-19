using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DragDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    private Vector3 posicaoOriginal;
    private Transform paiOriginal;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InventorySlot slotInv = GetComponentInParent<InventorySlot>();

        // Validação: Se veio do inventário principal e o slot estiver vazio, cancela
        if (slotInv != null && slotInv.GetItem() == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        posicaoOriginal = rectTransform.localPosition;
        paiOriginal = transform.parent;

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null)
        {
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling(); // Força a ficar à frente no frame do arrasto
        }

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        bool foiColocadoNaHotbar = false;

        // DETEÇÃO POR DISTÂNCIA: Procura todos os slots de Hotbar na cena
        HotbarSlotUI[] todosOsSlotsHotbar = Object.FindObjectsByType<HotbarSlotUI>(FindObjectsSortMode.None);
        
        // Pega no slot de inventário de onde este ícone saiu originalmente
        InventorySlot slotOrigem = paiOriginal.GetComponent<InventorySlot>();

        if (slotOrigem != null && slotOrigem.GetItem() != null)
        {
            ItemData itemArrastado = slotOrigem.GetItem();

            foreach (HotbarSlotUI slotHotbar in todosOsSlotsHotbar)
            {
                RectTransform rectSlot = slotHotbar.GetComponent<RectTransform>();
                
                // Calcula a distância em pixels entre o ícone do rato e o centro do slot da Hotbar
                float distancia = Vector2.Distance(rectTransform.position, rectSlot.position);

                // Se soltaste o item a menos de 60 pixels do centro do slot da Hotbar
                if (distancia < 60f) 
                {
                    if (HotbarManager.instance != null)
                    {
                        HotbarManager.instance.DefinirItemNoSlot(slotHotbar.indexDoSlot, itemArrastado);
                        Debug.Log($"[Sucesso] {itemArrastado.itemName} colocado no slot {slotHotbar.indexDoSlot} por proximidade!");
                        foiColocadoNaHotbar = true;
                    }
                    break;
                }
            }
        }

        // Se não foi solto perto de nenhum slot da hotbar, volta para a posição inicial no inventário
        transform.SetParent(paiOriginal, true);
        rectTransform.localPosition = posicaoOriginal;
    }
}