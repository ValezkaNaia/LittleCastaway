using UnityEngine;
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
}
