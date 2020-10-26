using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VisualQueueManager : MonoBehaviour
{
    public GameObject queueObjectPrefab;
    public Sprite maskSprite;
    public float lerpSpeed = 1;

    List<GameObject> queueObjects = new List<GameObject>();

    public void AddQueue(string queuedGuess)
    {
        //setup
        GameObject queueObject = Instantiate(queueObjectPrefab, transform);
        Transform mask = queueObject.transform.GetChild(1);
        int queueObjectsCount = queueObjects.Count - (queueObjects.Count != 0 ? (queueObjects.First().GetComponent<GuessObject>().Removing ? 1 : 0) : 0);
        mask.SetParent(transform);
        mask.localPosition = new Vector3(0, -0.75f * (queueObjectsCount + 1), 0);
        queueObject.transform.localPosition = queueObjects.Count == 0 ? new Vector3(0, -0.75f * queueObjectsCount, 0) : queueObjects.Last().transform.localPosition;
        queueObjects.Add(queueObject);
        queueObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = queuedGuess;
        queueObject.GetComponent<SpriteRenderer>().sortingOrder = ++queueObjectsCount;
        queueObject.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = queueObjectsCount;

        //lerp ready
        GuessObject guessObjectProperties = queueObject.GetComponent<GuessObject>();
        guessObjectProperties.Mask = mask;
        guessObjectProperties.OriginY = queueObject.transform.localPosition.y;
        guessObjectProperties.OriginTextAlpha = 0;
        guessObjectProperties.TargetY = -0.75f * queueObjectsCount;
        guessObjectProperties.TargetTextAlpha = 1;
        guessObjectProperties.Idle = false;

        //update queue length
        transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Queued Guesses ({queueObjects.Count})";
    }

    public void Dequeue()
    {
        // get objects
        GameObject queueObjectToRemove = queueObjects.First();
        GuessObject queueObjectToRemoveProperties = queueObjectToRemove.GetComponent<GuessObject>();

        // prepare removal
        // mask creation
        GameObject mask = new GameObject("BG Mask Removing");
        mask.transform.parent = transform;
        mask.AddComponent<SpriteMask>().sprite = maskSprite;
        mask.transform.localScale = new Vector3(400, 100);
        mask.transform.localPosition = queueObjectToRemove.transform.localPosition;
        queueObjectToRemove.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask; // renable masking
        // set properties
        queueObjectToRemoveProperties.Mask = mask.transform;
        queueObjectToRemoveProperties.OriginTextAlpha = 1;
        queueObjectToRemoveProperties.TargetTextAlpha = 0;
        queueObjectToRemoveProperties.Removing = true;
        // y will be set in foreach loop

        // shift everything up
        foreach (var queueObject in queueObjects)
        {
            GuessObject properties = queueObject.GetComponent<GuessObject>();

            properties.OriginY = queueObject.transform.localPosition.y;
            properties.TargetY = -0.75f * queueObjects.FindIndex(obj => obj == queueObject);
            properties.Idle = false;
        }
    }

    public void Clear()
    {
        foreach (GameObject queueObject in queueObjects)
        {
            GuessObject properties = queueObject.GetComponent<GuessObject>();

            // prepare removal
            // mask creation
            GameObject mask = new GameObject("BG Mask Removing");
            mask.transform.parent = transform;
            mask.AddComponent<SpriteMask>().sprite = maskSprite;
            mask.transform.localScale = new Vector3(400, 100);
            mask.transform.localPosition = queueObject.transform.localPosition;
            queueObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask; // renable masking
            // set properties
            properties.Mask = mask.transform;
            properties.OriginTextAlpha = 1;
            properties.TargetTextAlpha = 0;
            properties.Removing = true;
            properties.OriginY = queueObject.transform.localPosition.y;
            properties.TargetY = 0;
            properties.Idle = false;
        }
    }

    private void Update()
    {
        // lerp the guesses as needed
        foreach (GameObject queueObject in queueObjects)
        {
            var properties = queueObject.GetComponent<GuessObject>(); // get lerp properties
            if (properties.Idle)
                continue;

            // lerp the properties
            properties.LerpTime += Time.deltaTime;
            if (properties.LerpTime > lerpSpeed)
                properties.LerpTime = lerpSpeed;
            float t = properties.LerpTime / lerpSpeed;
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            queueObject.transform.localPosition = new Vector3(0, Mathf.Lerp(properties.OriginY, properties.TargetY, t)); // lerp pos
            var textColor = queueObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color;
            queueObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(textColor.r, textColor.g, textColor.b, Mathf.Lerp(properties.OriginTextAlpha, properties.TargetTextAlpha, t));

            if (t == 1)
            {
                properties.Idle = true;
                properties.LerpTime = 0;

                int sortingOrder = queueObjects.FindIndex(obj => obj == queueObject);
                queueObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
                queueObject.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = sortingOrder;

                // delete mask
                if (properties.Mask != null)
                {
                    queueObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
                    Destroy(properties.Mask.gameObject);
                }

                // see if removal
                if (properties.Removing)
                {
                    queueObjects.Remove(queueObject);
                    Destroy(queueObject);

                    //update queue length
                    transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Queued Guesses ({queueObjects.Count})";
                }
            }
        }
    }

    private void Start()
    {
        //StartCoroutine(testEnqueue());
        //StartCoroutine(testDequeue());
    }

    IEnumerator testEnqueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            AddQueue("Tester - 69");
            yield return new WaitForSeconds(1);
            AddQueue("I do be testing doe - 1000");
            AddQueue("I am mister funny man - 420");
        }
    }

    IEnumerator testDequeue()
    {
        yield return new WaitForSeconds(3);
        while (true)
        {
            yield return new WaitForSeconds(3);
            Dequeue();
        }
    }
}
