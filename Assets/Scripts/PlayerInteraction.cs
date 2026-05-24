using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações Globais")]
    public float interactionRange = 5f;
    // NOVO: Aumenta a "grossura" do laser. 0.5f é um bom valor para não precisar mirar exato no centro.
    public float interactionRadius = 0.5f; 
    public TextMeshProUGUI textoInteracao;
    public float danoLanca = 35f;

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        // Se estiveres a ler uma nota, bloqueia a interação com outras coisas
        if (NoteManager.isReading) return;

        // Define a origem e direção baseadas na câmera
        Vector3 rayOrigin = cam.position;
        Vector3 rayDirection = cam.forward;
        RaycastHit hit;

        // Visualização do laser no Editor (Raio vermelho fino no centro)
        Debug.DrawRay(rayOrigin, rayDirection * interactionRange, Color.red);

        bool olhouParaAlgoInterativo = false;

        // MUDANÇA: Usamos SphereCast em vez de Raycast para criar um volume de detetor maior
        if (Physics.SphereCast(rayOrigin, interactionRadius, rayDirection, out hit, interactionRange))
        {
            // =================================================================
            // 1. APANHAR ITENS DO CHÃO
            // =================================================================
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            // Nota: Com SphereCast, às vezes detetamos o colisor do chão se o raio for muito grande. 
            // É boa prática verificar se o objeto detetado não é o próprio jogador.
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

            // =================================================================
            // 2. LER NOTAS NORMAIS (1 Página)
            // =================================================================
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

            // =================================================================
            // 2.5 LER GUIA DE TUTORIAL (Várias Páginas)
            // =================================================================
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

            // =================================================================
            // 3. INTERAÇÃO COM ANIMAIS (ATACAR OU DOMESTICAR)
            // =================================================================
            else if (hit.collider.CompareTag("Animal"))
            {
                AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                if (animal != null && animal.vida > 0)
                {
                    olhouParaAlgoInterativo = true;

                    // Sistema de domesticação individual (Correção do bug anterior)
                    if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && !animal.domesticado && !animal.provocado)
                    {
                        // Feedback dinâmico em Inglês
                        int applesLeft = 5 - animal.macasDadas;
                        DefinirTexto("Press [F] to Give Apple (" + applesLeft + " more needed)");

                        if (Keyboard.current.fKey.wasPressedThisFrame)
                        {
                            animal.TentarDomesticar();
                        }
                    }
                    else if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado)
                    {
                        DefinirTexto("Loyal Companion");
                    }
                    else // Predador ou Presa normal
                    {
                        FerramentaAtaque armaEquipada = GetComponentInChildren<FerramentaAtaque>();

                        if (armaEquipada != null && armaEquipada.ehLanca)
                        {
                            DefinirTexto("Press [E] to Attack with Spear");
                            if (Keyboard.current.eKey.wasPressedThisFrame)
                            {
                                armaEquipada.JogarAnimacaoGatilho();
                                animal.ReceberDano(danoLanca);
                            }
                        }
                        else
                        {
                            DefinirTexto("You need to equip a Spear!");
                        }
                    }
                }
            }

            // =================================================================
            // 4. CORTAR ÁRVORES DE MADEIRA
            // =================================================================
            else if (hit.collider.CompareTag("ArvoreMadeira"))
            {
                ArvoreMadeira arvore = hit.collider.GetComponent<ArvoreMadeira>();
                if (arvore != null)
                {
                    olhouParaAlgoInterativo = true;
                    FerramentaAtaque armaEquipada = GetComponentInChildren<FerramentaAtaque>();

                    if (armaEquipada != null && armaEquipada.ehMachado)
                    {
                        DefinirTexto("Press [E] to Chop Tree");
                        if (Keyboard.current.eKey.wasPressedThisFrame)
                        {
                            armaEquipada.JogarAnimacaoGatilho();
                            arvore.LevarMachadada();
                        }
                    }
                    else
                    {
                        DefinirTexto("You need to equip an Axe!");
                    }
                }
            }

            // =================================================================
            // 5. APANHAR FRUTA DAS ÁRVORES
            // =================================================================
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

            // =================================================================
            // 6. INTERAGIR COM A FOGUEIRA
            // =================================================================
            else if (hit.collider.GetComponent<Fogueira>() != null)
            {
                Fogueira fogueira = hit.collider.GetComponent<Fogueira>();
                olhouParaAlgoInterativo = true;

                if (fogueira.GetEstaAcesa())
                {
                    // Mostra o tempo restante caso esteja acesa
                    int tempoInt = Mathf.CeilToInt(fogueira.GetTempoRestante());
                    DefinirTexto($"Fireplace (Active: {tempoInt}s) [E]");
                }
                else
                {
                    DefinirTexto("Open Fireplace [E]");
                }

                // Quando o jogador prime a tecla [E], abre a interface de culinária
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

            // Se o jogador parar de olhar para a fogueira (ou se afastar), a interface fecha sozinha!
            Fogueira fogueiraAtiva = Object.FindFirstObjectByType<Fogueira>();
            
            // CORREÇÃO CRUCIAL: Só mexe no rato se a Fogueira existir E o painel visual dela estiver aberto!
            if (fogueiraAtiva != null && fogueiraAtiva.painelFogueiraUI != null && fogueiraAtiva.painelFogueiraUI.gameObject.activeSelf)
            {
                // NOVO: Fecha o inventário se o jogador se afastar da fogueira
                InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                if (invUI != null)
                {
                    invUI.FecharInventarioExterno();
                }
                // Como o jogador parou de olhar/se afastou, fecha a fogueira e prende o rato
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

    // Opcional: Desenha a esfera do SphereCast no Editor para te ajudar a ajustar o tamanho
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