using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameChange : NetworkBehaviour
{

    [SyncVar(hook = nameof(OnDisplayNameChanged))]
    private string displayName = "DefaultName";

    [SyncVar]
    public bool isRenaming = false;

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


    string[] badWords = new string[] {"bobo", "bitch","tanga","gago","pakyu","fuck","inutil","tarantado", "tangina", "shit","bullshit" };

    //public static bool isRenaming = false;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            wholeCanvas.SetActive(false);
          
        }
        playernameIN.text = gamertag.text;
        playerDisplayName.text = gamertag.text;
        isRenaming = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Update()
    {
        if (!isLocalPlayer)
            return;
 
        if (isRenaming && Input.GetKeyDown(KeyCode.Return))
        {
            Rename();
        }

        string inputText = playernameIN.text.ToLower();
        foreach (string s in badWords)
        {
            if (inputText.Contains(s))
            {
                string asteriskString = new string('*', s.Length);
                inputText = inputText.Replace(s, asteriskString);
                playernameIN.text = inputText;
            }
        }
        if (inputText.Contains("sacramento") || inputText.Contains("cabrera"))
        {
            playernameIN.text = "LOW BORN PEASANT";
        }


    }
    private void OnDisplayNameChanged(string oldValue, string newValue)
    {
        playerDisplayName.text = newValue;
        gamertag.text = newValue;
    }
    public void Rename()
    {
        isRenaming = false;
        InputCanvas.SetActive(false);
   
        Cursor.lockState = CursorLockMode.Locked;
       
        CmdChangeDisplayName(playernameIN.text);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeDisplayName(string playerName)
    {
        displayName = playerName;
      
    }

    //[ClientRpc]
    //public void RpcChangeName(string playerName)
    //{     
    //    gamertag.text = playerName;    
    //}
}
