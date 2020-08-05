using TMPro;
using UnityEngine;

public class DisplayChanger : MonoBehaviour
{
    public Camera camera;

    private void Start()
    {
        Display.main.Activate();
        var dropdown = gameObject.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.GetComponent<TMP_Dropdown>();
        var displays = new System.Collections.Generic.List<string>();
        var displayNumber = 1;
        foreach (Display display in Display.displays)
        {
            displays.Add("Display " + displayNumber);
            if (display.active)
                dropdown.value = displayNumber - 1;
            displayNumber++;
        }

        dropdown.AddOptions(displays);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            var panel = gameObject.transform.GetChild(0).gameObject;
            panel.SetActive(!panel.activeSelf);
        }
    }

    void ChangeDisplays()
    {
        var dropdown = gameObject.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.GetComponent<TMP_Dropdown>();
        // Switch Displays
        PlayerPrefs.SetInt("UnitySelectMonitor", dropdown.value);
        PlayerPrefs.Save();
        Application.Quit();
    }
}
