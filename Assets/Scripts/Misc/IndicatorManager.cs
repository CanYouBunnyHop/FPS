using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FPS.Weapon;
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

        foreach(GunBehaviour gb in gm.gunBehaviors) // reset cooldown for every cd in weapons list (make sure not in cd when starting)
        {
            if(gb.gunData.indicatorType == GunDataSO.AmmoOrCd.Cd)
            {
                IndicatorDataSO indicatorDataSO = gb.gunData.indicatorData;
                cooldownDataSO cddata = indicatorDataSO.cddata;
                cddata.AwakeTimer();
            }
        }
        
    }
    private void Update()
    {
        radialBar.enabled = gm.currentBehavior.gunData.indicatorType != GunDataSO.AmmoOrCd.NotUsed? true : false; //see if indicator is enabled
        BarBehavior();
    }
    private void FixedUpdate()
    {
        
    }
    public virtual void BarBehavior()// determines how the indicator is displayed, seperated from logic
    {
        switch(gm.currentBehavior.gunData.indicatorType)
        {
            case GunDataSO.AmmoOrCd.NotUsed:
            {

            }
            break;

            case GunDataSO.AmmoOrCd.Ammo:
            {
                IndicatorDataSO indicatorDataSO = gm.currentBehavior.gunData.indicatorData; //get values from SO
                mat.SetFloat("_RemovedSegments", indicatorDataSO.removedSegment);

                if(indicatorDataSO.segmentCount-indicatorDataSO.removedSegment <= 1)
                {
                    mat.SetColor("_Color", indicatorDataSO.turnColor);
                }
                else
                {
                    mat.SetColor("_Color", indicatorDataSO.defaultColor);
                }
            }
            break;

            case GunDataSO.AmmoOrCd.Cd:
            {
                IndicatorDataSO indicatorDataSO = gm.currentBehavior.gunData.indicatorData; //get values from SO

                //get values from SO
                cooldownDataSO cddata = indicatorDataSO.cddata;

                float cdTimerFlipped = cddata.cdTime - cddata.cdTimer;//
                float percent = (cdTimerFlipped) / (cddata.cdTime);// range =  0 - 1

                if(percent < 1)
                mat.SetColor("_Color", indicatorDataSO.turnColor);

                else
                mat.SetColor("_Color", indicatorDataSO.defaultColor);

                mat.SetFloat("_Fill", percent);
            } 
            break;

            case GunDataSO.AmmoOrCd.Other:
            {

            }
            break;
        }
    }
    public void InitMat(GunBehaviour _gunToSwitch)
    {
        //get values from SO
        //IndicatorData indicatorData = gm.currentBehavior.gunData.indicatorData;
        if(_gunToSwitch.gunData.indicatorType != GunDataSO.AmmoOrCd.NotUsed)
        {
            IndicatorData indicatorData = new IndicatorData(_gunToSwitch.gunData.indicatorData);

            mat.SetColor("_Color", indicatorData.defaultColor);
            mat.SetFloat("_Radius", indicatorData.radius);
            mat.SetFloat("_LineWidth", indicatorData.lineWidth);
            mat.SetFloat("_SegmentSpacing", indicatorData.segmentSpacing);
            mat.SetFloat("_SegmentRotation", indicatorData.segmentRotation);
            mat.SetFloat("_SegmentCount", indicatorData.segmentCount);
            mat.SetFloat("_RemovedSegments", indicatorData.removedSegment);
            mat.SetFloat("_Fill", indicatorData.fill);
        }
        
    }
}
