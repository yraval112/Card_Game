using UnityEngine;

public class TurnTimer : MonoBehaviour
{
    public float TimeRemaining { get; private set; }
    private float turnStartTime;
    private bool isRunning;

    private const float TURN_DURATION = 20f; // 20 seconds per turn

    void Update()
    {
        if (!isRunning) return;

        TimeRemaining = TURN_DURATION - (Time.time - turnStartTime);

        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            isRunning = false;
            OnTimerExpired();
        }

        EventBus.OnTurnTimerTick?.Invoke(TimeRemaining);
    }

    public void StartTurn()
    {
        isRunning = true;
        turnStartTime = Time.time;
        TimeRemaining = TURN_DURATION;
        Debug.Log("Turn timer started");
    }

    public void StopTurn()
    {
        isRunning = false;
        TimeRemaining = 0;
    }

    private void OnTimerExpired()
    {
        Debug.Log("Turn time expired!");
        EventBus.OnTurnTimerExpired?.Invoke();

        // Auto-submit fold
        DeckManager.Instance.SubmitFold();
    }
}
