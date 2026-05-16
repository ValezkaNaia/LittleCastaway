using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 5f;
    public TextMeshProUGUI textoInteracao;

    void Update()
    {
        // Se estiveres a ler uma nota ou no menu de coleção, não podes interagir com o mundo!
        if (NoteManager.isReading) return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward * interactionRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            WorldNote nota = hit.collider.GetComponent<WorldNote>();

            if (item != null)
            {
                if (textoInteracao != null) 
                {
                    textoInteracao.text = "Apanhar " + hit.collider.gameObject.name + " [F]";
                    textoInteracao.gameObject.SetActive(true);
                }

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    item.SerApanhado();
                    if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
                }
            }
            else if (nota != null) 
            {
                if (textoInteracao != null) 
                {
                    textoInteracao.text = "Ler " + hit.collider.gameObject.name + " [F]";
                    textoInteracao.gameObject.SetActive(true);
                }

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    nota.LerNota();
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