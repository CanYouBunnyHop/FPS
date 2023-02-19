using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessOutlineRenderer : PostProcessEffectRenderer<PostProcessOutline>
{
    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Hidden/Outline"));
        sheet.properties.SetFloat("_Thickness", settings.thickness);
        sheet.properties.SetFloat("_Thickness", settings.depthMin);
        sheet.properties.SetFloat("_Thickness", settings.depthMax);

        //throw new System.NotImplementedException();
    }
}
