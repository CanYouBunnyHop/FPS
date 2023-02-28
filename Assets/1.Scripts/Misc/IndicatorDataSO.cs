using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IndicatorData", menuName = "Weapons/IndicatorData")]
public class IndicatorDataSO : ScriptableObject
{
    [Header("Init Material")]
    public Color defaultColor;
    public Color turnColor;
    [Range(0,0.1f)] public float radius , lineWidth, segmentSpacing, fill;
    public uint segmentCount;
    public uint removedSegment;
    public int segmentRotation;
    [Header("Used if cd type")]
    public cooldownDataSO cddata;
}
public class IndicatorData // used like an instance
{
    [Header("Init Material")]
    public Color defaultColor;
    public Color turnColor;
    [Range(0,1)] public float radius , lineWidth, segmentSpacing, fill;
    public uint segmentCount;
    public uint removedSegment;
    public int segmentRotation;
    [Header("Used if cd type")]
    public cooldownDataSO cddata;

    public IndicatorData(IndicatorDataSO _InData) //constructor
    {
        defaultColor = _InData.defaultColor;
        turnColor = _InData.turnColor;

        radius = _InData.radius;
        lineWidth = _InData.lineWidth;
        segmentSpacing = _InData.segmentSpacing;
        segmentRotation = _InData.segmentRotation;

        segmentCount = _InData.segmentCount;

        fill = _InData.fill;
        removedSegment = _InData.removedSegment;

        if(_InData.cddata != null)
        cddata = _InData.cddata;
    }
}
