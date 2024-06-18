using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    private int score = 0;
    private int[] tierScores;
    private FruitManager fruitManager;

    void Start()
    {
        fruitManager = transform.parent.GetComponentInChildren<FruitManager>();
        InitializeScores();
        UpdateScoreText();
    }

    private void InitializeScores()
    {
        int numTiers = fruitManager.fruitPrefabs.Length;
        tierScores = new int[numTiers];
        tierScores[0] = 1;
        for (int i = 1; i < numTiers; i++)
        {
            tierScores[i] = (i + 1) + tierScores[i - 1];
        }
    }

    public void AddScore(int tier)
    {
        score += tierScores[tier];
        UpdateScoreText();
    }

    public int GetTierScore(int tier)
    {
        return tierScores[tier];
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }
}

