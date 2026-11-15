using System.Collections.Generic;
using UnityEngine;

public class SymptomSelection 
{
    private List<SO_SymptomData> selectedSymptoms = new List<SO_SymptomData>();
    public IReadOnlyList<SO_SymptomData> SelectedSymptoms => selectedSymptoms;



    public void AddSymptom(SO_SymptomData symptom)
    {
        if (symptom == null)
        {
            Debug.Log("Bu butona semptom referansi eklemedin");
            return;
        }


        if (selectedSymptoms.Contains(symptom))
        {
            return;
        }
        
        selectedSymptoms.Add(symptom);

    }

    public void RemoveSymptom(SO_SymptomData symptom)
    {
        
        if (selectedSymptoms.Contains(symptom))
        {
            selectedSymptoms.Remove(symptom);
        }
        
    }

    public void ClearSelection()
    {
        selectedSymptoms.Clear();
    }
}
