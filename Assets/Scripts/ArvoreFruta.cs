// ÁRVORE DE FRUTA
using UnityEngine;

public class ArvoreFruta : MonoBehaviour
{
    public ItemData itemFruta;
    private bool jaFoiColhida = false; // Guarda se a árvore já foi limpa

    // Esta função devolve o estado da árvore para o Player saber o que exibir
    public bool TemFruta()
    {
        return !jaFoiColhida;
    }

    public void ApanharFruta()
    {
        // Se por algum motivo o código correr e já tiver sido colhida, interrompe
        if (jaFoiColhida) return;

        InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
        if (inv != null && itemFruta != null)
        {
            inv.AddItem(itemFruta);
            jaFoiColhida = true; // A árvore agora lembra-se que ficou vazia!
            
            // Exibe no terminal para saberes que correu bem
            Debug.Log(itemFruta.itemName + " adicionado ao inventário!");
        }
    }
}
/*using UnityEngine;

public class ArvoreFruta : MonoBehaviour
{
    public ItemData itemFruta;

    public void ApanharFruta()
    {
        InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
        if (inv != null && itemFruta != null)
        {
            inv.AddItem(itemFruta);
            Debug.Log(itemFruta.itemName + " colhido!");
        }
    }
}*/
