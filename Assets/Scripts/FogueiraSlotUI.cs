using UnityEngine;
using UnityEngine.UI;

public class FogueiraSlotUI : MonoBehaviour
{
    public enum TipoSlotFogueira { Entrada, Saida }
    public TipoSlotFogueira tipoDeSlot;

    [Header("Componentes Visuais")]
    public Image imagemIcon;

    private ItemData itemNoSlot;

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
    }

    public ItemData GetItem() => itemNoSlot;
}