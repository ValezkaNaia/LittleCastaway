using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class FimDeJogo : MonoBehaviour
{
    public VideoPlayer videoFinal;
    public string nomeDoMenu = "MainMenu";

    void Start()
    {
        // Garante que o rato aparece nas telas de fim
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    

        if (videoFinal != null)
        {
            videoFinal.loopPointReached += VoltarAoMenu;
        }
    }

    // Esta função é chamada pelo evento do vídeo (automaticamente)
    void VoltarAoMenu(VideoPlayer vp)
    {
        IrParaMenu();
    }

    // 1. ESTA É A NOVA FUNÇÃO PARA O BOTÃO
    public void BotaoVoltarAoMenu()
    {
        IrParaMenu();
    }

    private void IrParaMenu()
    {
        if (SceneFader.instance != null)
        {
            SceneFader.instance.FazerFadeEIrParaCena(nomeDoMenu);
        }
        else
        {
            SceneManager.LoadScene(nomeDoMenu);
        }
    }
}