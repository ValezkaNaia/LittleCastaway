using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeCenaJogo = "CutsceneIntro";
    
    [Header("Interface e Animações")]
    public GameObject painelOpcoes;
    public CanvasGroup grupoBotoesIniciais;
    public CanvasGroup grupoOpcoes;
    
    // 1. AGORA TEMOS DUAS VELOCIDADES DIFERENTES!
    public float velocidadeFadeIniciais = 1.5f; // Lento para o ecrã inicial
    public float velocidadeFadeOpcoes = 0.3f;   // Muito rápido (0.3s) para as opções
    public float atrasoBotoesIniciais = 2.0f; 

    void Start()
    {
        if (grupoBotoesIniciais != null)
        {
            grupoBotoesIniciais.alpha = 0f;
            grupoBotoesIniciais.interactable = false; 
            StartCoroutine(AtrasarFadeBotoes()); 
            Cursor.visible = true;
        }
    }

    private IEnumerator AtrasarFadeBotoes()
    {
        yield return new WaitForSeconds(atrasoBotoesIniciais);
        
        // 2. Usamos a velocidade LENTA aqui
        yield return StartCoroutine(FazerFade(grupoBotoesIniciais, 0f, 1f, velocidadeFadeIniciais));
        grupoBotoesIniciais.interactable = true;
    }

     public void NovoJogo()
    {
        PlayerPrefs.DeleteAll(); 
        // OLD: SceneManager.LoadScene(nomeCenaJogo);
        SceneFader.instance.FazerFadeEIrParaCena(nomeCenaJogo); // NEW
    }

    public void Continuar()
    {
        if (PlayerPrefs.HasKey("CenaGuardada"))
        {
            string cenaParaCarregar = PlayerPrefs.GetString("CenaGuardada");
            // OLD: SceneManager.LoadScene(cenaParaCarregar);
            SceneFader.instance.FazerFadeEIrParaCena(cenaParaCarregar); // NEW
        }
        else
        {
            Debug.Log("Não existe nenhum jogo guardado!");
        }
    }

    public void AbrirOpcoes()
    {
        painelOpcoes.SetActive(true); 
        grupoOpcoes.alpha = 0f; 
        
        // 3. Usamos a velocidade RÁPIDA aqui
        StartCoroutine(FazerFade(grupoOpcoes, 0f, 1f, velocidadeFadeOpcoes)); 
    }

    public void FecharOpcoes()
    {
        // 4. Mandamos a velocidade rápida também para o fechar
        StartCoroutine(FadeOutEFechar(grupoOpcoes, velocidadeFadeOpcoes)); 
    }

    public void SairDoJogo()
    {
        Debug.Log("O jogo fechou!"); 
        Application.Quit(); 
    }
    
    // --- O MOTOR FOI ATUALIZADO ---
    // Agora ele pede um "tempoDuracao" sempre que for chamado
    private IEnumerator FazerFade(CanvasGroup canvasGrupo, float inicio, float fim, float tempoDuracao)
    {
        float tempoPassado = 0f;
        canvasGrupo.alpha = inicio;

        while (tempoPassado < tempoDuracao)
        {
            tempoPassado += Time.deltaTime;
            canvasGrupo.alpha = Mathf.Lerp(inicio, fim, tempoPassado / tempoDuracao);
            yield return null; 
        }
        canvasGrupo.alpha = fim; 
    }

    // Também atualizámos esta para aceitar a duração
    private IEnumerator FadeOutEFechar(CanvasGroup canvasGrupo, float tempoDuracao)
    {
        yield return StartCoroutine(FazerFade(canvasGrupo, 1f, 0f, tempoDuracao));
        painelOpcoes.SetActive(false);
    }
}