using UnityEngine;
using UnityEngine.UI;

public class TimerCircle : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image timeCircle;

    [Header("Timer Settings")]
    [SerializeField] private float totalTime = 5f;

    private float currentTime;
    private bool isRunning = false;

    public System.Action onTimerEnd; // Опциональный колбэк

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!isRunning)
            return;

        currentTime -= Time.deltaTime;
        timeCircle.fillAmount = Mathf.Clamp01(currentTime / totalTime);

        if (currentTime <= 0f)
        {
            isRunning = false;
            timeCircle.fillAmount = 0f;

            if (onTimerEnd != null)
                onTimerEnd.Invoke();
        }
    }

    public void StartTimer()
    {
        currentTime = totalTime;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = totalTime;
        timeCircle.fillAmount = 1f;
        isRunning = false;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }
}
