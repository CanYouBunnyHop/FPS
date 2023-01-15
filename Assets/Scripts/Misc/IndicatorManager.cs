using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorManager : MonoBehaviour
{
    [SerializeField]private Image radialBar;
    protected Material mat;
    
    [Header("probably should be static")]
    [SerializeField] private GunManager gm;
    //-----------------------------------------------------------
    private void Awake()
    {
        mat = radialBar.material;
    }
    private void Update()
    {
        BarBehavior();
    }
    private void FixedUpdate()
    {
        
    }
    public virtual void BarBehavior()
    {
        

        switch(gm.currentBehavior.gunData.indicatorType)// determines how the indicator is displayed
        {
            case GunData.AmmoOrCd.NotUsed:
            {

            }
            break;

            case GunData.AmmoOrCd.Ammo:
            {

            }
            break;

            case GunData.AmmoOrCd.Cd:
            {
                IndicatorData indicatorData = gm.currentBehavior.gunData.indicatorData; //get values from SO

                //get values from SO
                cooldownData cddata = indicatorData.cddata;

                float cdTimerFlipped = cddata.cdTime - cddata.cdTimer;//
                float percent = (cdTimerFlipped) / (cddata.cdTime);// range =  0 - 1

                if(percent < 1)
                mat.SetColor("_Color", indicatorData.turnColor);

                else
                mat.SetColor("_Color", indicatorData.defaultColor);

                mat.SetFloat("_Fill", percent);
            } 
            break;

            case GunData.AmmoOrCd.Other:
            {

            }
            break;
        }
        if(gm.currentBehavior.gunData.indicatorType == GunData.AmmoOrCd.Cd) //if it is cool down based
        {
            
        }
        
    }
    public void InitMat()
    {
        //get values from SO
        IndicatorData indicatorData = gm.currentBehavior.gunData.indicatorData;

        mat.SetColor("_Color", indicatorData.defaultColor);
        mat.SetFloat("_Radius", indicatorData.radius);
        mat.SetFloat("_LineWidth", indicatorData.lineWidth);
        mat.SetFloat("_SegmentSpacing", indicatorData.segmentSpacing);
        mat.SetFloat("_SegmentRotation", indicatorData.segmentRotation);
        mat.SetFloat("_SegmentCount", indicatorData.segmentCount);
        mat.SetFloat("_RemovedSegment", indicatorData.removedSegment);
        mat.SetFloat("_Fill", indicatorData.fill);
    }
}
