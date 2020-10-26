﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Objects and Stuff
    public new Camera camera;
    public LeaderboardUpdater LeaderboardUpdater;
    public VisualQueueManager guessesUi;
    public TwitchClient tc;
    public GameObject ControlPanel;
    public GameObject generatedBar;
    public TextMeshProUGUI generatedBarText;
    public GameObject guessBar;
    public TextMeshProUGUI guessBarText;
    public TextMeshProUGUI guessNumberText;
    public TextMeshProUGUI guesserText;
    public TextMeshProUGUI guessVerifyText;
    public Image guessVerifyColour;
    public Color colourIncorrect = new Color(0.8901961f, 0.2470588f, 0.4156863f);
    public Color colourCorrect = new Color(0.4196078f, 0.8862745f, 0.2470588f);
    public Color colourYield = new Color(0.8862745f, 0.7176471f, 0.2470588f);
    public TextMeshProUGUI commandInfoText;

    // Settings
    public bool SingleGuessQueue = false;

    // Game Stuff
    private int currentNumber;
    public bool started = false;
    bool displayingGuess = false;

    private GameObject openedControlPanel;

    // Start is called before the first frame update
    void Start()
    {
        // Load settings
        if (PlayerPrefs.HasKey("Single Guess Queue"))
            SingleGuessQueue = bool.Parse(PlayerPrefs.GetString("Single Guess Queue"));

        // Set bars to 0
        generatedBar.SendMessage("ChangeValue", value: 0);
        guessBar.SendMessage("ChangeValue", value: 0);

        // Start co-routines
        StartCoroutine(StartFunctions());
    }

    IEnumerator StartFunctions()
    {
        yield return new WaitForSeconds(3);
        currentNumber = Random.Range(1, 1001);
        generatedBar.SendMessage("ChangeValue", currentNumber);
        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (openedControlPanel == null)
            {
                openedControlPanel = Instantiate(ControlPanel);
                var settingsScript = openedControlPanel.GetComponent<SettingsUpdater>();
                settingsScript.limitGuesses.isOn = SingleGuessQueue;
                settingsScript.GameManager = gameObject.GetComponent<GameManager>();
                settingsScript.GetComponent<Canvas>().worldCamera = camera;
                settingsScript.GetComponent<Canvas>().planeDistance = 2;
            }
            else
            {
                Destroy(openedControlPanel);
            }
        }
    }

    Queue<KeyValuePair<string, int>> queuedGuesses = new Queue<KeyValuePair<string, int>>();
    KeyValuePair<string, int> bestGuess = new KeyValuePair<string, int>();

    public string QueueGuess(string id, int guess, string name)
    {
        if (!started)
            return "The game is still starting please wait...";

        if (SingleGuessQueue)
        {
            Debug.LogWarning("Single Guess Enabled");
            foreach (KeyValuePair<string, int> queuedGuess in queuedGuesses)
            {
                Debug.Log(queuedGuess);
                if (queuedGuess.Key == id)
                    return name + ", due to a high rate of guesses, you can only guess 1 number at a time.";
            }
        }
        queuedGuesses.Enqueue(new KeyValuePair<string, int>(id, guess));
        guessesUi.AddQueue(name + " - " + guess);

        // Start up co-routine if not running
        if (!displayingGuess)
        {
            StartCoroutine(DisplayNextGuess());
        }

        return name + ", your guess of " + guess + " has been queued"; //Queued
    }

    IEnumerator DisplayNextGuess()
    {
        displayingGuess = true;

        KeyValuePair<string, int> nextGuess = queuedGuesses.Dequeue();
        guessesUi.Dequeue();

        string name = tc.GetDisplayName(nextGuess.Key).Result;

        guesserText.text = $"{name} guessed:";
        guessNumberText.text = nextGuess.Value.ToString();
        guessBarText.text = $"{name}'s guess";
        guessVerifyText.text = "...";
        guessVerifyColour.color = colourYield;
        guessBar.SendMessage("ChangeValue", nextGuess.Value);

        UpdateLeaderboard(nextGuess.Key, LeaderboardUpdater.LeaderboardType.MostGuesses); // Update Leaderboard

        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => guessBar.GetComponent<Change3DBarValue>().idle);
        yield return new WaitForSeconds(1);
        if (nextGuess.Value == currentNumber) // The number is correct, restart
        {
            UpdateLeaderboard(nextGuess.Key, LeaderboardUpdater.LeaderboardType.MostWins); // Update Leaderboard
            guessVerifyText.text = "Correct!";
            guessVerifyColour.color = colourCorrect;
            generatedBarText.text = "The number was " + currentNumber;
            guessBarText.text = name + " won!";
            yield return new WaitForSeconds(5);
            guessVerifyText.text = "Restarting...";
            guessVerifyColour.color = colourYield;
            yield return new WaitForSeconds(2);
            StopAndRestart();
        }
        else
        {
            string highOrLow = nextGuess.Value > currentNumber ? "high" : "low";
            guessVerifyText.text = $"Too {highOrLow}!";
            guessVerifyColour.color = colourIncorrect;
        }


        if (DistanceFromNumber(nextGuess.Value) < DistanceFromNumber(bestGuess.Value) || bestGuess.Key == null)
            bestGuess = new KeyValuePair<string, int>(name, nextGuess.Value);

        yield return new WaitForSeconds(3);

        if (queuedGuesses.Count > 0)
            StartCoroutine(DisplayNextGuess());
        else
        {
            displayingGuess = false;
            DisplayBestGuess();
        }
    }

    void UpdateLeaderboard(string id, LeaderboardUpdater.LeaderboardType type)
    {
        switch (type)
        {
            case LeaderboardUpdater.LeaderboardType.MostWins:
                ulong oldWins;
                LeaderboardUpdater.mostWins.TryGetValue(id, out oldWins);
                LeaderboardUpdater.mostWins[id] = ++oldWins;
                break;
            case LeaderboardUpdater.LeaderboardType.MostGuesses:
                ulong oldGuesses;
                LeaderboardUpdater.mostGuesses.TryGetValue(id, out oldGuesses);
                LeaderboardUpdater.mostGuesses[id] = ++oldGuesses;
                break;
        }

        LeaderboardUpdater.SaveStats();
        if (LeaderboardUpdater.leaderboardType == type)
            LeaderboardUpdater.LoadLeaderboard(type);
    }

    void DisplayBestGuess()
    {
        string highOrLow = bestGuess.Value > currentNumber ? "high" : "low";
        guesserText.text = $"Best guess by {bestGuess.Key}:";
        guessNumberText.text = bestGuess.Value.ToString();
        guessVerifyText.text = $"Too {highOrLow}!";
        guessVerifyColour.color = colourIncorrect;
        guessBarText.text = $"Best Guess ({bestGuess.Value})";
        guessBar.SendMessage("ChangeValue", bestGuess.Value);
    }

    int DistanceFromNumber(int guess)
    {
        int rawDistance = currentNumber - guess;
        if (rawDistance < 0)
            rawDistance *= -1;
        return rawDistance;
    }

    void StopAndRestart()
    {
        StopAllCoroutines();
        StartCoroutine(RestartGame());
    }

    IEnumerator RestartGame(int predefinedNumber = 0)
    {
        // Prevent guesses and display the status
        started = false;
        guesserText.text = "Starting next game...";
        guessNumberText.text = "Get ready to !guess";

        // Reset displaying guesses state
        displayingGuess = false;

        // Clear the guess queue and reset the best guess
        queuedGuesses.Clear();
        bestGuess = new KeyValuePair<string, int>(null, 0);
        //Clear guesses gui
        guessesUi.Clear();

        // Reset the guess bar
        guessBar.SendMessage("ChangeValue", value: 0);
        guessBarText.text = "No Guesses! Be the first to guess!";

        // Set the new number
        if (predefinedNumber == 0)
            predefinedNumber = Random.Range(1, 1001);
        currentNumber = predefinedNumber;
        generatedBar.SendMessage("ChangeValue", predefinedNumber);
        generatedBarText.text = "The Unknown Number";
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => generatedBar.GetComponent<Change3DBarValue>().idle && guessBar.GetComponent<Change3DBarValue>().idle);

        // Reset the guess info
        guesserText.text = "No guesses...";
        guessNumberText.text = "0-1000";
        guessVerifyText.text = "Be the first to guess!";
        guessVerifyColour.color = colourYield;
        //commandInfoText.text = "Guess with !guess";

        // Started!!!
        started = true;
    }
}