using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TreatmentEvaluator 
{

    public TreatmentResult Evaluate(IReadOnlyList<SO_TreatmentData> expectedSteps,
        IReadOnlyList<SO_TreatmentData> performedSteps , bool isCorrectDiagnosis)
    {
        int incorrectSteps = 0;
        int correctSteps = 0;
        int totalSteps = Mathf.Max(expectedSteps.Count, performedSteps.Count);
        
        for (int i = 0; i < totalSteps; i++)
        {
            SO_TreatmentData expected =
                i < expectedSteps.Count ? expectedSteps[i] : null;
            
            SO_TreatmentData performed =
                i < performedSteps.Count ? performedSteps[i] : null;

            if (expected == null && performed == null)
            {
                continue;
            }
            
            else if (expected == performed)
            {
                correctSteps++;
            }
            else
            {
                incorrectSteps++;

            }
        }
        return new TreatmentResult(totalSteps,correctSteps,incorrectSteps,isCorrectDiagnosis);
    }
}
