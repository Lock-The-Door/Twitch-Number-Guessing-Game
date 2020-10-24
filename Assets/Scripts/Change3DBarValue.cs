using UnityEngine;

public class Change3DBarValue : MonoBehaviour
{
    public float yOffset = 3;
    public float speed = 1;

    float endScaleValue;
    float startValue;
    float currentLerpTime = 0;
    float t = 0;

    public bool idle = true;

    // Update is called once per frame
    void Update()
    {
        if (t < 1 && !idle)
        {
            currentLerpTime += Time.deltaTime;
            t = currentLerpTime / speed;
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            gameObject.GetComponent<SpriteRenderer>().size = new Vector2(gameObject.GetComponent<SpriteRenderer>().size.x, Mathf.Lerp(startValue, endScaleValue, t));

            gameObject.transform.position = new Vector3(gameObject.transform.position.x, (gameObject.GetComponent<SpriteRenderer>().size.y + yOffset)/2, 0); // move position based on scale
        }
        else
        {
            idle = true;
            currentLerpTime = 0;
            t = 0;
        }
    }

    void ChangeValue(float value)
    {
        endScaleValue = value * 0.008f;
        startValue = gameObject.GetComponent<SpriteRenderer>().size.y;

        idle = false;
    }
}
