using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // ADICIONADO: Necessário para carregar as cenas de vitória e derrota

public class SurvivalManager : MonoBehaviour
{
    [Header("Elementos de UI - Barras")]
    public Slider healthBar;
    public Slider hungerBar;
    public Slider thirstBar;
    public Slider staminaBar; 
    public GameObject staminaBarObject; 

    [Header("Elementos de UI - Tempo")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;

    [Header("Ciclo Dia/Noite")]
    [Tooltip("Arrasta a tua Directional Light (Sol) da hierarquia para aqui")]
    public Transform sunTransform;

    [Header("Estatísticas Máximas")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxStamina = 100f;

    // Valores atuais
    private float currentHealth;
    private float currentHunger;
    private float currentThirst;
    private float currentStamina;

    [Header("Taxas (Fome/Sede)")]
    public float hungerDepletionRate = 1f;
    public float thirstDepletionRate = 2f;
    public float healthDamageRate = 5f;

    [Header("Mecânica de Stamina")]
    public float staminaDrainRate = 30f;  
    public float staminaRegenRate = 15f;  
    private bool isExhausted = false;     
 
    [Header("Sistema de Tempo")] 
    [Tooltip("Duração de 1 dia no jogo em segundos reais (600s = 10 minutos)")]
    public float dayDurationInRealSeconds = 600f; 
    [Tooltip("Número máximo de dias até o jogo acabar")]
    public int maxDays = 5;
    [Tooltip("Hora a que o jogo começa (ex: 8 para 08:00)")]
    public float startHour = 8f;

    [Header("Cenas de Fim de Jogo")] // ADICIONADO: Nomes das cenas
    public string cenaVitoria = "WinScene";
    public string cenaDerrota = "LoseScene";

    private float currentTimeInGameHours;
    private int currentDay = 1;
    private bool isGameFinished = false; 

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentStamina = maxStamina;

        currentTimeInGameHours = startHour;

        if (staminaBarObject != null) staminaBarObject.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (isGameFinished) return;

        HandleTime();
        HandleStats();
        HandleStamina();
        TestInputs();
    }

    void HandleTime()
    {
        float inGameHoursPerRealSecond = 24f / dayDurationInRealSeconds;
        currentTimeInGameHours += Time.deltaTime * inGameHoursPerRealSecond;

        if (currentTimeInGameHours >= 24f)
        {
            currentTimeInGameHours -= 24f; 
            currentDay++;

            if (currentDay > maxDays)
            {
                currentDay = maxDays;
                currentTimeInGameHours = 0f; 
                
                // ADICIONADO: Substituímos o Debug.Log pela função de ganhar
                GanharJogo();
                return; 
            }
        }

        // --- ATUALIZAÇÃO DO CICLO DIA/NOITE ---
        if (sunTransform != null)
        {
            float sunRotationX = (currentTimeInGameHours / 24f) * 360f - 90f;
            sunTransform.rotation = Quaternion.Euler(sunRotationX, 30f, 0f);
        }

        int hours = Mathf.FloorToInt(currentTimeInGameHours);
        if (hours >= 24) hours = 0; 
        
        int minutes = Mathf.FloorToInt((currentTimeInGameHours - hours) * 60);
        
        dayText.text = "Dia " + currentDay;
        timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    void HandleStats()
    {
        if (currentHunger > 0) currentHunger -= hungerDepletionRate * Time.deltaTime;
        if (currentThirst > 0) currentThirst -= thirstDepletionRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

        if (currentHunger <= 0 || currentThirst <= 0)
        {
            currentHealth -= healthDamageRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            
            // ADICIONADO: Verifica se morreu de fome/sede
            if (currentHealth <= 0)
            {
                PerderJogo();
            }
        }

        UpdateUI();
    }

    void HandleStamina()
    {
        bool isTryingToRun = Input.GetKey(KeyCode.LeftShift) && !isExhausted;

        if (isTryingToRun)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true; 
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                isExhausted = false;
            }
        }

        staminaBar.value = currentStamina / maxStamina;

        if (currentStamina == maxStamina && !Input.GetKey(KeyCode.LeftShift))
        {
            if (staminaBarObject != null && staminaBarObject.activeSelf)
                staminaBarObject.SetActive(false);
        }
        else
        {
            if (staminaBarObject != null && !staminaBarObject.activeSelf)
                staminaBarObject.SetActive(true);
        }
    }

    void UpdateUI()
    {
        healthBar.value = currentHealth / maxHealth;
        hungerBar.value = currentHunger / maxHunger;
        thirstBar.value = currentThirst / maxThirst;
    }

    public void DrinkWater(float amount) { currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst); }
    public void EatFood(float amount) { currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger); }

    // Função externa para retirar vida (ataques de animais, quedas, etc)
    public void ReceberDano(float quantidade)
    {
        currentHealth -= quantidade;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0 && !isGameFinished)
        {
            PerderJogo();
        }
    }

    void TestInputs()
    {
        if (Input.GetKeyDown(KeyCode.Q)) DrinkWater(20f);
        if (Input.GetKeyDown(KeyCode.E)) EatFood(30f);

        // --- ADICIONADO: BOTÕES DE CHEAT ---
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            Debug.Log("Cheat ativado: Morte instantânea!");
            ReceberDano(1000f); // Mata o jogador logo
        }

        if (Input.GetKeyDown(KeyCode.G)) 
        {
            Debug.Log("Cheat ativado: Ganhar jogo!");
            GanharJogo(); // Salta logo para a vitória
        }
    }

    // Função que avança o tempo quando dormes!
    public void AvancarTempo(float horas)
    {
        currentTimeInGameHours += horas;
        Debug.Log("O tempo avançou " + horas + " horas.");
    }

    // --- ADICIONADO: FUNÇÕES DE FIM DE JOGO ---
    private void GanharJogo()
    {
        isGameFinished = true;
        
        // Apaga o save para o próximo jogo começar do zero
        PlayerPrefs.DeleteKey("CenaGuardada"); 
        PlayerPrefs.Save();

    // OLD: SceneManager.LoadScene(cenaVitoria);
        SceneFader.instance.FazerFadeEIrParaCena(cenaVitoria); // NEW
    }

    private void PerderJogo()
    {
        isGameFinished = true;
        
        // Apaga o save para o jogador não fazer "Continue" e renascer morto
        PlayerPrefs.DeleteKey("CenaGuardada");
        PlayerPrefs.Save();

    // OLD: SceneManager.LoadScene(cenaDerrota);
        SceneFader.instance.FazerFadeEIrParaCena(cenaDerrota); // NEW
    }
}