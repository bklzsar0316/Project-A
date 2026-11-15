
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DiseaseSearchController : MonoBehaviour
{

    [SerializeField] private SO_DiseaseDatabase database;
    

    private SearchDiseases _search;
    private SymptomSelection _selection;

    private void Awake()
    {

        if (database == null)
        {
            Debug.Log("Database Atamadin");

        }
        else
        {
            
            _selection = new SymptomSelection();
            _search = new SearchDiseases(database);
        }
    }

    public void SelectSymptom(SO_SymptomData symptom)
    {
        _selection.AddSymptom(symptom);
    }
    
    public void DeselectSymptom(SO_SymptomData symptom)
    {
        _selection.RemoveSymptom(symptom);
    }

    
    public List<DiseaseSearchResult> Search()
    {

        List<DiseaseSearchResult> result = _search.FindMatchingDiseases(_selection.SelectedSymptoms);
        
        return result;

    }

   
}
