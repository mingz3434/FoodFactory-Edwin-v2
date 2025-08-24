using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Image availablePlateUI; // 左下角 UI Image
    public TMP_Text availablePlateText; // Image 的 TMP_Text 子物件
    public TMP_Text scoreText; // Image 的 TMP_Text 子物件

    public GameObject gameOverPanel; // 結束畫面 Panel
    public TMP_Text gameOverText; // 結束畫面的 TMP_Text (顯示 "勝利" 或 "失敗")

    private int totalScore = 0;

    void Start()
    {
        UpdateAvailablePlateUI(null); // 初始禁用 UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 初始隱藏結束畫面
        }
    }

    public void UpdateAvailablePlateUI(string plateName)
    {
        if (availablePlateUI != null && availablePlateText != null)
        {
            if (string.IsNullOrEmpty(plateName))
            {
                availablePlateUI.gameObject.SetActive(false);
            }
            else
            {
                availablePlateUI.gameObject.SetActive(true);
                availablePlateText.text = plateName; // 例如 "訂單 1"
            }
        }
    }

    public void AddScore(int score)
    {
        totalScore += Mathf.Max(0, score); // 確保分數不為負
        scoreText.text = "分數: " + totalScore.ToString();
        Debug.Log($"Score added: {score}, Total Score: {totalScore}");
    }
    public void WinGame()
    {
        Time.timeScale = 0;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverText != null)
        {
            gameOverText.text = "勝利！";
        }
        Debug.Log("Game Over: You won!");
    }

    public void LoseGame()
    {
        Time.timeScale = 0;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverText != null)
        {
            gameOverText.text = "失敗！";
        }
        Debug.Log("Game Over: You lost!");
    }
}