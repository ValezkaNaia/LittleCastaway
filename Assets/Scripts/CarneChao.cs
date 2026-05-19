using UnityEngine;

public class CarneDoChao : MonoBehaviour
{
    public ItemData itemCarne; // Arrasta o ItemData da Carne aqui

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem passou por cima da carne foi o Player
        if (other.CompareTag("Player"))
        {
            InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
            
            if (inv != null && itemCarne != null)
            {
                inv.AddItem(itemCarne);
                Debug.Log("Apanhaste carne do chão!");
                
                // Destrói a carne do chão para ela sumir após ser recolhida
                Destroy(gameObject);
            }
        }
    }
}