using UnityEngine;
using TMPro;

public class PetNamingManager : MonoBehaviour
{
    public static PetNamingManager instance;
    public static bool isNaming = false;

    [Header("UI Elementos")]
    public GameObject painelNomear;
    public TMP_InputField inputFieldNome;
    
    private AnimalAI petAtual;

    void Awake()
    {
        if (instance == null) instance = this;
        if (painelNomear != null) painelNomear.SetActive(false);
    }

    public void AbrirPainel(AnimalAI petAdotado)
    {
        petAtual = petAdotado;
        painelNomear.SetActive(true);
        inputFieldNome.text = ""; // Limpa a caixa de texto
        
        isNaming = true;
        
        // Pausa o jogo e solta o rato para poderes escrever
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ConfirmarNome()
    {
        if (petAtual != null)
        {
            string nomeEscolhido = inputFieldNome.text.Trim();
            // Guarda o nome (se deixares em branco, a lógica usa "Loyal Companion" na mesma)
            petAtual.nomeDoPet = nomeEscolhido; 
        }
        
        painelNomear.SetActive(false);
        isNaming = false;
        
        // Volta ao jogo
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}