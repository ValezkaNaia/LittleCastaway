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

        somFogueira = GetComponent<AudioSource>();
        somFogueira.spatialBlend = 1.0f; 
        somFogueira.maxDistance = 15f;   
        somFogueira.rolloffMode = AudioRolloffMode.Linear;
        somFogueira.loop = true; 

        // ==============================================================
        // CORREÇÃO: Vamos buscar a referência diretamente ao Singleton!
        // ==============================================================
        if (FogueiraUI.instance != null)
        {
            painelFogueiraUI = FogueiraUI.instance;
        }
        else
        {
            // Se ainda for nulo (ex: ordem de carregamento), não dês erro já,
            // porque tentaremos novamente ao interagir!
            Debug.LogWarning("[Fogueira] FogueiraUI ainda não se registou, vai ligar-se na interação.");
        }

        if (efeitoFogoPrefab != null) efeitoFogoPrefab.SetActive(false);
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
        // SEGUNDA CHANCE: Se por algum motivo falhou no Start, garante a ligação aqui
        if (painelFogueiraUI == null && FogueiraUI.instance != null)
        {
            painelFogueiraUI = FogueiraUI.instance;
        }

        if (painelFogueiraUI != null)
        {
            // Ativa o objeto antes de chamar qualquer lógica interna
            painelFogueiraUI.gameObject.SetActive(true);
            
            // Avisa o painel de UI qual é a fogueira específica que foi aberta
            painelFogueiraUI.InicializarInterface(this);
        }
        else
        {
            Debug.LogError("Erro Crítico: Não foi possível interagir porque FogueiraUI.instance não existe na cena!");
        }
    }

    public void FecharInterfaceFogueira()
    {
        // Garante que usamos a referência certa para fechar
        var uiParaFechar = painelFogueiraUI != null ? painelFogueiraUI : FogueiraUI.instance;

        if (uiParaFechar != null)
        {
            uiParaFechar.gameObject.SetActive(false);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}