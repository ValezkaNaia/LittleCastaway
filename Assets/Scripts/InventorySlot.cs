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
            iconeObject.SendMessage("set_sprite", item.itemIcon, SendMessageOptions.DontRequireReceiver);
            iconeObject.SetActive(true);

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

    public void ClicouNoSlot()
    {
        if (itemNoSlot != null && MesaCraftingManager.instance != null)
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
                    
                    // Atualiza a parte visual
                    Object.FindFirstObjectByType<InventoryUI>().AtualizarUI();
                }
            }
        }
    }
}
/*using UnityEngine;

public class InventorySlot : MonoBehaviour
{
// Arrastarás o objeto "Icon" (o filho do Slot) para aqui no Inspector.
    public GameObject iconeObject; 

    public void DefineItem(ItemData item)
    {
        if (iconeObject == null) return;

        if (item != null)
        {
            // Enviamos uma mensagem para o componente de imagem mudar o sprite
            // Isto funciona mesmo sem a referência de UI ativa no VS Code!
            iconeObject.SendMessage("set_sprite", item.itemIcon, SendMessageOptions.DontRequireReceiver);
            iconeObject.SetActive(true);
        }
        else
        {
            iconeObject.SetActive(false);
        }
    }

}*/
