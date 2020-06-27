using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QueuedGuesses : MonoBehaviour
{
    public GameObject guessPrefab;
    Queue<GameObject> queuedGuesses = new Queue<GameObject>();
    public int guessesOnScreen;
    public float guiSpeed;

    TextMeshProUGUI title;

    private void Start()
    {
        title = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void AddQueue(string guessInfo)
    {
        GameObject guess = Instantiate(guessPrefab, gameObject.transform);
        queuedGuesses.Enqueue(guess);
        guess.GetComponent<TextMeshProUGUI>().text = guessInfo;
        float guessY = -5 - 50 * queuedGuesses.Count;
        guess.GetComponent<RectTransform>().localPosition = new Vector3(430, guessY);
        if (queuedGuesses.Count - 1 < guessesOnScreen)
            StartCoroutine(LerpGuess(guess, false));

        title.text = $"Queued Guesses ({queuedGuesses.Count})";
    }

    public void Dequeue()
    {
        GameObject guess = queuedGuesses.Dequeue();
        StartCoroutine(LerpGuess(guess, true));// Lerp and destroy
        foreach (GameObject otherguess in queuedGuesses)
            StartCoroutine(ShiftGuess(otherguess));

        title.text = $"Queued Guesses ({queuedGuesses.Count})";
    }

    public void Clear()
    {
        while (queuedGuesses.Count > 0)
        {
            GameObject guess = queuedGuesses.Dequeue();
            StartCoroutine(LerpGuess(guess, true));
        }
        title.text = $"Queued Guesses (0)";
    }

    IEnumerator LerpGuess(GameObject guess, bool isDequeue)
    {
        float start = guess.GetComponent<RectTransform>().localPosition.x;
        float destination = isDequeue ? 430 : 0;
        float time = 0;
        while (time < 1)
        {
            guess.GetComponent<RectTransform>().localPosition = new Vector3(Mathf.SmoothStep(start, destination, time), guess.GetComponent<RectTransform>().localPosition.y);
            time += Time.deltaTime * guiSpeed;
            yield return new WaitForFixedUpdate();
        }

        if (isDequeue)//Destroy if queueing
            Destroy(guess);
    }

    IEnumerator ShiftGuess(GameObject guess)
    {
        float start = guess.GetComponent<RectTransform>().localPosition.y;
        float destination = start + 50;
        float time = 0;
        while (time < 1)
        {
            guess.GetComponent<RectTransform>().localPosition = new Vector3(guess.GetComponent<RectTransform>().localPosition.x, Mathf.SmoothStep(start, destination, time));
            time += Time.deltaTime * guiSpeed / 2;
            yield return new WaitForFixedUpdate();
        }

        if (queuedGuesses.Count >= guessesOnScreen && queuedGuesses.ElementAt(guessesOnScreen - 1) == guess)// Shift in if necessary
            StartCoroutine(LerpGuess(guess, false));
    }
}
