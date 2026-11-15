using UnityEngine;

public class PatientNPC : MonoBehaviour, IInteractable
{
    private PatientSession _session;
    private PatientSequenceController _sequenceController;

    public PatientSession Session => _session;

    public void Initialize(PatientSession session)
    {
        _session = session;
    }

    public void SetSequenceController(PatientSequenceController controller)
    {
        _sequenceController = controller;
    }

    public void Interact()
    {
        if (_sequenceController == null)
        {
            return;
        }

        _sequenceController.TryInteractWithPatient(this);
    }

    public void PrintPatientInfo()
    {
        Debug.Log(_session.CaseData.PatientName);
    }
}