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
