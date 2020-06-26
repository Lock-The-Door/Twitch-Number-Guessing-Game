using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class SecretGetter
{
    public readonly static string Client_id = "hwttf1lj43tl3rwuqz9gi2k41wi4jq";
    //public readonly static string Client_secret = Environment.GetEnvironmentVariable(Client_id, EnvironmentVariableTarget.User);
    public static string Api_Token = Environment.GetEnvironmentVariable("TWITCH_API_KEY_botty_120", EnvironmentVariableTarget.User);
}
