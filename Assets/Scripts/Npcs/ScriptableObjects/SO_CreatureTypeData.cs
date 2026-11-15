using UnityEngine;

[CreateAssetMenu(fileName = "SO_CreatureTypeData", menuName = "Scriptable Objects/CreatureType")]
public class SO_CreatureTypeData : ScriptableObject
{

    [SerializeField] private string creatureName;
    [SerializeField] private string creatureDescription;
    
    public string CreatureName => creatureName;
    public string CreatureDescription => creatureDescription;
}
