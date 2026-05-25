using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações Globais")]
    public float interactionRange = 5f;
    public float interactionRadius = 0.5f; 
    public TextMeshProUGUI textoInteracao;
    
    [Header("Dano em Combate")]
    public float danoLanca = 35f;
    public float danoSoco = 10f; 

    [Header("Efeitos Visuais (VFX)")]
    [Tooltip("Coloca aqui o Prefab do efeito de Sangue")]
    public GameObject vfxSanguePrefab;
    public float alturaSangueAnimal = 1.0f;

    [Space(10)]
    [Tooltip("Coloca aqui o Prefab das partículas de Madeira (Ex: WoodChips)")]
    public GameObject vfxMadeiraPrefab; // TEU NOVO VFX AQUI!

    [Header("Animações")]
    public Animator animatorJogador; 
    public string nomeDoTriggerSocoEsquerdo = "PunchLeft"; 
    public string nomeDoTriggerSocoDireito = "PunchRight"; 

    private bool proximoSocoEsquerdo = true; 
    private float coltdownTextoEspecial = 0f; 

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    // Permite que o AnimalAI envie mensagens de texto longas para a tua UI
    public void MostrarMensagemEspecial(string msg, float duracao)
    {
        DefinirTexto(msg);
        coltdownTextoEspecial = duracao;
    }

    void Update()
    {
        // ==========================================================
        // NOVO: Impede que faças ações enquanto escreves o nome do pet!
        if (NoteManager.isReading || PetNamingManager.isNaming) return;
        // ==========================================================

        Vector3 rayOrigin = cam.position;
        Vector3 rayDirection = cam.forward;
        RaycastHit hit;

        Debug.DrawRay(rayOrigin, rayDirection * interactionRange, Color.red);

        // =================================================================
        bool acertouEmAlgo = Physics.SphereCast(rayOrigin, interactionRadius, rayDirection, out hit, interactionRange);
        
        // =================================================================
        // A. SISTEMA DE COMBATE (CLIQUE ESQUERDO)
        // =================================================================
        if (Cursor.lockState == CursorLockMode.Locked && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Tenta encontrar qualquer item equipado na mão (filho do jogador)
            FerramentaAtaque armaEquipada = GetComponentInChildren<FerramentaAtaque>();
            ObjetoNaMao objetoNaMao = GetComponentInChildren<ObjetoNaMao>(); // <--- PROCURA A ETIQUETA

            // 1. LÓGICA DE DESTRUIÇÃO DE FLORES (Independente do item na mão)
            if (acertouEmAlgo && hit.collider.CompareTag("Flower"))
            {
                Destroy(hit.collider.gameObject);
            }

            // ==========================================================
            // MUDANÇA: Prioridade Total ao Item Equipado (Bloqueia Socos)
            // ==========================================================
            if (objetoNaMao != null || armaEquipada != null)
            {
                // Se o jogador tem UM ITEM NA MÃO, o soco é BLOQUEADO.
                // Apenas executamos as ações específicas se for uma arma.

                if (armaEquipada != null)
                {
                    // 1. É UMA LANÇA?
                    if (armaEquipada.ehLanca)
                    {
                        armaEquipada.JogarAnimacaoGatilho(); 
                        
                        // --- ÁUDIO DA LANÇA ---
                        if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXSpear");
                        
                        if (acertouEmAlgo && hit.collider.CompareTag("Animal"))
                        {
                            AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                            if (animal != null && animal.vida > 0 && !(animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado))
                            {
                                animal.ReceberDano(danoLanca);
                                EspirrarSangue(animal.transform.position); 
                                ComandarPetsParaAtacar(animal); 
                            }
                        }
                    }
                    // 2. É UM MACHADO?
                    else if (armaEquipada.ehMachado)
                    {
                        armaEquipada.JogarAnimacaoGatilho(); 
                        
                        if (acertouEmAlgo && hit.collider.CompareTag("ArvoreMadeira"))
                        {
                            ArvoreMadeira arvore = hit.collider.GetComponent<ArvoreMadeira>();
                            if (arvore != null)
                            {
                                arvore.LevarMachadada();

                                // --- ÁUDIO DO MACHADO ---
                                if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXMachado");
                                
                                // ==============================================
                                // NOVO: SPAWN DO VFX DE MADEIRA NO IMPACTO!
                                // ==============================================
                                if (vfxMadeiraPrefab != null)
                                {
                                    Instantiate(vfxMadeiraPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                                }
                            }
                        }
                    }
                }
            }
            // ==========================================================
            // 3. LÓGICA DO SOCO (MÃOS TOTALMENTE VAZIAS)
            // ==========================================================
            else 
            {
                // Esta secção SÓ corre se armaEquipada E objetoNaMao forem NULL (mãos vazias)
                
                string triggerParaUsar = proximoSocoEsquerdo ? nomeDoTriggerSocoEsquerdo : nomeDoTriggerSocoDireito;
                if (animatorJogador != null)
                {
                    animatorJogador.SetTrigger(triggerParaUsar);
                }
                proximoSocoEsquerdo = !proximoSocoEsquerdo; 

                // --- ÁUDIO DO SOCO ---
                if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXPunch");

                if (acertouEmAlgo && hit.collider.CompareTag("Animal"))
                {
                    AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                    if (animal != null && animal.vida > 0 && !(animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado))
                    {
                        animal.ReceberDano(danoSoco);
                        EspirrarSangue(animal.transform.position); 
                        ComandarPetsParaAtacar(animal); 
                    }
                }
            }
        }

        // =================================================================
        // B. SISTEMA DE INTERAÇÃO (TEXTOS E CRÃ)
        // =================================================================
        bool olhouParaAlgoInterativo = false;

        if (coltdownTextoEspecial > 0)
        {
            coltdownTextoEspecial -= Time.deltaTime;
            olhouParaAlgoInterativo = true;
        }

        if (acertouEmAlgo)
        {
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            if (item != null && hit.collider.gameObject != gameObject)
            {
                olhouParaAlgoInterativo = true;
                DefinirTexto("Pick up " + item.referenciaItem.itemName + " [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    // =========================================================
                    // NOVO ÁUDIO: Apanhar itens soltos (pedras, madeira, cocos, etc)
                    if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXPickUpItems");
                    // =========================================================
                    
                    item.SerApanhado();
                    EsconderTexto();
                }
            }
            else if (hit.collider.GetComponent<WorldNote>() != null)
            {
                WorldNote nota = hit.collider.GetComponent<WorldNote>();
                olhouParaAlgoInterativo = true;
                DefinirTexto("Read Note [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    // --- ÁUDIO DAS NOTAS ---
                    if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXPaper");

                    nota.LerNota();
                    EsconderTexto();
                }
            }
            else if (hit.collider.GetComponent<TutorialItem>() != null)
            {
                TutorialItem tutorial = hit.collider.GetComponent<TutorialItem>();
                olhouParaAlgoInterativo = true;
                DefinirTexto("Read Survival Guide [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    // --- ÁUDIO DAS NOTAS (TUTORIAL) ---
                    if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXPaper");

                    tutorial.ApanharLivro();
                    EsconderTexto();
                }
            }
            else if (hit.collider.CompareTag("Animal"))
            {
                AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                if (animal != null && animal.vida > 0)
                {
                    if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && !animal.domesticado && !animal.provocado)
                    {
                        olhouParaAlgoInterativo = true;
                        int applesLeft = 5 - animal.macasDadas;
                        DefinirTexto("Press [F] to Give Apple (" + applesLeft + " more needed)");

                        if (Keyboard.current.fKey.wasPressedThisFrame) animal.TentarDomesticar();
                    }
                    else if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado)
                    {
                        olhouParaAlgoInterativo = true;
                        
                        // =========================================================
                        // NOVO: MOSTRAMOS O NOME SE EXISTIR, SENÃO "Loyal Companion"
                        string nomeParaMostrar = string.IsNullOrEmpty(animal.nomeDoPet) ? "Loyal Companion" : animal.nomeDoPet;
                        DefinirTexto(nomeParaMostrar);
                        // =========================================================
                    }
                    else 
                    {
                        FerramentaAtaque armaEquipadaUI = GetComponentInChildren<FerramentaAtaque>();
                        
                        if (armaEquipadaUI == null) 
                        {
                            olhouParaAlgoInterativo = true;
                            DefinirTexto("Punch [Left Click]");
                        }
                        else if (armaEquipadaUI.ehLanca) 
                        {
                            olhouParaAlgoInterativo = true;
                            DefinirTexto("Attack with Spear [Left Click]");
                        }
                    }
                }
            }
            else if (hit.collider.CompareTag("ArvoreMadeira"))
            {
                ArvoreMadeira arvore = hit.collider.GetComponent<ArvoreMadeira>();
                if (arvore != null)
                {
                    FerramentaAtaque armaEquipadaUI = GetComponentInChildren<FerramentaAtaque>();

                    if (armaEquipadaUI != null && armaEquipadaUI.ehMachado)
                    {
                        olhouParaAlgoInterativo = true;
                        DefinirTexto("Chop Tree [Left Click]");
                    }
                }
            }
            else if (hit.collider.CompareTag("ArvoreFruta"))
            {
                ArvoreFruta arvoreFruta = hit.collider.GetComponent<ArvoreFruta>();
                if (arvoreFruta != null)
                {
                    olhouParaAlgoInterativo = true;
                    if (arvoreFruta.TemFruta())
                    {
                        DefinirTexto("Press [E] to Harvest Fruit");

                        if (Keyboard.current.eKey.wasPressedThisFrame)
                        {
                            // =========================================================
                            // NOVO ÁUDIO: Colher a fruta da árvore
                            if (AudioManager.instance != null) AudioManager.instance.TocarSFX("SFXPickUpItems");
                            // =========================================================
                            
                            arvoreFruta.ApanharFruta();
                            if (arvoreFruta.itemFruta != null) DefinirTexto(arvoreFruta.itemFruta.itemName + " added to inventory!");
                        }
                    }
                    else
                    {
                        DefinirTexto("This tree has no fruit left.");
                    }
                }
            }
            else if (hit.collider.GetComponent<Fogueira>() != null)
            {
                Fogueira fogueira = hit.collider.GetComponent<Fogueira>();
                olhouParaAlgoInterativo = true;

                if (fogueira.GetEstaAcesa())
                {
                    int tempoInt = Mathf.CeilToInt(fogueira.GetTempoRestante());
                    DefinirTexto($"Fireplace (Active: {tempoInt}s) [E]");
                }
                else
                {
                    DefinirTexto("Open Fireplace [E]");
                }

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    fogueira.AbrirInterfaceFogueira();
                    EsconderTexto();
                }
            }
        }

        if (!olhouParaAlgoInterativo)
        {
            EsconderTexto();

            Fogueira fogueiraAtiva = Object.FindFirstObjectByType<Fogueira>();
            if (fogueiraAtiva != null && fogueiraAtiva.painelFogueiraUI != null && fogueiraAtiva.painelFogueiraUI.gameObject.activeSelf)
            {
                InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                if (invUI != null) invUI.FecharInventarioExterno();
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                fogueiraAtiva.FecharInterfaceFogueira();
            }
        }
    }

    void EspirrarSangue(Vector3 posicaoAnimal)
    {
        if (vfxSanguePrefab != null)
        {
            Vector3 posicaoSpawn = posicaoAnimal + new Vector3(0, alturaSangueAnimal, 0);
            Instantiate(vfxSanguePrefab, posicaoSpawn, Quaternion.identity);
        }
    }

    void ComandarPetsParaAtacar(AnimalAI inimigo)
    {
        AnimalAI[] todosAnimais = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None);
        foreach (AnimalAI pet in todosAnimais)
        {
            pet.DefinirAlvoParaPet(inimigo);
        }
    }

    void DefinirTexto(string texto) 
    { 
        if (textoInteracao != null) 
        { 
            textoInteracao.text = texto; 
            textoInteracao.gameObject.SetActive(true); 
        } 
    }
    
    void EsconderTexto() 
    { 
        if (textoInteracao != null) 
        { 
            textoInteracao.gameObject.SetActive(false); 
        } 
    }
    
    void OnDrawGizmosSelected()
    {
        if (cam != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 endPosition = cam.position + cam.forward * interactionRange;
            Gizmos.DrawLine(cam.position, endPosition);
            Gizmos.DrawWireSphere(endPosition, interactionRadius);
        }
    }
}