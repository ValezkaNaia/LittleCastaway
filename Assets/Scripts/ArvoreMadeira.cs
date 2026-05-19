// ÁRVORE DE MADEIRA
using UnityEngine;

public class ArvoreMadeira : MonoBehaviour
{
    public int batidasParaCair = 3;
    public ItemData itemMadeira;

    public void LevarMachadada()
    {
        batidasParaCair--;
        if (batidasParaCair <= 0)
        {
            InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
            if (inv != null && itemMadeira != null) inv.AddItem(itemMadeira);
            Destroy(gameObject);
        }
    }
}