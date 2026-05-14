using UnityEngine;
using UnityEngine.AI; // Necessário para a IA andar

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalAI : MonoBehaviour
{
    public enum Comportamento { Presa, Predador, Pet }
    
    [Header("Configurações Principais")]
    public Comportamento tipoAnimal;
    public float vida = 100f;
    public float distanciaDetecao = 15f;
    public bool darCarneAoMorrer = true;

    [Header("Pets (Cão e Gato)")]
    public int macasDadas = 0;
    private int macasParaDomesticar = 5;
    public bool domesticado = false;
    private bool provocado = false; // Se bateres no pet, ele ataca

    private NavMeshAgent agente;
    private Transform player;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Põe o animal a andar pela ilha de 5 em 5 segundos
        InvokeRepeating("AndarAleatoriamente", Random.Range(0f, 2f), 5f);
    }

    void Update()
    {
        if (vida <= 0 || player == null) return;

        float distanciaProPlayer = Vector3.Distance(transform.position, player.position);

        // 1. SE FOR DOMESTICADO: Segue o jogador
        if (domesticado)
        {
            if (distanciaProPlayer > 4f) agente.SetDestination(player.position);
            return;
        }

        // 2. SE FOR PRESA (Galinha/Veado): Foge se o jogador chegar perto
        if (tipoAnimal == Comportamento.Presa)
        {
            if (distanciaProPlayer < distanciaDetecao) FogirDoPlayer();
        }
        
        // 3. SE FOR PREDADOR (Tigre) OU PET PROVOCADO: Ataca!
        else if (tipoAnimal == Comportamento.Predador || provocado)
        {
            if (distanciaProPlayer < distanciaDetecao)
            {
                agente.SetDestination(player.position);
                if (distanciaProPlayer <= 2f) 
                {
                    // Aqui entrará a animação/dano de ataque ao jogador no futuro
                    // Debug.Log(gameObject.name + " atacou o jogador!");
                }
            }
        }
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

    // Função para o teu Player chamar quando bater no animal
    public void ReceberDano(float dano)
    {
        vida -= dano;
        if (tipoAnimal == Comportamento.Pet) provocado = true; // Traíste o pet!
        
        if (vida <= 0) Morrer();
    }

    void Morrer()
    {
        if (darCarneAoMorrer && tipoAnimal != Comportamento.Pet)
        {
            // FUTURO: Instantiate(prefabDaCarne, transform.position, Quaternion.identity);
            Debug.Log(gameObject.name + " morreu e vai dropar carne!");
        }
        else if (tipoAnimal == Comportamento.Pet)
        {
            Debug.Log("Mataste um pet, não recebes nada seu monstro!");
        }
        
        Destroy(gameObject); // O animal desaparece
    }

    // Função para o teu PlayerInteraction chamar quando carregar no [F] com maçãs
    public void TentarDomesticar()
    {
        if (tipoAnimal != Comportamento.Pet || domesticado || provocado) return;

        macasDadas++;
        Debug.Log("Deste uma maçã ao " + gameObject.name + "! (" + macasDadas + "/5)");

        if (macasDadas >= macasParaDomesticar)
        {
            domesticado = true;
            Debug.Log(gameObject.name + " agora é teu amigo!");
        }
    }
}