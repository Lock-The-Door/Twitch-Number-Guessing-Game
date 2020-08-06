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

    public float guiMoveSpeed;
    public int timesToAdvertiseMusicCommand;
    int timesAdvertisedMusicCommand = 0;
    public int commandAdvertiseTime;

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
        playing = musicInfo;
        StartCoroutine(changeMusicInfoText("Now Playing\n" + musicInfo.Title + "\nBy " + musicInfo.Artist.Name));
        double length = audio.clip.samples / audio.clip.frequency;
        StartCoroutine(advertiseMusicCommand(length));
        yield return new WaitForSecondsRealtime(Convert.ToSingle(length + 2));
        if (++i == musicList.Count)
            i = 0;
        StartCoroutine(musicLoop(i));
    }

    IEnumerator advertiseMusicCommand(double length)
    {
        yield return new WaitForSecondsRealtime(Convert.ToSingle(length / (timesToAdvertiseMusicCommand + 1) - timesAdvertisedMusicCommand * commandAdvertiseTime));
        string info = musicInfoText.text;

        yield return changeMusicInfoText("Like the music?\nDo !music");

        yield return new WaitForSecondsRealtime(commandAdvertiseTime);

        yield return changeMusicInfoText(info);

        if (++timesAdvertisedMusicCommand < timesToAdvertiseMusicCommand)
            StartCoroutine(advertiseMusicCommand(length));
        else
            timesAdvertisedMusicCommand = 0;
    }

    IEnumerator changeMusicInfoText(string newtext)
    {
        var start = 770;
        var destination = 1130;
        yield return moveGui(destination);
        musicInfoText.text = newtext;
        yield return moveGui(start);
    }

    IEnumerator moveGui(float destination)
    {
        float start = musicInfoText.rectTransform.localPosition.x;
        float time = 0;
        while (time < 1)
        {
            musicInfoText.rectTransform.localPosition = new Vector3(Mathf.SmoothStep(start, destination, time), musicInfoText.rectTransform.localPosition.y, 0);
            time += Time.deltaTime * guiMoveSpeed;
            yield return new WaitForFixedUpdate();
        }
    }
}