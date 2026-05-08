using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para gerir cenas

public class MenuPrincipal : MonoBehaviour
{
    // Esta é a função que o botão vai chamar
    public void Jogar()
    {
        // Substitui "NomeDaTuaCenaDeJogo" pelo nome EXATO da cena do teu jogo
        SceneManager.LoadScene("SampleScene");
    }
}