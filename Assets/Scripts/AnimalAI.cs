using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))] 
public class AnimalAI : MonoBehaviour
{
    public enum Comportamento { Presa, Predador, Pet }
    
    [Header("Configurações Principais")]
    public Comportamento tipoAnimal;
    public float vida = 100f;
    private float vidaMaxima; // Guarda a vida inicial para curar predadores
    public float distanciaDetecao = 15f;
    public bool darCarneAoMorrer = true;

    [Header("Audio 3D do Animal")]
    public AudioClip sfxAtacar; 
    public AudioClip sfxSofrerDanoEFugir; 
    public AudioClip musicaCombateBoss; 
    private AudioSource emissorAudio; 

    [Header("Combate (Ataque)")]
    public float danoAtaque = 15f; 
    public float tempoEntreAtaques = 2f; 
    private float tempoDoUltimoAtaque = 0f;

    [Header("Pets (Cão e Gato)")]
    public int macasDadas = 0;
    private int macasParaDomesticar = 5;
    public bool domesticado = false;
    public bool provocado = false; 
    public AnimalAI alvoDoPet = null; 


    // ==========================================
    // NOME DO PET
    // ==========================================
    public string nomeDoPet = "";

    [Header("Drop de Itens")] public GameObject prefabCarneDoChao; 
    [Header("Efeitos Visuais - Geral")] public GameObject particulaMortePrefab; public float offsetAlturaParticulaMorte = 1.0f; 
    [Header("Efeitos Visuais - Pets")] public GameObject particulaDomesticaoPrefab; public float offsetAlturaCorações = 1.0f; 
    [Header("Efeitos Visuais - Combate")] public GameObject vfxSangueAtaquePrefab;
    [Header("Modo Noturno")] public GameObject olhosBrilhantes; public Transform luzDoSol;
    
    private NavMeshAgent agente; private Animator anim; private Transform player;
    private bool emCombateComPlayer = false;

    void Start()
    {
        //Debug.LogError("!!! O SCRIPT CERTO ESTÁ A CORRER !!!");

        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        emissorAudio = GetComponent<AudioSource>(); 
        
        vidaMaxima = vida; 
        
        emissorAudio.spatialBlend = 1.0f; 
        emissorAudio.maxDistance = 25f;   
        emissorAudio.rolloffMode = AudioRolloffMode.Linear; 

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (luzDoSol == null) { if (RenderSettings.sun != null) luzDoSol = RenderSettings.sun.transform; else { GameObject solNaCena = GameObject.Find("Directional Light"); if (solNaCena != null) luzDoSol = solNaCena.transform; } }
        if (olhosBrilhantes != null) olhosBrilhantes.SetActive(false);

        ColarAoNavMesh();
        InvokeRepeating("AndarAleatoriamente", Random.Range(0f, 2f), 5f);
    }

