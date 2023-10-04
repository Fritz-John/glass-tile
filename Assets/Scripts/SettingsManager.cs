using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings Control")]
    [SerializeField] GameObject settingsCanvas;
    [SerializeField] GameObject gameCanvas;
    public bool onSettings = false;

    [Header("Sensitivity")]
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
        //Open Settings
        if (Input.GetKeyDown(KeyCode.P))
        {
            onSettings = !onSettings;
            if (onSettings)
            {
                settingsCanvas.SetActive(true);
                gameCanvas.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;

            }
            else
            {
                gameCanvas.SetActive(true);
                settingsCanvas.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        //Sensitivity
        currentSens = sensSlider.value;
        ChangeSensitivity(currentSens);
    }

    void ChangeSensitivity(float sens)
    {
        PlayerPrefs.SetFloat("Sensitivity", sens);
    }
}
