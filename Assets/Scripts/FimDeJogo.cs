using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Necessário para controlar vídeos

public class FimDeJogo : MonoBehaviour
{
    public VideoPlayer videoFinal;
    public string nomeDoMenu = "MainMenu"; // Substitui pelo nome EXATO da tua cena do menu inicial

    void Start()
    {
        if (videoFinal != null)
        {
            // Isto diz ao Unity: "Quando o vídeo chegar ao fim, executa a função VoltarAoMenu"
            videoFinal.loopPointReached += VoltarAoMenu;
        }
        else
        {
            Debug.LogError("Esqueceste-te de arrastar o VideoPlayer para o script!");
        }
    }

    // Esta função é chamada automaticamente quando o vídeo acaba
    void VoltarAoMenu(VideoPlayer vp)
    {
        // Opcional: Desbloquear o rato para o jogador poder clicar no menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // OLD: SceneManager.LoadScene(nomeDoMenu);
        SceneFader.instance.FazerFadeEIrParaCena(nomeDoMenu); // NEW
    }
}