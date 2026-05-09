using UnityEngine;
using UnityEngine.UI;

public class MenuOpcoes : MonoBehaviour
{
    [Header("Controlos da Interface")]
    public Slider sliderVolume;
    public Toggle toggleEcraInteiro;

    void Start()
    {
        // Quando o menu abre, carregamos as definições guardadas
        if (PlayerPrefs.HasKey("VolumeGuardado"))
        {
            float volumeSalvo = PlayerPrefs.GetFloat("VolumeGuardado");
            sliderVolume.value = volumeSalvo;
            AudioListener.volume = volumeSalvo; // Aplica o som
        }

        if (PlayerPrefs.HasKey("EcraInteiroGuardado"))
        {
            // O PlayerPrefs não guarda "verdadeiro/falso", por isso guardamos como 1 ou 0
            bool ecraInteiroSalvo = PlayerPrefs.GetInt("EcraInteiroGuardado") == 1;
            toggleEcraInteiro.isOn = ecraInteiroSalvo;
            Screen.fullScreen = ecraInteiroSalvo; // Aplica o ecrã inteiro
        }
    }

    // Função para alterar o volume (recebe um número do Slider)
    public void AlterarVolume(float valorVolume)
    {
        AudioListener.volume = valorVolume; // Muda o som global do jogo
        PlayerPrefs.SetFloat("VolumeGuardado", valorVolume); // Guarda no PC
    }

    // Função para alterar o ecrã inteiro (recebe verdadeiro/falso do Toggle)
    public void AlterarEcraInteiro(bool eEcraInteiro)
    {
        Screen.fullScreen = eEcraInteiro; // Liga/Desliga ecrã inteiro
        PlayerPrefs.SetInt("EcraInteiroGuardado", eEcraInteiro ? 1 : 0); // Guarda no PC
    }
}