using UnityEngine;

public class TutorialItem : MonoBehaviour
{
    [Header("As Páginas do Guia (Mete Tamanho 5)")]
    public Sprite[] paginas;

    public void ApanharLivro()
    {
        if (NoteManager.instance != null)
        {
            // Envia as tuas 5 imagens para o NoteManager
            NoteManager.instance.PickUpTutorial(paginas);
            
            // Oculta o livro do chão para não ser lido vezes infinitas
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("O NoteManager não foi encontrado na cena!");
        }
    }
}