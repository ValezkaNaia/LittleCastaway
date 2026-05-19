using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações Globais")]
    public float interactionRange = 5f;
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

        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;

        Debug.DrawRay(cam.position, cam.forward * interactionRange, Color.red);

        bool olhouParaAlgoInterativo = false;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // =================================================================
            // 1. APANHAR ITENS DO CHÃO
            // =================================================================
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            if (item != null)
            {
                olhouParaAlgoInterativo = true;
                
                // Limpa o "(Clone)" e os espaços do nome que está na Hierarchy
                string nomeDoItem = hit.collider.gameObject.name.Replace("(Clone)", "").Trim();
                
                DefinirTexto("Pick up " + item.referenciaItem.itemName + " [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    item.SerApanhado();
                    EsconderTexto();
                }
            }

            // =================================================================
            // 2. LER NOTAS
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
            // 3. INTERAÇÃO COM ANIMAIS (ATACAR OU DOMESTICAR)
            // =================================================================
            else if (hit.collider.CompareTag("Animal"))
            {
                AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                if (animal != null && animal.vida > 0)
                {
                    olhouParaAlgoInterativo = true;

                    // Verifica se é um animal de estimação (cão/gato)
                    if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && !animal.domesticado)
                    {
                        DefinirTexto("Press [F] to Give Apple");
                    }
                    else if (animal.tipoAnimal == AnimalAI.Comportamento.Pet && animal.domesticado)
                    {
                        DefinirTexto("Loyal Companion");
                    }
                    else // Se for predador ou presa normal, usa o sistema da Lança
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
        }

        if (!olhouParaAlgoInterativo)
        {
            EsconderTexto();
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
}