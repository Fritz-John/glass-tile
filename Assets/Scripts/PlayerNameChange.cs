using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameChange : NetworkBehaviour
{


    [Header("Player Name Displayer Text")]
    [SerializeField]
    private Text playerDisplayName;

    [Header("Gamer Tag Canvas Text")]
    [SerializeField] 
    private Text gamertag;

    [Header("Player Name Input Field")]
    [SerializeField]
    private InputField playernameIN;

    [Header("Player Rename Canvas")]
    [SerializeField]
    private GameObject InputCanvas;

    [Header("Whole Canvas")]
    [SerializeField]
    private GameObject wholeCanvas;

    public static bool isRenaming = false;

    private void Start()
    {

      

        if (!isLocalPlayer)
        {
            wholeCanvas.SetActive(false);
        }
        if (isLocalPlayer)
        {
            playernameIN.text = gamertag.text;
            playerDisplayName.text = gamertag.text;
        }
    }
    public void Update()
    {
        if (!isLocalPlayer)
            return;


        if (Input.GetKeyDown(KeyCode.B) && !isRenaming)
        {
            InputCanvas.SetActive(true);
           
            isRenaming = true;
            playernameIN.text = gamertag.text;

            Cursor.lockState = CursorLockMode.None;

        }
        if (isRenaming && Input.GetKeyDown(KeyCode.Return))
        {
            Rename();
        }

    }

    public void Rename()
    {
        isRenaming = false;
        InputCanvas.SetActive(false);
   
        Cursor.lockState = CursorLockMode.Locked;
        playerDisplayName.text = playernameIN.text;
        CmdChangeDisplayName(playernameIN.text);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeDisplayName(string playerName)
    {
        RpcChangeName(playerName);
    }

    [ClientRpc]
    public void RpcChangeName(string playerName)
    {     
        gamertag.text = playerName;    
    }







}
