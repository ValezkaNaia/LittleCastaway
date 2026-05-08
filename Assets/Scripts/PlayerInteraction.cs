using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Adiciona isto para controlar o texto

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 5f;
    public TextMeshProUGUI textoInteracao; // Arrastaremos o texto para aqui

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Raio para debug
        Debug.DrawRay(transform.position, transform.forward * interactionRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            ItemObject item = hit.collider.GetComponent<ItemObject>();

            if (item != null)
            {
                // Liga o texto na tela
                if (textoInteracao != null) textoInteracao.gameObject.SetActive(true);

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    item.SerApanhado();
                    // Desliga o texto mal apanhamos o item
                    if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
                }
            }
            else
            {
                // Se o raio bater em algo que NÃO é um item (chão, parede)
                if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
            }
        }
        else
        {
            // Se o raio não bater em nada
            if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
        }
    }
}