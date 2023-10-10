using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSensitivity : MonoBehaviour
{
    public Slider sensSlider;
    private float currentSens;
    // Start is called before the first frame update
    void Start()
    {
        currentSens = PlayerPrefs.GetFloat("Sensitivity");
        sensSlider.value = currentSens;
    }

    // Update is called once per frame
    void Update()
    {
        currentSens = sensSlider.value;
        ChangeSensitivity(currentSens);
    }

    void ChangeSensitivity(float sens)
    {
        PlayerPrefs.SetFloat("Sensitivity", sens);
    }
}
