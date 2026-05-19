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
        if (newItem.isStackable)
        {
            foreach (var slotLogico in items)
            {
                if (slotLogico.item == newItem)
                {
                    slotLogico.quantidade += 1;
                    FinalizarAdicao(newItem, false); // false = NÃO vai para a hotbar
                    return;
                }
            }
        }

        items.Add(new ItemAcumulado(newItem, 1));
        FinalizarAdicao(newItem, false); // false = NÃO vai para a hotbar
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        if (itemToRemove == null) return;

        // Procura o item na lista lógica
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == itemToRemove)
            {
                // Se for acumulável, diminui 1 à quantidade
                if (itemToRemove.isStackable)
                {
                    items[i].quantidade -= 1;
                    
                    // Se a quantidade chegou a 0, remove completamente a entrada da lista
                    if (items[i].quantidade <= 0)
                    {
                        items.RemoveAt(i);
                    }
                }
                else
                {
                    // Se não for acumulável, remove diretamente da lista
                    items.RemoveAt(i);
                }

                Debug.Log("Item removido do inventário geral: " + itemToRemove.itemName);
                break;
            }
        }

        // Atualiza visualmente a interface do inventário geral
        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.AtualizarUI();
    }

    // NOVA FUNÇÃO: Usada para devolver itens de menus (Crafting, Baús, etc.) sem poluir a Hotbar
    public void AddItemDoMenu(ItemData newItem)
    {
        if (newItem.isStackable)
        {
            foreach (var slotLogico in items)
            {
                if (slotLogico.item == newItem)
                {
                    slotLogico.quantidade += 1;
                    FinalizarAdicao(newItem, false); // false = NÃO vai para a hotbar
                    return;
                }
            }
        }

        items.Add(new ItemAcumulado(newItem, 1));
        FinalizarAdicao(newItem, false); // false = NÃO vai para a hotbar
    }

    // Alterado para receber o booleano 'enviarParaHotbar'
    private void FinalizarAdicao(ItemData newItem, bool enviarParaHotbar)
    {
        Debug.Log("Inventário Geral Atualizado: " + newItem.itemName);

        // Só envia para a hotbar se o item veio do CHÃO (apanhado no mundo)
        if (enviarParaHotbar && HotbarManager.instance != null)
        {
            HotbarManager.instance.AdicionarItem(newItem);
        }

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

