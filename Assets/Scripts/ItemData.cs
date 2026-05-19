using UnityEngine;

[CreateAssetMenu(fileName = "Novo Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identificação")]
    public string itemName;
    public Sprite itemIcon;
    public GameObject prefabModel; // modelo 3D do item para quando este seja lançado no chão ou equipado
    public bool isStackable;       // indica se pode ter mais de um item no mesmo slot do inventário

    [Header("Configurações de Consumível")]
    public bool isConsumable;      // Marcar como verdadeiro se for comida/bebida
    public float pontosRestauracao; // Quantidade de fome ou sede que recupera

    public enum TipoConsumivel { Comida, Agua }
    public TipoConsumivel tipoConsumivel;

    [Header("Posição na Mão (quando equipado)")]
    [Tooltip("Offset de posição relativo ao transform da mão (HandTransform).")]
    public Vector3 holdOffset  = Vector3.zero;
    [Tooltip("Rotação local do modelo quando está na mão (em Euler).")]
    public Vector3 holdRotation = Vector3.zero;
    [Tooltip("Escala do modelo na mão. Deixa (0,0,0) para usar escala original.")]
    public Vector3 holdScale    = Vector3.zero;
}
/*using UnityEngine;

[CreateAssetMenu(fileName = "Novo Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identificação")]
    public string itemName;
    public Sprite itemIcon;
    public GameObject prefabModel; // modelo 3D do item para quando este seja lançado no chão
    public bool isStackable;       // indica se pode ter mais de um item no mesmo slot do inventário

    [Header("Posição na Mão (quando equipado)")]
    [Tooltip("Offset de posição relativo ao transform da mão (HandTransform).")]
    public Vector3 holdOffset  = Vector3.zero;
    [Tooltip("Rotação local do modelo quando está na mão (em Euler).")]
    public Vector3 holdRotation = Vector3.zero;
    [Tooltip("Escala do modelo na mão. Deixa (0,0,0) para usar escala original.")]
    public Vector3 holdScale    = Vector3.zero;
}*/

