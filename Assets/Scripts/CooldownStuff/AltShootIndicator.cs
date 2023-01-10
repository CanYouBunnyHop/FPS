using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AltShootIndicator : IndicatorBehavior
{
   
    // [SerializeField]private Color defaultColor;
    // [Range(0,1)] [SerializeField]private float radius , lineWidth, segmentSpacing;
    // [SerializeField]private uint segmentCount = 1;
    // [SerializeField] private uint removedSegment;
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
        mat.SetColor("_Color", indicatorData.defaultColor);

        mat.SetFloat("_Fill", percent);
    }

    
}
