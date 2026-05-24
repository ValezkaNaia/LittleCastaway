using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public GameObject iconeObject; 
    public TextMeshProUGUI textoQuantidade; 
    private ItemData itemNoSlot;
    private int quantidadeNoSlot;

    public void DefineItem(ItemData item, int quantidade)
    {
        itemNoSlot = item;
        quantidadeNoSlot = quantidade;

        if (iconeObject == null) return;

        if (item != null)
        {
            // Força a ativação do objeto ANTES de aplicar o sprite para evitar delays visuais
            iconeObject.SetActive(true);
            iconeObject.SendMessage("set_sprite", item.itemIcon, SendMessageOptions.DontRequireReceiver);

            // Atualiza o texto da quantidade no ecrã
            if (textoQuantidade != null)
            {
                if (quantidade > 1)
                {
                    textoQuantidade.text = quantidade.ToString();
                    textoQuantidade.gameObject.SetActive(true);
                }
                else
                {
                    textoQuantidade.gameObject.SetActive(false); // Esconde se for apenas 1
                }
            }
        }
        else
        {
            iconeObject.SetActive(false);
            if (textoQuantidade != null)
                textoQuantidade.gameObject.SetActive(false);
        }
    }

    public ItemData GetItem()
    {
        return itemNoSlot;
    }

    public void ClicouNoSlot()
    {
        // MELHORIA CRUCIAL: Só envia para o Crafting se o script existir E se a mesa de crafting estiver VISÍVEL/ATIVA no ecrã
        bool craftingEstaAberto = MesaCraftingManager.instance != null && 
                                  MesaCraftingManager.instance.gameObject.activeInHierarchy;

        if (itemNoSlot != null && craftingEstaAberto)
        {
            // Tenta colocar o item na mesa de crafting
            bool aceitou = MesaCraftingManager.instance.AdicionarIngredienteAMesa(itemNoSlot);
            
            if (aceitou)
            {
                InventoryManager invManager = Object.FindFirstObjectByType<InventoryManager>();
                if (invManager != null)
                {
                    // PROCURA O ITEM CORRETO NA LISTA PARA REDUZIR A QUANTIDADE
                    for (int i = 0; i < invManager.items.Count; i++)
                    {
                        if (invManager.items[i].item == itemNoSlot)
                        {
                            invManager.items[i].quantidade -= 1; // Retira 1 do bolo

                            // Se o monte acabou, remove o slot da lista
                            if (invManager.items[i].quantidade <= 0)
                            {
                                invManager.items.RemoveAt(i);
                            }
                            break;
                        }
                    }
                    
                    // Atualiza a parte visual de forma limpa
                    InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                    if (invUI != null) invUI.AtualizarUI();
                }
            }
        }
        else if (itemNoSlot != null && !craftingEstaAberto)
        {
            // [OPCIONAL] O que acontece se clicares num item com o inventário normal aberto?
            // Se o item for comida/consumível, podes adicionar aqui a lógica para o jogador comer!
            // Exemplo: 
            // if(itemNoSlot.isComida) { ComerItem(); }
            
            Debug.Log($"Clicaste em {itemNoSlot.itemName}, mas o Crafting está fechado. Nada acontece.");
        }
    }
}