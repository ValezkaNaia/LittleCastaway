using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fontes de Áudio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [System.Serializable]
    public struct SomBiblioteca
    {
        public string nome;
        public AudioClip clip;
    }

    [Header("Biblioteca de Efeitos Sonoros")]
    public List<SomBiblioteca> efeitosSonoros = new List<SomBiblioteca>();

    private AudioClip musicaPadraoIlha;
    private Coroutine corotinaFade;
    private float volumeMaximoOriginal = 1.0f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (musicSource != null)
        {
            musicaPadraoIlha = musicSource.clip;
            volumeMaximoOriginal = musicSource.volume;
        }
    }

    public void TocarSFX(string nomeSom)
    {
        AudioClip clip = EncontrarClip(nomeSom);
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Altera a música com um efeito suave de Fade Out e Fade In
    public void MudarMusicaDeFundo(AudioClip novaMusica)
    {
        if (musicSource == null || musicSource.clip == novaMusica) return;

        if (corotinaFade != null) StopCoroutine(corotinaFade);
        corotinaFade = StartCoroutine(TransicaoMusica(novaMusica));
    }

    // Função que os animais vão chamar ao morrer para parar a música de guerra
    public void ResetarMusicaParaNormal()
    {
        if (musicaPadraoIlha != null)
        {
            MudarMusicaDeFundo(musicaPadraoIlha);
        }
    }

    private IEnumerator TransicaoMusica(AudioClip novoClip)
    {
        // Fade Out (Desaparece a música antiga)
        while (musicSource.volume > 0)
        {
            musicSource.volume -= Time.deltaTime * 1.5f; // Velocidade do fade
            yield return null;
        }

        musicSource.clip = novoClip;
        
        if (novoClip != null)
        {
            musicSource.Play();
            // Fade In (Aparece a música nova)
            while (musicSource.volume < volumeMaximoOriginal)
            {
                musicSource.volume += Time.deltaTime * 1.5f;
                yield return null;
            }
        }
        musicSource.volume = volumeMaximoOriginal;
    }

    private AudioClip EncontrarClip(string nomeSom)
    {
        foreach (var som in efeitosSonoros)
        {
            if (som.nome == nomeSom) return som.clip;
        }
        return null;
    }
}