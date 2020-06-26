using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Objects and Stuff
    public LeaderboardUpdater LeaderboardUpdater;
    public QueuedGuesses guessesUi;
    public TwitchClient tc;
    public GameObject ControlPanel;
    public GameObject generatedBar;
    public TextMeshProUGUI generatedBarText;
    public GameObject guessBar;
    public TextMeshProUGUI guessBarText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI commandInfoText;

    // Settings
    public bool SingleGuessQueue = false;

    // Game Stuff
    private int currentNumber;
    public bool started = false;

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
        StartCoroutine(DisplayNextGuess());
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

        return name + ", your guess of " + guess + " has been queued"; //Queued
    }

    IEnumerator DisplayNextGuess()
    {
        yield return new WaitUntil(() => queuedGuesses.Count > 0);
        KeyValuePair<string, int> nextGuess = queuedGuesses.Dequeue();
        guessesUi.Dequeue();

        string name = tc.GetDisplayName(nextGuess.Key).Result;

        infoText.text = $"{name} guessed {nextGuess.Value}!!!";
        guessBarText.text = $"{name}'s guess";
        guessBar.SendMessage("ChangeValue", nextGuess.Value);

        UpdateLeaderboard(nextGuess.Key, LeaderboardUpdater.LeaderboardType.MostGuesses); // Update Leaderboard

        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => guessBar.GetComponent<ChangeBarValue>().idle);
        yield return new WaitForSeconds(1);
        if (nextGuess.Value == currentNumber) // The number is correct, restart
        {
            UpdateLeaderboard(nextGuess.Key, LeaderboardUpdater.LeaderboardType.MostWins); // Update Leaderboard
            infoText.text = $"And {nextGuess.Value} is correct! {name} has guessed the number and wins!!!";
            generatedBarText.text = "The number was " + currentNumber;
            guessBarText.text = name + " won!";
            yield return new WaitForSeconds(5);
            infoText.text = "Restarting...";
            yield return new WaitForSeconds(2);
            StopAndRestart();
        }
        else
        {
            string highOrLow = nextGuess.Value > currentNumber ? "high" : "small";
            infoText.text = $"But {nextGuess.Value} is too {highOrLow}!";
        }


        if (DistanceFromNumber(nextGuess.Value) < DistanceFromNumber(bestGuess.Value) || bestGuess.Key == null)
            bestGuess = new KeyValuePair<string, int>(name, nextGuess.Value);

        yield return new WaitForSeconds(3);

        DisplayBestGuess();
        StartCoroutine(DisplayNextGuess());
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
        string highOrLow = bestGuess.Value > currentNumber ? "high" : "small";
        infoText.text = $"Best guess is too {highOrLow} at {bestGuess.Value} by {bestGuess.Key}!!!";
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
        infoText.text = "Starting next game...";
        commandInfoText.text = "Get ready to !guess";


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
        yield return new WaitUntil(() => generatedBar.GetComponent<ChangeBarValue>().idle && guessBar.GetComponent<ChangeBarValue>().idle);

        infoText.text = "No Guesses! Be the first to guess!";
        commandInfoText.text = "Guess with !guess";
        started = true;

        StartCoroutine(DisplayNextGuess());
    }
}