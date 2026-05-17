using System.Collections.Generic;
using UnityEngine;

// Mudámos o nome para não haver conflito com o teu script InventorySlot!
[System.Serializable]
public class ItemAcumulado
{
    public ItemData item;
    public int quantidade;

    public ItemAcumulado(ItemData item, int quantidade)
    {
        this.item = item;
        this.quantidade = quantidade;
    }
}

public class InventoryManager : MonoBehaviour
{
    // Agora a lista guarda itens com as suas quantidades agregadas
    public List<ItemAcumulado> items = new List<ItemAcumulado>();

    public void AddItem(ItemData newItem)
    {
        // 1. Se o item for acumulável, procura se já existe um igual na lista
        if (newItem.isStackable)
        {
            foreach (var slotLogico in items)
            {
                if (slotLogico.item == newItem)
                {
                    slotLogico.quantidade += 1; // Soma +1 à quantidade existente
                    FinalizarAdicao(newItem);
                    return; // Para o código aqui porque já resolveu!
                }
            }
        }

        // 2. Se não for acumulável ou for o primeiro deste tipo, adiciona uma nova entrada na lista
        items.Add(new ItemAcumulado(newItem, 1));
        FinalizarAdicao(newItem);
    }

    private void FinalizarAdicao(ItemData newItem)
    {
        Debug.Log("Apanhaste: " + newItem.itemName);

        // Notifica a hotbar para mostrar o item na barra de acesso rápido
        if (HotbarManager.instance != null)
            HotbarManager.instance.AdicionarItem(newItem);

        // ATUALIZAÇÃO VISUAL: Notifica a UI do Inventário para se redesenhar
        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.AtualizarUI();
    }
}
/*using System.Collections.Generic;
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

        // ATUALIZAÇÃO VISUAL: Garante que se o inventário estiver aberto ele atualiza na hora
        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.AtualizarUI();
    }
}*/

