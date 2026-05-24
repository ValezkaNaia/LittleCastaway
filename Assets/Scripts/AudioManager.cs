using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string nomeDoSom; // Ex: "Soco", "TigreRugido", "PegarNota"
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fontes de Som (Colocar AudioSources aqui)")]
    public AudioSource musicSource; // Para a música de fundo
    public AudioSource sfxSource;   // Para sons do jogador e UI

    [Header("Biblioteca de Efeitos Sonoros")]
    public Sound[] efeitosSonoros;

    private void Awake()
    {
        // Garante que só existe um AudioManager no jogo inteiro
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: mantê-lo entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // FUNÇÃO PARA TOCAR EFEITOS SONOROS (SFX)
    // ==========================================
    public void TocarSFX(string nome)
    {
        Sound somParaTocar = null;

        // Procura na lista o som com o nome que pedimos
        foreach (Sound s in efeitosSonoros)
        {
            if (s.nomeDoSom == nome)
            {
                somParaTocar = s;
                break;
            }
        }

        if (somParaTocar != null)
        {
            sfxSource.PlayOneShot(somParaTocar.clip, somParaTocar.volume);
        }
        else
        {
            Debug.LogWarning("Som não encontrado: " + nome);
        }
    }

    // ==========================================
    // FUNÇÃO PARA MUDAR MÚSICA DE FUNDO
    // ==========================================
    public void MudarMusicaDeFundo(AudioClip novaMusica)
    {
        if (musicSource.clip == novaMusica) return; // Já está a tocar esta!

        musicSource.Stop();
        musicSource.clip = novaMusica;
        musicSource.Play();
    }
}