using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuessObject : MonoBehaviour
{
    public Transform Mask;
    public bool Removing = false;

    public float OriginY;
    public float TargetY;

    public float OriginTextAlpha;
    public float TargetTextAlpha;

    public float LerpTime = 0;
    public bool Idle = true;
}
