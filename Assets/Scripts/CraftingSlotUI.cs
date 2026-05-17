using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    private ItemData currentItem;
    private int currentQuantity;

    void Awake()
    {
        ClearSlot();
    }

    public void SetupSlot(ItemData item, int quantity)
    {
        currentItem = item;
        currentQuantity = quantity;

        if (item != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.enabled = true;
            }
            
            // Só mostra texto se for maior que 1, se for acumulável E se o componente de texto existir!
            if (quantity > 1 && item.isStackable && quantityText != null)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                if (quantityText != null) quantityText.enabled = false;
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentQuantity = 0;
        if (iconImage != null) iconImage.enabled = false;
        if (quantityText != null) quantityText.enabled = false;
    }

    public ItemData GetItem() => currentItem;
    public int GetQuantity() => currentQuantity;
}
/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    private ItemData currentItem;
    private int currentQuantity;

    void Awake()
    {
        ClearSlot();
    }

    public void SetupSlot(ItemData item, int quantity)
    {
        currentItem = item;
        currentQuantity = quantity;

        if (item != null)
        {
            iconImage.sprite = item.itemIcon;
            iconImage.enabled = true;
            
            // Só mostra texto se for maior que 1 e acumulável
            if (quantity > 1 && item.isStackable)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentQuantity = 0;
        if (iconImage != null) iconImage.enabled = false;
        if (quantityText != null) quantityText.enabled = false;
    }

    public ItemData GetItem() => currentItem;
    public int GetQuantity() => currentQuantity;
}*/
