using System;
using UnityEngine;

public class CaseConsequenceHandler : MonoBehaviour
{

    private float _humanRejectPenalty;
    private float _maximumTreatmentPenalty;
    private float _wrongDiagnosisMultiplier;


    private void OnEnable()
    {
        EventBus.Subscribe<PatientCaseCompletedEvent>(OnPatientCaseCompleted);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<PatientCaseCompletedEvent>(OnPatientCaseCompleted);
    }
    
    
    private void OnPatientCaseCompleted(PatientCaseCompletedEvent obj)
    {
        
        PatientSession session = obj.Session;
        
        
        if (session.CaseData.PatientIdentity == PatientIdentity.Monster)
        {
            if (session.Decision == PatientDecision.Treat)
            {
                Debug.Log("Canavari Tedavi ettin sonucunu bitince goreceksin");
            }
            else
            {
                Debug.Log("Canavari Kovdun bravo");
            }
            return;
        }
        
        if (session.Decision == PatientDecision.Reject)
        {
            PlayerStats.Instance.Sanity.DecreaseSanity(_humanRejectPenalty);
            return;
        }

        TreatmentResult result = session.TreatmentResult;
        if (session.Decision == PatientDecision.Treat && result != null)
        {
            float penalty = _maximumTreatmentPenalty * result.MistakeRate;

            if (!result.IsCorrectDiagnosis)
            {
                penalty *= _wrongDiagnosisMultiplier;
            }
            
            PlayerStats.Instance.Sanity.DecreaseSanity(penalty);
        }
    }

    private void Start()
    {
        if (PlayerStats.Instance)
        {
            _humanRejectPenalty = PlayerStats.Instance.GetHumanRejectPenalty();
            _maximumTreatmentPenalty =  PlayerStats.Instance.GetMaximumTreatmentPenalty();
            _wrongDiagnosisMultiplier = PlayerStats.Instance.GetWrongDiagnosisMultiplier();
        }
    }
}
