using UnityEngine;

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

}
