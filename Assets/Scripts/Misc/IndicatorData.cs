using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IndicatorData", menuName = "Weapons/IndicatorData")]
public class IndicatorData : ScriptableObject
{
    [Header("Init Material")]
    public Color defaultColor;
    public Color turnColor;
    [Range(0,1)] public float radius , lineWidth, segmentSpacing, fill;
    public uint segmentCount;
    public uint removedSegment;
    public int segmentRotation;
    [Header("Used if cd type")]
    public cooldownData cddata;

    // [Header("used in runtime")]
    // public Color curColor;

}
