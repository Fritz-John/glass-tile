using Mirror; 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [SerializeField] SceneChange sceneChange;
    kcp2k.KcpTransport netTrans;

    private static CustomNM instance;
 
    public static CustomNM Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CustomNM>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("CustomNM");
                    instance = obj.AddComponent<CustomNM>();
                }
            }
            return instance;
        }
    }
    public static void DestroyInstance()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }

    private new void Awake()
    {
        if (instance != null && instance != this)
        {       
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public override void Start()
    {
        netTrans = GetComponent<kcp2k.KcpTransport>();
        base.Start();
    }

    private new void Update()
    {

        if (!NetworkClient.isConnected )
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
    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.LogWarning("Server has stopped!");
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
    }
    public override void OnStartClient()
    {
        //disconnectBtn.onClick.AddListener(Disconnect);
    }
    
    public void DisconnectHost()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            StopHost();
        }

    }
    public void Disconnect()
    {

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            sceneChange.ChangeScene("UI");
            StopHost();
        }

         if (NetworkClient.isConnected)
        {
            sceneChange.ChangeScene("UI");
            StopClient();
        }

        // Cancel Join
        if (NetworkClient.isConnecting)
        {

            sceneChange.ChangeScene("UI");
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
    public override void OnStopClient()
    {
        base.OnStopClient();
        Disconnect();
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

        StartClient();
      
    }
   


}
