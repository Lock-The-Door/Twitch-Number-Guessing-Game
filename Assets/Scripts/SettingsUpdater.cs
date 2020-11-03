using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUpdater : MonoBehaviour
{
    public Toggle limitGuesses;
    public TMP_Dropdown displayDropdown;
    public TMP_InputField frameRateInput;

    private int currentDisplay = 0;

    public GameManager GameManager;

    private void Start()
    {
        GetComponent<Canvas>().sortingLayerName = "Top";

        // Show saved settings
        currentDisplay = PlayerPrefs.GetInt("UnitySelectMonitor");

        // Add Listeners
        limitGuesses.onValueChanged.AddListener(limitGuessesChanged);

        // Display config
        var displays = new System.Collections.Generic.List<string>();
        int displayNumber = 1;
        foreach (Display display in Display.displays)
        {
            displays.Add("Display " + displayNumber);
            displayNumber++;
        }

        displayDropdown.AddOptions(displays);

        displayDropdown.value = currentDisplay;

        frameRateInput.text = PlayerPrefs.GetInt("FrameRate", 60).ToString();
    }

    private void OnDestroy()
    {
        // Change display
        if (currentDisplay != displayDropdown.value)
            ChangeDisplays();
    }

    void limitGuessesChanged(bool newValue)
    {
        GameManager.SingleGuessQueue = newValue;
        PlayerPrefs.SetString("Single Guess Queue", newValue.ToString());
    }

    void changeFrameRate(string newFrameRateText)
    {
        if (int.TryParse(newFrameRateText, out int newFrameRate))
        {
            Application.targetFrameRate = newFrameRate;
            PlayerPrefs.SetInt("FrameRate", newFrameRate);
        }
    }

    void ChangeDisplays()
    {
        // Switch Displays
        Debug.Log("Changing Displays");
        PlayerPrefs.SetInt("UnitySelectMonitor", displayDropdown.value);
        PlayerPrefs.Save();
        Application.Quit();
    }
}
