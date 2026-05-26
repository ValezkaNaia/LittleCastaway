using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuPausaManager : MonoBehaviour
{
    public static MenuPausaManager instance;
    public static bool isPaused = false;

    [Header("UI do Menu")]
    public GameObject painelPausa;
    public Slider sliderVolume;
    public Toggle toggleFullscreen;

    [Header("Configurações")]
    public string nomeMainMenu = "MainMenu"; 

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (painelPausa != null) painelPausa.SetActive(false);

        if (sliderVolume != null)
        {
            sliderVolume.value = AudioListener.volume;
            sliderVolume.onValueChanged.AddListener(MudarVolume);
        }

        if (toggleFullscreen != null)
        {
            toggleFullscreen.isOn = Screen.fullScreen;
            toggleFullscreen.onValueChanged.AddListener(MudarFullscreen);
        }
    }

    void Update()
    {
        if (NoteManager.isReading || PetNamingManager.isNaming) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                RetomarJogo();
            }
            else
            {
                PausarJogo();
            }
        }

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void PausarJogo()
    {
        isPaused = true;
        painelPausa.SetActive(true);
        Time.timeScale = 0f; 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RetomarJogo()
    {
        isPaused = false;
        painelPausa.SetActive(false);
        Time.timeScale = 1f; 

        InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
        if (invUI != null && invUI.inventoryPanel != null && invUI.inventoryPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void MudarVolume(float volume) { AudioListener.volume = volume; }
    public void MudarFullscreen(bool isFullscreen) { Screen.fullScreen = isFullscreen; }

    // ====================================================================
    // SISTEMA DE SAVE AUTOMÁTICO
    // ====================================================================
    private void GuardarJogoAntesDeSair()
    {
        Debug.Log("A gravar o jogo antes de sair...");
        
        // COLOCA AQUI O TEU CÓDIGO DE SAVE!
        // Exemplo: if (SaveManager.instance != null) SaveManager.instance.SaveGame();
    }

    public void VoltarAoMenuPrincipal()
    {
        GuardarJogoAntesDeSair(); // Grava o jogo!

        Time.timeScale = 1f; 
        isPaused = false;
        
        if (SceneFader.instance != null) SceneFader.instance.FazerFadeEIrParaCena(nomeMainMenu);
        else SceneManager.LoadScene(nomeMainMenu);
    }

    public void SairDoJogo()
    {
        GuardarJogoAntesDeSair(); // Grava o jogo!

        Debug.Log("A sair do jogo...");
        Application.Quit();
    }
}