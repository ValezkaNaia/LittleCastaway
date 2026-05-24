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

    [Header("Animações")]
    public Animator animatorJogador; 
    public string nomeDoTriggerSocoEsquerdo = "PunchLeft"; 
    public string nomeDoTriggerSocoDireito = "PunchRight"; 

    private bool proximoSocoEsquerdo = true; 

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (NoteManager.isReading) return;

        Vector3 rayOrigin = cam.position;
        Vector3 rayDirection = cam.forward;
        RaycastHit hit;

        Debug.DrawRay(rayOrigin, rayDirection * interactionRange, Color.red);

        bool acertouEmAlgo = Physics.SphereCast(rayOrigin, interactionRadius, rayDirection, out hit, interactionRange);

        // =================================================================
        // A. SISTEMA DE COMBATE (CLIQUE ESQUERDO) - Funciona mesmo no ar!
        // =================================================================
        if (Cursor.lockState == CursorLockMode.Locked && Mouse.current.leftButton.wasPressedThisFrame)
        {
            FerramentaAtaque armaEquipada = GetComponentInChildren<FerramentaAtaque>();

            // =================================================================
            // NOVO: DESTRUIR OBJETOS FRÁGEIS (Flores, arbustos pequenos, etc)
            // =================================================================
            if (acertouEmAlgo && hit.collider.CompareTag("Flower"))
            {
                // Destrói a flor instantaneamente!
                Destroy(hit.collider.gameObject);
            }

            // Se o jogador tiver alguma ferramenta equipada nas mãos...
            if (armaEquipada != null)
            {
                // 1. É UMA LANÇA?
                if (armaEquipada.ehLanca)
                {
                    armaEquipada.JogarAnimacaoGatilho(); 
                    
                    if (acertouEmAlgo && hit.collider.CompareTag("Animal"))
                    {
                        AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                        if (animal != null && animal.vida > 0 && !(animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado))
                        {
                            animal.ReceberDano(danoLanca);
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
                        if (arvore != null) arvore.LevarMachadada();
                    }
                }
            }
            // 3. MÃOS NUAS (Não tem nenhuma arma equipada) -> SOCO!
            else 
            {
                string triggerParaUsar = proximoSocoEsquerdo ? nomeDoTriggerSocoEsquerdo : nomeDoTriggerSocoDireito;
                if (animatorJogador != null)
                {
                    animatorJogador.SetTrigger(triggerParaUsar);
                }
                proximoSocoEsquerdo = !proximoSocoEsquerdo; 

                if (acertouEmAlgo && hit.collider.CompareTag("Animal"))
                {
                    AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                    if (animal != null && animal.vida > 0 && !(animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado))
                    {
                        animal.ReceberDano(danoSoco);
                    }
                }
            }
        }

        // =================================================================
        // B. SISTEMA DE INTERAÇÃO (TEXTOS NO ECRÃ E TECLAS DE AÇÃO)
        // =================================================================
        bool olhouParaAlgoInterativo = false;

        if (acertouEmAlgo)
        {
            // 1. APANHAR ITENS DO CHÃO
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            if (item != null && hit.collider.gameObject != gameObject)
            {
                olhouParaAlgoInterativo = true;
                DefinirTexto("Pick up " + item.referenciaItem.itemName + " [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    item.SerApanhado();
                    EsconderTexto();
                }
            }

            // 2. LER NOTAS NORMAIS
            else if (hit.collider.GetComponent<WorldNote>() != null)
            {
                WorldNote nota = hit.collider.GetComponent<WorldNote>();
                olhouParaAlgoInterativo = true;
                DefinirTexto("Read Note [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    nota.LerNota();
                    EsconderTexto();
                }
            }

            // 2.5 LER GUIA DE TUTORIAL
            else if (hit.collider.GetComponent<TutorialItem>() != null)
            {
                TutorialItem tutorial = hit.collider.GetComponent<TutorialItem>();
                olhouParaAlgoInterativo = true;
                DefinirTexto("Read Survival Guide [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    tutorial.ApanharLivro();
                    EsconderTexto();
                }
            }

            // 3. OLHAR PARA ANIMAIS
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

                        if (Keyboard.current.fKey.wasPressedThisFrame)
                        {
                            animal.TentarDomesticar();
                        }
                    }
                    else if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado)
                    {
                        olhouParaAlgoInterativo = true;
                        DefinirTexto("Loyal Companion");
                    }
                    else 
                    {
                        FerramentaAtaque armaEquipada = GetComponentInChildren<FerramentaAtaque>();
                        
                        if (armaEquipada == null) 
                        {
                            olhouParaAlgoInterativo = true;
                            DefinirTexto("Punch [Left Click]");
                        }
                        else if (armaEquipada.ehLanca) 
                        {
                            olhouParaAlgoInterativo = true;
                            DefinirTexto("Attack with Spear [Left Click]");
                        }
                    }
                }
            }

            // 4. OLHAR PARA ÁRVORES DE MADEIRA
            else if (hit.collider.CompareTag("ArvoreMadeira"))
            {
                ArvoreMadeira arvore = hit.collider.GetComponent<ArvoreMadeira>();
                if (arvore != null)
                {
                    FerramentaAtaque armaEquipada = GetComponentInChildren<FerramentaAtaque>();

                    if (armaEquipada != null && armaEquipada.ehMachado)
                    {
                        olhouParaAlgoInterativo = true;
                        DefinirTexto("Chop Tree [Left Click]");
                    }
                }
            }

            // 5. APANHAR FRUTA DAS ÁRVORES
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
                            arvoreFruta.ApanharFruta();
                            
                            if (arvoreFruta.itemFruta != null)
                            {
                                DefinirTexto(arvoreFruta.itemFruta.itemName + " added to inventory!");
                            }
                        }
                    }
                    else
                    {
                        DefinirTexto("This tree has no fruit left.");
                    }
                }
            }

            // 6. INTERAGIR COM A FOGUEIRA
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
                if (invUI != null)
                {
                    invUI.FecharInventarioExterno();
                }
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                fogueiraAtiva.FecharInterfaceFogueira();
            }
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