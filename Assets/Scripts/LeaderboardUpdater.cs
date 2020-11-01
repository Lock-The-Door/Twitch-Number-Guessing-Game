using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class LeaderboardUpdater : MonoBehaviour
{
    public TwitchClient tc;
    public float switchRate;
    public float guiMoveSpeed;

    //public static List<KeyValuePair<string, ulong>> mostWins = new List<KeyValuePair<string, ulong>>();
    //public static List<KeyValuePair<string, ulong>> mostGuesses = new List<KeyValuePair<string, ulong>>();
    public static Dictionary<string, ulong> mostWins = new Dictionary<string, ulong>();
    public static Dictionary<string, ulong> mostGuesses = new Dictionary<string, ulong>();
    //public KeyValuePair<string, double> mostAccurate;

    private void Start()
    {
        LoadStats();

        StartCoroutine(LeaderboardSwitcher());
    }

    IEnumerator LeaderboardSwitcher()
    {
        yield return new WaitUntil(() => tc.api.Settings.AccessToken != null);
        while (true)
        {
            yield return FadeLeaderboard(false);
            LoadLeaderboard(mostWins, "Most Wins\n(All Time)");
            yield return FadeLeaderboard(true);
            yield return new WaitForSeconds(switchRate);
            yield return FadeLeaderboard(false);
            LoadLeaderboard(mostGuesses, "Most Guesses\n(All Time)");
            yield return FadeLeaderboard(true);
            yield return new WaitForSeconds(switchRate);
        }
    }

    IEnumerator FadeLeaderboard(bool fadeIn)
    {
        float originTransparency = fadeIn ? 0 : 1;
        float targetTransparency = fadeIn ? 1 : 0;
        float lerpTime = 0;

        while (lerpTime < guiMoveSpeed)
        {
            lerpTime += Time.deltaTime;
            if (lerpTime > guiMoveSpeed)
                lerpTime = guiMoveSpeed;
            float t = lerpTime / guiMoveSpeed;
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            // lerp transparency on header
            var headerText = transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
            headerText.color = new Color(headerText.color.r, headerText.color.g, headerText.color.b, Mathf.Lerp(originTransparency, targetTransparency, t));
            // lerp leaderboard values
            for (int placement = 0; placement < 10; placement++)
            {
                Transform panel = transform.GetChild(1).GetChild(placement);

                var valueText = panel.GetChild(1).GetComponent<TextMeshProUGUI>();
                var usernameText = panel.GetChild(2).GetComponent<TextMeshProUGUI>();

                valueText.color = new Color(valueText.color.r, valueText.color.g, valueText.color.b, Mathf.Lerp(originTransparency, targetTransparency, t));
                usernameText.color = new Color(usernameText.color.r, usernameText.color.g, usernameText.color.b, Mathf.Lerp(originTransparency, targetTransparency, t));
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void LoadLeaderboard(Dictionary<string, ulong> leaderboardDictionary, string leaderboardType)
    {
        SortLeaderboards();

        transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = leaderboardType;

        for (int placement = 0; placement < 10; placement++)
        {
            Transform panel = transform.GetChild(1).GetChild(placement);
            var placementValues = leaderboardDictionary.ElementAt(placement);
            string username = tc.GetDisplayName(placementValues.Key).Result;

            panel.GetChild(1).GetComponent<TextMeshProUGUI>().text = placementValues.Value.ToString(); // Put in the value text
            panel.GetChild(2).GetComponent<TextMeshProUGUI>().text = username; // Put in the username
        }
    }

    static void SortLeaderboards()
    {
        var mostWinsList = mostWins.ToList();
        mostWinsList.Sort((pair1, pair2) =>
        {
            var valueComparison = pair1.Value.CompareTo(pair2.Value) * -1;
            if (valueComparison == 0)
                return pair1.Key.CompareTo(pair2.Key);
            return valueComparison;
        });
        mostWins = mostWinsList.ToDictionary(x => x.Key, x => x.Value);

        var mostGuessesList = mostGuesses.ToList();
        mostGuessesList.Sort((pair1, pair2) =>
        {
            var valueComparison = pair1.Value.CompareTo(pair2.Value) * -1;
            if (valueComparison == 0)
                return pair1.Key.CompareTo(pair2.Key);
            return valueComparison;
        });
        mostGuesses = mostGuessesList.ToDictionary(x => x.Key, x => x.Value);
    }

    void LoadStats()
    {
        if (File.Exists(Application.persistentDataPath
               + "/Leaderboard.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file =
                       File.Open(Application.persistentDataPath
                       + "/Leaderboard.dat", FileMode.Open);
            Leaderboard data = (Leaderboard)bf.Deserialize(file);
            file.Close();
            mostWins = data.mostWins;
            mostGuesses = data.mostGuesses;
            Debug.Log("Data loaded!");
        }
        else
            Debug.LogWarning("There is no data!");
    }

    public bool save = true;
    public void SaveStats()
    {
        if (!save)
        {
            Debug.LogWarning("Saving is OFF!");
            return;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath
                     + "/Leaderboard.dat");
        Leaderboard data = new Leaderboard();
        data.mostWins = mostWins;
        data.mostGuesses = mostGuesses;
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Data saved!");
    }
}

[Serializable]
class Leaderboard
{
    public Dictionary<string, ulong> mostWins;
    public Dictionary<string, ulong> mostGuesses;
    //public KeyValuePair<string, double> mostAccurate;
}