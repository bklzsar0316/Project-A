
using System.Collections.Generic;

public class PatientSession 
{
    private readonly SO_PatientCaseData _caseData;
    public SO_PatientCaseData  CaseData => _caseData;
    public PatientCaseState State { get; private set; } = PatientCaseState.Waiting;
    public SO_DiseaseData SelectedDiagnosis { get; private set; }
    public PatientDecision  Decision { get; private set; } = PatientDecision.Undecided;
    private List<SO_TreatmentData> _performedSteps = new();
    public IReadOnlyList<SO_TreatmentData> PerformedSteps => _performedSteps;
    public TreatmentResult TreatmentResult {get; private set;}
    public PatientSession(SO_PatientCaseData caseData)
    {
        _caseData = caseData;
    }

    public void StartCase()
    {
        if (State != PatientCaseState.Waiting)
        {
            return;
        }
        
        State = PatientCaseState.InProgress;
    }

    public void CompleteCase()
    {
        if (State == PatientCaseState.InProgress)
        {
            State = PatientCaseState.Completed;
        }
    }

    public void SetDiagnosis(SO_DiseaseData disease)
    {
        if(_performedSteps.Count > 0 )return;

        if(disease == null) return;

        
        
        if (State == PatientCaseState.InProgress)
        {
            SelectedDiagnosis = disease;
        }
        
    }

    public void SetDecision(PatientDecision decision)
    {
        if(_performedSteps.Count > 0 )return;
        
        if (State == PatientCaseState.InProgress)
        {
            if (decision != PatientDecision.Undecided)
            {
                Decision = decision;
            }
        }
    }


    public bool SavePerformedSteps(IReadOnlyList<SO_TreatmentData> slots)
    {
        if(State == PatientCaseState.InProgress 
           && Decision == PatientDecision.Treat
           && SelectedDiagnosis != null
           && _performedSteps.Count == 0 
           && slots != null 
           && slots.Count > 0)
        {
            _performedSteps.AddRange(slots);
            return true;
        }

        return false;
    }
    
    public void SaveTreatmentResult(TreatmentResult treatmentResult)
    {
        if (treatmentResult != null && State == PatientCaseState.InProgress && Decision == PatientDecision.Treat && TreatmentResult == null)
        {
            TreatmentResult = treatmentResult;
        }
    }


}

public enum PatientDecision
{
    Undecided,
    Treat,
    Reject
}


