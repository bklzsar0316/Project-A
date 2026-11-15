using System;
using UnityEngine;

public class DayManager : MonoBehaviour
{

    [SerializeField] private SO_DayData[] days;
    [SerializeField] private PatientSequenceController patientQueue;
    
    private  int _currentDayIndex = 0;
    private DayState _dayState = DayState.NotStarted;


    private void Start()
    {
        StartDay();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<AllCasesCompletedEvent>(OnAllCasesCompleted);
    }
    

    private void OnDisable()
    {
        EventBus.UnSubscribe<AllCasesCompletedEvent>(OnAllCasesCompleted);
    }


    private void StartDay()
    {

        if (_dayState != DayState.NotStarted) return;
        if ( days==null || _currentDayIndex >= days.Length) return;

        
        SO_DayData currentDay = days[_currentDayIndex];
        
        patientQueue.SpawnDailyPatients(currentDay.PatientCases);
        _dayState = DayState.InProgress;
    }
    [ContextMenu("Next Day")]
    public void NextDay()
    {
        if (_dayState == DayState.Finished)
        {
            patientQueue.ClearDailyPatients();
            _currentDayIndex++;
            _dayState = DayState.NotStarted;
            StartDay();
            
        }

    }
    [ContextMenu("Complete Day")]
    public void CompleteDay()
    {
        if (_dayState == DayState.InProgress)
        {
            _dayState = DayState.Finished;
        }
    }
    
    private void OnAllCasesCompleted(AllCasesCompletedEvent obj)
    {
        CompleteDay();
    }
}
