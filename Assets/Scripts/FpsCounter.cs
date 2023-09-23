using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class FpsCounter : NetworkBehaviour
{
    public Text fps;
    public Text ping;
    private float time;
    private float pollingTime = 1f;
    private int frameCount;
    double rtt = NetworkTime.rtt;
    double pingMilliseconds = Math.Round(NetworkTime.rtt * 1000);
    // Start is called before the first frame update
    void Start()
    {
        rtt = NetworkTime.rtt;
        pingMilliseconds = Math.Round(rtt * 1000);
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
       
        frameCount++;

        if(time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fps.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }

     
        ping.text = $"{Math.Round(pingMilliseconds)} ms";
    }
}
