using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SymptomsListUI : MonoBehaviour
{
    [SerializeField] private SO_DiseaseDatabase database;
    [SerializeField] private DiseaseSearchController searchController;

    [SerializeField] private SymptomItemUI symptomPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private TMP_InputField searchInput;

    private readonly List<SymptomItemUI> _items = new();

    private void Start()
    {
        CreateSymptomItems();

        searchInput.onValueChanged.AddListener(FilterSymptoms);
    }

    private void OnDestroy()
    {
        if (searchInput != null)
            searchInput.onValueChanged.RemoveListener(FilterSymptoms);
    }

    private void CreateSymptomItems()
    {
        HashSet<SO_SymptomData> addedSymptoms = new();

        foreach (SO_DiseaseData disease in database.DiseaseDatas)
        {
            if (disease == null) continue;

            foreach (SO_SymptomData symptom in disease.SymptomList)
            {
                if (symptom == null) continue;

                if (!addedSymptoms.Add(symptom))
                    continue;

                SymptomItemUI item =
                    Instantiate(symptomPrefab, content);

                item.Setup(symptom, OnSymptomChanged);

                _items.Add(item);
            }
        }
    }

    private void OnSymptomChanged(
        SO_SymptomData symptom, bool isSelected)
    {
        if (isSelected)
        {
            searchController.SelectSymptom(symptom);
        }
        else
        {
            searchController.DeselectSymptom(symptom);
        }

    }

    private void FilterSymptoms(string searchText)
    {
        foreach (SymptomItemUI item in _items)
        {
            bool matches =
                item.DisplayName.IndexOf(
                    searchText,
                    StringComparison.OrdinalIgnoreCase) >= 0;

            item.gameObject.SetActive(matches);
        }
    }
}