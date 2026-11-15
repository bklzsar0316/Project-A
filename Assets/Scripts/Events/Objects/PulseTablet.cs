
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PulseTablet : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private Camera playerCam;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform basePoint;
    [SerializeField] private Transform sensorPoint;
    [SerializeField] private TMP_Text bpmText;
    [SerializeField] private PulseGraph pulseGraph;

    [Header("Settings")]
    [SerializeField] private float scanRadius = 3f;
    [SerializeField] private float placementDistance = 3f;
    [SerializeField] private Vector3 placedRotation;
    [SerializeField] private LayerMask chestArea;

    private PatientNPC _currentPatient;

    private float _baseInterval;
    private float _beatInterval;
    private float _irregularity;
    private float _timer;

    private PulseRhythmType _rhythmType;

    private bool _isHeld = false;
    private bool _nextBeatIsExtra = false;

    private BoxCollider _boxCollider;

    private readonly Collider[] _hits = new Collider[8];
    private readonly Queue<float> _recentIntervals = new Queue<float>();

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        bpmText.text = "-- BPM";
    }

    public void Interact()
    {
        if (_isHeld)
        {
            TryPlace();
        }
        else
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (holdPoint == null || !interactor.TryHold(this))
        {
            return;
        }

        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _boxCollider.enabled = false;
        _isHeld = true;

        ResetSignal();
    }

    private void TryPlace()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, placementDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        if (!hit.collider.CompareTag("Counter"))
        {
            return;
        }

        if (Vector3.Dot(hit.normal, Vector3.up) < 0.7f)
        {
            return;
        }

        transform.SetParent(null, true);

        transform.rotation =
            Quaternion.Euler(0f, playerCam.transform.eulerAngles.y, 0f)
            * Quaternion.Euler(placedRotation);

        transform.position += hit.point - basePoint.position;

        _boxCollider.enabled = true;
        _isHeld = false;

        interactor.ReleaseHeld(this);
        ResetSignal();
    }

    private void Update()
    {
        if (!_isHeld)
        {
            return;
        }

        PatientNPC patient = FindNearbyPatient();

        if (patient != _currentPatient)
        {
            SetPatient(patient);
        }

        if (_currentPatient == null)
        {
            return;
        }

        _timer += Time.deltaTime;

        while (_timer >= _beatInterval)
        {
            float completedInterval = _beatInterval;
            _timer -= completedInterval;

            bool isExtraBeat = _nextBeatIsExtra;

            bool nextBeatIsExtra;

            float nextInterval = CalculateNextInterval(
                isExtraBeat,
                completedInterval,
                out nextBeatIsExtra
            );

            _nextBeatIsExtra = nextBeatIsExtra;
            _beatInterval = nextInterval;

            HeartBeat(
                completedInterval,
                nextInterval,
                isExtraBeat,
                nextBeatIsExtra
            );
        }
    }

    private void SetPatient(PatientNPC patient)
    {
        _currentPatient = patient;

        _timer = 0f;
        _nextBeatIsExtra = false;

        _recentIntervals.Clear();

        bpmText.text = "-- BPM";
        pulseGraph.ClearGraph();

        if (_currentPatient == null)
        {
            return;
        }

        SO_PatientCaseData caseData = patient.Session.CaseData;

        int baseBPM = caseData.BaseBPM;

        _irregularity = Mathf.Clamp(caseData.PulseIrregularity, 0f, 0.5f);
        _rhythmType = caseData.RhythmType;

        _baseInterval = 60f / Mathf.Max(1, baseBPM);
        _beatInterval = _baseInterval;
    }

    private float CalculateNextInterval(
        bool isExtraBeat,
        float completedInterval,
        out bool nextBeatIsExtra)
    {
        nextBeatIsExtra = false;

        float severity = Mathf.Clamp01(_irregularity / 0.5f);

        switch (_rhythmType)
        {
            case PulseRhythmType.Normal:
            {
                float variation = Mathf.Max(0.025f, _irregularity);
                float deviation = Random.Range(-variation, variation);

                return _baseInterval * (1f + deviation);
            }

            case PulseRhythmType.ExtraBeats:
            {
                if (isExtraBeat)
                {
                    return Mathf.Max(
                        0.2f,
                        2f * _baseInterval - completedInterval
                    );
                }

                float extraBeatChance = Mathf.Lerp(0.08f, 0.35f, severity);

                if (Random.value < extraBeatChance)
                {
                    nextBeatIsExtra = true;

                    float earlyFactor = Mathf.Lerp(0.75f, 0.45f, severity);

                    return _baseInterval * earlyFactor;
                }

                float deviation = Random.Range(-0.03f, 0.03f);

                return _baseInterval * (1f + deviation);
            }

            case PulseRhythmType.Irregular:
            {
                float variation = Mathf.Lerp(0.20f, 0.60f, severity);

                float deviation = Random.Range(-variation, variation);

                return Mathf.Max(
                    0.2f,
                    _baseInterval * (1f + deviation)
                );
            }

            default:
            {
                return _baseInterval;
            }
        }
    }

    private PatientNPC FindNearbyPatient()
    {
        int count = Physics.OverlapSphereNonAlloc(
            sensorPoint.position,
            scanRadius,
            _hits,
            chestArea,
            QueryTriggerInteraction.Collide
        );

        PatientNPC closestPatient = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            PatientNPC patient = _hits[i].GetComponentInParent<PatientNPC>();

            if (patient == null || patient.Session == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                sensorPoint.position,
                patient.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPatient = patient;
            }
        }

        return closestPatient;
    }

    private void HeartBeat(
        float interval,
        float nextInterval,
        bool isExtraBeat,
        bool nextBeatIsExtra)
    {
        _recentIntervals.Enqueue(interval);

        if (_recentIntervals.Count > 6)
        {
            _recentIntervals.Dequeue();
        }

        float total = 0f;

        foreach (float value in _recentIntervals)
        {
            total += value;
        }

        int currentBPM = Mathf.RoundToInt(
            60f * _recentIntervals.Count / total
        );

        bpmText.text = currentBPM + " BPM";

        EventBus.Publish(new PulseBeatEvent(
            _currentPatient,
            interval,
            nextInterval,
            _rhythmType,
            isExtraBeat,
            nextBeatIsExtra
        ));
    }

    private void ResetSignal()
    {
        _currentPatient = null;

        _timer = 0f;
        _baseInterval = 0f;
        _beatInterval = 0f;
        _irregularity = 0f;

        _rhythmType = PulseRhythmType.Normal;
        _nextBeatIsExtra = false;

        _recentIntervals.Clear();

        bpmText.text = "-- BPM";
        pulseGraph.ClearGraph();
    }

    private void OnDestroy()
    {
        if (_isHeld && interactor != null)
        {
            interactor.ReleaseHeld(this);
        }
    }
}

public enum PulseRhythmType
{
    Normal,
    ExtraBeats,
    Irregular
}

public struct PulseBeatEvent
{
    public PatientNPC Patient;
    public float Interval;
    public float NextInterval;
    public PulseRhythmType RhythmType;
    public bool IsExtraBeat;
    public bool NextBeatIsExtra;

    public PulseBeatEvent(
        PatientNPC patient,
        float interval,
        float nextInterval,
        PulseRhythmType rhythmType,
        bool isExtraBeat,
        bool nextBeatIsExtra)
    {
        Patient = patient;
        Interval = interval;
        NextInterval = nextInterval;
        RhythmType = rhythmType;
        IsExtraBeat = isExtraBeat;
        NextBeatIsExtra = nextBeatIsExtra;
    }
}
