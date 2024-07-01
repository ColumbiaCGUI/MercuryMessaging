public class GoNogoTrialData
{
    public int TrialIndex;
    public int TrialType;
    public float ReactionTime;
    public int CurrentScore;

    public GoNogoTrialData(int trialIndex, int trialType, float reactionTime, int currentScore)
    {
        TrialIndex = trialIndex;
        TrialType = trialType;
        ReactionTime = reactionTime;
        currentScore = currentScore;
    }

    public override string ToString()
    {
        return $"{TrialIndex},{TrialType},{ReactionTime},{CurrentScore}";
    }
}