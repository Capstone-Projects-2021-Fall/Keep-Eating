using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticSettings : MonoBehaviour
{
    public static bool FreshRoom { get; set; }
    public static bool IsPrivate { get; set; }
    public static bool Bots { get; set; }
    public static string Map { get; set; }
    public static int MaxPlayers { get; set; }
    public static int MaxEaters { get; set; }
    public static int MaxEnforcers { get; set; }
    public static int MinEaters { get; set; }
    public static int MinEnforcers { get; set; }

    public static void SetVars()
    {
        IsPrivate = true;
        Bots = false;
        Map = "SmallGameMap";
        MaxPlayers = 2;
        MaxEaters = 1;
        MaxEnforcers = 1;
        MinEaters = 1;
        MinEnforcers = 1;
        FreshRoom = true;
    }
}
