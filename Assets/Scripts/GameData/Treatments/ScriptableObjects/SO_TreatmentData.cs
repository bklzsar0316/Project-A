using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Treatment Item")] 
public class SO_TreatmentData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject itemPrefab;
    
    public string ItemName =>itemName;
    public Sprite Icon => icon;
    public GameObject ItemPrefab => itemPrefab;
}
