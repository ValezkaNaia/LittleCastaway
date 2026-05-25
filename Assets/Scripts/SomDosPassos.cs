using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SomDosPassos : MonoBehaviour
{
    [Header("Configurações do Jogador")]
    public CharacterController controlador; 
    public float velocidadeParaCorrer = 5f; 

    [Header("Tempo entre Passos")]
    public float intervaloCaminhar = 0.5f;
    public float intervaloCorrer = 0.3f;
    private float temporizadorPasso;

    [Header("Efeitos Sonoros (Sons de Terra/Ilha)")]
    public AudioClip caminharSom;
    public AudioClip correrSom;
    public AudioClip saltarSom;   
    public AudioClip aterrarSom;  

    private AudioSource emissorAudio;
    private bool estavaNoChao = true;
    private Vector3 posicaoAnterior;

    void Start()
    {
        emissorAudio = GetComponent<AudioSource>();
        
        // FORÇA O SOM A 2D: Garante que ouves os teus próprios passos sempre altos e em ambos os ouvidos
        emissorAudio.spatialBlend = 0f; 

        // Busca inteligente pelo CharacterController (mesmo que esteja em objetos filhos)
        if (controlador == null) controlador = GetComponent<CharacterController>();
        if (controlador == null) controlador = GetComponentInChildren<CharacterController>();
        
        if (controlador == null)
        {
            Debug.LogError("Erro Crítico: O script SomDosPassos não encontrou o CharacterController no Player! Arraste-o manualmente para a caixa 'Controlador' no Inspector.");
        }
        else
        {
            // Guarda a posição inicial exata da cápsula que se mexe
            posicaoAnterior = controlador.transform.position;
        }
    }

    void Update()
    {
        if (controlador == null) return;

        // 1. CÁLCULO DE VELOCIDADE REAL (Agora focado na cápsula que se mexe!)
        Vector3 posicaoAtual = controlador.transform.position;
        posicaoAtual.y = 0; // Ignora subidas e quedas para o ritmo dos passos
        
        Vector3 posAnteriorSemY = posicaoAnterior;
        posAnteriorSemY.y = 0;

        float distanciaMovida = Vector3.Distance(posicaoAtual, posAnteriorSemY);
        float velocidadeReal = distanciaMovida / Time.deltaTime;
        
        posicaoAnterior = controlador.transform.position; // Atualiza para o próximo frame

        // 2. DETEÇÃO DE CHÃO MULTI-TERRENO
        // Dispara um pequeno raio a partir da sola dos pés da CÁPSULA para baixo
        Vector3 pontoDosPes = controlador.transform.position + controlador.center + (Vector3.down * (controlador.height * 0.5f));
        bool noChao = controlador.isGrounded || Physics.Raycast(pontoDosPes + Vector3.up * 0.1f, Vector3.down, 0.4f, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore);

        // 3. SISTEMA DE SALTO E ATERRAGEM
        if (estavaNoChao && !noChao && controlador.velocity.y > 0)
        {
            TocarSom(saltarSom, 0.9f, 1.1f);
        }
        else if (!estavaNoChao && noChao)
        {
            TocarSom(aterrarSom, 0.85f, 1.0f); 
        }

        estavaNoChao = noChao;

        // 4. CONTROLO DOS PASSOS
        // Se não estiver no chão ou estiver praticamente parado, cancela
        if (!noChao || velocidadeReal < 0.2f)
        {
            temporizadorPasso = 0f;
            return;
        }

        bool estaACorrer = velocidadeReal > velocidadeParaCorrer;
        temporizadorPasso -= Time.deltaTime;

        if (temporizadorPasso <= 0f)
        {
            AudioClip somPassoAtual = estaACorrer ? correrSom : caminharSom;
            TocarSom(somPassoAtual, 0.9f, 1.1f);
            
            temporizadorPasso = estaACorrer ? intervaloCorrer : intervaloCaminhar;
        }
    }

    void TocarSom(AudioClip clip, float pitchMin, float pitchMax)
    {
        if (clip != null)
        {
            emissorAudio.pitch = Random.Range(pitchMin, pitchMax);
            emissorAudio.PlayOneShot(clip);
        }
    }
}