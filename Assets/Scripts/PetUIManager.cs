using UnityEngine;
using TMPro;

public class PetUIManager : MonoBehaviour
{
    [Header("Configurações da UI")]
    public GameObject painelListaPets;
    public TextMeshProUGUI textoListaPets;

    // Usamos um temporizador para não pesquisar os animais todos os frames (Evita Lag)
    private float temporizadorAtualizacao = 0f;
    private float intervaloAtualizacao = 0.5f; 

    void Start()
    {
        // Garante que o painel começa escondido
        if (painelListaPets != null) painelListaPets.SetActive(false);
    }

    void Update()
    {
        temporizadorAtualizacao -= Time.deltaTime;

        if (temporizadorAtualizacao <= 0f)
        {
            AtualizarEcraDePets();
            temporizadorAtualizacao = intervaloAtualizacao; // Reinicia o relógio
        }
    }

    void AtualizarEcraDePets()
    {
        // Procura todos os animais no mapa
        AnimalAI[] todosAnimais = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None);
        
        string textoMontado = "<color=#FFD700><b>My Pets</b></color>\n"; // Título a amarelo
        int petsVivos = 0;

        foreach (AnimalAI animal in todosAnimais)
        {
            // Se for um pet, estiver domesticado e estiver vivo...
            if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado && animal.vida > 0)
            {
                string nomeDoPet = string.IsNullOrEmpty(animal.nomeDoPet) ? "Loyal Companion" : animal.nomeDoPet;
                int vidaArredondada = Mathf.CeilToInt(animal.vida);

                // Muda a cor da vida se estiver a morrer (menos de 30 HP)
                string corDaVida = vidaArredondada > 30 ? "<color=#00FF00>" : "<color=#FF0000>";

                textoMontado += $"- {nomeDoPet} | HP: {corDaVida}{vidaArredondada}</color>\n";
                petsVivos++;
            }
        }

        // Só mostramos o painel se tivermos pelo menos 1 pet vivo!
        if (petsVivos > 0)
        {
            painelListaPets.SetActive(true);
            textoListaPets.text = textoMontado;
        }
        else
        {
            painelListaPets.SetActive(false);
        }
    }
}