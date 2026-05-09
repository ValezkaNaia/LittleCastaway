using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Adicionado para podermos ler o nome da cena e gravar

public class TendaScript : MonoBehaviour
{
    private bool playerPerto = false;
    
    [Header("Arraste os objetos aqui:")]
    public GameObject interactionUI;
    public Image fadeImage;

    [Header("Configurações")]
    public float tempoFade = 1.0f;

    void Update()
    {
        if (playerPerto && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(SequenciaDormir());
        }
    }

    IEnumerator SequenciaDormir()
    {
        if (fadeImage == null) yield break;

        playerPerto = false;
        if (interactionUI != null) interactionUI.SetActive(false);
        fadeImage.gameObject.SetActive(true);

        // --- ESCURECER ---
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime / tempoFade;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t));
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        // --- PASSAR O TEMPO ---
        SurvivalManager survival = FindFirstObjectByType<SurvivalManager>();
        if (survival != null)
        {
            // Salta 8 horas na perfeição
            survival.AvancarTempo(8f); 
        }

        // --- NOVO: GUARDAR O JOGO ---
        // Gravamos a cena enquanto o ecrã está preto.
        string cenaAtual = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("CenaGuardada", cenaAtual);
        PlayerPrefs.Save();
        Debug.Log("Jogo guardado com sucesso (Cena: " + cenaAtual + ")!");
        // ----------------------------

        yield return new WaitForSeconds(1.0f);

        // --- CLAREAR ---
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