using System;
using System.Collections.Generic;
using UnityEngine;

public class DiseaseResultsUI : MonoBehaviour
{
    [SerializeField] private DiseaseResultItem resultPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private DiseaseSearchController searchController;
    [SerializeField] private DiseaseDetailsUI detailsUI;


    public void ShowResults(IReadOnlyList<DiseaseSearchResult> results)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (DiseaseSearchResult result in results)
        {
            DiseaseResultItem item =  Instantiate(resultPrefab, content);
            item.Setup(result,detailsUI.ShowDisease);
            
        }
    }

    public void OnSearchClicked()
    {
        var results = searchController.Search();
        
        ShowResults(results);
    }
}
