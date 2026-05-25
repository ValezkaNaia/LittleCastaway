using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventario; 
    public Transform grelhaSlots;      
    public GameObject inventoryPanel; 

    private List<InventorySlot> slotsInstanciados = new List<InventorySlot>();

    void Start()
    {
        // Recolhe todos os slots fixos na hierarquia
        foreach (Transform filho in grelhaSlots)
        {
            InventorySlot slot = filho.GetComponent<InventorySlot>();
            if (slot != null)
            {
                slotsInstanciados.Add(slot);
                slot.DefineItem(null, 0); // Limpa o slot ao começar
            }
        }

        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // ==========================================================
        // NOVO: Bloqueia o inventário se estiver a ler o jornal ou a dar nome ao pet!
        if (NoteManager.isReading || PetNamingManager.isNaming) return;
        // ==========================================================

        if (Input.GetKeyDown(KeyCode.I))
        {
            bool inverterEstado = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(inverterEstado);

            if (inverterEstado == true) // O INVENTÁRIO ABRIU!
            {
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;                  
                
                if (MesaCraftingManager.instance != null) 
                    MesaCraftingManager.instance.AbrirMesa();
            }
            else // O INVENTÁRIO FECHOU!
            {
                Cursor.lockState = CursorLockMode.Locked; 
                Cursor.visible = false;      

                // LINHA DE SEGURANÇA: Limpa o foco do EventSystem para o inventário receber cliques frescos
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                }             
                
                if (MesaCraftingManager.instance != null) 
                    MesaCraftingManager.instance.FecharMesa();
            }

            AtualizarUI();
        }
    }

    public void AtualizarUI()
    {
        if (inventario == null || slotsInstanciados.Count == 0) return;

        // Distribui os itens da lista acumulada pelos slots visuais
        for (int i = 0; i < slotsInstanciados.Count; i++)
        {
            if (i < inventario.items.Count)
            {
                // Extrai as variáveis da classe intermédia do teu InventoryManager
                ItemAcumulado dadosDoSlot = inventario.items[i];
                
                // Passa os dados separados para o Slot Visual
                slotsInstanciados[i].DefineItem(dadosDoSlot.item, dadosDoSlot.quantidade);
            }
            else
            {
                slotsInstanciados[i].DefineItem(null, 0); // Limpa os slots restantes
            }
        }
    }

    // =================================================================
    // NOVOS MÉTODOS: CHAMADOS PELA FOGUEIRA
    // =================================================================
    
    // Força a abertura do inventário de forma limpa
    public void AbrirInventarioExterno()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            AtualizarUI();
        }
    }

    // Força o fecho do inventário de forma limpa
    public void FecharInventarioExterno()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}
/*using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventario; 
    public GameObject slotPrefab;      
    public Transform grelhaSlots;      
    public GameObject inventoryPanel; // ARRASTA O PAINEL PARA AQUI

    void Start()
    {
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool estaAtivo = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(estaAtivo);

            if (estaAtivo)
            {
                AtualizarUI();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void AtualizarUI()
    {
        foreach (Transform filho in grelhaSlots)
        {
            Destroy(filho.gameObject);
        }

        foreach (ItemData item in inventario.items)
        {
            GameObject novoSlot = Instantiate(slotPrefab, grelhaSlots);
            // AJUSTE: Usa o nome exato da função que criámos no InventorySlot (DefineItem ou DefinirItem)
            novoSlot.GetComponent<InventorySlot>().DefineItem(item);
        }
    }
}*/