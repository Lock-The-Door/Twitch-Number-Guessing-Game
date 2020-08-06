using System.Collections;
using System.Threading.Tasks;
using TMPro;
using TwitchLib.Api.Models.v5.Users;
using TwitchLib.Client.Models;
using TwitchLib.Unity;
using UnityEngine;

public class TwitchClient : MonoBehaviour
{
    public Client client;
    public Api api = new Api();
    private string channel_name = "second_120";
    public PubSub pubSub;

    public GameObject HumanOperatorGUI;

    public GameManager gameManager;
    public BackgroundMusicManager musicManager;
    public TwitchAlerts twitchAlerts;

    void Start()
    {
        Application.runInBackground = true;

        StartCoroutine(WaitAndConnect());
    }

    IEnumerator WaitAndConnect()
    {
        // See if in editor
        if (!Application.isEditor)
            Debug.Log(SecretGetter.Api_Token);
            yield return new WaitUntil(() => SecretGetter.Api_Token != null);// If not editor wait for api token input
        Debug.Log(SecretGetter.Api_Token);

        Connect();
    }

    private void Connect()
    {
        //Set up connection
        ConnectionCredentials connectionCredentials = new ConnectionCredentials("botty_120", SecretGetter.Api_Token);
        client = new Client();
        pubSub = new PubSub();
        api.Settings.ClientId = SecretGetter.Client_id;
        api.Settings.AccessToken = SecretGetter.Api_Token;
        client.Initialize(connectionCredentials, channel_name, '!', '!', true);
        client.OverrideBeingHostedCheck = true;
        pubSub.Connect();

        //Bot Subscriptions
        client.OnChatCommandReceived += Client_OnChatCommandReceived;
        client.OnConnected += Client_OnConnected;
        client.OnDisconnected += Client_OnDisconnected;

        client.OnBeingHosted += Client_OnBeingHosted;
        client.OnHostingStopped += Client_OnHostingStopped;

        // Get User Id
        string userId = "";
        api.InvokeAsync(api.Users.helix.GetUsersAsync(logins: new System.Collections.Generic.List<string>() { channel_name }), users => { userId = users.Users[0].Id; });

        // Channel PubSub subscriptions
        pubSub.ListenToFollows(userId);
        pubSub.ListenToSubscriptions(userId);

        client.Connect(); //Connect
        Debug.Log("Connecting!");
    }

    private void Client_OnHostingStopped(object sender, TwitchLib.Client.Events.OnHostingStoppedArgs e)
    {
        if (e.HostingStopped.HostingChannel == client.JoinedChannels[0].Channel)
            return;

        Debug.Log("We're no longer being hosted by " + e.HostingStopped.HostingChannel);
        client.LeaveChannel(e.HostingStopped.HostingChannel);
    }

    private void Client_OnBeingHosted(object sender, TwitchLib.Client.Events.OnBeingHostedArgs e)
    {
        Debug.Log("We're being hosted by " + e.BeingHostedNotification.HostedByChannel);
        client.JoinChannel(e.BeingHostedNotification.HostedByChannel);

        Alert alert = new Alert()
        {
            alert = $"{e.BeingHostedNotification.HostedByChannel} is now hosting with {e.BeingHostedNotification.Viewers} viewers!",
            message = null
        };
        twitchAlerts.QueueAlert(alert);
    }

    private void Client_OnDisconnected(object sender, TwitchLib.Client.Events.OnDisconnectedArgs e)
    {
        Debug.Log("Disconnected!");
    }

    private void Client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        Debug.Log("Connected!");
    }

    private void Client_OnChatCommandReceived(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
    {
        Debug.Log("Command entered");
        switch (e.Command.CommandText)
        {
            case "guess":
                Guess(e.Command.ArgumentsAsString, e.Command.ChatMessage.UserId, e.Command.ChatMessage.Channel);
                break;
            case "music":
                if (e.Command.ChatMessage.Channel != client.JoinedChannels[0].Channel) // Don't respond in other channels
                    return;
                Music(e.Command.ChatMessage.Channel);
                break;
        }
    }

    private GameObject HOGUIClone;
    public GameObject Canvas;

    private void Update()
    {
        if (client == null || !client.IsConnected)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log(HOGUIClone);

            if (HOGUIClone == null)
            {
                HOGUIClone = Instantiate(HumanOperatorGUI, Canvas.transform);
                return;
            }
            //Get the message in the human operator textbox
            string message = HOGUIClone.transform.Find("Text Area").Find("Text").GetComponent<TextMeshProUGUI>().text;
            client.SendMessage(client.JoinedChannels[0], message);
            Debug.Log("Sent!");

            // delete the clone
            Destroy(HOGUIClone);
        }
        
    }

    //Command Functions
    void Guess(string stringNumber, string id, string channel)
    {
        if (!int.TryParse(stringNumber, out int number))
        {
            client.SendMessage(channel, "Guess with !guess [number]");
            return;
        }
        if (number <= 0 || number > 1000)
        {
            client.SendMessage(channel, "Please guess a number between 1 and 1000");
            return;
        }

        string name = GetDisplayName(id).Result;

        client.SendMessage(channel, gameManager.QueueGuess(id, number, name));
    }

    void Music(string channel)
    {
        var playing = musicManager.playing;
        client.SendMessage(channel, $"Listen to {playing.Title} at {playing.Link} or check out more of {playing.Artist.Name}'s songs at {playing.Artist.Link}");
    }

    public Task<string> GetDisplayName(string id)
    {
        User user = api.Users.v5.GetUserByIDAsync(id).Result;
        return Task.FromResult(user.DisplayName);
    }
}
