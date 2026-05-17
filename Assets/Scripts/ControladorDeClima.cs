using UnityEngine;
using DynamicWeatherSystem; 

public class ControladorDeClima : MonoBehaviour
{
    [Header("O Sistema Deles")]
    // Referência explícita ao script do asset
    public DynamicWeatherSystem.WeatherManager weatherSystemMaster; 

    [Header("Os Presets (Arrasta das pastas)")]
    public WeatherStateData presetClear;
    public WeatherStateData presetRain;
    public WeatherStateData presetFog;
    public WeatherStateData presetStorm;

    [Header("Tempos (Segundos)")]
    public float tempoMinimo = 200f; 
    public float tempoMaximo = 500f; 

    private float timer;

    void Start()
    {
        MudarClima(presetClear);
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            EscolherClimaAleatorio();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(tempoMinimo, tempoMaximo);
    }

    void EscolherClimaAleatorio()
    {
        int sorteio = Random.Range(0, 4); 

        switch (sorteio)
        {
            case 0: MudarClima(presetClear); break;
            case 1: MudarClima(presetRain); break;
            case 2: MudarClima(presetFog); break;
            case 3: MudarClima(presetStorm); break;
        }
    }

    void MudarClima(WeatherStateData novoPreset)
    {
        if (weatherSystemMaster != null && novoPreset != null)
        {
            // AQUI ESTÁ A PALAVRA MÁGICA CORRETA: SetWeather!
            weatherSystemMaster.SetWeather(novoPreset, 5f); 
            Debug.Log("Clima mudou para: " + novoPreset.name);
        }
    }
}