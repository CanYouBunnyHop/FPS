using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class IndicatorBehavior : MonoBehaviour
{
    [SerializeField]protected Image radialBar;
    [SerializeField]protected Color turnColor;
    protected Material mat;
    [SerializeField]protected cooldownData cddata;

    [Header("Init Material")]
    [SerializeField] protected IndicatorData indicatorData;
    private void Update()
    {
        
    }
    private void FixedUpdate()
    {
        
    }
    public virtual void BarBehavior()
    {

    }
    public void InitMat()
    {
        mat.SetColor("_Color", indicatorData.defaultColor);
        mat.SetFloat("_Radius", indicatorData.radius);
        mat.SetFloat("_LineWidth", indicatorData.lineWidth);
        mat.SetFloat("_SegmentSpacing", indicatorData.segmentSpacing);
        mat.SetFloat("_SegmentCount", indicatorData.segmentCount);
        mat.SetFloat("_RemovedSegment", indicatorData.removedSegment);
    }
}
