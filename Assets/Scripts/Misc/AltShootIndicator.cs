using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AltShootIndicator : MonoBehaviour
{
    [SerializeField]private Image radialBar;
    [SerializeField]private cooldownData cddata;
    private Color defaultColor;
    public Color turnColor;
    public float maxFill;
    private Vector3 currentRot;

    private void Awake()
    {
        defaultColor = radialBar.color;
        cddata.AwakeTimer();

    }
    private void Update()
    {
        float percent = (cddata.cdTimer) / (cddata.cdTime);// range =  0 - 1
        float moddedPercent =  percent * maxFill;          // range = 0 - maxfill

        radialBar.fillAmount = moddedPercent;
        radialBar.fillAmount = Mathf.Clamp(radialBar.fillAmount, 0, maxFill);

        float z = 180 * radialBar.fillAmount; //calc rotation needed
        
        Vector3 rot = new Vector3(0,0, z);
        radialBar.rectTransform.rotation = Quaternion.Euler(rot);

        Debug.Log(z);

        if(moddedPercent < maxFill)
        radialBar.color = turnColor;

        else
        radialBar.color = defaultColor;
    }


}
