using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextStatic : MonoBehaviour
{
    public Text textMesh;
    public float maxDisplacement = 0.1f; 
    public float glitchFrequency = 0.1f; 
    public float glitchDuration = 0.05f; 

    private float timeSinceLastGlitch = 0f;
    private Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<Text>();
        }

        
        originalPosition = textMesh.rectTransform.anchoredPosition3D;
    }

    // Update is called once per frame
    void Update()
    {
        
        timeSinceLastGlitch += Time.deltaTime;

       
        if (timeSinceLastGlitch >= glitchFrequency)
        {
            StartCoroutine(ApplyGlitchEffect());
            timeSinceLastGlitch = 0f;
        }
    }

    IEnumerator ApplyGlitchEffect()
    {
      
        Vector3 glitchOffset = Random.insideUnitSphere * maxDisplacement;

        
        textMesh.rectTransform.anchoredPosition3D += glitchOffset;

      
        yield return new WaitForSeconds(glitchDuration);

     
        textMesh.rectTransform.anchoredPosition3D = originalPosition;
    }
}
