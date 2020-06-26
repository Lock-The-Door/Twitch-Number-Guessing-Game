using UnityEngine;
using UnityEngine.UI;

public class manualStartupToken : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isEditor)
            Destroy(gameObject);
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
        Destroy(gameObject);
    }
}
