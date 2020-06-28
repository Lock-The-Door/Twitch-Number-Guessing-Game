﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Models.v5.Channels;
using TwitchLib.Client.Enums;
using UnityEngine;


class Alert
{
    public string alert;
    public string message;
}

public class TwitchAlerts : MonoBehaviour
{
    public TwitchClient tc;

    Queue<Alert> queuedAlerts = new Queue<Alert>();
    private bool displayingAlert = false;

    // Y values
    float hidden = -150;
    float shown = 315;


    public float guiMoveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndSubscribe());
    }

    IEnumerator WaitAndSubscribe()
    {
        // Wait for everything to be ready
        yield return new WaitUntil(() => tc.api.Settings.AccessToken != null);
        yield return new WaitUntil(() => tc.followerServiceReady);

        // Subscribe to events
        tc.followerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;
        tc.client.OnNewSubscriber += Client_OnNewSubscriber;
        tc.client.OnReSubscriber += Client_OnReSubscriber;
    }

    private void Client_OnReSubscriber(object sender, TwitchLib.Client.Events.OnReSubscriberArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void Client_OnNewSubscriber(object sender, TwitchLib.Client.Events.OnNewSubscriberArgs e)
    {
        if (e.Channel != tc.client.JoinedChannels[0].Channel)
            return;

        string subscriptionMessage = $"{e.Subscriber.DisplayName} just subscribed ";
        subscriptionMessage += e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime ? "with Twitch Prime!" : "at " + e.Subscriber.SubscriptionPlanName + "!!!";

        Alert alert = new Alert()
        {
            alert = subscriptionMessage,
            message = null
        };
        QueueAlert(alert);
    }

    private void FollowerService_OnNewFollowersDetected(object sender, TwitchLib.Api.Services.Events.FollowerService.OnNewFollowersDetectedArgs e)
    {
        foreach (IFollow newFollower in e.NewFollowers)
        {
            Alert alert = new Alert
            {
                alert = newFollower.User.DisplayName + " just followed!",
                message = null
            };

            QueueAlert(alert);
        }
    }

    void QueueAlert(Alert alert)
    {
        if (!displayingAlert)
        {
            StartCoroutine(ShowAlert(alert));
            return;
        }

        queuedAlerts.Enqueue(alert);
    }

    IEnumerator ShowAlert(Alert alert)
    {
        displayingAlert = true;

        gameObject.GetComponent<TextMeshProUGUI>().text = alert.alert;
        gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = alert.message;

        yield return moveGui(shown);

        yield return new WaitForSeconds(5);

        yield return moveGui(hidden);

        if (queuedAlerts.Count > 0)
            StartCoroutine(ShowAlert(queuedAlerts.Dequeue()));
        else
            displayingAlert = false;
    }

    IEnumerator moveGui(float destination)
    {
        float start = gameObject.GetComponent<RectTransform>().position.y;
        float time = 0;
        while (time < 1)
        {
            gameObject.GetComponent<RectTransform>().position = new Vector3(gameObject.GetComponent<RectTransform>().position.x, Mathf.SmoothStep(start, destination, time));
            time += Time.deltaTime * guiMoveSpeed;
            yield return new WaitForFixedUpdate();
        }
    }
}