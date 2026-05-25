// ÁRVORE DE MADEIRA
using UnityEngine;

public class ArvoreMadeira : MonoBehaviour
{
    public int batidasParaCair = 3;
    public ItemData itemMadeira;

    [Header("Efeitos Visuais")]
    // ARRASTA O TEU PREFAB DE PARTÍCULAS VERDES PARA ESTE CAMPO NO INSPECTOR
    public GameObject prefabParticulasVerdes;

    public void LevarMachadada()
    {
        batidasParaCair--;
        if (batidasParaCair <= 0)
        {
            InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
            if (inv != null && itemMadeira != null) inv.AddItem(itemMadeira);
            
            DestruirArvore();
        }
    }

    public void DestruirArvore()
    {
        // 1. Verifica se o prefab foi colocado para evitar erros
        if (prefabParticulasVerdes != null)
        {
            // 2. Cria as partículas na posição exata da árvore e com a mesma rotação
            // Podes somar um Vector3(0, 2, 0) ao transform.position se quiseres que as partículas saiam do meio das folhas em vez da base do tronco
            Instantiate(prefabParticulasVerdes, transform.position, transform.rotation);
        }

        // 3. Tua lógica atual de dar madeira ao jogador...
        // DarMadeiraAoJogador();

        // 4. Destrói o objeto da árvore da cena de forma limpa
        Destroy(gameObject);
    }
}