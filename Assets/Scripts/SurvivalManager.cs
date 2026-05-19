using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SurvivalManager : MonoBehaviour
{
    public static SurvivalManager instance; 

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
    public Transform sunTransform;

    [Header("Estatísticas Máximas")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxStamina = 100f;

    private float currentHealth;
    private float currentHunger;
    private float currentThirst;
    private float currentStamina;

    [Header("Taxas (Fome/Sede)")]
    public float hungerDepletionRate = 0.2f; 
    public float thirstDepletionRate = 0.3f; 
    [Tooltip("Velocidade com que perde vida quando a fome ou a sede batem no 0")]
    public float starvationDamageRate = 0.5f; 

    [Header("Mecânica de Stamina")]
    public float staminaDrainRate = 25f;  
    public float staminaRegenRate = 15f;  
    public bool isExhausted = false;     
 
    [Header("Sistema de Tempo")] 
    public float dayDurationInRealSeconds = 600f; 
    public int maxDays = 5;
    public float startHour = 8f;

    [Header("Cenas de Fim de Jogo")] 
    public string cenaVitoria = "WinScene";
    public string cenaDerrota = "LoseScene";

    [Header("Sistema de Fadiga e Sono")]
    public CanvasGroup visaoTurvaOverlay; 
    public GameObject painelEcraPreto; 
    public TextMeshProUGUI textoSonoUI;
    
    [Tooltip("Quantas horas IN-GAME o jogador aguenta sem dormir")]
    public float horasParaDesmaiar = 36f; 
    public float danoPorDesmaio = 20f;

    private float currentFatigue = 0f;
    public float currentTimeInGameHours; 
    private int currentDay = 1;
    private bool isGameFinished = false; 
    private bool estaADormir = false; 

    // Guardar valores antigos para evitar atualizar a UI sem necessidade
    private float lastHealth, lastHunger, lastThirst;

    void Awake() { if (instance == null) instance = this; }

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentStamina = maxStamina;
        currentTimeInGameHours = startHour;

        if (staminaBarObject != null) staminaBarObject.SetActive(false);
        if (painelEcraPreto != null) painelEcraPreto.SetActive(false);
        if (visaoTurvaOverlay != null) visaoTurvaOverlay.alpha = 0f;

        ForçarAtualizacaoUI();
    }

    void Update()
    {
        if (isGameFinished || estaADormir || NoteManager.isReading) return;

        HandleTime();
        HandleStats();
        HandleStamina();
        HandleFatigue(); 
    }

    void HandleTime()
    {
        float inGameHoursPerRealSecond = 24f / dayDurationInRealSeconds;
        AdicionarHorasLogicas(Time.deltaTime * inGameHoursPerRealSecond);
    }

    // Centraliza a adição de tempo para evitar bugs ao passar a noite
    private void AdicionarHorasLogicas(float horas)
    {
        currentTimeInGameHours += horas;

        if (currentTimeInGameHours >= 24f)
        {
            currentTimeInGameHours -= 24f; 
            currentDay++;

            if (currentDay > maxDays)
            {
                currentDay = maxDays;
                currentTimeInGameHours = 0f; 
                GanharJogo();
                return; 
            }
        }

        AtualizarVisualTempo();
    }

    void AtualizarVisualTempo()
    {
        if (sunTransform != null)
        {
            float sunRotationX = (currentTimeInGameHours / 24f) * 360f - 90f;
            sunTransform.rotation = Quaternion.Euler(sunRotationX, 30f, 0f);
        }

        int hours = Mathf.FloorToInt(currentTimeInGameHours);
        if (hours >= 24) hours = 0; 
        int minutes = Mathf.FloorToInt((currentTimeInGameHours - hours) * 60);
        
        if (dayText != null) dayText.text = "Day " + currentDay;
        if (timeText != null) timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    void HandleStats()
    {
        if (currentHunger > 0) currentHunger -= hungerDepletionRate * Time.deltaTime;
        if (currentThirst > 0) currentThirst -= thirstDepletionRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

        if (currentHunger <= 0 || currentThirst <= 0)
        {
            currentHealth -= starvationDamageRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth <= 0 && !isGameFinished)
            {
                PerderJogo();
            }
        }

        VerificarEAtualizarUI();
    }

    void HandleStamina()
    {
        bool isTryingToRun = Input.GetKey(KeyCode.LeftShift) && !isExhausted;

        if (isTryingToRun)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0) { currentStamina = 0; isExhausted = true; }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina >= 20f && isExhausted) isExhausted = false; 
            if (currentStamina >= maxStamina) currentStamina = maxStamina;
        }

        if (staminaBar != null) staminaBar.value = currentStamina / maxStamina;

        if (currentStamina == maxStamina && !Input.GetKey(KeyCode.LeftShift))
        {
            if (staminaBarObject != null && staminaBarObject.activeSelf) staminaBarObject.SetActive(false);
        }
        else
        {
            if (staminaBarObject != null && !staminaBarObject.activeSelf) staminaBarObject.SetActive(true);
        }
    }

    void HandleFatigue()
    {
        float inGameHoursPerRealSecond = 24f / dayDurationInRealSeconds;
        float horasPassadasNesteFrame = Time.deltaTime * inGameHoursPerRealSecond;

        currentFatigue += (100f / horasParaDesmaiar) * horasPassadasNesteFrame;
        currentFatigue = Mathf.Clamp(currentFatigue, 0f, 100f);

        if (currentFatigue >= 70f)
        {
            if (visaoTurvaOverlay != null)
            {
                float progressoCansaco = (currentFatigue - 70f) / 30f;
                visaoTurvaOverlay.alpha = progressoCansaco * 0.90f; 
            }
        }
        else
        {
            if (visaoTurvaOverlay != null) visaoTurvaOverlay.alpha = 0f;
        }

        if (currentFatigue >= 100f) ForçarDesmaio();
    }

    private void ForçarDesmaio()
    {
        currentFatigue = 0f; 
        if (textoSonoUI != null) textoSonoUI.text = "You were so exhausted that you passed out...";
        StartCoroutine(RotinaSono(true));
    }

    private IEnumerator RotinaSono(bool foiForçado)
    {
        estaADormir = true;
        CanvasGroup cg = null;
        
        if (painelEcraPreto != null) 
        {
            cg = painelEcraPreto.GetComponent<CanvasGroup>();
            if (cg == null) cg = painelEcraPreto.AddComponent<CanvasGroup>();

            painelEcraPreto.SetActive(true);
            cg.alpha = 0f; 
        }

        float t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime / 1.5f;
            if (cg != null) cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        
        if (foiForçado) ReceberDano(danoPorDesmaio);

        yield return new WaitForSecondsRealtime(3.0f);

        AvancarTempo(8f);
        ResetarCansaco();

        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / 2.0f;
            if (cg != null) cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        if (painelEcraPreto != null) painelEcraPreto.SetActive(false);
        estaADormir = false;
    }

    public void ResetarCansaco()
    {
        currentFatigue = 0f; 
        if (visaoTurvaOverlay != null) visaoTurvaOverlay.alpha = 0f; 
    }

    public void DrinkWater(float amount) 
    { 
        currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst); 
        ForçarAtualizacaoUI();
    }
    
    public void EatFood(float amount) 
    { 
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger); 
        ForçarAtualizacaoUI();
    }

    public void ReceberDano(float quantidade)
    {
        currentHealth -= quantidade;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        ForçarAtualizacaoUI();
        if (currentHealth <= 0 && !isGameFinished) PerderJogo();
    }

    public void ReceberNutricao(float quantidade, ItemData.TipoConsumivel tipo)
    {
        if (tipo == ItemData.TipoConsumivel.Comida)
        {
            currentHunger = Mathf.Min(currentHunger + quantidade, maxHunger);
            Debug.Log($"[Survival] Comeste! Fome atual: {currentHunger}");
        }
        else if (tipo == ItemData.TipoConsumivel.Agua)
        {
            currentThirst = Mathf.Min(currentThirst + quantidade, maxThirst);
            Debug.Log($"[Survival] Bebeste! Sede atual: {currentThirst}");
        }
        ForçarAtualizacaoUI();
    }

    public void AvancarTempo(float horas) 
    { 
        AdicionarHorasLogicas(horas); 
    }

    // Otimização: Só atualiza se houver mudanças reais nos números
    void VerificarEAtualizarUI()
    {
        if (currentHealth != lastHealth || currentHunger != lastHunger || currentThirst != lastThirst)
        {
            ForçarAtualizacaoUI();
        }
    }

    public void ForçarAtualizacaoUI()
    {
        if (healthBar != null) healthBar.value = currentHealth / maxHealth;
        if (hungerBar != null) hungerBar.value = currentHunger / maxHunger;
        if (thirstBar != null) thirstBar.value = currentThirst / maxThirst;

        lastHealth = currentHealth;
        lastHunger = currentHunger;
        lastThirst = currentThirst;
    }

    private void GanharJogo()
    {
        isGameFinished = true;
        PlayerPrefs.DeleteKey("CenaGuardada"); 
        PlayerPrefs.Save();
        SceneFader.instance.FazerFadeEIrParaCena(cenaVitoria);
    }

    private void PerderJogo()
    {
        isGameFinished = true;
        PlayerPrefs.DeleteKey("CenaGuardada");
        PlayerPrefs.Save();
        SceneFader.instance.FazerFadeEIrParaCena(cenaDerrota);
    }
}
/*ing UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SurvivalManager : MonoBehaviour
{
    public static SurvivalManager instance; 

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
    public Transform sunTransform;

    [Header("Estatísticas Máximas")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxStamina = 100f;

    private float currentHealth;
    private float currentHunger;
    private float currentThirst;
    private float currentStamina;

    [Header("Taxas (Fome/Sede)")]
    public float hungerDepletionRate = 0.2f; 
    public float thirstDepletionRate = 0.3f; 
    [Tooltip("Velocidade com que perde vida quando a fome ou a sede batem no 0 (0.5f significa 1 de vida a cada 2 segundos)")]
    public float starvationDamageRate = 0.5f; // ADICIONADO: Dano lento por fome/sede

    [Header("Mecânica de Stamina")]
    public float staminaDrainRate = 25f;  
    public float staminaRegenRate = 15f;  
    public bool isExhausted = false;     
 
    [Header("Sistema de Tempo")] 
    public float dayDurationInRealSeconds = 600f; 
    public int maxDays = 5;
    public float startHour = 8f;

    [Header("Cenas de Fim de Jogo")] 
    public string cenaVitoria = "WinScene";
    public string cenaDerrota = "LoseScene";

    [Header("Sistema de Fadiga e Sono")]
    public CanvasGroup visaoTurvaOverlay; 
    public GameObject painelEcraPreto; 
    public TextMeshProUGUI textoSonoUI;
    
    [Tooltip("Quantas horas IN-GAME o jogador aguenta sem dormir (36h = 1 dia e meio)")]
    public float horasParaDesmaiar = 36f; 
    public float danoPorDesmaio = 20f;

    private float currentFatigue = 0f;
    public float currentTimeInGameHours; 
    private int currentDay = 1;
    private bool isGameFinished = false; 
    private bool estaADormir = false; 

    void Awake() { if (instance == null) instance = this; }

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentStamina = maxStamina;
        currentTimeInGameHours = startHour;

        if (staminaBarObject != null) staminaBarObject.SetActive(false);
        if (painelEcraPreto != null) painelEcraPreto.SetActive(false);
        if (visaoTurvaOverlay != null) visaoTurvaOverlay.alpha = 0f;

        UpdateUI();
    }

    void Update()
    {
        if (isGameFinished || estaADormir || NoteManager.isReading) return;

        HandleTime();
        HandleStats();
        HandleStamina();
        HandleFatigue(); 
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
                GanharJogo();
                return; 
            }
        }

        if (sunTransform != null)
        {
            float sunRotationX = (currentTimeInGameHours / 24f) * 360f - 90f;
            sunTransform.rotation = Quaternion.Euler(sunRotationX, 30f, 0f);
        }

        int hours = Mathf.FloorToInt(currentTimeInGameHours);
        if (hours >= 24) hours = 0; 
        int minutes = Mathf.FloorToInt((currentTimeInGameHours - hours) * 60);
        
        dayText.text = "Day " + currentDay;
        timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    void HandleStats()
    {
        // Reduz a fome e a sede continuamente
        if (currentHunger > 0) currentHunger -= hungerDepletionRate * Time.deltaTime;
        if (currentThirst > 0) currentThirst -= thirstDepletionRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

        // MODIFICADO: Se a fome OU a sede chegarem a 0, o jogador começa a perder vida lentamente
        if (currentHunger <= 0 || currentThirst <= 0)
        {
            currentHealth -= starvationDamageRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // Se a vida chegar a zero por fome/sede, o jogo acaba
            if (currentHealth <= 0 && !isGameFinished)
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
            if (currentStamina <= 0) { currentStamina = 0; isExhausted = true; }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina >= 20f && isExhausted) isExhausted = false; 
            if (currentStamina >= maxStamina) currentStamina = maxStamina;
        }

        staminaBar.value = currentStamina / maxStamina;

        if (currentStamina == maxStamina && !Input.GetKey(KeyCode.LeftShift))
        {
            if (staminaBarObject != null && staminaBarObject.activeSelf) staminaBarObject.SetActive(false);
        }
        else
        {
            if (staminaBarObject != null && !staminaBarObject.activeSelf) staminaBarObject.SetActive(true);
        }
    }

    void HandleFatigue()
    {
        float inGameHoursPerRealSecond = 24f / dayDurationInRealSeconds;
        float horasPassadasNesteFrame = Time.deltaTime * inGameHoursPerRealSecond;

        currentFatigue += (100f / horasParaDesmaiar) * horasPassadasNesteFrame;
        currentFatigue = Mathf.Clamp(currentFatigue, 0f, 100f);

        if (currentFatigue >= 70f)
        {
            if (visaoTurvaOverlay != null)
            {
                float progressoCansaco = (currentFatigue - 70f) / 30f;
                visaoTurvaOverlay.alpha = progressoCansaco * 0.90f; 
            }
        }
        else
        {
            if (visaoTurvaOverlay != null) visaoTurvaOverlay.alpha = 0f;
        }

        if (currentFatigue >= 100f) ForçarDesmaio();
    }

    private void ForçarDesmaio()
    {
        currentFatigue = 0f; 
        if (textoSonoUI != null) textoSonoUI.text = "You were so exhausted that you passed out...";
        StartCoroutine(RotinaSono(true));
    }

    private IEnumerator RotinaSono(bool foiForçado)
    {
        estaADormir = true;
        CanvasGroup cg = null;
        
        if (painelEcraPreto != null) 
        {
            cg = painelEcraPreto.GetComponent<CanvasGroup>();
            if (cg == null) cg = painelEcraPreto.AddComponent<CanvasGroup>();

            painelEcraPreto.SetActive(true);
            cg.alpha = 0f; 
        }

        // FADE IN
        float t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime / 1.5f;
            if (cg != null) cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        
        if (foiForçado) ReceberDano(danoPorDesmaio);

        yield return new WaitForSecondsRealtime(3.0f);

        AvancarTempo(8f);
        ResetarCansaco();

        // FADE OUT
        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / 2.0f;
            if (cg != null) cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        if (painelEcraPreto != null) painelEcraPreto.SetActive(false);
        estaADormir = false;
    }

    public void ResetarCansaco()
    {
        currentFatigue = 0f; 
        if (visaoTurvaOverlay != null) visaoTurvaOverlay.alpha = 0f; 
    }

    public void DrinkWater(float amount) { currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst); }
    public void EatFood(float amount) { currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger); }

    public void ReceberDano(float quantidade)
    {
        currentHealth -= quantidade;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
        if (currentHealth <= 0 && !isGameFinished) PerderJogo();
    }

    public void ReceberNutricao(float quantidade, ItemData.TipoConsumivel tipo)
    {
        if (tipo == ItemData.TipoConsumivel.Comida)
        {
            currentHunger = Mathf.Min(currentHunger + quantidade, maxHunger);
            Debug.Log($"[Survival] Comeste! Fome atual: {currentHunger}");
        }
        else if (tipo == ItemData.TipoConsumivel.Agua)
        {
            currentThirst = Mathf.Min(currentThirst + quantidade, maxThirst);
            Debug.Log($"[Survival] Bebeste! Sede atual: {currentThirst}");
        }
    }

    public void AvancarTempo(float horas) { currentTimeInGameHours += horas; }

    void UpdateUI()
    {
        healthBar.value = currentHealth / maxHealth;
        hungerBar.value = currentHunger / maxHunger;
        thirstBar.value = currentThirst / maxThirst;
    }

    private void GanharJogo()
    {
        isGameFinished = true;
        PlayerPrefs.DeleteKey("CenaGuardada"); 
        PlayerPrefs.Save();
        SceneFader.instance.FazerFadeEIrParaCena(cenaVitoria);
    }

    private void PerderJogo()
    {
        isGameFinished = true;
        PlayerPrefs.DeleteKey("CenaGuardada");
        PlayerPrefs.Save();
        SceneFader.instance.FazerFadeEIrParaCena(cenaDerrota);
    }
}*/