    void Update()
{
    if (vida <= 0 || player == null) return;

    if (olhosBrilhantes != null && luzDoSol != null)
    {
        bool estaDeNoite = luzDoSol.forward.y > 0f;
        if (olhosBrilhantes.activeSelf != estaDeNoite) olhosBrilhantes.SetActive(estaDeNoite);
    }

    if (anim != null) { float vel = agente.velocity.magnitude; anim.SetFloat("Vert", vel); anim.SetFloat("State", vel > 0.1f ? 1f : 0f); }

    if (!agente.isOnNavMesh) { ColarAoNavMesh(); return; }

    float distanciaProPlayer = Vector3.Distance(transform.position, player.position);

    // ======================================================================================
    // CONTROLO DE ÁUDIO DE PERSEGUIÇÃO/COMBATE (SÓ ATIVA UMA VEZ)
    // ======================================================================================
    if (tipoAnimal == Comportamento.Predador)
    {
        if (distanciaProPlayer < distanciaDetecao)
        {
            // Se o jogador entrou no raio e o Tigre ainda não sabia, ativa o combate!
            if (!emCombateComPlayer)
            {
                emCombateComPlayer = true;
                if (musicaCombateBoss != null && AudioManager.instance != null) 
                {
                    AudioManager.instance.MudarMusicaDeFundo(musicaCombateBoss);
                }
            }
        }
        else
        {
            // Se o jogador fugiu do raio e o Tigre ainda achava que estava em combate, desliga tudo!
            if (emCombateComPlayer)
            {
                PararSonsDeCombateFuga();
            }
        }
    }

    // ======================================================================================
    // 1. COMPORTAMENTO: PET DOMESTICADO
    // ======================================================================================
    if (tipoAnimal == Comportamento.Pet && domesticado)
    {
        if (alvoDoPet != null && alvoDoPet.vida > 0)
        {
            if (Vector3.Distance(agente.destination, alvoDoPet.transform.position) > 0.5f)
            {
                agente.SetDestination(alvoDoPet.transform.position);
            }

            if (Vector3.Distance(transform.position, alvoDoPet.transform.position) <= 2.5f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques) 
            {
                AtacarOutroAnimal(alvoDoPet);
            }
        }
        else 
        { 
            alvoDoPet = null; 
            if (Vector3.Distance(transform.position, player.position) > 4f) 
            {
                if (Vector3.Distance(agente.destination, player.position) > 1.0f)
                {
                    agente.SetDestination(player.position); 
                }
            }
            else 
            {
                if (agente.hasPath) agente.ResetPath(); 
            }
        }
        return; 
    }

    // ======================================================================================
    // 2. COMPORTAMENTO: PRESA
    // ======================================================================================
    if (tipoAnimal == Comportamento.Presa) 
    { 
        if (distanciaProPlayer < distanciaDetecao) FogirDoPlayer(); 
        else
        {
            // Se a presa fugiu completamente, para o som de choro/fuga
            if (emissorAudio.isPlaying && emissorAudio.clip == sfxSofrerDanoEFugir) emissorAudio.Stop();
        }
        return; 
    }
    
    // ======================================================================================
    // 3. COMPORTAMENTO: PREDADOR (TIGRE)
    // ======================================================================================
    if (tipoAnimal == Comportamento.Predador)
    {
        Transform alvoFinal = null;
        float menorDistanciaAlvo = distanciaDetecao;
        bool encontrouPetDomesticado = false;

        AnimalAI[] todosAnimais = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None);
        foreach (AnimalAI outroAnimal in todosAnimais)
        {
            if (outroAnimal != this && outroAnimal.vida > 0 && outroAnimal.tipoAnimal == Comportamento.Pet && outroAnimal.domesticado)
            {
                float dist = Vector3.Distance(transform.position, outroAnimal.transform.position);
                if (dist < menorDistanciaAlvo)
                {
                    alvoFinal = outroAnimal.transform;
                    menorDistanciaAlvo = dist;
                    encontrouPetDomesticado = true; 
                }
            }
        }

        if (!encontrouPetDomesticado)
        {
            if (distanciaProPlayer < menorDistanciaAlvo)
            {
                alvoFinal = player;
                menorDistanciaAlvo = distanciaProPlayer;
            }
            else
            {
                foreach (AnimalAI outroAnimal in todosAnimais)
                {
                    if (outroAnimal != this && outroAnimal.vida > 0)
                    {
                        if (outroAnimal.tipoAnimal == Comportamento.Presa || 
                            outroAnimal.tipoAnimal == Comportamento.Predador || 
                            (outroAnimal.tipoAnimal == Comportamento.Pet && !outroAnimal.domesticado))
                        {
                            float dist = Vector3.Distance(transform.position, outroAnimal.transform.position);
                            if (dist < menorDistanciaAlvo)
                            {
                                alvoFinal = outroAnimal.transform;
                                menorDistanciaAlvo = dist;
                            }
                        }
                    }
                }
            }
        }

        if (alvoFinal != null)
        {
            if (Vector3.Distance(agente.destination, alvoFinal.position) > 0.5f)
            {
                agente.SetDestination(alvoFinal.position);
            }

            float distanciaAtual = Vector3.Distance(transform.position, alvoFinal.position);
            if (distanciaAtual <= 2.5f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques)
            {
                if (alvoFinal == player) AtacarJogador();
                else AtacarOutroAnimal(alvoFinal.GetComponent<AnimalAI>());
            }
        }
        return; 
    }

