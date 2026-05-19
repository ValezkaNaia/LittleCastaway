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
        // Garante que o laser segue rigorosamente para onde o jogador olha com a câmara
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (NoteManager.isReading) return;

        // Dispara o laser a partir dos olhos (Câmara) e não da base do jogador
        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;

        Debug.DrawRay(cam.position, cam.forward * interactionRange, Color.red);

        bool olhouParaAlgoInterativo = false;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // =================================================================
            // 1. APANHAR ITENS DO CHÃO (Sistema Antigo - Tecla F)
            // =================================================================
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            if (item != null)
            {
                olhouParaAlgoInterativo = true;
                DefinirTexto("Pick up " + hit.collider.gameObject.name + " [F]");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    item.SerApanhado();
                    EsconderTexto();
                }
            }

            // =================================================================
            // 2. LER NOTAS (Sistema Antigo - Tecla F)
            // =================================================================
            else if (hit.collider.CompareTag("Animal"))
            {
                AnimalAI animal = hit.collider.GetComponent<AnimalAI>();
                if (animal != null && animal.vida > 0)
                {
                    olhouParaAlgoInterativo = true;
                    FerramentaAtaque armaEquipada = GetComponentInChildren<FerramentaAtaque>();

                    if (armaEquipada != null && armaEquipada.ehLanca)
                    {
                        DefinirTexto("Premir [E] para Atacar com a Lança");
                        if (Keyboard.current.eKey.wasPressedThisFrame)
                        {
                            armaEquipada.JogarAnimacaoGatilho();
                            animal.ReceberDano(danoLanca);
                        }
                    }
                    else
                    {
                        DefinirTexto("Precisas de equipar a Lança!");
                    }
                }
            }

            // =================================================================
            // 4. CORTAR ÁRVORES DE MADEIRA (Sistema Novo - Tecla E)
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
                        DefinirTexto("Premir [E] para Cortar Árvore");
                        if (Keyboard.current.eKey.wasPressedThisFrame)
                        {
                            armaEquipada.JogarAnimacaoGatilho();
                            arvore.LevarMachadada();
                        }
                    }
                    else
                    {
                        DefinirTexto("Precisas de equipar o Machado!");
                    }
                }
            }

            // =================================================================
            // 5. APANHAR FRUTA DAS ÁRVORES (Sistema Novo - Tecla E)
            // =================================================================
            else if (hit.collider.CompareTag("ArvoreFruta"))
            {
                ArvoreFruta arvoreFruta = hit.collider.GetComponent<ArvoreFruta>();
                if (arvoreFruta != null)
                {
                    olhouParaAlgoInterativo = true;

                    if (arvoreFruta.TemFruta())
                    {
                        DefinirTexto("Premir [E] para Apanhar Fruta");

                        if (Keyboard.current.eKey.wasPressedThisFrame)
                        {
                            arvoreFruta.ApanharFruta();
                            
                            // Mostra um feedback imediato no ecrã de que foi guardado
                            if (arvoreFruta.itemFruta != null)
                            {
                                DefinirTexto(arvoreFruta.itemFruta.itemName + " guardado no inventário!");
                            }
                        }
                    }
                    else
                    {
                        // Se a árvore já foi colhida, mostra esta mensagem em vez de pedir para carregar no E
                        DefinirTexto("Esta árvore já não tem mais fruta.");
                    }
                }
            }
        }

        // Se o jogador não estiver a olhar para nada válido, desativa o texto na hora
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
/*using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 5f;
    public TextMeshProUGUI textoInteracao;

    void Update()
    {
        // Se estiveres a ler uma nota ou no menu de coleção, não podes interagir com o mundo!
        if (NoteManager.isReading) return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward * interactionRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            WorldNote nota = hit.collider.GetComponent<WorldNote>();

            if (item != null)
            {
                if (textoInteracao != null) 
                {
                    textoInteracao.text = "Pick up " + hit.collider.gameObject.name + " [F]";
                    textoInteracao.gameObject.SetActive(true);
                }

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    item.SerApanhado();
                    if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
                }
            }
            else if (nota != null) 
            {
                if (textoInteracao != null) 
                {
                    textoInteracao.text = "Read " + hit.collider.gameObject.name + " [F]";
                    textoInteracao.gameObject.SetActive(true);
                }

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    nota.LerNota();
                    if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
                }
            }
            else
            {
                if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
            }
        }
        else
        {
            if (textoInteracao != null) textoInteracao.gameObject.SetActive(false);
        }
    }
}*/