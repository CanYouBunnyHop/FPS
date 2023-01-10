using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IndicatorData", menuName = "Weapons/IndicatorData")]
public class IndicatorData : ScriptableObject
{
    [Header("Init Material")]
    public Color defaultColor;
    [Range(0,1)] public float radius , lineWidth, segmentSpacing;
    public uint segmentCount;
    public uint removedSegment;

    // [Header("CountType")]
    // public CountType countType;
    // public enum CountType
    // {
    //     ammo,
    //     Cd,
        
    // }
}
