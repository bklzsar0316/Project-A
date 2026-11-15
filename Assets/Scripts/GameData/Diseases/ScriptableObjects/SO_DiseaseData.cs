using UnityEngine;
    [CreateAssetMenu(fileName = "DiseaseData",menuName = "Scriptable Objects/Disease Data")]
    public class SO_DiseaseData : ScriptableObject
    {

        [SerializeField] private string disease;
        [SerializeField] private SO_SymptomData[] symptoms;
        [SerializeField] private SO_TreatmentData[] treatmentSteps;

        public string DiseaseName => disease;
        public SO_SymptomData[] SymptomList => symptoms;
        public SO_TreatmentData[] TreatmentSteps => treatmentSteps;
    }

