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
    [Tooltip("Já não é necessário, o Survival Manager agora trata do Ecrã Preto! Podes deixar aqui para não dar erro.")]
    public Image fadeImage;

    [Header("Sistema de Avisos")]
    public TextMeshProUGUI avisoTexto; 
    public string mensagemCedo = "It's too early to sleep! You can only rest after 19:00.";

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

            // Verifica se está na hora de dormir (depois das 19h ou antes das 6h)
            if (survival.currentTimeInGameHours >= 19f || survival.currentTimeInGameHours < 6f)
            {
                if (avisoTexto != null) avisoTexto.gameObject.SetActive(false);
                if (interactionUI != null) interactionUI.SetActive(false);
                
                // 1. CHAMA A NOVA FUNÇÃO DO SURVIVAL MANAGER (Cura, Fome, Sede e Ecrã Preto)
                survival.DormirNaTenda();

                // 2. GRAVA O JOGO (Mantive a tua lógica de save intacta!)
                string cenaAtual = SceneManager.GetActiveScene().name;
                PlayerPrefs.SetString("CenaGuardada", cenaAtual);
                PlayerPrefs.Save();
                Debug.Log("Game saved successfully (Scene: " + cenaAtual + ")!");
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