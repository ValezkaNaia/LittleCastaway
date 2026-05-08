using UnityEngine;

[CreateAssetMenu(fileName = "Novo Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject prefabModel; //modelo 3D do item para quando este seja lançado no chão
    public bool isStackable; //indica se pode ter mais de um item no mesmo slot do inventário
    
}
