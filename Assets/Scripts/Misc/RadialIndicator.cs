using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class RadialIndicator : MonoBehaviour
{
    [SerializeField]private Image overlay;
    [SerializeField]private Image bg;
    [SerializeField]private TMP_Text text;
    private Color bgDefaultColor;
    public Color bgFlashColor;
    [SerializeField]private int flashCount = 0;
    
    [SerializeField]private cooldownData cddata;

    private void Awake()
    {
        bgDefaultColor = bg.color;
        cddata.AwakeTimer();
    }
    private void Update()
    {
        float percent = (cddata.cdTimer) / (cddata.cdTime);

        if(!cddata.isUsing) // not using
        {
            overlay.fillAmount = percent;
        }

        else //using
        {
            overlay.fillAmount = 1;
        }
        

        float time = Mathf.Round(cddata.cdTimer);

        if(!cddata.isUsing && cddata.cdTimer > 0)
        {
            text.text = time.ToString();
            flashCount = 0;
        }
       

        else
        text.ClearMesh();

        if(percent <= 0 && flashCount == 0)
         {
            StartCoroutine(FlashBG());
         }

        //Debug.Log(percent);
        //default 1 is max
    }

    private IEnumerator FlashBG()
    {
        bg.color = bgFlashColor;
        flashCount ++;
        yield return new WaitForSeconds(0.2f);

        bg.color = bgDefaultColor;
    }
    
}
