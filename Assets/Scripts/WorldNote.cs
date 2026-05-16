using UnityEngine;

public class WorldNote : MonoBehaviour
{
    [Header("The Photoshop Image")]
    public Sprite myNoteImage; // Arrasta a imagem desta nota específica aqui!

    // Esta função agora é ativada pelo teu PlayerInteraction!
    public void LerNota()
    {
        // Envia a imagem para o ecrã
        NoteManager.instance.PickUpAndReadNote(myNoteImage);
        
        // Destrói o objeto 3D
        Destroy(gameObject); 
    }
}