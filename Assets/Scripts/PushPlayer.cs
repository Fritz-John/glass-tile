using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushPlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    public Rigidbody rb;

    [Command(requiresAuthority = true)]
    public void CmdPushPlayer(GameObject player, Vector3 pos, ForceMode force)
    {
        RpcPushPlayer(player, pos, force);
    }

    [ClientRpc]
    private void RpcPushPlayer(GameObject objToPush, Vector3 pos, ForceMode force)
    {
        Rigidbody rb = objToPush.GetComponent<Rigidbody>();
        rb.AddForce(pos, force);
    }

  

}
