using UnityEngine;

public static class PlayerProgress
{
    private const string BestSurvivalTimeKey = "BestSurvivalTime";
    private const string FlawlessLevelKeyPrefix = "FlawlessLevel.";

    public static float BestSurvivalTime => PlayerPrefs.GetFloat(BestSurvivalTimeKey, 0f);

    public static void RecordSurvivalTime(float survivalTime)
    {
        if (survivalTime <= BestSurvivalTime)
        {
            return;
        }

        PlayerPrefs.SetFloat(BestSurvivalTimeKey, survivalTime);
        PlayerPrefs.Save();
    }

    public static bool HasFlawlessCompletion(int sceneIndex)
    {
        return PlayerPrefs.GetInt(FlawlessLevelKeyPrefix + sceneIndex, 0) == 1;
    }

    public static void RecordFlawlessCompletion(int sceneIndex)
    {
        if (sceneIndex <= 0 || HasFlawlessCompletion(sceneIndex))
        {
            return;
        }

        PlayerPrefs.SetInt(FlawlessLevelKeyPrefix + sceneIndex, 1);
        PlayerPrefs.Save();
    }
}
