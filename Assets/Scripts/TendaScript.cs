using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 

public class TendaScript : MonoBehaviour
{
    private bool playerPerto = false;
    
    [Header("Arraste os objetos aqui:")]
    public GameObject interactionUI;
    public Image fadeImage;

    [Header("Sistema de Avisos")]
    public TextMeshProUGUI avisoTexto; 
    public string mensagemCedo = "It's too early to sleep! You can only rest after 19:00.";

    [Header("Configurações")]
    public float tempoFade = 1.0f;

    private Coroutine avisoCoroutine; 

    void Start()
    {
        if (avisoTexto != null) avisoTexto.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerPerto && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SurvivalManager survival = SurvivalManager.instance;

            if (survival.currentTimeInGameHours >= 19f || survival.currentTimeInGameHours < 6f)
            {
                if (avisoTexto != null) avisoTexto.gameObject.SetActive(false);
                StartCoroutine(SequenciaDormir(survival));
            }
            else
            {
                MostrarAviso(mensagemCedo);
            }
        }
    }

    private void MostrarAviso(string mensagem)
    {
        if (avisoTexto == null) return;
        if (avisoCoroutine != null) StopCoroutine(avisoCoroutine);
        avisoCoroutine = StartCoroutine(RotinaOcultarAviso(mensagem));
    }

    private IEnumerator RotinaOcultarAviso(string mensagem)
    {
        avisoTexto.text = mensagem;
        avisoTexto.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        avisoTexto.gameObject.SetActive(false);
    }

    IEnumerator SequenciaDormir(SurvivalManager survival)
    {
        if (fadeImage == null) yield break;

        playerPerto = false;
        if (interactionUI != null) interactionUI.SetActive(false);
        fadeImage.gameObject.SetActive(true);

        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime / tempoFade;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t));
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        if (survival != null)
        {
            survival.AvancarTempo(8f); 
            survival.ResetarCansaco(); 
        }

        string cenaAtual = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("CenaGuardada", cenaAtual);
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully (Scene: " + cenaAtual + ")!");

        yield return new WaitForSeconds(1.0f);

        while (t > 0f)
        {
            t -= Time.deltaTime / tempoFade;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t));
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
        playerPerto = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerPerto = true;
            if (interactionUI != null) interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerPerto = false;
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }
}