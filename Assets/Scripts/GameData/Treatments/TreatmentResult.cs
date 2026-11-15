
public class TreatmentResult
{
    private int _totalSteps;
    private int _correctSteps;
    private int _incorrectSteps;
    private float _mistakeRate;
    private bool _isCorrectDiagnosis;

    public int TotalSteps => _totalSteps;
    public int CorrectSteps => _correctSteps;
    public int IncorrectSteps => _incorrectSteps;
    public bool IsCorrectDiagnosis => _isCorrectDiagnosis;
    public float  MistakeRate => _mistakeRate;
    public TreatmentResult(int totalSteps, int correctSteps, int incorrectSteps,bool isCorrectDiagnosis)
    {
        _totalSteps = totalSteps;
        _correctSteps = correctSteps;
        _incorrectSteps = incorrectSteps;
        _isCorrectDiagnosis = isCorrectDiagnosis;
    
        _mistakeRate = _totalSteps > 0 ? (float)_incorrectSteps / _totalSteps : 0f;
        
    }
}
