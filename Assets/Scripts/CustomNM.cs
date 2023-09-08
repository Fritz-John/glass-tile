using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomNM : NetworkManager
{

   

    private List<Color> usedColors = new List<Color>();

    [SerializeField] InputField HostportNumber;
    [SerializeField] InputField HostipAddress;

    [SerializeField] InputField ClientportNumber;
    [SerializeField] InputField ClientipAddress;

    kcp2k.KcpTransport netTrans;

    public override void Start()
    {
        netTrans = GetComponent<kcp2k.KcpTransport>();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

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
    public void SceneChanger(int num)
    {
        SceneManager.LoadScene(num);
    }
}
