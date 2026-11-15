
using System.Collections.Generic;
using System.Linq;

public class SearchDiseases
{
    private readonly SO_DiseaseDatabase _diseaseDatabase; 
    
    public SearchDiseases(SO_DiseaseDatabase diseaseDatabase)
    {
        _diseaseDatabase = diseaseDatabase;
    }


    public List<DiseaseSearchResult> FindMatchingDiseases(IReadOnlyList<SO_SymptomData> selectedSymptoms)
    {
        List<DiseaseSearchResult> _resultList = new List<DiseaseSearchResult>();

        foreach (SO_DiseaseData disease in _diseaseDatabase.DiseaseDatas)
        {
            int matchCount = FindSymptomps(disease,selectedSymptoms);
            if (matchCount > 0)
            { 
                
                DiseaseSearchResult result = new DiseaseSearchResult(disease, matchCount); 
                _resultList.Add(result);
            }
        }

        
        _resultList.Sort((a, b) => b.MatchCount.CompareTo(a.MatchCount));
        return _resultList;
    }

    private int FindSymptomps(SO_DiseaseData disease, IReadOnlyList<SO_SymptomData> selectedSymptoms)
    {

        int matchingSymptomps = 0;

        foreach (SO_SymptomData symptom in selectedSymptoms)
        {
            if (disease.SymptomList.Contains(symptom))
            {
                matchingSymptomps++;
            }
        }
        
        return matchingSymptomps;
    }
}

