using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuProgressDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text bestSurvivalTimeText;
    [SerializeField] private Image[] flawlessLevelStars;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (bestSurvivalTimeText != null)
        {
            var bestTime = PlayerProgress.BestSurvivalTime;
            bestSurvivalTimeText.text = bestTime > 0f
                ? bestTime.ToString("0.00", CultureInfo.InvariantCulture) + "s"
                : string.Empty;
        }

        if (flawlessLevelStars == null)
        {
            return;
        }

        for (var i = 0; i < flawlessLevelStars.Length; i++)
        {
            if (flawlessLevelStars[i] != null)
            {
                var sceneIndex = i + 1;
                var isLevelFiveSurvivalStar = sceneIndex == 5 && PlayerProgress.BestSurvivalTime > 100f;
                var isFlawlessFiniteLevelStar = sceneIndex < 5 && PlayerProgress.HasFlawlessCompletion(sceneIndex);
                flawlessLevelStars[i].gameObject.SetActive(isLevelFiveSurvivalStar || isFlawlessFiniteLevelStar);
            }
        }
    }
}
