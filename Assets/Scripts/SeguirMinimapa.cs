using UnityEngine;

public class SeguirMinimapa : MonoBehaviour
{
    public Transform player; 
    public float alturaCamera = 50f; 

    [Header("Bússola")]
    public RectTransform anelBussola; // O objeto vazio que tem as letras N, S, E, W

    void LateUpdate()
    {
        if (player != null)
        {
            // 1. Segue o jogador
            transform.position = new Vector3(player.position.x, player.position.y + alturaCamera, player.position.z);
            
            // 2. Roda a câmara com o jogador! (O mapa no ecrã vai rodar)
            // Mantém os 90 no X (para olhar para baixo) e copia a rotação Y do jogador
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);

            // 3. Roda o anel das letras no ecrã (se tiveres ligado o anel no Inspector)
            if (anelBussola != null)
            {
                // A bússola roda no sentido oposto ao jogador para manter o Norte no sítio certo
                anelBussola.localRotation = Quaternion.Euler(0f, 0f, player.eulerAngles.y);
            }
        }
    }
}