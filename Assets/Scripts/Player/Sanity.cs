using UnityEngine;

public class Sanity
{
    public float CurrentSanity {get; private set;}
    public float MaxSanity {get; private set;}

    public Sanity(float maxSanity)
    {
        MaxSanity = maxSanity;
        CurrentSanity = maxSanity;
    }
    
    public void DecreaseSanity(float amount)
    {
        if (amount < 0)
        {
            return;
        }
        
        CurrentSanity = Mathf.Max(CurrentSanity - amount, 0);
    }
}
