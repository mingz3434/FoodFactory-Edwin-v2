using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float levelTime = 300f; // 關卡時間 (秒)，Inspector 設置不同關卡
    public TMP_Text timeText; // 顯示 "00:00" 的 TMP Text
    private float remainingTime;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        remainingTime = levelTime;
    }

    void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimeDisplay();
        }
        else
        {
            remainingTime = 0;
            UpdateTimeDisplay();
            GameOver();
        }
    }

    void UpdateTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        gameManager.LoseGame(); // 時間到失敗
    }
}