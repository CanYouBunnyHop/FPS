using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class RadialIndicator : MonoBehaviour
{
    [SerializeField]private Image overlay;
    [SerializeField]private Image bg;
    private Color bgDefaultColor;
    public Color bgFlashColor;
    
    [SerializeField]private cooldownData cddata;

    private void Awake()
    {
        bgDefaultColor = bg.color;
    }
    private void Update()
    {
        float percent = (cddata.cdTimer) / (cddata.cdTime);

        if(!cddata.isUsing)
        overlay.fillAmount = percent;

        else
        overlay.fillAmount = 1;

        // if(percent < 0.05f)
        // {
        //     if(Input.GetKeyDown(KeyCode.Alpha0))
        //    StartCoroutine(FlashBG());
        // }

        //Debug.Log(percent);
        //default 1 is max
    }

    private IEnumerator FlashBG()
    {
        bg.color = bgFlashColor;

        yield return new WaitForSeconds(0.2f);

        bg.color = bgDefaultColor;
    }
    
}
