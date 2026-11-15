using System;
using UnityEngine;

public class PatientSpawner : MonoBehaviour
{
    
    public PatientNPC SpawnPatient(PatientSession session,Transform patientSpawnPoint)
    {
        GameObject patientPrefab = Instantiate(session.CaseData.PatientPrefab, patientSpawnPoint.position, patientSpawnPoint.rotation);
        PatientNPC npc = patientPrefab.GetComponent<PatientNPC>();
        npc.Initialize(session);
        npc.PrintPatientInfo();
        
        return npc;
    }
}
