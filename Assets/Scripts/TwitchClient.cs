using System.Collections;
using System.Threading.Tasks;
using TMPro;
using TwitchLib.Api.Models.v5.Users;
using TwitchLib.Client.Models;
using TwitchLib.Unity;
using UnityEngine;
using System.Drawing;

public class TwitchClient : MonoBehaviour
{
    public Client client;
    //public static Api api;
    public Api _api = new Api();
    private string channel_name = "second_120";

    public GameObject HumanOperatorGUI;

    public GameManager gameManager;
    public BackgroundMusicManager musicManager;

    void Start()
    {
        Application.runInBackground = true;

        StartCoroutine(WaitAndConnect());
        //WaitAndConnect();
    }

    IEnumerator WaitAndConnect()
    {
        // See if in editor
        if (!Application.isEditor)
            Debug.Log(SecretGetter.Api_Token);
            yield return new WaitUntil(() => SecretGetter.Api_Token != null);// If not editor wait for api token input
        Debug.LogError(SecretGetter.Api_Token);


        //Set up connection
        ConnectionCredentials connectionCredentials = new ConnectionCredentials("botty_120", SecretGetter.Api_Token);
        client = new Client();
        _api.Settings.ClientId = SecretGetter.Client_id;
        _api.Settings.AccessToken = SecretGetter.Api_Token;
        client.Initialize(connectionCredentials, channel_name, '!', '!', true);

        //Bot Subscriptions
        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnChatCommandReceived += Client_OnChatCommandReceived;
        client.OnConnected += Client_OnConnected;
        client.OnDisconnected += Client_OnDisconnected;

        client.Connect(); //Connect
        Debug.LogError("Connecting!");
    }

    private void Client_OnDisconnected(object sender, TwitchLib.Client.Events.OnDisconnectedArgs e)
    {
        Debug.LogError("Disconnected!");
    }

    private void Client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        Debug.LogError("Connected!");
    }

    private void Client_OnChatCommandReceived(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
    {
        Debug.LogError("Command entered");
        switch (e.Command.CommandText)
        {
            case "guess":
                Guess(e.Command.ArgumentsAsString, e.Command.ChatMessage.UserId, e.Command.ChatMessage.Channel);
                break;
            case "music":
                Music(e.Command.ChatMessage.Channel);
                break;
        }
    }

    private void Client_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        Debug.LogError("Reached Other");
        if (e.ChatMessage.IsMe)
            return;
        string message = e.ChatMessage.Message;
        UnityEngine.Color color = e.ChatMessage.Color.ToUnity();
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

    public async Task<string> GetDisplayName(string id)
    {
        User user = _api.Users.v5.GetUserByIDAsync(id).Result;
        return user.DisplayName;
    }
}
