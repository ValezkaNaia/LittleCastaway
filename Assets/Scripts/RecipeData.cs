using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ingrediente
{
    public ItemData item;
    public int quantidade;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
public class RecipeData : ScriptableObject
{
    public string nomeReceita;
    public List<Ingrediente> ingredientesRequired = new List<Ingrediente>();
    public ItemData itemResultado;
    public int quantidadeResultado = 1;
}
