using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CooldownData", menuName = "ability/Cooldown")]
public class cooldownData : ScriptableObject
{
    public float cdTime;

    [Header("Debug")]
    public float cdTimer;
    public bool canUseAbility;
    public bool isUsing = false;
}
