using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Lista para armazenar os itens do inventário
    public List<ItemData> items = new List<ItemData>();

    public void AddItem(ItemData newItem)
    {
        items.Add(newItem);
        Debug.Log("Apanhaste: " + newItem.itemName);
    }
}
