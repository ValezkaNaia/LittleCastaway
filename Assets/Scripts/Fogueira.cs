using UnityEngine;

public class Fogueira : MonoBehaviour
{
    [Header("Configurações do Fogo")]
    public GameObject efeitoFogoPrefab; // O prefab que tem a partícula de fogo e luz
    public float tempoAcesaMax = 60f;
    
    private bool estaAcesa = false;
    private float tempoRestante = 0f;

    [Header("Configurações de UI")]
    public FogueiraUI painelFogueiraUI; // O teu FireplacePanel

    void Start()
    {
        // Encontra automaticamente o script do painel que está no Canvas da cena
        if (painelFogueiraUI == null)
        {
            painelFogueiraUI = Object.FindFirstObjectByType<FogueiraUI>();
        }

        if (efeitoFogoPrefab != null) efeitoFogoPrefab.SetActive(false);
        
        // Desativa o painel no início do jogo de forma segura
        if (painelFogueiraUI != null) painelFogueiraUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (estaAcesa)
        {
            tempoRestante -= Time.deltaTime;
            
            // Atualiza o valor dinâmico para o texto do temporizador na UI
            if (tempoRestante <= 0)
            {
                ApagarFogueira();
            }
        }
    }

    public void TentarAcender(int quantidadeMadeira)
    {
        if (estaAcesa) return;

        if (quantidadeMadeira >= 3)
        {
            estaAcesa = true;
            tempoRestante = tempoAcesaMax;
            if (efeitoFogoPrefab != null) efeitoFogoPrefab.SetActive(true);
            Debug.Log("A fogueira foi acesa!");
        }
    }

    public void ApagarFogueira()
    {
        estaAcesa = false;
        tempoRestante = 0f;
        if (efeitoFogoPrefab != null) efeitoFogoPrefab.SetActive(false);
        
        // Se a fogueira apagar com a UI aberta, atualiza os botões instantaneamente
        FogueiraUI uiControlador = Object.FindFirstObjectByType<FogueiraUI>();
        if (uiControlador != null) uiControlador.AtualizarEstadoDaUI();
        
        Debug.Log("A fogueira apagou-se.");
    }

    public bool GetEstaAcesa() => estaAcesa;
    public float GetTempoRestante() => tempoRestante;

    // Função que vais chamar no teu script de Interação (PlayerInteraction) quando o jogador carregar no [E]
    public void AbrirInterfaceFogueira()
    {
        if (painelFogueiraUI != null)
        {
            painelFogueiraUI.gameObject.SetActive(true);
            
            // Avisa o painel de UI qual é a fogueira específica que foi aberta
            painelFogueiraUI.InicializarInterface(this);
        }
    }

    public void FecharInterfaceFogueira()
    {
        if (painelFogueiraUI != null) painelFogueiraUI.gameObject.SetActive(false);
    }
}