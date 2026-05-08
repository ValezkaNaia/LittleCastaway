using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public ItemData referenciaItem;

    public void SerApanhado()
    {
        // Procuramos o Manager na cena
        InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
        
        if (inv != null)
        {
            // ATENÇÃO: Tem de ser AddItem (exatamente como no teu Manager)
            inv.AddItem(referenciaItem); 
            Destroy(gameObject); // O objeto só some se esta linha for lida!
        }
        else
        {
            Debug.LogError("Não encontrei o InventoryManager no objeto Managers!");
        }
    }
}