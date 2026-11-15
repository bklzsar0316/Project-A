using System;
using UnityEngine;

public class PulseHeartUI : MonoBehaviour
{

    [SerializeField] private float pulseMultiplier = 1.4f;
    [SerializeField] private float returnSpeed = 7f;

    private Vector3 _normalScale;


    private void Awake()
    {
        _normalScale = transform.localScale;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PulseBeatEvent>(OnPulseBeat);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<PulseBeatEvent>(OnPulseBeat);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _normalScale, Time.deltaTime * returnSpeed);
    }


    private void OnPulseBeat(PulseBeatEvent obj)
    {
        transform.localScale = _normalScale * pulseMultiplier;
    }
}
