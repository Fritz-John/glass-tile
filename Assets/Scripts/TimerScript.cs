using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class TimerScript : NetworkBehaviour
{
    public Text timerText;
    [SyncVar] public float timer;
    public float setTimer = 600.0f;
    [SyncVar] private bool isCountingDown = false;

    TileSpawner tileSpawner;
    private void Start()
    {
        if (isServer)
        {
            timer = setTimer;
        }
        tileSpawner = FindObjectOfType<TileSpawner>();
    }
    private void Update()
    {
        if (!isServer)
            return;

        if (timer > 0 && isCountingDown)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = 0;
                isCountingDown = false;
                if (!tileSpawner.isDestroyingTiles)
                {
                    tileSpawner.isDestroyingTiles = true;
                    StartCoroutine(tileSpawner.DestroyTilesWithDelay());
                }
            }
        }
       

        UpdateTimerDisplay();
    }
    [Command(requiresAuthority = false)]
    public void CmdStartCountdown()
    {
        RpcStartCountdown();
    }
    [Command(requiresAuthority = false)]
    public void CmdStopCountdown()
    {
        RpcStopCountdown();
    }
    [ClientRpc]
    public void RpcStartCountdown()
    {
        isCountingDown = true;
       
    }

    [ClientRpc]
    public void RpcStopCountdown()
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
   
