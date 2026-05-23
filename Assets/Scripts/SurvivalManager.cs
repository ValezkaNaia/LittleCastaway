using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; 

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

    [Header("Taxas (Fome/Sede/Cura)")]
    public float hungerDepletionRate = 0.05f; 
    public float thirstDepletionRate = 0.08f; 
    public float starvationDamageRate = 1.0f; 
    public float healthRegenRate = 0.3f;

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
    public GameObject painelEcraPreto; 
    public TextMeshProUGUI textoSonoUI;
    public float horasParaDesmaiar = 36f; 
    public float danoPorDesmaio = 20f;

    [Header("Sistema de Afogamento")]
    [Tooltip("Abaixo de que altura (Y) a CABEÇA (Câmara) tem de estar para afogar?")]
    public float alturaDoMar = 2f; 
    public float tempoSeguroNaAgua = 10f; 
    public float danoPorGole = 20f;   
    public float intervaloEntreDanos = 2f; 
    
    // Contadores invisíveis de água
    private float tempoNaAgua = 0f;
    private float cronometroDano = 0f;

    [Header("Efeitos Visuais e Câmara")]
    public Image imagemDanoTela; 
    public Image visaoVerdeImage; 
    public Image visaoSonoImage; 
    public Camera mainCamera; 

    private float currentHealth;
    private float currentHunger;
    private float currentThirst;
    private float currentStamina;
    private float currentFatigue = 0f;
    public float currentTimeInGameHours; 
    private int currentDay = 1;
    private bool isGameFinished = false; 
    private bool estaADormir = false; 
    private float lastHealth, lastHunger, lastThirst;

    private float baseFOV;
    private float shakeIntensity = 0f;
    private float shakeDecay = 0f;
    private float tempoUltimoDanoFome;

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
        
        SetImageAlpha(imagemDanoTela, 0f);
        SetImageAlpha(visaoVerdeImage, 0f);
        SetImageAlpha(visaoSonoImage, 0f);

        if (mainCamera != null)
        {
            baseFOV = mainCamera.fieldOfView;
        }

        ForçarAtualizacaoUI();
    }

    void Update()
    {
        if (isGameFinished || estaADormir || NoteManager.isReading) return;

        HandleTime();
        HandleStats();
        HandleStamina();
        HandleFatigue(); 
        HandleDrowning(); // Adicionámos o afogamento aqui!
        HandleFOVEffects();
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    // =================================================================
    // SISTEMA DE DANO E CURA
    // =================================================================
    private void DeduzirVida(float quantidade, bool usarEfeitosVisuais, float forcaEfeito = 1.0f)
    {
        if (currentHealth <= 0 || isGameFinished) return;

        currentHealth -= quantidade;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        ForçarAtualizacaoUI();

        if (usarEfeitosVisuais)
        {
            AplicarEfeitoDeDano(0.2f, forcaEfeito);
        }

        if (currentHealth <= 0) PerderJogo();
    }

    public void ReceberDano(float quantidade)
    {
        DeduzirVida(quantidade, true, 1.0f);
    }

    // =================================================================
    // AFOGAMENTO (Controlado pela Cabeça / Câmara)
    // =================================================================
    void HandleDrowning()
    {
        if (mainCamera == null) return;

        // Se a CÂMARA descer abaixo do nível do mar, começa a sufocar
        if (mainCamera.transform.position.y <= alturaDoMar)
        {
            tempoNaAgua += Time.deltaTime;

            if (tempoNaAgua >= tempoSeguroNaAgua)
            {
                cronometroDano += Time.deltaTime;

                if (cronometroDano >= intervaloEntreDanos)
                {
                    // Tira vida, abana a câmara forte e pisca o sangue!
                    DeduzirVida(danoPorGole, true, 1.2f);
                    cronometroDano = 0f;
                }
            }
        }
        else
        {
            // Tirou a cabeça da água, respira fundo!
            tempoNaAgua = 0f;
            cronometroDano = 0f;
        }
    }

    // =================================================================
    // RESTANTES MÉTODOS
    // =================================================================
    void HandleStats()
    {
        if (currentHunger > 0) currentHunger -= hungerDepletionRate * Time.deltaTime;
        if (currentThirst > 0) currentThirst -= thirstDepletionRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

        float targetGreenAlpha = 0f;

        if (currentHunger <= 0 || currentThirst <= 0)
        {
            float danoFomeSede = starvationDamageRate * Time.deltaTime;
            DeduzirVida(danoFomeSede, false); 

            if (Time.time >= tempoUltimoDanoFome + 1.0f)
            {
                if (imagemDanoTela != null) StartCoroutine(PiscarSangue(0.3f)); 
                tempoUltimoDanoFome = Time.time;
            }
        }
        else if (currentHunger >= (maxHunger * 0.9f) && currentThirst >= (maxThirst * 0.9f))
        {
            if (currentHealth < maxHealth)
            {
                currentHealth += healthRegenRate * Time.deltaTime;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                targetGreenAlpha = 0.6f;
            }
        }

        if (visaoVerdeImage != null)
        {
            Color currentGreenColor = visaoVerdeImage.color;
            currentGreenColor.a = Mathf.MoveTowards(currentGreenColor.a, targetGreenAlpha, Time.deltaTime * 1.0f); 
            visaoVerdeImage.color = currentGreenColor;
        }

        VerificarEAtualizarUI();
    }

    void HandleFOVEffects()
    {
        if (mainCamera == null) return;
        float fovOffset = 0f;
        if (currentFatigue >= 50f)
        {
            float intensidadeSono = (currentFatigue - 50f) / 50f;
            fovOffset += Mathf.Sin(Time.time * 2f) * 3f * intensidadeSono;
        }
        if (shakeIntensity > 0)
        {
            fovOffset -= shakeIntensity * 12f; 
            shakeIntensity -= shakeDecay * Time.deltaTime;
            if (shakeIntensity < 0f) shakeIntensity = 0f;
        }
        mainCamera.fieldOfView = baseFOV + fovOffset;
    }

    public void AplicarEfeitoDeDano(float duracao, float forca)
    {
        shakeIntensity = forca;
        shakeDecay = forca / duracao;
        if (imagemDanoTela != null) StartCoroutine(PiscarSangue(0.6f)); 
    }

    System.Collections.IEnumerator PiscarSangue(float alphaMaximo)
    {
        Color corSangue = imagemDanoTela.color;
        corSangue.a = alphaMaximo; 
        imagemDanoTela.color = corSangue;

        while (imagemDanoTela.color.a > 0)
        {
            corSangue.a -= Time.deltaTime * 1.5f; 
            imagemDanoTela.color = corSangue;
            yield return null;
        }
    }

    void HandleTime()
    {
        float inGameHoursPerRealSecond = 24f / dayDurationInRealSeconds;
        AdicionarHorasLogicas(Time.deltaTime * inGameHoursPerRealSecond);
    }

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
            float progressoCansaco = (currentFatigue - 70f) / 30f;
            SetImageAlpha(visaoSonoImage, progressoCansaco * 0.90f);
        }
        else SetImageAlpha(visaoSonoImage, 0f);
        
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
        if (foiForçado) DeduzirVida(danoPorDesmaio, true, 0.5f); 
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
        SetImageAlpha(visaoSonoImage, 0f);
        if (mainCamera != null) mainCamera.fieldOfView = baseFOV;
    }

    public void DrinkWater(float amount) { currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst); ForçarAtualizacaoUI(); }
    public void EatFood(float amount) { currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger); ForçarAtualizacaoUI(); }

    public void ReceberNutricao(float quantidade, ItemData.TipoConsumivel tipo)
    {
        if (tipo == ItemData.TipoConsumivel.Comida) currentHunger = Mathf.Min(currentHunger + quantidade, maxHunger);
        else if (tipo == ItemData.TipoConsumivel.Agua) currentThirst = Mathf.Min(currentThirst + quantidade, maxThirst);
        ForçarAtualizacaoUI();
    }

    public void AvancarTempo(float horas) { AdicionarHorasLogicas(horas); }

    void VerificarEAtualizarUI()
    {
        if (currentHealth != lastHealth || currentHunger != lastHunger || currentThirst != lastThirst) ForçarAtualizacaoUI();
    }

    public void ForçarAtualizacaoUI()
    {
        if (healthBar != null) healthBar.value = currentHealth / maxHealth;
        if (hungerBar != null) hungerBar.value = currentHunger / maxHunger;
        if (thirstBar != null) thirstBar.value = currentThirst / maxThirst;
        lastHealth = currentHealth; lastHunger = currentHunger; lastThirst = currentThirst;
    }

    private void GanharJogo()
    {
        isGameFinished = true; PlayerPrefs.DeleteKey("CenaGuardada"); PlayerPrefs.Save();
        if (SceneFader.instance != null) SceneFader.instance.FazerFadeEIrParaCena(cenaVitoria);
        else SceneManager.LoadScene(cenaVitoria); 
    }

    private void PerderJogo()
    {
        isGameFinished = true; PlayerPrefs.DeleteKey("CenaGuardada"); PlayerPrefs.Save();
        if (SceneFader.instance != null) SceneFader.instance.FazerFadeEIrParaCena(cenaDerrota);
        else SceneManager.LoadScene(cenaDerrota); 
    }
}