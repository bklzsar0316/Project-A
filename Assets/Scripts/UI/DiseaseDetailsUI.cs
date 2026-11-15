using TMPro;
using Unity.InferenceEngine;
using UnityEngine;

public class DiseaseDetailsUI : MonoBehaviour
{

    [SerializeField] private TMP_Text diseaseName;
    [SerializeField] private TMP_Text treatmentSteps;
    [SerializeField] private GameObject resultsList;
    [SerializeField] private PatientSequenceController patientQueue;

    private SO_DiseaseData _selectedDisease;

    public void ShowDisease(SO_DiseaseData disease)
    {
        if (disease)
        {
            _selectedDisease = disease;
            diseaseName.text = disease.name;
            treatmentSteps.text = "";

            for (int i =0; i <  disease.TreatmentSteps.Length; i++)
            {
                SO_TreatmentData item = disease.TreatmentSteps[i];
                string itemName = item != null ? item.ItemName : "Tanimsiz";
                
                treatmentSteps.text += $"{i + 1}. {itemName}\n";
            }
            resultsList.SetActive(false);
            gameObject.SetActive(true);
        }
        
    }

    public void Back()
    {
        resultsList.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ConfirmButton()
    {
        if (_selectedDisease)
        {
            bool success = patientQueue.TrySetDiagnosis(_selectedDisease);


            if (!success)
            {
                return;
            }
            
            Debug.Log("Teshis Secildi");
        }
    }
}
