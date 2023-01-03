using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AltShootIndicator : MonoBehaviour
{
    [SerializeField]private Image radialBar;
    [SerializeField]private cooldownData cddata;
    
    [SerializeField]private Color turnColor;
    private Material mat;
    private Vector3 currentRot;

    [Header("Init Material")]
    [SerializeField]private Color defaultColor;
    [Range(0,1)] [SerializeField]private float radius , lineWidth, segmentSpacing;
    [SerializeField]private uint segmentCount = 1;
    [SerializeField] private uint removedSegment;
    private void Awake()
    {
        cddata.AwakeTimer();
        mat = radialBar.material;

        InitMat();
    }
    private void Update()
    {
        float cdTimerFlipped = cddata.cdTime - cddata.cdTimer;//
        float percent = (cdTimerFlipped) / (cddata.cdTime);// range =  0 - 1

        if(percent < 1)
        mat.SetColor("_Color", turnColor);

        else
        mat.SetColor("_Color", defaultColor);

        mat.SetFloat("_Fill", percent);
    }

    public void InitMat()
    {
        mat.SetColor("_Color", defaultColor);
        mat.SetFloat("_Radius", radius);
        mat.SetFloat("_LineWidth", lineWidth);
        mat.SetFloat("_SegmentSpacing", segmentSpacing);
        mat.SetFloat("_SegmentCount", segmentCount);
        mat.SetFloat("_RemovedSegment", removedSegment);
    }
}
