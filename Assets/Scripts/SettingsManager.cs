using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : NetworkBehaviour
{
    [Header("Settings Control")]
    [SerializeField] GameObject settingsCanvas;
    [SerializeField] GameObject gameCanvas;
    public bool onSettings = false;

    [Header("Sensitivity")]
    public Slider sensSlider;
    private float currentSens;

    public bool isPaused;

    public PlayerNameChange playerNameChange;
    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            currentSens = PlayerPrefs.GetFloat("Sensitivity");
            sensSlider.value = currentSens;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        //Open Settings
        if (Input.GetKeyDown(KeyCode.P) && !playerNameChange.isRenaming)
        {
            onSettings = !onSettings;
            if (onSettings)
            {
                settingsCanvas.SetActive(true);
                gameCanvas.SetActive(false);
                //Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                isPaused = true;

            }
            else
            {
                gameCanvas.SetActive(true);
                settingsCanvas.SetActive(false);
                //Cursor.visible = false;
                isPaused = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        Debug.Log(onSettings);
        //Sensitivity
        currentSens = sensSlider.value;
        ChangeSensitivity(currentSens);
    }
    public void changeOnSettings()
    {
        onSettings = false;
        gameCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void ChangeSensitivity(float sens)
    {
        PlayerPrefs.SetFloat("Sensitivity", sens);
    }
}
