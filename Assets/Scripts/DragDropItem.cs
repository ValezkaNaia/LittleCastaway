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

        bool foiColocado = false;

        // 1. VERIFICAÇÃO PARA HOTBAR
        HotbarSlotUI[] todosOsSlotsHotbar = Object.FindObjectsByType<HotbarSlotUI>(FindObjectsSortMode.None);
        InventorySlot slotOrigemInv = paiOriginal.GetComponent<InventorySlot>();
        FogueiraSlotUI slotOrigemFogueira = paiOriginal.GetComponent<FogueiraSlotUI>();

        ItemData itemArrastado = null;
        if (slotOrigemInv != null) itemArrastado = slotOrigemInv.GetItem();
        if (slotOrigemFogueira != null) itemArrastado = slotOrigemFogueira.GetItem();

        if (itemArrastado != null)
        {
            // Testar se foi solto perto da Hotbar
            foreach (HotbarSlotUI slotHotbar in todosOsSlotsHotbar)
            {
                if (Vector2.Distance(rectTransform.position, slotHotbar.GetComponent<RectTransform>().position) < 60f) 
                {
                    if (HotbarManager.instance != null)
                    {
                        HotbarManager.instance.DefinirItemNoSlot(slotHotbar.indexDoSlot, itemArrastado);
                        foiColocado = true;
                    }
                    break;
                }
            }

            // 2. VERIFICAÇÃO PARA SLOTS DA FOGUEIRA (Se falhou a Hotbar)
            if (!foiColocado)
            {
                FogueiraSlotUI[] slotsFogueira = Object.FindObjectsByType<FogueiraSlotUI>(FindObjectsSortMode.None);
                foreach (FogueiraSlotUI slotFog in slotsFogueira)
                {
                    if (Vector2.Distance(rectTransform.position, slotFog.GetComponent<RectTransform>().position) < 60f)
                    {
                        // Regra: O slot de entrada só aceita coisas cruas. O de saída não aceita drops diretos
                        if (slotFog.tipoDeSlot == FogueiraSlotUI.TipoSlotFogueira.Entrada && itemArrastado.isCru)
                        {
                            slotFog.DefinirItem(itemArrastado);
                            
                            // Se tirou do inventário geral para pôr na fogueira, remove uma unidade logicamente
                            if (slotOrigemInv != null && Object.FindFirstObjectByType<InventoryManager>() != null)
                            {
                                Object.FindFirstObjectByType<InventoryManager>().RemoveItem(itemArrastado);
                            }
                            
                            foiColocado = true;
                            
                            // Atualiza os botões da fogueira
                            if (Object.FindFirstObjectByType<FogueiraUI>() != null)
                                Object.FindFirstObjectByType<FogueiraUI>().AtualizarEstadoDaUI();
                        }
                        break;
                    }
                }
            }
        }

        // Se o arrasto terminou no meio do nada, devolve ao quadrado de origem
        transform.SetParent(paiOriginal, true);
        rectTransform.localPosition = posicaoOriginal;
    }
}