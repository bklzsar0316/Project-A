
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LineRenderer))]
public class PulseGraph : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int sampleCount = 300;
    [SerializeField] private float sampleInterval = 0.01f;
    [SerializeField] private float graphWidth = 0.2f;
    [SerializeField] private float amplitude = 0.025f;

    private LineRenderer _lineRenderer;

    private float[] _samples;
    private Vector3[] _positions;

    private float _sampleTimer;
    private float _timeFromBeat;
    private float _clock;

    private float _nextInterval = 0.75f;
    private float _beatHeight = 1f;

    private PulseRhythmType _rhythmType;

    private bool _hasSignal = false;
    private bool _isExtraBeat = false;
    private bool _nextBeatIsExtra = false;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;

        sampleCount = Mathf.Max(2, sampleCount);
        sampleInterval = Mathf.Max(0.005f, sampleInterval);

        _samples = new float[sampleCount];
        _positions = new Vector3[sampleCount];

        _lineRenderer.positionCount = sampleCount;

        DrawGraph();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PulseBeatEvent>(OnPulseBeat);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<PulseBeatEvent>(OnPulseBeat);
    }

    private void OnPulseBeat(PulseBeatEvent eventData)
    {
        _hasSignal = true;

        _timeFromBeat = 0f;
        _nextInterval = Mathf.Max(0.2f, eventData.NextInterval);

        _rhythmType = eventData.RhythmType;
        _isExtraBeat = eventData.IsExtraBeat;
        _nextBeatIsExtra = eventData.NextBeatIsExtra;

        _beatHeight = Random.Range(0.96f, 1.04f);
    }

    private void Update()
    {
        if (!_hasSignal)
        {
            return;
        }

        _sampleTimer += Time.deltaTime;

        bool graphChanged = false;

        while (_sampleTimer >= sampleInterval)
        {
            _sampleTimer -= sampleInterval;

            float value = GetWaveValue(_timeFromBeat);

            AddSample(value);

            _timeFromBeat += sampleInterval;
            _clock += sampleInterval;

            graphChanged = true;
        }

        if (graphChanged)
        {
            DrawGraph();
        }
    }

    private float GetWaveValue(float time)
    {
        float value = 0f;

        float tCenter = Mathf.Min(0.28f, _nextInterval * 0.42f);

        if (_isExtraBeat)
        {
            value -= 0.12f * Gaussian(time, 0.01f, 0.015f);
            value += 1.15f * Gaussian(time, 0.05f, 0.033f);
            value -= 0.55f * Gaussian(time, 0.12f, 0.025f);

            value -= 0.30f * Gaussian(time, tCenter, 0.07f);
        }
        else
        {
            value -= 0.14f * Gaussian(time, 0.01f, 0.009f);
            value += 1.00f * Gaussian(time, 0.035f, 0.011f);
            value -= 0.32f * Gaussian(time, 0.065f, 0.012f);

            value += 0.26f * Gaussian(time, tCenter, 0.055f);
        }

        if (_rhythmType != PulseRhythmType.Irregular &&
            !_nextBeatIsExtra)
        {
            float pCenter = _nextInterval -
                            Mathf.Min(0.18f, _nextInterval * 0.25f);

            value += 0.13f * Gaussian(time, pCenter, 0.035f);
        }

        if (_rhythmType == PulseRhythmType.Irregular)
        {
            value += Mathf.Sin(_clock * 13f) * 0.025f;
            value += Mathf.Sin(_clock * 23f) * 0.012f;
        }
        else
        {
            value += Mathf.Sin(_clock * 1.8f) * 0.005f;
        }

        return value * _beatHeight;
    }

    private float Gaussian(float time, float center, float width)
    {
        float x = (time - center) / width;

        return Mathf.Exp(-0.5f * x * x);
    }

    private void AddSample(float value)
    {
        for (int i = 0; i < _samples.Length - 1; i++)
        {
            _samples[i] = _samples[i + 1];
        }

        _samples[_samples.Length - 1] = value;
    }

    private void DrawGraph()
    {
        for (int i = 0; i < _samples.Length; i++)
        {
            float x = i * graphWidth / (_samples.Length - 1);
            float y = _samples[i] * amplitude;

            _positions[i] = new Vector3(x, y, 0f);
        }

        _lineRenderer.SetPositions(_positions);
    }

    public void ClearGraph()
    {
        if (_samples == null)
        {
            return;
        }

        System.Array.Clear(_samples, 0, _samples.Length);

        _sampleTimer = 0f;
        _timeFromBeat = 0f;
        _clock = 0f;

        _hasSignal = false;
        _isExtraBeat = false;
        _nextBeatIsExtra = false;

        DrawGraph();
    }
}
