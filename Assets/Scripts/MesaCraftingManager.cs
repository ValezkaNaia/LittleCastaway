using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MesaCraftingManager : MonoBehaviour
{
    public static MesaCraftingManager instance;

    [Header("Slots da Interface")]
    public List<CraftingSlotUI> slotsIngredientes = new List<CraftingSlotUI>(); // Devem ser exatamente 4
    public CraftingSlotUI slotResultado;
    public Button botaoCraftear;
    public GameObject craftingPanel;

    [Header("Lista de Receitas do Jogo")]
    public List<RecipeData> todasAsReceitas = new List<RecipeData>();

    private RecipeData receitaAtiva = null;
    private InventoryManager inventory;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Força o painel do Crafting a começar fechado
        if (craftingPanel != null) craftingPanel.SetActive(false);

        inventory = Object.FindFirstObjectByType<InventoryManager>();
        botaoCraftear.interactable = false;
        botaoCraftear.onClick.AddListener(ExecutarCrafting);
    }

    // Chamado pelo InventoryUI quando o jogador clica num item para enviar para o Crafting
    public bool AdicionarIngredienteAMesa(ItemData item)
    {
        // 1. Tenta encontrar se o item já está num slot para somar a quantidade
        foreach (var slot in slotsIngredientes)
        {
            if (slot.GetItem() == item && item.isStackable)
            {
                slot.SetupSlot(item, slot.GetQuantity() + 1);
                VerificarReceitas();
                return true;
            }
        }

        // 2. Se não encontrou, mete no primeiro slot totalmente vazio
        foreach (var slot in slotsIngredientes)
        {
            if (slot.GetItem() == null)
            {
                slot.SetupSlot(item, 1);
                VerificarReceitas();
                return true;
            }
        }

        Debug.Log("Mesa de Crafting cheia! Não podes meter mais tipos de ingredientes.");
        return false;
    }

    // Remove o item da mesa e devolve ao inventário se o jogador clicar no slot da mesa
    public void ClicouNoSlotDaMesa(int index)
    {
        if (index < 0 || index >= slotsIngredientes.Count) return;
        
        CraftingSlotUI slot = slotsIngredientes[index];
        ItemData item = slot.GetItem();

        if (item != null)
        {
            inventory.AddItemDoMenu(item); // Devolve sem mandar para a hotbar
            
            int novaQtd = slot.GetQuantity() - 1;
            if (novaQtd <= 0)
                slot.ClearSlot();
            else
                slot.SetupSlot(item, novaQtd);

            // Atualiza as UIs
            Object.FindFirstObjectByType<InventoryUI>().AtualizarUI();
            VerificarReceitas();
        }
    }

    // Varre as receitas criadas para ver se bate certo com o que está na mesa
    public void VerificarReceitas()
    {
        // --- TESTE DE FORÇA EM CASO DE EMERGÊNCIA ---
        // Vamos ignorar as receitas e ver se a UI do resultado acorda!
        /*if (todasAsReceitas.Count > 0 && todasAsReceitas[0] != null)
        {
            receitaAtiva = todasAsReceitas[0]; // Pega na primeira receita da tua lista
            slotResultado.SetupSlot(receitaAtiva.itemResultado, receitaAtiva.quantidadeResultado);
            botaoCraftear.interactable = true;
            Debug.Log("TESTE: Forçámos a UI a mostrar o item: " + receitaAtiva.itemResultado.itemName);
            return; // Para o código aqui e ignora o resto do script!
        }
        // --------------------------------------------*/

        receitaAtiva = null;
        slotResultado.ClearSlot();
        botaoCraftear.interactable = false;

        // Cria um dicionário unificado para somar tudo o que está na mesa por tipo
        Dictionary<ItemData, int> itensNaMesa = new Dictionary<ItemData, int>();
        foreach (var slot in slotsIngredientes)
        {
            ItemData itemNoSlot = slot.GetItem();
            if (itemNoSlot != null)
            {
                if (itensNaMesa.ContainsKey(itemNoSlot))
                {
                    itensNaMesa[itemNoSlot] += slot.GetQuantity();
                }
                else
                {
                    itensNaMesa.Add(itemNoSlot, slot.GetQuantity());
                }
            }
        }

        if (itensNaMesa.Count == 0) return;

        // --- LOGS DE TESTE ---
        Debug.Log("--- ITENS ATUAIS NA MESA ---");
        foreach(var par in itensNaMesa)
        {
            Debug.Log("Item na Mesa: " + par.Key.itemName + " | Quantidade: " + par.Value);
        }
        Debug.Log("----------------------------");

        // Compara com cada receita configurada no teu projeto
        foreach (RecipeData receita in todasAsReceitas)
        {
            bool receitaCorreta = true;

            // VERIFICAÇÃO FLEXÍVEL: Garante que a mesa tem pelo menos os ingredientes que a receita pede
            foreach (Ingrediente ing in receita.ingredientesRequired)
            {
                // Se o ingrediente não existe na mesa OU se a quantidade na mesa é MENOR do que a receita pede
                if (!itensNaMesa.ContainsKey(ing.item) || itensNaMesa[ing.item] < ing.quantidade)
                {
                    receitaCorreta = false;
                    break;
                }
            }

            if (receitaCorreta)
            {
                receitaAtiva = receita;
                break; // Encontrou a receita válida, pode parar o loop
            }
        }

        // Se encontrou uma receita válida, liberta a pré-visualização e ativa o clique!
        if (receitaAtiva != null)
        {
            Debug.Log("RECEITA DETETADA COM SUCESSO: " + receitaAtiva.itemResultado.itemName);
            slotResultado.SetupSlot(receitaAtiva.itemResultado, receitaAtiva.quantidadeResultado);
            botaoCraftear.interactable = true;
        }
    }

    void ExecutarCrafting()
    {
        if (receitaAtiva == null) return;

        Debug.Log("A tentar fabricar o item: " + receitaAtiva.itemResultado.itemName);

        // 1. Adiciona o resultado respeitando a quantidade configurada na receita
        for (int i = 0; i < receitaAtiva.quantidadeResultado; i++)
        {
            inventory.AddItem(receitaAtiva.itemResultado);
        }

        // 2. AVISA A HOTBAR E LIMPA OS SLOTS UTILIZADOS NA MESA DE CRAFTING
        foreach (var slot in slotsIngredientes)
        {
            ItemData ingredienteConsumido = slot.GetItem();

            // Se o slot tinha um item válido, avisa o HotbarManager para o remover caso estivesse equipado
            if (ingredienteConsumido != null && HotbarManager.instance != null)
            {
                HotbarManager.instance.RemoverItemGasto(ingredienteConsumido);
            }

            // Agora sim, limpa o slot da mesa com segurança
            slot.ClearSlot();
        }
        
        slotResultado.ClearSlot();
        botaoCraftear.interactable = false;

        // 3. ATUALIZAÇÃO VISUAL OBRIGATÓRIA DOS DOIS LADOS: 
        InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
        if (invUI != null) 
        {
            invUI.AtualizarUI();
        }

        receitaAtiva = null;
        
        Debug.Log("Item fabricado com sucesso e adicionado ao inventário!");
    }

  /*void ExecutarCrafting()
    {
        if (receitaAtiva == null) return;

        Debug.Log("A tentar fabricar o item: " + receitaAtiva.itemResultado.itemName);

        // 1. Adiciona o resultado respeitando a quantidade configurada na receita
        for (int i = 0; i < receitaAtiva.quantidadeResultado; i++)
        {
            inventory.AddItem(receitaAtiva.itemResultado);
        }

        // 2. Limpa os slots utilizados na mesa de crafting
        foreach (var slot in slotsIngredientes)
        {
            slot.ClearSlot();
        }
        slotResultado.ClearSlot();
        botaoCraftear.interactable = false;


        // 3. ATUALIZAÇÃO VISUAL OBRIGATÓRIA DOS DOIS LADOS:
        // Atualiza a UI do inventário principal para mostrar o novo Machado e remover as madeiras
        InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
        if (invUI != null) 
        {
            invUI.AtualizarUI();
        }

        receitaAtiva = null;
        
        Debug.Log("Item fabricado com sucesso e adicionado ao inventário!");
    }*/

    
    
    // Funções para ligar à abertura automática do painel
    public void AbrirMesa() => craftingPanel.SetActive(true);
    public void FecharMesa() 
    {
        // Devolve tudo o que ficou esquecido na mesa para o inventário ao fechar
        foreach (var slot in slotsIngredientes)
        {
            if (slot.GetItem() != null)
            {
                for (int i = 0; i < slot.GetQuantity(); i++)
                {
                    inventory.AddItemDoMenu(slot.GetItem()); // Devolve sem mandar para a hotbar
                }
                slot.ClearSlot();
            }
        }
        slotResultado.ClearSlot();
        craftingPanel.SetActive(false);
    }

    
}