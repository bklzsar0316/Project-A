
public class DiseaseSearchResult
{
    public SO_DiseaseData Disease { get; }
    public int MatchCount { get; }


    public DiseaseSearchResult(SO_DiseaseData disease, int matchCount)
    {
        Disease = disease;
        MatchCount = matchCount;
    }


}
