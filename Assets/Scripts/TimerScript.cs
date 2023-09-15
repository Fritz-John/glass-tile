using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class TimerScript : NetworkBehaviour
{
    public Text timerText;
    [SyncVar] private float timer;
    public float setTimer = 600.0f; 
    private bool isCountingDown = false;

    private void Start()
    {
        if (isServer)
        {
          
            timer = setTimer;
        }
    }

    private void Update()
    {
        if (!isServer)
            return;

        if (timer > 0 && isCountingDown)
        {
            timer -= Time.deltaTime;
        }

        UpdateTimerDisplay();
    }

    public void StartCountdown()
    {
        isCountingDown = true;
    }
    public void ResetCountdown()
    {
        isCountingDown = false;
        timer = setTimer;
    }
    [ClientRpc]
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
