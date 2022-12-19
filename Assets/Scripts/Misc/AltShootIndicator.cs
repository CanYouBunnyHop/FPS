using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AltShootIndicator : MonoBehaviour
{
    [SerializeField]private Image radialBar;
    [SerializeField]private cooldownData cddata;
    public float maxFill;

    private float defaultRot;

    private void Awake()
    {
        defaultRot = radialBar.rectTransform.rotation.z;
    }
    private void Update()
    {
        float percent = (cddata.cdTimer) / (cddata.cdTime);// range =  0 - 1
        float moddedPercent =  percent * maxFill;          // range = 0 - maxfill

        radialBar.fillAmount = moddedPercent;

        float z = 180 * radialBar.fillAmount; //calc rotation needed
        Vector3 rot = new Vector3(0,0, z + defaultRot);
        radialBar.rectTransform.rotation = Quaternion.Euler(rot);
    }


}
