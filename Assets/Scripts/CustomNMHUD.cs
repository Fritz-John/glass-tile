using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Add this line
using UnityEngine;
using Mirror;

public class CustomNMHUD : NetworkManagerHUD
{
    // Customize the appearance of the HUD
    public GUIStyle customStyle; // You can adjust this in the Inspector

    private void OnGUI()
    {
        // Display a custom label
        GUI.Label(new Rect(10, 10, 200, 20), "Custom Network HUD", customStyle);

        if (NetworkManager.singleton == null)
            return;

        if (!NetworkManager.singleton.isNetworkActive)
        {
            // Customize the Start Host button
            if (GUI.Button(new Rect(10, 40, 120, 20), "Start Custom Host"))
            {
                SceneManager.LoadScene("GameScene");
                NetworkManager.singleton.StartHost();

            }

            // Customize the Start Client button
            if (GUI.Button(new Rect(10, 70, 120, 20), "Start Custom Client"))
            {
                NetworkManager.singleton.StartClient();
            }

            // Customize the Start Server button
            if (GUI.Button(new Rect(10, 100, 120, 20), "Start Custom Server"))
            {
                NetworkManager.singleton.StartServer();
            }
        }
        else
        {
            // Customize the Stop Host button
            if (GUI.Button(new Rect(10, 40, 120, 20), "Stop Custom Host"))
            {
                NetworkManager.singleton.StopHost();
            }

            // Customize the Stop Client button
            if (GUI.Button(new Rect(10, 70, 120, 20), "Stop Custom Client"))
            {
                NetworkManager.singleton.StopClient();
            }

            // Customize the Stop Server button
            if (GUI.Button(new Rect(10, 100, 120, 20), "Stop Custom Server"))
            {
                NetworkManager.singleton.StopServer();
            }
        }
    }
}
