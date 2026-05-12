using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Lista completa de itens no inventário
    public List<ItemData> items = new List<ItemData>();

    public void AddItem(ItemData newItem)
    {
        items.Add(newItem);
        Debug.Log("Apanhaste: " + newItem.itemName);

        // Notifica a hotbar para mostrar o item na barra de acesso rápido
        if (HotbarManager.instance != null)
            HotbarManager.instance.AdicionarItem(newItem);
    }
}

