using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SymptomItemUI : MonoBehaviour
{

    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text symptomNameText;
    
    private SO_SymptomData _symptomData;
    
    public string DisplayName => symptomNameText.text;


    public void Setup(SO_SymptomData symptom, Action<SO_SymptomData, bool> onChanged)
    {
        _symptomData = symptom;
        symptomNameText.text = symptom.SymptomName;
        
        toggle.SetIsOnWithoutNotify(false);
        
        toggle.onValueChanged.AddListener(isOn =>
        {
            onChanged?.Invoke(_symptomData, isOn);
        });
    }
}
