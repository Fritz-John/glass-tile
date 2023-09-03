using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoun : NetworkBehaviour
{
    public AudioSource breakGlass;
    public AudioClip breakglassClip;
    public static PlaySoun instance;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ClientRpc]
    public void RpcPlaySounds()
    {
        breakGlass.clip = breakglassClip;
        breakGlass.Play();
    }
}
