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

    public static string loadedLeaderboardName;

    public static DateTime weeklyReset;
    public static Dictionary<DateTime, List<LeaderboardData>> weeklyLeaderboardArchives = new Dictionary<DateTime, List<LeaderboardData>>();

    public static List<LeaderboardData> leaderboardDatas = new List<LeaderboardData>()
    {
        new LeaderboardData("Most Wins", "(All Time)", new Dictionary<string, ulong>()),
        new LeaderboardData("Most Guesses", "(All Time)", new Dictionary<string, ulong>()),
        new LeaderboardData("Most Wins", "(Weekly)", new Dictionary<string, ulong>()),
        new LeaderboardData("Most Wins", "(Last Week)", new Dictionary<string, ulong>()),
        new LeaderboardData("Most Guesses", "(Weekly)", new Dictionary<string, ulong>()),
        new LeaderboardData("Most Guesses", "(Last Week)", new Dictionary<string, ulong>())
    };

    private void Start()
    {
        LoadStats();

        StartCoroutine(LeaderboardSwitcher());

        StartCoroutine(leaderboardReseter());
    }

    IEnumerator LeaderboardSwitcher()
    {
        yield return new WaitUntil(() => tc.api.Settings.AccessToken != null);
        while (true)
        {
            foreach (LeaderboardData leaderboard in leaderboardDatas)
            {
                yield return FadeLeaderboard(false);
                LoadLeaderboard(leaderboard);
                loadedLeaderboardName = leaderboard.fullName;
                yield return FadeLeaderboard(true);
                yield return new WaitForSeconds(switchRate);
            }
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

    public void LoadLeaderboard(LeaderboardData leaderboardData)
    {
        SortLeaderboards();

        transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{leaderboardData.typeName}\n{leaderboardData.timeframeName}";

        for (int placement = 0; placement < 10; placement++)
        {
            Transform panel = transform.GetChild(1).GetChild(placement);

            if (leaderboardData.data.Count <= placement) // no data here
            {
                panel.GetChild(1).GetComponent<TextMeshProUGUI>().text = "-"; // Put in the value text
                panel.GetChild(2).GetComponent<TextMeshProUGUI>().text = "No one!"; // Put in the username
                continue;
            }

            var placementValues = leaderboardData.data.ElementAt(placement);
            string username = tc.GetDisplayName(placementValues.Key).Result;

            panel.GetChild(1).GetComponent<TextMeshProUGUI>().text = placementValues.Value.ToString(); // Put in the value text
            panel.GetChild(2).GetComponent<TextMeshProUGUI>().text = username; // Put in the username
        }
    }

    static void SortLeaderboards()
    {
        foreach (LeaderboardData leaderboardData in leaderboardDatas)
        {
            var dataList = leaderboardData.data.ToList();
            dataList.Sort((pair1, pair2) =>
            {
                var valueComparison = pair1.Value.CompareTo(pair2.Value) * -1;
                if (valueComparison == 0)
                    return pair1.Key.CompareTo(pair2.Key);
                return valueComparison;
            });
            leaderboardData.data = dataList.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    IEnumerator leaderboardReseter()
    {
        //weekly
        if (weeklyReset == null || DateTime.Today >= weeklyReset)
        {
            // Recalculate next week
            DateTime today = DateTime.Today;
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysUntilNextWeek = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            DateTime nextWeek = daysUntilNextWeek == 0 ? today.AddDays(7) : today.AddDays(daysUntilNextWeek);
            Debug.Log(nextWeek);
            weeklyReset = nextWeek;

            // Reset leaderboards and move this week to last week
            weeklyLeaderboardArchives.Add(DateTime.Now, leaderboardDatas.FindAll(leaderboards => leaderboards.timeframeName == "(Weekly)")); // make archive

            List<LeaderboardData> lastWeekLeaderboards = leaderboardDatas.FindAll(leaderboards => leaderboards.timeframeName == "(Last Week)");
            foreach (LeaderboardData leaderboard in lastWeekLeaderboards)
            {
                leaderboardDatas.Remove(leaderboard);
                var weeklyLeaderboardIndex = leaderboardDatas.FindIndex(leaderboardData => leaderboardData.typeName == leaderboard.typeName && leaderboardData.timeframeName == "(Weekly)");
                leaderboardDatas[weeklyLeaderboardIndex].timeframeName = "(Last Week)";
                leaderboardDatas[weeklyLeaderboardIndex].fullName = $"{leaderboard.typeName} (Last Week)";
                leaderboardDatas.Add(new LeaderboardData(leaderboard.typeName, "(Weekly)", new Dictionary<string, ulong>()));
            }
        }

        yield return new WaitForSecondsRealtime(1);
    }

    void LoadStats()
    {
        if (File.Exists(Application.persistentDataPath
               + "/Leaderboard-v2.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file =
                       File.Open(Application.persistentDataPath
                       + "/Leaderboard-v2.dat", FileMode.Open);
            LeaderboardV2 data = (LeaderboardV2)bf.Deserialize(file);
            file.Close();

            foreach (LeaderboardData leaderboard in data.activeLeaderboards)
            {
                var emptyLeaderboard = leaderboardDatas.Find(leaderboardData => leaderboardData.fullName == leaderboard.fullName);
                leaderboardDatas.Remove(emptyLeaderboard);
                leaderboardDatas.Add(leaderboard);
            }

            weeklyReset = data.weeklyResetDate;
            weeklyLeaderboardArchives = data.weeklyLeaderboardArchives;

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
                     + "/Leaderboard-v2.dat");

        LeaderboardV2 leaderboards = new LeaderboardV2();
        leaderboards.activeLeaderboards = leaderboardDatas;

        bf.Serialize(file, leaderboards);
        file.Close();
        Debug.Log("Data saved!");
    }
}

[Serializable]
class LeaderboardV2
{
    public List<LeaderboardData> activeLeaderboards;

    // reset dates
    public DateTime weeklyResetDate;

    // archives
    public Dictionary<DateTime, List<LeaderboardData>> weeklyLeaderboardArchives;
}
[Serializable]
class Leaderboard
{
    public Dictionary<string, ulong> mostWins;
    public Dictionary<string, ulong> mostGuesses;
}



[Serializable]
public class LeaderboardData
{
    public Dictionary<string, ulong> data;
    public string typeName;
    public string timeframeName;
    public string fullName;

    public LeaderboardData(string _typeName, string _timeframeName, Dictionary<string, ulong> _data)
    {
        data = _data;
        typeName = _typeName;
        timeframeName = _timeframeName;
        fullName = $"{typeName} {timeframeName}";
    }
}