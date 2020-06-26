using UnityEngine;

public class ChangeBarValue : MonoBehaviour
{
    public float speed;

    float endScaleValue;
    float startValue;
    float t = 0;

    public bool idle = true;

    // Update is called once per frame
    void Update()
    {
        if (t < 1 && !idle)
        {
            t += speed * Time.deltaTime;
            gameObject.transform.localScale = new Vector3(1, Mathf.Lerp(startValue, endScaleValue, t));

            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.5f * gameObject.transform.localScale.y - 3.5f, 0); // move position based on scale
        }
        else
            idle = true;
    }

    void ChangeValue(float value)
    {
        idle = false;

        endScaleValue = value / 100;
        startValue = gameObject.transform.localScale.y;
        t = 0;
    }
}
