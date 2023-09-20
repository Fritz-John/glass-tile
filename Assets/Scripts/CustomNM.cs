using Mirror; 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CustomNM : NetworkManager
{
    private List<Color> usedColors = new List<Color>();
 
    [SerializeField] InputField HostportNumber;
    [SerializeField] InputField HostipAddress;
    //[SerializeField] InputField HostPlayerName;

    [SerializeField] InputField ClientportNumber;
    [SerializeField] InputField ClientipAddress;

   
    [SerializeField] GameObject connecting;
    [SerializeField] GameObject mainLevel;
    [SerializeField] GameObject dummyLevel;
    [SerializeField] GameObject wholeHost;
    [SerializeField] GameObject wholeClient;
    [SerializeField] GameObject wholeHostandClient;

    [SerializeField] Text connectingText;
    [SerializeField] Button disconnectBtn;

    //[SerializeField] InputField ClientPlayerName;


    kcp2k.KcpTransport netTrans;

    private bool isHost;

    public override void Start()
    {
        netTrans = GetComponent<kcp2k.KcpTransport>();
        base.Start();
    }

    private new void Update()
    {

        if (!NetworkClient.isConnected)
        {

            connectingText.text = "Connecting to " + networkAddress;
            disconnectBtn.gameObject.SetActive(true);
        }
        else
        {
            connectingText.text = "Succesful Connected!";
            disconnectBtn.gameObject.SetActive(false);
            connecting.SetActive(false);
        }

    }
    public override void OnStartServer()
    {
        base.OnStartServer();
    }
    public override void OnStartClient()
    {
        //disconnectBtn.onClick.AddListener(Disconnect);
    }
    public void DisconnectClient()
    {
        Debug.Log("Disconnect is Called!");
       
            StopClient();
        
        connecting.SetActive(false);
        wholeClient.SetActive(true);
       
    }
    public void DisconnectHost()
    {
        //Debug.Log("Disconnect is Called!");
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            StopHost();
        }
        //connecting.SetActive(false);
       // wholeClient.SetActive(true);

    }
    public void Disconnect()
    {

        // Stop Host if local user is host
        if (NetworkServer.active && NetworkClient.isConnected)
        {


            PlayerMoveCamera[] networkIdentity = FindObjectsOfType<PlayerMoveCamera>();

            foreach (PlayerMoveCamera item in networkIdentity)
            {
                NetworkServer.Destroy(item.gameObject);
            }
            wholeHost.SetActive(false);
            dummyLevel.SetActive(true);
            mainLevel.SetActive(false);
            wholeHostandClient.SetActive(true);
            StopHost();
        }


        

        // Stop Client if local user is client
        if (NetworkClient.isConnected)
        {
        }

        // Cancel Join
        if (NetworkClient.isConnecting)
        {
            StopClient();
        }

    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        ColorChange colorChange = conn.identity.GetComponent<ColorChange>();

        Color newColor = GetUniqueRandomColor();
        usedColors.Add(newColor);

        colorChange.SetDisplayColor(newColor);
        
      

    }

    private Color GetUniqueRandomColor()
    {
        Color randomColor;
        do
        {
            randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
        while (usedColors.Contains(randomColor));

        return randomColor;
    }

    public void Starthost()
    {
        mainLevel.SetActive(true);
        wholeHost.SetActive(false);
        wholeHostandClient.SetActive(false);
        dummyLevel.SetActive(false);
        networkAddress = HostipAddress.text;
        netTrans.Port = ushort.Parse(HostportNumber.text);
       
      //StartServer();    
        StartHost();
    }

    public void Startclient()
    {
        mainLevel.SetActive(true);
        dummyLevel.SetActive(false);
        wholeClient.SetActive(false);
        connecting.SetActive(true);
        wholeHostandClient.SetActive(false);

        networkAddress = ClientipAddress.text;
        netTrans.Port = ushort.Parse(ClientportNumber.text);

        //StartServer();
        StartClient();
      
    }
    // Callback when the client successfully connects to the server
   


}
