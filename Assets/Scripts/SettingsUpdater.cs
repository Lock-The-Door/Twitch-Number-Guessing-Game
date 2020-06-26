using UnityEngine;
using UnityEngine.UI;

public class SettingsUpdater : MonoBehaviour
{
    public Toggle limitGuesses;
    public GameManager GameManager;

    private void Start()
    {
        // Add Listeners
        limitGuesses.onValueChanged.AddListener(limitGuessesChanged);
    }

    void limitGuessesChanged(bool newValue)
    {
        GameManager.SingleGuessQueue = newValue;
        PlayerPrefs.SetString("Single Guess Queue", newValue.ToString());
    }
}
