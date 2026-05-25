using UnityEngine;

[RequireComponent(typeof(AudioSource))] // NOVO: Garante que a fogueira tem emissor de som
public class Fogueira : MonoBehaviour
{
    [Header("Configurações do Fogo")]
    public GameObject efeitoFogoPrefab; // O prefab que tem a partícula de fogo e luz
    public float tempoAcesaMax = 60f;
    
    private bool estaAcesa = false;
    private float tempoRestante = 0f;

    [Header("Configurações de UI")]
    public FogueiraUI painelFogueiraUI; // O teu FireplacePanel

    // NOVO: Variável para o som
    private AudioSource somFogueira;

    void Start()
    {
        // ==============================================================
        // NOVO: CONFIGURAÇÃO DE ÁUDIO 3D DA FOGUEIRA
        // ==============================================================
        somFogueira = GetComponent<AudioSource>();
        somFogueira.spatialBlend = 1.0f; // 1 = Totalmente 3D
        somFogueira.maxDistance = 15f;   // Deixa de ouvir a 15 metros
        somFogueira.rolloffMode = AudioRolloffMode.Linear;
        somFogueira.loop = true; // O fogo crepita infinitamente enquanto está aceso
        // ==============================================================

        // Se a variável estiver vazia, faz uma busca inteligente pelos filhos do Canvas
        if (painelFogueiraUI == null)
        {
            // 1. Encontra o Canvas da cena (que está ativo)
            Canvas canvasGeral = Object.FindFirstObjectByType<Canvas>();
            
            if (canvasGeral != null)
            {
                // 2. Procura dentro dele (incluindo objetos desativados) pelo componente FogueiraUI
                painelFogueiraUI = canvasGeral.GetComponentInChildren<FogueiraUI>(true);
            }
        }

        // Verificação de segurança para sabermos se correu bem
        if (painelFogueiraUI == null)
        {
            Debug.LogError("Erro Crítico: Não foi possível encontrar o componente FogueiraUI dentro do Canvas!");
        }

        if (efeitoFogoPrefab != null) efeitoFogoPrefab.SetActive(false);
        
        // Garante que começa fechado de forma segura
        if (painelFogueiraUI != null) painelFogueiraUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (estaAcesa)
        {
            tempoRestante -= Time.deltaTime;
            
            // Atualiza o texto visual da UI enquanto o tempo corre!
            if (painelFogueiraUI != null && painelFogueiraUI.gameObject.activeSelf)
            {
                // Aqui chamamos o método que atualiza os textos e o tempo da UI
                painelFogueiraUI.AtualizarEstadoDaUI(); 
            }

            // Atualiza o valor dinâmico para o texto do temporizador na UI
            if (tempoRestante <= 0)
            {
                ApagarFogueira();
            }
        }
    }

    public void TentarAcender(int quantidadeMadeira)
    {
        // Se já estiver acesa, podes querer apenas acumular tempo (combustível)
        if (estaAcesa)
        {
            tempoRestante = Mathf.Clamp(tempoRestante + 20f, 0f, tempoAcesaMax);
            return;
        }

        if (quantidadeMadeira >= 3)
        {
            estaAcesa = true;
            tempoRestante = tempoAcesaMax;
            
            // LIGA O OBJETO DO FOGO NA CENA
            if (efeitoFogoPrefab != null) 
            {
                efeitoFogoPrefab.SetActive(true);
            }

            // NOVO: LIGA O SOM DA FOGUEIRA
            if (!somFogueira.isPlaying) somFogueira.Play();
            
            Debug.Log("A fogueira foi acesa!");
        }
    }

    public void ApagarFogueira()
    {
        estaAcesa = false;
        tempoRestante = 0f;
        if (efeitoFogoPrefab != null) efeitoFogoPrefab.SetActive(false);
        
        // NOVO: DESLIGA O SOM DA FOGUEIRA
        if (somFogueira.isPlaying) somFogueira.Stop();
        
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
        if (painelFogueiraUI != null)
        {
            painelFogueiraUI.gameObject.SetActive(false);

            // 2. CORREÇÃO DE FOCO: Força o Unity a prender o rato de forma limpa
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}