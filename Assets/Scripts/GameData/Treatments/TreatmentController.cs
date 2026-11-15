using UnityEngine;

public class TreatmentController : MonoBehaviour
{
    [SerializeField] private PatientSequenceController patientQueue;
    private TreatmentEvaluator _evaluator;

    private TreatmentItem _selectedItem;
    private TreatmentItem[] _assignedItems;
    private PatientSession _preparedSession;
    private const int SlotCapacity = 9;
    
    public TreatmentItem  SelectedItem => _selectedItem;
    public int SlotCount => _assignedItems?.Length ?? 0;

    
    
    
    [SerializeField] private float requiredHoldTime = 5f;
    private float _holdTime = 0f;
    
    
    

    private void Awake()
    {
        _evaluator = new TreatmentEvaluator();
    }

    public bool PrepareTreatment()
    {
        PatientSession session = patientQueue.CurrentSession;
        
        
        if (session == null ||
            session.State != PatientCaseState.InProgress ||
            session.Decision != PatientDecision.Treat ||
            session.SelectedDiagnosis == null ||
            session.TreatmentResult != null ||
            session.PerformedSteps.Count > 0)
        {
            return false;
        }
        
        int stepCount = session.SelectedDiagnosis.TreatmentSteps.Length;

        if (stepCount < 1 || stepCount > 9)
        {
            return false;
        }

        _assignedItems = new TreatmentItem[SlotCapacity];
        _selectedItem = null;
        _preparedSession = session;
        
        return true;

    }


    public void SelectItem(TreatmentItem  item)
    {
        if(item == null)
        {
            return;
        }

        if (!IsPreparationValid())
        {
            if (!PrepareTreatment())
            {
                return;
            }
        }
        
        _selectedItem = item;

        TryAddItem(_selectedItem);
    }

    private bool TryAddItem(TreatmentItem item)
    {
        if (item == null)
        {
            return false;
        }

        if (IsPreparationValid())
        {
            for (int i = 0; i < _assignedItems.Length; i++)
            {

                if (_assignedItems[i] == null)
                {
                    _assignedItems[i] = item;
                    item.gameObject.SetActive(false);
                    return true;
                }
            }
        }

        return false;
    }
    public bool TryAssignItem(int slotIndex)
    {
        if (!IsPreparationValid() || _selectedItem == null)
        {
            return false;
        }
        if(slotIndex < 0 || slotIndex >= _assignedItems.Length)
        {
            return false;
        }
        
        _assignedItems[slotIndex] = _selectedItem;
        return true;
    }

    public bool TryRemoveItem(int slotIndex)
    {
        if (IsPreparationValid())
        {
            if (slotIndex < 0 || slotIndex >= _assignedItems.Length)
            {
                return false;
            }

            if (_assignedItems[slotIndex] == null)
            {
                return false;
                
            }
            TreatmentItem treatmentItem = _assignedItems[slotIndex];
            _assignedItems[slotIndex] = null;
            treatmentItem.ReturnToTable();
            return true;
        }

        return false;
    }
    
    public bool TryMoveItem(int fromIndex, int toIndex)
    {

        if (IsPreparationValid())
        {
            if (fromIndex < 0 || fromIndex >= _assignedItems.Length || toIndex < 0 || toIndex >= _assignedItems.Length)
            {
                return false;
            }

            if (_assignedItems[fromIndex] == null || fromIndex == toIndex)
            {
                return false;
            }
            
            TreatmentItem itemToMove = _assignedItems[fromIndex];
            if (fromIndex < toIndex)
            {
                for (int i = fromIndex; i < toIndex; i++)
                {
                    _assignedItems[i] = _assignedItems[i + 1];
                }
            }
            else
            {
                for (int i = fromIndex; i > toIndex; i--)
                {
                    _assignedItems[i] = _assignedItems[i - 1];
                }
            }
            _assignedItems[toIndex] = itemToMove;
            return true;
        }

        return false;
    }
    
    public TreatmentItem GetSelectedItem(int slotIndex)
    {
        if(_assignedItems == null ||
           slotIndex < 0 ||
           slotIndex >= _assignedItems.Length)
        {
            return null;
        }
        return _assignedItems[slotIndex];
    }
    private bool IsPreparationValid()
    {
        PatientSession session = patientQueue.CurrentSession;

        return session != null
               && session == _preparedSession
               && session.State == PatientCaseState.InProgress
               && session.Decision == PatientDecision.Treat
               && session.SelectedDiagnosis != null
               && session.TreatmentResult == null
               && session.PerformedSteps.Count == 0
               && _assignedItems != null
               && _assignedItems.Length ==
               SlotCapacity;
    }
    
   

    public bool FinishTreatment()
    {
        if (!IsPreparationValid())
        {
            return false;
        }
        
        PatientSession patientSession = patientQueue.CurrentSession;
        if (patientSession.CaseData.DiseaseData == null)
        {
            return false;
        }

        SO_TreatmentData[] treatmentSteps = new SO_TreatmentData[_assignedItems.Length];

        for (int i = 0; i < treatmentSteps.Length; i++)
        {
            if (_assignedItems[i] == null)
            {
                continue;
            }

            treatmentSteps[i] = _assignedItems[i].ItemData;
        }
       
        if (!patientSession.SavePerformedSteps(treatmentSteps))
        {
            return false;
        }
        
        
        bool isCorrectDiagnosis = patientSession.SelectedDiagnosis == patientQueue.CurrentSession.CaseData.DiseaseData;

        TreatmentResult result = _evaluator.Evaluate
        (
            patientSession.CaseData.DiseaseData.TreatmentSteps,
            patientSession.PerformedSteps,
            isCorrectDiagnosis
        );

        patientSession.SaveTreatmentResult(result);

        if (patientSession.TreatmentResult != result)
        {
            return false;
        }

        patientQueue.TryCompletePatientCase();

        if (patientSession.State != PatientCaseState.Completed)
        {
            return false;
        }

        _assignedItems = null;
        _selectedItem = null;
        _preparedSession = null;
        return true;

    }
    

}
