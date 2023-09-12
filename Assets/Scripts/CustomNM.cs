using Mirror;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
   //[SerializeField] InputField ClientPlayerName;
   

    kcp2k.KcpTransport netTrans;

    private bool isHost;

    public override void Start()
    {
        netTrans = GetComponent<kcp2k.KcpTransport>();
        base.Start();
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
        
        networkAddress = HostipAddress.text;
        netTrans.Port = ushort.Parse(HostportNumber.text);
       
      
        StartHost();
    }

    public void Startclient()
    {
      
        networkAddress = ClientipAddress.text;
        netTrans.Port = ushort.Parse(ClientportNumber.text);
       
       
        StartClient();
    }
   



}
