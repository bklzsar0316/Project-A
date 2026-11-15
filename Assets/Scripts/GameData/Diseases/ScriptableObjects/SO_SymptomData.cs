using UnityEngine;


    [CreateAssetMenu(fileName = "SymptomData", menuName = "Scriptable Objects/SymptomData")]
    public class SO_SymptomData : ScriptableObject
    {
        [SerializeField] private string symptomName;
        public string SymptomName => symptomName;
    }

