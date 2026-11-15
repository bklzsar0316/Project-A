using UnityEngine;

[CreateAssetMenu(fileName = "SO_TestData", menuName = "Scriptable Objects/TestData")]
public class SO_TestData : ScriptableObject
{
    [SerializeField] private string testName;
    [SerializeField] private string testDescription;
    
    public string TestName => testName;
    public string TestDescription => testDescription;
}
