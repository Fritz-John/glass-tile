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


    void Start()
    {
        playerText.text = string.Empty;
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
    public void AnnounceWinners()
    {
        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < playersInside.Count; i++)
        {
            stringBuilder.Append((i + 1).ToString()).Append(". ").Append(playersInside[i]).Append("\n");
        }

        playerText.text = stringBuilder.ToString();

    }
    public void ClearNames()
    {
        playersInside.Clear();
        playerText.text = "";
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
