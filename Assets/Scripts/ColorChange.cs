using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ColorChange : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnColorChanged))][SerializeField] private Color displayColor;

    [SerializeField] private MeshRenderer rend;

    [Server]
    public void SetDisplayColor(Color newColor)
    {
        displayColor = newColor;
    }
    private void OnColorChanged(Color oldColor, Color newColor)
    {
        rend.material.color = newColor;
    }
}
