using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance {get; private set;}

    [Header("Hareket Ayarlari")]
    [SerializeField] private float playerSpeed;
    
    [Header("Kamera Ayarlari")]
    [SerializeField] private float mouseSensitivity = 1.0f;
    [SerializeField] private float cameraBobSpeed = 5f;
    [SerializeField] private float amplitude = 0.07f;
    [SerializeField] private float timeToReturnNormalHeight = 0.2f;
    [SerializeField] private bool setCursorVisible = false;
    
    [Header("Akil Sagligi Ayarlari")]
    [SerializeField,Min(1f)] private float maxSanity;

    [SerializeField,Min(0f)] private float humanRejectPenalty;
    [SerializeField,Min(0f)] private float maximumTreatmentPenalty;
    [SerializeField,Min(1f)] private float wrongDiagnosisMultiplier;
    

    
    private Sanity _sanity;
    public Sanity Sanity => _sanity;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        _sanity = new Sanity(maxSanity);
    }

    public float GetPlayerSpeed()
    {
        return playerSpeed;
    }

    public float GetMouseSensitivity()
    {
        return mouseSensitivity;
    }

    public float GetCameraBobSpeed()
    {
        return cameraBobSpeed;
    }

    public float GetAmplitude()
    {
        return amplitude;
    }

    public float GetTimeToReturnNormalHeight()
    {
        return timeToReturnNormalHeight;
    }

    public bool GetCursorVisible()
    {
        return setCursorVisible;
    }

    public float GetMaximumTreatmentPenalty()
    {
        return maximumTreatmentPenalty;
    }
    public float GetWrongDiagnosisMultiplier()
    {
        return wrongDiagnosisMultiplier;
    }

    public float GetHumanRejectPenalty()
    {
        return humanRejectPenalty;
    }
}
