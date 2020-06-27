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
    public TextMeshProUGUI Leaderboard;
    public TwitchClient tc;
    public float switchRate;
    public float guiMoveSpeed;

    //public static List<KeyValuePair<string, ulong>> mostWins = new List<KeyValuePair<string, ulong>>();
    //public static List<KeyValuePair<string, ulong>> mostGuesses = new List<KeyValuePair<string, ulong>>();
    public static Dictionary<string, ulong> mostWins = new Dictionary<string, ulong>();
    public static Dictionary<string, ulong> mostGuesses = new Dictionary<string, ulong>();
    //public KeyValuePair<string, double> mostAccurate;

    public LeaderboardType leaderboardType = LeaderboardType.MostWins;

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
            LoadLeaderboard(LeaderboardType.MostWins);
            yield return new WaitForSeconds(switchRate);
            LoadLeaderboard(LeaderboardType.MostGuesses);
            yield return new WaitForSeconds(switchRate);
        }
    }

    public enum LeaderboardType
    {
        MostWins,
        MostGuesses,
        MostAccurate
    }

    public void LoadLeaderboard(LeaderboardType type)
    {
        SortLeaderboards();

        string leaderboard = "";

        switch (type)
        {
            case LeaderboardType.MostWins:
                leaderboard = "<b><size=50>Most Wins</size></b>\n";
                for (int i = 0; i < 10; i++)
                {
                    leaderboard += (i + 1) + ". ";
                    if (mostWins.Count > i)
                    {
                        string id = mostWins.ElementAt(i).Key;
                        string displayName = tc.GetDisplayName(id).Result;
                        leaderboard += displayName + " - " + mostWins[id];
                    }
                    if (i != 10)
                        leaderboard += "\n";
                }
                break;
            case LeaderboardType.MostGuesses:
                leaderboard = "<b><size=50>Most Guesses</size></b>\n";
                for (int i = 0; i < 10; i++)
                {
                    leaderboard += (i + 1) + ". ";
                    if (mostGuesses.Count > i)
                    { 
                        string id = mostGuesses.ElementAt(i).Key;
                        string displayName = tc.GetDisplayName(id).Result;
                        leaderboard += displayName + " - " + mostGuesses[id];
                    }
                    if (i != 10)
                        leaderboard += "\n";
                }
                break;
            case LeaderboardType.MostAccurate:
                break;
        }

        leaderboardType = type;
        StartCoroutine(changeLeaderboardInfoText(leaderboard, leaderboardType));
    }

    IEnumerator changeLeaderboardInfoText(string newtext, LeaderboardType type)
    {
        if (leaderboardType == type)
            Leaderboard.text = newtext;
        else
        {
            var start = 125;
            var destination = -300;
            yield return moveGui(destination);
            Leaderboard.text = newtext;
            yield return moveGui(start);
        }
    }

    IEnumerator moveGui(float destination)
    {
        float start = Leaderboard.rectTransform.position.x;
        float time = 0;
        while (time < 1)
        {
            Leaderboard.rectTransform.position = new Vector3(Mathf.SmoothStep(start, destination, time), Leaderboard.rectTransform.position.y);
            time += Time.deltaTime * guiMoveSpeed;
            yield return new WaitForFixedUpdate();
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
            Debug.LogError("There is no data!");
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