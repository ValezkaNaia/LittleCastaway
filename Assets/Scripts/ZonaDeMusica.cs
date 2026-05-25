using UnityEngine;

public class ZonaDeMusica : MonoBehaviour
{
    [Header("Configuração de Música")]
    public AudioClip musicaDestaZona; // Arrasta a MusicaCaverna ou MusicaTemplo
    public AudioClip musicaNormalDaIlha; // Arrasta a MusicaIlha

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.instance.MudarMusicaDeFundo(musicaDestaZona);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Quando sai da caverna, volta à música da ilha
            AudioManager.instance.MudarMusicaDeFundo(musicaNormalDaIlha);
        }
    }
}