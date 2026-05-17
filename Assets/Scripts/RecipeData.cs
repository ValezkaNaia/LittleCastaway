using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ingrediente
{
    public ItemData item;
    public int quantidade;
}

[CreateAssetMenu(fileName = "Nova Receita", menuName = "Inventario/Receita")]
public class RecipeData : ScriptableObject
{
    public string nomeReceita;
    public List<Ingrediente> ingredientesRequired = new List<Ingrediente>();
    public ItemData itemResultado;
    public int quantidadeResultado = 1;
}
