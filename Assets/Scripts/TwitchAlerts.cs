using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitchLib.Api.Interfaces;
using TwitchLib.Client.Enums;
using UnityEngine;


public class Alert
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

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            tc.client.InvokeNewSubscriber(new List<KeyValuePair<string, string>>(), "000000", System.Drawing.Color.Black, "TestSubscriber", "test", "000000", "test", "test", "test", "test", SubscriptionPlan.Tier1, "Tier 1", "test", "test", false, false, true, false, "test", UserType.Viewer, "test", "test");
        }
    }*/

    IEnumerator WaitAndSubscribe()
    {
        // Wait for everything to be ready
        yield return new WaitUntil(() => tc.api.Settings.AccessToken != null);
        yield return new WaitUntil(() => tc.pubSub != null);

        // Subscribe to events
        tc.pubSub.OnFollow += PubSub_OnFollow;
        tc.pubSub.OnChannelSubscription += PubSub_OnChannelSubscription;
    }

    private void PubSub_OnChannelSubscription(object sender, TwitchLib.PubSub.Events.OnChannelSubscriptionArgs e)
    {
        string subscriptionMessage = $"{e.Subscription.RecipientDisplayName} just subscribed for {e.Subscription.Months} months ";
        
        switch (e.Subscription.SubscriptionPlan)
        {
            case TwitchLib.PubSub.Enums.SubscriptionPlan.Prime:
                subscriptionMessage += "with Twitch Prime!";
                break;
            case TwitchLib.PubSub.Enums.SubscriptionPlan.Tier1:
                subscriptionMessage += "at Tier 1!!";
                break;
            case TwitchLib.PubSub.Enums.SubscriptionPlan.Tier2:
                subscriptionMessage += "at Tier 2!!!";
                break;
            case TwitchLib.PubSub.Enums.SubscriptionPlan.Tier3:
                subscriptionMessage += "at Tier 3!!!!!!";
                break;
        }

        Alert alert = new Alert()
        {
            alert = subscriptionMessage,
            message = e.Subscription.SubMessage.Message
        };
        QueueAlert(alert);
    }

    private void PubSub_OnFollow(object sender, TwitchLib.PubSub.Events.OnFollowArgs e)
    {
        Debug.Log(e.DisplayName + " has followed!");

        Alert alert = new Alert
        {
            alert = e.DisplayName + " just followed!",
            message = null
        };

        QueueAlert(alert);
    }

    private void Client_OnReSubscriber(object sender, TwitchLib.Client.Events.OnReSubscriberArgs e)
    {
        if (e.Channel != tc.client.JoinedChannels[0].Channel)
            return;

        string subscriptionMessage = $"{e.ReSubscriber.DisplayName} just resubscribed ";
        subscriptionMessage += e.ReSubscriber.SubscriptionPlan == SubscriptionPlan.Prime ? "with Twitch Prime!" : "at " + e.ReSubscriber.SubscriptionPlanName + "!!!";

        Alert alert = new Alert()
        {
            alert = subscriptionMessage,
            message = null
        };
        QueueAlert(alert);
    }

    public void QueueAlert(Alert alert)
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

        Debug.Log(alert.alert + DateTime.Now);

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
