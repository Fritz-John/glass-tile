using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSceneChanger : NetworkBehaviour
{
    [Command]
    public void CmdChangeScene(string sceneName)
    {
        if (sceneName == "UI")
        {
            CustomNM.DestroyInstance();
        }
        RpcLoadScene(sceneName);
    }

    [ClientRpc]
    private void RpcLoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
