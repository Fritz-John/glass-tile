using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCounter : NetworkBehaviour
{
    
    private List<string> playersInside = new List<string>();
    public Text playerText;

    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string displayName = "DefaultName";

    void Start()
    {
        //displayName = "";
        //playerText.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        //foreach (var player in playersInside)
        //{
        //    Debug.Log(player);
        //}
        if (!TileSpawner.instance.isActivated)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            GetComponent<BoxCollider>().enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerNameChange player = other.gameObject.GetComponent<PlayerNameChange>();
        if (other.CompareTag("Player") && TileSpawner.instance.isActivated)
        {
            StoreName(player.gamertag.text);      
        }     
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerNameChange player = other.gameObject.GetComponent<PlayerNameChange>();
        if (other.CompareTag("Player") && TileSpawner.instance.isActivated)
        {
            RemoveName(player.gamertag.text);
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdAnnounce()
    {
        RpcAnnounce();
    }
    [ClientRpc]
    void RpcAnnounce()
    {
        AnnounceWinners();
    }
    private void OnPlayerNameChanged(string oldValue, string newValue)
    {
        displayName = newValue;
        playerText.text = displayName;
    }

    public void AnnounceWinners()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (playersInside.Count > 0)
        {
            for (int i = 0; i < playersInside.Count; i++)
            {
                displayName = stringBuilder.Append((i + 1).ToString()).Append(". ").Append(playersInside[i]).Append("\n").ToString();
            }
        }
        else
        {
            displayName = stringBuilder.Append("NO ONE HAS SURVIVED!").ToString();
        }
        
      

        playerText.text = displayName;

    }
    [Command(requiresAuthority = false)]
    public void CmdClearNames()
    {
        RpcClearNames();
    }
    void RpcClearNames()
    {
        ClearNames();
    }
    public void ClearNames()
    {
        playersInside.Clear();
        displayName = "";
        playerText.text = displayName;
    }
    void RemoveName(string name)
    {
        playersInside.Remove(name);
    }
    void StoreName(string name)
    {
        Debug.Log(name);
        playersInside.Add(name);
    }

    //public List<string> GetPlayersInside()
    //{
    //    return playersInside;
    //}
}