    // ======================================================================================
    // 4. COMPORTAMENTO: PET SELVAGEM PROVOCADO
    // ======================================================================================
    if (tipoAnimal == Comportamento.Pet && provocado) 
    { 
        if (distanciaProPlayer < distanciaDetecao) 
        { 
            if (Vector3.Distance(agente.destination, player.position) > 0.5f)
            {
                agente.SetDestination(player.position); 
            }
            
            if (distanciaProPlayer <= 2.5f && Time.time >= tempoDoUltimoAtaque + tempoEntreAtaques) AtacarJogador(); 
        } 
    }
}

    public void DefinirAlvoParaPet(AnimalAI novoAlvo) { if (tipoAnimal == Comportamento.Pet && domesticado) alvoDoPet = novoAlvo; }
    
    public void TentarDomesticar()
    {
        if (tipoAnimal != Comportamento.Pet || domesticado || provocado) return;

        if (HotbarManager.instance != null)
        {
            ItemData itemNaMao = HotbarManager.instance.GetItemSelecionado();

            if (itemNaMao != null && itemNaMao.isConsumable)
            {
                InventoryManager invManager = Object.FindFirstObjectByType<InventoryManager>();
                if (invManager != null) invManager.RemoveItem(itemNaMao); 

                HotbarManager.instance.RemoverItemGasto(itemNaMao);
                
                InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                if (invUI != null) invUI.AtualizarUI();
            }
            else
            {
                Debug.LogWarning("Não podes domesticar o animal sem comida adequada na mão!");
                return; 
            }
        }

        macasDadas++;
        
        if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXGivePetFood"); 

        if (macasDadas >= macasParaDomesticar)
        {
            // ==========================================================
            // DE VOLTA AOS 50% DE CHANCE!
            // ==========================================================
            if (Random.value <= 0.5f) 
            { 
                domesticado = true; 
                if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXSucessAdopt"); 
                if (particulaDomesticaoPrefab != null) Instantiate(particulaDomesticaoPrefab, transform.position + new Vector3(0, offsetAlturaCorações, 0), Quaternion.identity); 

                if (PetNamingManager.instance != null) PetNamingManager.instance.AbrirPainel(this);
            }
            else
            {
                if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXFailAdopt"); 

                PlayerInteraction interactionUI = Object.FindFirstObjectByType<PlayerInteraction>();
                if (interactionUI != null) interactionUI.MostrarMensagemEspecial("The animal rejected the food, bit you, and ran away!", 4f); 
                
                // Morde-te uma vez
                AtacarJogador(); 
                
                // Transforma-se numa Presa e limpa o caminho para garantir que foge no frame seguinte
                tipoAnimal = Comportamento.Presa;
                if (agente.isOnNavMesh) agente.ResetPath();
            }
        }
    }

    void ColarAoNavMesh() { NavMeshHit hit; if (NavMesh.SamplePosition(transform.position, out hit, 20.0f, NavMesh.AllAreas)) { transform.position = hit.position; agente.Warp(hit.position); } }
    
    void AtacarJogador() 
    { 
        tempoDoUltimoAtaque = Time.time; 
        if (SurvivalManager.instance != null) SurvivalManager.instance.ReceberDano(danoAtaque); 
        
        if (sfxAtacar != null) emissorAudio.PlayOneShot(sfxAtacar);
        //if (tipoAnimal == Comportamento.Predador && musicaCombateBoss != null && AudioManager.instance != null) AudioManager.instance.MudarMusicaDeFundo(musicaCombateBoss);

        if (vfxSangueAtaquePrefab != null && player != null) Instantiate(vfxSangueAtaquePrefab, player.position + new Vector3(0, 1.0f, 0), Quaternion.identity);
        AnimalAI[] todosAnimais = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None); foreach (AnimalAI pet in todosAnimais) { pet.DefinirAlvoParaPet(this); }
    }

    void AtacarOutroAnimal(AnimalAI vitima) 
    { 
        if (vitima == null) return; 
        tempoDoUltimoAtaque = Time.time; 
        vitima.ReceberDano(danoAtaque, tipoAnimal == Comportamento.Predador); 
        
        if (sfxAtacar != null) emissorAudio.PlayOneShot(sfxAtacar);

        if (vfxSangueAtaquePrefab != null) Instantiate(vfxSangueAtaquePrefab, vitima.transform.position + new Vector3(0, 1.0f, 0), Quaternion.identity);

        if (vitima.vida <= 0 && tipoAnimal == Comportamento.Predador)
        {
            vida = vidaMaxima; 
            Debug.Log("O Tigre matou uma presa e recuperou a sua vida toda!");
        }
    }

    void AndarAleatoriamente() 
    { 
        if (!agente.isOnNavMesh || domesticado) return; 
        
        float distPlayer = Vector3.Distance(transform.position, player.position);
        
        if (tipoAnimal == Comportamento.Presa && distPlayer < distanciaDetecao) return;
        if (tipoAnimal == Comportamento.Predador && distPlayer < distanciaDetecao) return;
        if (tipoAnimal == Comportamento.Pet && provocado && distPlayer < distanciaDetecao) return;

        Vector3 dir = Random.insideUnitSphere * 10f + transform.position; NavMeshHit hit; 
        if (NavMesh.SamplePosition(dir, out hit, 10f, 1)) agente.SetDestination(hit.position); 
    }
    
    /*void FogirDoPlayer() 
    { 
        if (!agente.isOnNavMesh) return; 
        
        if (!agente.hasPath || agente.velocity.sqrMagnitude < 0.2f)
        {
            Vector3 direcaoFuga = (transform.position - player.position).normalized;
            Vector3 pontoDestino = transform.position + direcaoFuga * 15f;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pontoDestino, out hit, 5f, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
            }
        }
        
        if (sfxSofrerDanoEFugir != null && !emissorAudio.isPlaying) emissorAudio.PlayOneShot(sfxSofrerDanoEFugir);
    }*/
    void FogirDoPlayer() 
    { 
        if (!agente.isOnNavMesh) return; 
        
        if (!agente.hasPath || agente.velocity.sqrMagnitude < 0.2f)
        {
            Vector3 direcaoFuga = (transform.position - player.position).normalized;
            Vector3 pontoDestino = transform.position + direcaoFuga * 15f;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pontoDestino, out hit, 5f, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
            }
        }
        
        // CORREÇÃO: Usar .clip e .Play() com loop para podermos PARAR o áudio quando ele se afastar
        if (sfxSofrerDanoEFugir != null && !emissorAudio.isPlaying) 
        {
            emissorAudio.clip = sfxSofrerDanoEFugir;
            emissorAudio.loop = true; // Continua a rosnar/gritar enquanto foge ou persegue
            emissorAudio.Play();
        }
    }

    void PararSonsDeCombateFuga()
    {
        // Se o áudio do próprio animal estiver a tocar os rosnados de perseguição/fuga, para imediatamente
        if (emissorAudio.isPlaying && (emissorAudio.clip == sfxAtacar || emissorAudio.clip == sfxSofrerDanoEFugir))
        {
            emissorAudio.Stop();
        }

        // Se for um predador (Tigre) e o jogador fugiu da distância de deteção, desliga a música de boss
        if (tipoAnimal == Comportamento.Predador && AudioManager.instance != null)
        {
            AudioManager.instance.ResetarMusicaParaNormal();
        }
    }
    
    public void ReceberDano(float dano, bool porPredador = false) 
    { 
        vida -= dano; 
        
        // Se bateres num pet normal que ainda não é teu, ele ataca-te
        if (tipoAnimal == Comportamento.Pet && !domesticado) provocado = true; 
        
        if (sfxSofrerDanoEFugir != null) emissorAudio.PlayOneShot(sfxSofrerDanoEFugir);

        if (vida <= 0) Morrer(porPredador); 
    }
    
    void Morrer(bool porPredador) 
    { 
        if (olhosBrilhantes != null) olhosBrilhantes.SetActive(false);

        if (tipoAnimal == Comportamento.Predador && AudioManager.instance != null)
        {
            AudioManager.instance.ResetarMusicaParaNormal();
        }

        if (particulaMortePrefab != null) Instantiate(particulaMortePrefab, transform.position + new Vector3(0, offsetAlturaParticulaMorte, 0), Quaternion.identity);
        if (darCarneAoMorrer && tipoAnimal != Comportamento.Pet && !porPredador && prefabCarneDoChao != null) Instantiate(prefabCarneDoChao, transform.position + Vector3.up, Quaternion.identity); 
        Destroy(gameObject); 
    }
}