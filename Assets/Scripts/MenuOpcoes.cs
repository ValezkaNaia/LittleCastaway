using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuOpcoes : MonoBehaviour
{
    [Header("Onde estamos?")]
    public bool estaNoMenuPrincipal = false;

    [Header("Menu de Pausa")]
    public GameObject painelOpcoes; 
    public static bool jogoPausado = false; // Tornámos isto "public static" para o Player poder ler!

    [Header("Controlos da Interface")]
    public Slider sliderVolume;
    public Toggle toggleEcraInteiro;
    
    [Header("Câmaras")]
    public Toggle toggleCamera;
    public GameObject cameraPrimeiraPessoa;
    public GameObject cameraTerceiraPessoa;

    void Start()
    {
        if (PlayerPrefs.HasKey("VolumeGuardado"))
        {
            float volumeSalvo = PlayerPrefs.GetFloat("VolumeGuardado");
            if(sliderVolume != null) sliderVolume.value = volumeSalvo;
            AudioListener.volume = volumeSalvo;
        }

        if (PlayerPrefs.HasKey("EcraInteiroGuardado"))
        {
            bool ecraInteiroSalvo = PlayerPrefs.GetInt("EcraInteiroGuardado") == 1;
            if(toggleEcraInteiro != null) toggleEcraInteiro.isOn = ecraInteiroSalvo;
            Screen.fullScreen = ecraInteiroSalvo;
        }

        if (!estaNoMenuPrincipal) ContinuarJogo(); 
    }

    void Update()
    {
        if (!estaNoMenuPrincipal)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (jogoPausado) ContinuarJogo();
                else PausarJogo();
            }
        }
    }

    public void PausarJogo()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(true);
        Time.timeScale = 0f; 
        jogoPausado = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ContinuarJogo()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
        Time.timeScale = 1f; 
        jogoPausado = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void AlterarVolume(float valorVolume)
    {
        AudioListener.volume = valorVolume;
        PlayerPrefs.SetFloat("VolumeGuardado", valorVolume);
    }

    public void AlterarEcraInteiro(bool eEcraInteiro)
    {
        Screen.fullScreen = eEcraInteiro;
        PlayerPrefs.SetInt("EcraInteiroGuardado", eEcraInteiro ? 1 : 0);
    }

    public void AlterarCamera(bool isPrimeiraPessoa)
    {
        if (cameraPrimeiraPessoa != null && cameraTerceiraPessoa != null)
        {
            cameraPrimeiraPessoa.SetActive(isPrimeiraPessoa);
            cameraTerceiraPessoa.SetActive(!isPrimeiraPessoa);
        }
    }

    public void SairParaMenu()
    {
        Time.timeScale = 1f; 
        jogoPausado = false; // Garante que reinicia a variável
        if (SceneFader.instance != null) SceneFader.instance.FazerFadeEIrParaCena("MainMenu"); 
        else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}