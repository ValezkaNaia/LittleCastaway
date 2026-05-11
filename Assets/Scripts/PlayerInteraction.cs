using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 5f;
    public TextMeshProUGUI textoInteracao;

    void Update()
    {
        // Nota: Se o laser estiver a sair dos "pés" do jogador, muda transform.position para a posição da tua Câmara!
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward * interactionRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            ItemObject item = hit.collider.GetComponent<ItemObject>();

            if (item != null)
            {
                if (textoInteracao != null) 
                {
                    // --- AQUI ESTÁ A MAGIA! ---
                    // Pega no nome do objeto 3D e junta com o texto de instrução
                    textoInteracao.text = "Apanhar " + hit.collider.gameObject.name + " [F]";
                    
                    textoInteracao.gameObject.SetActive(true);
                }

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    item.SerApanhado();
                    if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
                }
            }
            else
            {
                if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
            }
        }
        else
        {
            if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
        }
    }
}