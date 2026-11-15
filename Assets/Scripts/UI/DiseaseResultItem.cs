using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiseaseResultItem : MonoBehaviour
{

    [SerializeField] private TMP_Text diseaseNameText;
    [SerializeField] private TMP_Text matchCountText;
    [SerializeField] private Button button;

    public void Setup(DiseaseSearchResult result,Action<SO_DiseaseData> onSelected)
    {
        diseaseNameText.text = result.Disease.DiseaseName;
        matchCountText.text = result.MatchCount + " Belirti Eslesti";

        button.onClick.AddListener(() =>
        {
            onSelected?.Invoke(result.Disease);
        });

    }
    
}
