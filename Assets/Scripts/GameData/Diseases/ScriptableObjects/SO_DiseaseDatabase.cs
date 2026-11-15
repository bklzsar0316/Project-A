using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_DiseaseDatabase", menuName = "Scriptable Objects/SO_DiseaseDatabase")]
public class SO_DiseaseDatabase : ScriptableObject
{ 
    [SerializeField] private SO_DiseaseData[] diseaseDatas;

    public SO_DiseaseData[] DiseaseDatas => diseaseDatas;



}
