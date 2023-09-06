using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNM : NetworkManager
{
    private List<Color> usedColors = new List<Color>();
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
}
