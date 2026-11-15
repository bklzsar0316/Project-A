using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SO_PatientCaseData", menuName = "Scriptable Objects/PatientCaseData")]
public class SO_PatientCaseData : ScriptableObject
{
    [SerializeField] private string patientName;
    [SerializeField] private PatientIdentity patientIdentity;
    [SerializeField] private SO_CreatureTypeData creatureType;
    [SerializeField] private SO_DiseaseData disease;
    [SerializeField] private GameObject patientPrefab;
    [SerializeField] private string[] examinationDialogues;
    [SerializeField] private string[] treatmentDialogues;
    
    [SerializeField] private float temperature;
    [SerializeField] private int baseBPM;
    [SerializeField,Range(0f,0.5f)] private float pulseIrregularity;
    [SerializeField] private PulseRhythmType pulseRhythmType = PulseRhythmType.Normal;

    
    [SerializeField,Range(0.1f,1f)] private float pupilMinScale;
    [SerializeField] private float pupilShrinkSpeed;
    [SerializeField] private float pupilExpandSpeed;
    
    
    public float Temperature => temperature;
    public int BaseBPM => baseBPM;
    public float PulseIrregularity => pulseIrregularity;
    public float PupilMinScale => pupilMinScale;
    public float PupilShrinkSpeed => pupilShrinkSpeed;
    public float PupilExpandSpeed => pupilExpandSpeed;
    public PulseRhythmType RhythmType => pulseRhythmType;


    
    
    public  string PatientName => patientName;
    public PatientIdentity PatientIdentity => patientIdentity;
    public SO_CreatureTypeData CreatureType => creatureType;
    public SO_DiseaseData DiseaseData => disease;
    public GameObject PatientPrefab => patientPrefab;
    public string[] ExaminationDialogues => examinationDialogues;
    public string[] TreatmentDialogues => treatmentDialogues;
    
}


