using UnityEngine;
using UnityEngine.Video;

public class IntroVideoHandler : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    [Header("Configuração")]
    public string nomeCenaJogo = "Game"; 

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += AoTerminarVideo;
        Cursor.visible = true;
    }

    // Tornamos esta função PUBLIC para o botão a encontrar
    public void PularCutscene()
    {
        CarregarJogo();
    }

    void AoTerminarVideo(VideoPlayer vp)
    {
        CarregarJogo();
    }

    void CarregarJogo()
    {
        if (SceneFader.instance != null)
        {
            SceneFader.instance.FazerFadeEIrParaCena(nomeCenaJogo);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nomeCenaJogo);
        }
    }
}