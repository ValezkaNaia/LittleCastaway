using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(NavMeshAgent))]
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
    private bool provocado = false;
    private AnimalAI alvoDoPet = null; 

    [Header("Drop de Itens")]
    public GameObject prefabCarneDoChao; 

    [Header("Anti-Stuck System")]
    public float timeToConsiderStuck = 3f; 
    private float tempoEncravado = 0f;
    private float tempoADesencravar = 0f; // Tempo que ele passa a caminhar para longe do obstáculo

    private NavMeshAgent agente;
    private Transform player;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        InvokeRepeating("AndarAleatoriamente", Random.Range(0f, 2f), 5f);
    }

    void Update()
    {
        if (vida <= 0 || player == null || !agente.isOnNavMesh) return;

        // --- SISTEMA ANTI-STUCK (SEM TELETRANSPORTES) ---
        if (agente.hasPath && agente.remainingDistance > 0.5f)
        {
            // Usa a velocidade real do Unity para saber se está a andar ou preso
            if (agente.velocity.magnitude < 0.1f) 
            {
                tempoEncravado += Time.deltaTime;
                if (tempoEncravado >= timeToConsiderStuck)
                {
                    Desencravar(); 
                }
            }
            else
            {
                tempoEncravado = 0f; 
            }
        }
        else
        {
            tempoEncravado = 0f;
        }
        
        // Se o animal estiver a tentar desentalar-se, ele ignora a IA normal por uns segundos
        if (tempoADesencravar > 0)
        {
            tempoADesencravar -= Time.deltaTime;
            return; 
        }
        // ------------------------------------------------

        float distanciaProPlayer = Vector3.Distance(transform.position, player.position);

        if (tipoAnimal == Comportamento.Pet && !domesticado && !provocado)
        {
            if (distanciaProPlayer <= 3f && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                TentarDomesticar();
            }
        }

        if (domesticado)
        {
            if (alvoDoPet != null && alvoDoPet.vida > 0)
            {
                float distParaAlvo = Vector3.Distance(transform.position, alvoDoPet.transform.position);
                if (agente.isOnNavMesh) agente.SetDestination(alvoDoPet.transform.position);

                if (distParaAlvo <= 2f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques)
                {
                    AtacarOutroAnimal(alvoDoPet);
                }
            }
            else
            {
                alvoDoPet = null; 
                if (distanciaProPlayer > 4f) 
                {
                    if (agente.isOnNavMesh) agente.SetDestination(player.position);
                }
            }
            return;
        }

        if (tipoAnimal == Comportamento.Presa)
        {
            if (distanciaProPlayer < distanciaDetecao) FogirDoPlayer();
        }
        
        else if (tipoAnimal == Comportamento.Predador)
        {
            Transform alvoAtual = null;
            float menorDistancia = distanciaDetecao;

            if (distanciaProPlayer < menorDistancia)
            {
                alvoAtual = player;
                menorDistancia = distanciaProPlayer;
            }

            AnimalAI[] todosAnimais = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None);
            
            foreach (AnimalAI outroAnimal in todosAnimais)
            {
                if (outroAnimal != this && outroAnimal.vida > 0 && 
                    (outroAnimal.tipoAnimal == Comportamento.Presa || (outroAnimal.tipoAnimal == Comportamento.Pet && !outroAnimal.domesticado)))
                {
                    float distParaAnimal = Vector3.Distance(transform.position, outroAnimal.transform.position);
                    if (distParaAnimal < menorDistancia)
                    {
                        alvoAtual = outroAnimal.transform;
                        menorDistancia = distParaAnimal;
                    }
                }
            }

            if (alvoAtual != null)
            {
                if (agente.isOnNavMesh) agente.SetDestination(alvoAtual.position);
                
                if (menorDistancia <= 2f) 
                {
                    if (Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques)
                    {
                        if (alvoAtual == player) AtacarJogador();
                        else AtacarOutroAnimal(alvoAtual.GetComponent<AnimalAI>());
                    }
                }
            }
        }
        
        else if (tipoAnimal == Comportamento.Pet && provocado)
        {
            if (distanciaProPlayer < distanciaDetecao)
            {
                if (agente.isOnNavMesh) agente.SetDestination(player.position);
                if (distanciaProPlayer <= 2f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques)
                {
                    AtacarJogador();
                }
            }
        }
    }

    void Desencravar()
    {
        agente.ResetPath(); // Para de tentar empurrar a parede
        
        // Escolhe uma direção aleatória em redor para se afastar
        Vector3 randomDirection = Random.insideUnitSphere * 4f;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 4f, 1))
        {
            agente.SetDestination(hit.position); // Camina normalmente para a nova direção
        }
        
        tempoEncravado = 0f; 
        tempoADesencravar = 2f; // Dá-lhe 2 segundos para ele caminhar à vontade e desentalar-se sem o script o mandar voltar atrás
    }

    void AtacarJogador()
    {
        tempoDoUltimoAtaque = Time.time; 
        if (SurvivalManager.instance != null) SurvivalManager.instance.ReceberDano(danoAtaque);
        PedirAjudaAosPets(this);
    }

    void AtacarOutroAnimal(AnimalAI vitima)
    {
        if (vitima == null) return;
        tempoDoUltimoAtaque = Time.time;
        bool foiPredadorSelvagem = (tipoAnimal == Comportamento.Predador);
        vitima.ReceberDano(danoAtaque, foiPredadorSelvagem); 
    }

    private void PedirAjudaAosPets(AnimalAI alvoAAtacar)
    {
        AnimalAI[] todosAnimais = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None);
        
        foreach (AnimalAI animal in todosAnimais)
        {
            if (animal.tipoAnimal == Comportamento.Pet && animal.domesticado && animal.vida > 0)
            {
                animal.alvoDoPet = alvoAAtacar; 
            }
        }
    }

    void AndarAleatoriamente()
    {
        if (!agente.isOnNavMesh || domesticado || tempoADesencravar > 0) return;
        if (tipoAnimal == Comportamento.Predador && Vector3.Distance(transform.position, player.position) < distanciaDetecao) return;

        Vector3 direcaoAleatoria = Random.insideUnitSphere * 10f;
        direcaoAleatoria += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(direcaoAleatoria, out hit, 10f, 1))
        {
            agente.SetDestination(hit.position);
        }
    }

    void FogirDoPlayer()
    {
        if (!agente.isOnNavMesh || tempoADesencravar > 0) return;
        Vector3 direcaoFuga = transform.position - player.position;
        Vector3 novaPosicao = transform.position + direcaoFuga.normalized * 10f;
        agente.SetDestination(novaPosicao);
    }

    public void ReceberDano(float dano, bool porPredador = false)
    {
        vida -= dano;
        if (tipoAnimal == Comportamento.Pet && !domesticado) provocado = true; 
        if (!porPredador && tipoAnimal != Comportamento.Pet) PedirAjudaAosPets(this);
        if (vida <= 0) Morrer(porPredador);
    }

    void Morrer(bool porPredador)
    {
        if (darCarneAoMorrer && tipoAnimal != Comportamento.Pet && !porPredador)
        {
            if (prefabCarneDoChao != null) Instantiate(prefabCarneDoChao, transform.position + Vector3.up, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    public void TentarDomesticar()
    {
        if (tipoAnimal != Comportamento.Pet || domesticado || provocado) return;
        macasDadas++;
        if (macasDadas >= macasParaDomesticar) 
        {
            domesticado = true;
        }
    }
}