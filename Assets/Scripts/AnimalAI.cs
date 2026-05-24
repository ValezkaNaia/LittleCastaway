using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AnimalAI : MonoBehaviour
{
    public enum Comportamento { Presa, Predador, Pet }
    
    [Header("Configurações Principais")]
    public Comportamento tipoAnimal;
    public float vida = 100f;
    public float distanciaDetecao = 15f;
    public bool darCarneAoMorrer = true;

    [Header("Combate (Ataque)")]
    public float danoAtaque = 15f; 
    public float tempoEntreAtaques = 2f; 
    private float tempoDoUltimoAtaque = 0f;

    [Header("Pets (Cão e Gato)")]
    public int macasDadas = 0;
    private int macasParaDomesticar = 5;
    public bool domesticado = false;
    public bool provocado = false; 
    private AnimalAI alvoDoPet = null; 

    [Header("Drop de Itens")]
    public GameObject prefabCarneDoChao; 

    // =================================================================
    // NOVAS VARIÁVEIS PARA PARTÍCULAS
    // =================================================================
    [Header("Efeitos Visuais - Geral")]
    [Tooltip("Arrasta o teu Prefab da partícula configurada com 'Stop Action = Destroy'")]
    public GameObject particulaMortePrefab;
    [Tooltip("Altura onde a partícula de MORTE vai aparecer")]
    public float offsetAlturaParticulaMorte = 1.0f; 

    [Header("Efeitos Visuais - Pets (SOMENTE PETS)")]
    [Tooltip("Prefab das partículas de coração ao domesticar")]
    public GameObject particulaDomesticaoPrefab; // TEU PREFAB DE CORAÇÕES AQUI
    [Tooltip("Offset de altura para as partículas de coração")]
    public float offsetAlturaCorações = 1.0f; 
    // =================================================================

    [Header("Anti-Stuck System")]
    public float timeToConsiderStuck = 3f; 
    private float tempoEncravado = 0f;
    private float tempoADesencravar = 0f; 

    private NavMeshAgent agente;
    private Animator anim;
    private Transform player;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        ColarAoNavMesh();
        InvokeRepeating("AndarAleatoriamente", Random.Range(0f, 2f), 5f);
    }

    void Update()
    {
        if (vida <= 0 || player == null) return;

        // --- ANIMAÇÃO (Substitui o CreatureMover) ---
        if (anim != null)
        {
            float vel = agente.velocity.magnitude;
            anim.SetFloat("Vert", vel); 
            anim.SetFloat("State", vel > 0.1f ? 1f : 0f);
        }

        // --- SISTEMA DE NAVEGAÇÃO ---
        if (!agente.isOnNavMesh)
        {
            ColarAoNavMesh();
            return; 
        }

        if (tempoADesencravar > 0)
        {
            tempoADesencravar -= Time.deltaTime;
            return; 
        }

        // --- LÓGICA DE PET ---
        if (tipoAnimal == Comportamento.Pet)
        {
            if (domesticado)
            {
                if (alvoDoPet != null && alvoDoPet.vida > 0)
                {
                    agente.SetDestination(alvoDoPet.transform.position);
                    if (Vector3.Distance(transform.position, alvoDoPet.transform.position) <= 2f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques)
                    {
                        AtacarOutroAnimal(alvoDoPet);
                    }
                }
                else
                {
                    alvoDoPet = null;
                    if (Vector3.Distance(transform.position, player.position) > 4f) agente.SetDestination(player.position);
                    else agente.ResetPath();
                }
                return;
            }
        }

        // --- LÓGICA DE PRESA / PREDADOR ---
        float distanciaProPlayer = Vector3.Distance(transform.position, player.position);

        if (tipoAnimal == Comportamento.Presa)
        {
            if (distanciaProPlayer < distanciaDetecao) FogirDoPlayer();
        }
        else if (tipoAnimal == Comportamento.Predador)
        {
            Transform alvoAtual = null;
            float menorDistancia = distanciaDetecao;

            if (distanciaProPlayer < menorDistancia) { alvoAtual = player; menorDistancia = distanciaProPlayer; }

            AnimalAI[] todosAnimais = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None);
            foreach (AnimalAI outroAnimal in todosAnimais)
            {
                if (outroAnimal != this && outroAnimal.vida > 0 && (outroAnimal.tipoAnimal == Comportamento.Presa || (outroAnimal.tipoAnimal == Comportamento.Pet && !outroAnimal.domesticado)))
                {
                    float dist = Vector3.Distance(transform.position, outroAnimal.transform.position);
                    if (dist < menorDistancia) { alvoAtual = outroAnimal.transform; menorDistancia = dist; }
                }
            }

            if (alvoAtual != null)
            {
                agente.SetDestination(alvoAtual.position);
                if (menorDistancia <= 2f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques)
                {
                    if (alvoAtual == player) AtacarJogador();
                    else AtacarOutroAnimal(alvoAtual.GetComponent<AnimalAI>());
                }
            }
        }
        else if (tipoAnimal == Comportamento.Pet && provocado)
        {
            if (distanciaProPlayer < distanciaDetecao)
            {
                agente.SetDestination(player.position);
                if (distanciaProPlayer <= 2f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques) AtacarJogador();
            }
        }
    }

    // --- MÉTODOS DE CONTROLO ---
    public void TentarDomesticar()
    {
        if (tipoAnimal != Comportamento.Pet || domesticado || provocado) return;
        macasDadas++;

        // =================================================================
        // LÓGICA DE SUCESSO NA DOMESTICAÇÃO (Corações)
        // =================================================================
        if (macasDadas >= macasParaDomesticar)
        {
            domesticado = true;
            Debug.Log(gameObject.name + " foi domesticado! <3");

            // Cria as partículas de coração
            if (particulaDomesticaoPrefab != null)
            {
                Vector3 posicaoSpawn = transform.position + new Vector3(0, offsetAlturaCorações, 0);
                // Quaternion.identity assume que as partículas voam para cima por defeito
                Instantiate(particulaDomesticaoPrefab, posicaoSpawn, Quaternion.identity);
            }
        }
    }

    void ColarAoNavMesh() { NavMeshHit hit; if (NavMesh.SamplePosition(transform.position, out hit, 20.0f, NavMesh.AllAreas)) { transform.position = hit.position; agente.Warp(hit.position); } }
    void Desencravar() { agente.ResetPath(); Vector3 randomDirection = Random.insideUnitSphere * 4f + transform.position; NavMeshHit hit; if (NavMesh.SamplePosition(randomDirection, out hit, 4f, 1)) agente.SetDestination(hit.position); tempoEncravado = 0f; tempoADesencravar = 2f; }
    void AtacarJogador() { tempoDoUltimoAtaque = Time.time; if (SurvivalManager.instance != null) SurvivalManager.instance.ReceberDano(danoAtaque); }
    void AtacarOutroAnimal(AnimalAI vitima) { if (vitima == null) return; tempoDoUltimoAtaque = Time.time; vitima.ReceberDano(danoAtaque, tipoAnimal == Comportamento.Predador); }
    void AndarAleatoriamente() { if (!agente.isOnNavMesh || domesticado || tempoADesencravar > 0) return; if (tipoAnimal == Comportamento.Predador && Vector3.Distance(transform.position, player.position) < distanciaDetecao) return; Vector3 dir = Random.insideUnitSphere * 10f + transform.position; NavMeshHit hit; if (NavMesh.SamplePosition(dir, out hit, 10f, 1)) agente.SetDestination(hit.position); }
    void FogirDoPlayer() { if (!agente.isOnNavMesh || tempoADesencravar > 0) return; agente.SetDestination(transform.position + (transform.position - player.position).normalized * 10f); }
    public void ReceberDano(float dano, bool porPredador = false) { vida -= dano; if (tipoAnimal == Comportamento.Pet && !domesticado) provocado = true; if (vida <= 0) Morrer(porPredador); }
    
    void Morrer(bool porPredador) 
    { 
        // Cria a partícula de morte
        if (particulaMortePrefab != null)
        {
            Vector3 posicaoSpawn = transform.position + new Vector3(0, offsetAlturaParticulaMorte, 0);
            Instantiate(particulaMortePrefab, posicaoSpawn, Quaternion.identity);
        }

        // Deixa cair carne se aplicável
        if (darCarneAoMorrer && tipoAnimal != Comportamento.Pet && !porPredador && prefabCarneDoChao != null) 
        {
            Instantiate(prefabCarneDoChao, transform.position + Vector3.up, Quaternion.identity); 
        }

        // Destrói o animal
        Destroy(gameObject); 
    }
}