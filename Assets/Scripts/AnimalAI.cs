using UnityEngine;
using UnityEngine.AI;

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
    public float danoAtaque = 15f; // Quanto de vida tira ao jogador
    public float tempoEntreAtaques = 2f; // Espera 2 segundos antes de morder outra vez
    private float tempoDoUltimoAtaque = 0f;

    [Header("Pets (Cão e Gato)")]
    public int macasDadas = 0;
    private int macasParaDomesticar = 5;
    public bool domesticado = false;
    private bool provocado = false;

    [Header("Drop de Itens")]
    public GameObject prefabCarneDoChao; // O item 3D da carne que vai rolar no chão

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
        if (vida <= 0 || player == null) return;

        float distanciaProPlayer = Vector3.Distance(transform.position, player.position);

        // 1. DOMESTICADO
        if (domesticado)
        {
            if (distanciaProPlayer > 4f) agente.SetDestination(player.position);
            return;
        }

        // 2. PRESA
        if (tipoAnimal == Comportamento.Presa)
        {
            if (distanciaProPlayer < distanciaDetecao) FogirDoPlayer();
        }
        
        // 3. PREDADOR OU PET PROVOCADO
        else if (tipoAnimal == Comportamento.Predador || provocado)
        {
            if (distanciaProPlayer < distanciaDetecao)
            {
                agente.SetDestination(player.position);
                
                // Se estiver a 2 metros ou menos do jogador, ATACA!
                if (distanciaProPlayer <= 2f) 
                {
                    if (Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques)
                    {
                        AtacarJogador();
                    }
                }
            }
        }
    }

    void AtacarJogador()
    {
        tempoDoUltimoAtaque = Time.time; // Regista a hora do ataque para o cooldown
        
        Debug.Log(gameObject.name + " deu " + danoAtaque + " de dano ao jogador!");

        // --- AQUI VAIS LIGAR AO SCRIPT DA VIDA DO TEU JOGADOR ---
        // Exemplo:
        // GestorDeVida vidaPlayer = player.GetComponent<GestorDeVida>();
        // if (vidaPlayer != null) vidaPlayer.PerderVida(danoAtaque);
    }

    void AndarAleatoriamente()
    {
        if (domesticado || (tipoAnimal == Comportamento.Predador && Vector3.Distance(transform.position, player.position) < distanciaDetecao)) return;

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
        Vector3 direcaoFuga = transform.position - player.position;
        Vector3 novaPosicao = transform.position + direcaoFuga.normalized * 10f;
        agente.SetDestination(novaPosicao);
    }

    public void ReceberDano(float dano)
    {
        vida -= dano;
        if (tipoAnimal == Comportamento.Pet) provocado = true; 
        
        if (vida <= 0) Morrer();
    }

    void Morrer()
    {
        if (darCarneAoMorrer && tipoAnimal != Comportamento.Pet)
        {
            Debug.Log(gameObject.name + " morreu e dropou carne!");
            
            // Cria a carne na posição onde o animal morreu
            if (prefabCarneDoChao != null)
            {
                Instantiate(prefabCarneDoChao, transform.position + Vector3.up, Quaternion.identity);
            }
        }
        
        Destroy(gameObject);
    }

    /*void Morrer()
    {
        if (darCarneAoMorrer && tipoAnimal != Comportamento.Pet)
        {
            Debug.Log(gameObject.name + " morreu e vai dropar carne!");
        }
        
        Destroy(gameObject);
    }*/

    public void TentarDomesticar()
    {
        if (tipoAnimal != Comportamento.Pet || domesticado || provocado) return;

        macasDadas++;
        if (macasDadas >= macasParaDomesticar) domesticado = true;
    }
}