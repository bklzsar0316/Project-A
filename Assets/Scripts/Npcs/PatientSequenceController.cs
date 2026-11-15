
using System.Collections.Generic;
using UnityEngine;

public class PatientSequenceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] queuePoints;
    [SerializeField] private PatientSpawner spawner;
    [SerializeField] private PatientDialogueUI dialogueUI;

    [Header("Patient Positions")]
    [SerializeField] private Transform examinationPoint;
    [SerializeField] private Transform treatmentPoint;

    private List<PatientNPC> _spawnedPatients = new();
    private List<PatientSession> _session = new();

    private int _currentPatientIndex = 0;
    private PatientSession _currentSession;

    private PatientStage _currentStage = PatientStage.None;

    public PatientSession CurrentSession => _currentSession;

    private enum PatientStage
    {
        None,
        Conversation,
        Examination,
        Treatment
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BellRingEvent>(OnBellRing);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<BellRingEvent>(OnBellRing);
    }

    public void SpawnDailyPatients(
        IReadOnlyList<SO_PatientCaseData> patientCases)
    {
        if (_spawnedPatients.Count > 0)
        {
            return;
        }

        for (int i = 0; i < Mathf.Min(queuePoints.Length, patientCases.Count); i++)
        {
            PatientSession session = new PatientSession(patientCases[i]);

            PatientNPC patient =
                spawner.SpawnPatient(session, queuePoints[i]);

            if (patient != null)
            {
                patient.SetSequenceController(this);

                _spawnedPatients.Add(patient);
                _session.Add(session);
            }
        }
    }

    public void ClearDailyPatients()
    {
        if (dialogueUI != null)
        {
            dialogueUI.Close();
        }

        foreach (PatientNPC patient in _spawnedPatients)
        {
            if (patient != null)
            {
                Destroy(patient.gameObject);
            }
        }

        _spawnedPatients.Clear();
        _session.Clear();

        _currentPatientIndex = 0;
        _currentSession = null;
        _currentStage = PatientStage.None;
    }

    public void StartCurrentPatientCase()
    {
        if (_currentPatientIndex >= _session.Count)
        {
            return;
        }

        PatientSession session = _session[_currentPatientIndex];

        if (session.State != PatientCaseState.Waiting)
        {
            return;
        }

        _currentSession = session;
        _currentSession.StartCase();

        _currentStage = PatientStage.Conversation;
    }

    public void TryInteractWithPatient(PatientNPC patient)
    {
        if (patient == null || dialogueUI == null ||
            _currentSession == null ||
            _currentSession.State != PatientCaseState.InProgress)
        {
            return;
        }

        if (patient.Session != _currentSession)
        {
            return;
        }

        if (_currentStage == PatientStage.Conversation)
        {
            dialogueUI.OpenIntroduction(
                _currentSession.CaseData,
                SendCurrentPatientToExamination,
                RejectCurrentPatient
            );
        }
        else if (_currentStage == PatientStage.Examination)
        {
            dialogueUI.OpenExaminationDecision(
                _currentSession.CaseData,
                AcceptCurrentPatientForTreatment,
                RejectCurrentPatient
            );
        }
    }

    private void SendCurrentPatientToExamination()
    {
        if (_currentStage != PatientStage.Conversation ||
            _currentSession == null || examinationPoint == null)
        {
            return;
        }

        PatientNPC patient = _spawnedPatients[_currentPatientIndex];

        patient.transform.SetPositionAndRotation(
            examinationPoint.position,
            examinationPoint.rotation
        );

        _currentStage = PatientStage.Examination;
    }

    private void AcceptCurrentPatientForTreatment()
    {
        if (_currentStage != PatientStage.Examination ||
            _currentSession == null || treatmentPoint == null)
        {
            return;
        }

        if (!TrySetDecision(PatientDecision.Treat))
        {
            return;
        }

        PatientNPC patient = _spawnedPatients[_currentPatientIndex];

        patient.transform.SetPositionAndRotation(
            treatmentPoint.position,
            treatmentPoint.rotation
        );

        _currentStage = PatientStage.Treatment;
    }

    private void RejectCurrentPatient()
    {
        if (_currentStage != PatientStage.Conversation &&
            _currentStage != PatientStage.Examination)
        {
            return;
        }

        if (TrySetDecision(PatientDecision.Reject))
        {
            TryConfirmRejection();
        }
    }

    private void CompleteCurrentPatientCase()
    {
        if (_currentSession == null ||
            _currentSession.State != PatientCaseState.InProgress)
        {
            return;
        }

        _currentSession.CompleteCase();

        EventBus.Publish(
            new PatientCaseCompletedEvent(_currentSession)
        );

        PatientNPC patient = _spawnedPatients[_currentPatientIndex];

        if (patient != null)
        {
            patient.gameObject.SetActive(false);
        }

        _currentSession = null;
        _currentStage = PatientStage.None;

        _currentPatientIndex++;

        if (_currentPatientIndex >= _session.Count)
        {
            EventBus.Publish(new AllCasesCompletedEvent());
        }
    }

    public bool TryCompletePatientCase()
    {
        if (_currentSession != null &&
            _currentSession.State == PatientCaseState.InProgress &&
            _currentStage == PatientStage.Treatment &&
            _currentSession.Decision == PatientDecision.Treat &&
            _currentSession.TreatmentResult != null)
        {
            CompleteCurrentPatientCase();
            return true;
        }

        return false;
    }

    private void OnBellRing(BellRingEvent obj)
    {
        StartCurrentPatientCase();
    }

    public bool TrySetDiagnosis(SO_DiseaseData disease)
    {
        if (_currentSession == null ||
            _currentSession.State != PatientCaseState.InProgress ||
            disease == null)
        {
            return false;
        }

        _currentSession.SetDiagnosis(disease);
        return true;
    }

    public bool TrySetDecision(PatientDecision decision)
    {
        if (_currentSession == null ||
            _currentSession.State != PatientCaseState.InProgress ||
            decision == PatientDecision.Undecided)
        {
            return false;
        }

        _currentSession.SetDecision(decision);

        return _currentSession.Decision == decision;
    }

    public bool TryConfirmRejection()
    {
        if (_currentSession != null &&
            _currentSession.State == PatientCaseState.InProgress &&
            _currentSession.Decision == PatientDecision.Reject)
        {
            CompleteCurrentPatientCase();
            return true;
        }

        return false;
    }
}

public struct AllCasesCompletedEvent
{
}

public struct PatientCaseCompletedEvent
{
    public PatientSession Session;

    public PatientCaseCompletedEvent(PatientSession session)
    {
        Session = session;
    }
}
