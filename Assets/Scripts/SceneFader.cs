using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    // This 'instance' allows any other script to find this fader instantly!
    public static SceneFader instance; 

    [Header("Arraste o Canvas Group aqui")]
    public CanvasGroup fadeGroup;
    public float fadeSpeed = 1.5f;

    void Awake()
    {
        // Set up the instance
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Every time a scene loads, start black and fade into the game (Fade In)
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f; // Start completely black
            fadeGroup.blocksRaycasts = true; // Block clicks during the fade
            StartCoroutine(FadeIn());
        }
    }

    // Other scripts will call this function to change scenes
    public void FazerFadeEIrParaCena(string nomeCena)
    {
        StartCoroutine(FadeOut(nomeCena));
    }

    private IEnumerator FadeIn()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * fadeSpeed;
            fadeGroup.alpha = t;
            yield return null;
        }
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false; // Allow clicks again once visible
    }

    private IEnumerator FadeOut(string nomeCena)
    {
        fadeGroup.blocksRaycasts = true; // Block clicks while fading to black
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            fadeGroup.alpha = t;
            yield return null;
        }
        fadeGroup.alpha = 1f;
        
        // Once the screen is totally black, load the new scene!
        SceneManager.LoadScene(nomeCena);
    }
}