using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public TextMeshProUGUI musicInfoText;
    public Music playing;

    List<Music> musicList = new List<Music>();

    void Start()
    {
        foreach (AudioSource audio in gameObject.transform.GetChild(1).GetComponentsInChildren<AudioSource>())
        {
            musicList.Add(audio.gameObject.GetComponent<Music>());
        }

        musicList = musicList.OrderBy(x => UnityEngine.Random.Range(-1, 2)).ToList();

        StartCoroutine(musicLoop(0));
    }

    IEnumerator musicLoop(int i)
    {
        //Debug.Log("Playing next music on queue...");
        Music musicInfo = musicList[i];
        AudioSource audio = musicInfo.AudioSource;
        audio.Play();
        musicInfoText.text = $"{musicInfo.Title}\nby {musicInfo.Artist.Name}";
        double length = audio.clip.samples / audio.clip.frequency;
        yield return new WaitForSecondsRealtime(Convert.ToSingle(length + 2));
        if (++i == musicList.Count)
            i = 0;
        StartCoroutine(musicLoop(i));
    }
}