using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_DayData", menuName = "Scriptable Objects/DayData")]
public class SO_DayData : ScriptableObject
{
    
    [SerializeField] private int dayNumber;
    [SerializeField] private SO_PatientCaseData[] patientCases;
    
    
    public int DayNumber => dayNumber;
    public IReadOnlyList<SO_PatientCaseData> PatientCases => patientCases;
}
