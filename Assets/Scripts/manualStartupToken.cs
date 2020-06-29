using UnityEngine;
using UnityEngine.UI;

public class manualStartupToken : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isEditor)
            Destroy(gameObject);
        else if (PlayerPrefs.HasKey("API_TOKEN"))
        {
            SecretGetter.Api_Token = PlayerPrefs.GetString("API_TOKEN");
            Destroy(gameObject);
        }
        else
            Debug.Log(SecretGetter.Api_Token);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            UpdateToken();
    }

    void UpdateToken()
    {
        SecretGetter.Api_Token = gameObject.GetComponent<InputField>().text;
        PlayerPrefs.SetString("API_TOKEN", SecretGetter.Api_Token);
        Destroy(gameObject);
    }
}
