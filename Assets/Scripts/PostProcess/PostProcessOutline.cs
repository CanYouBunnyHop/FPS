using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System;

[Serializable]
[PostProcess(typeof(PostProcessOutlineRenderer), PostProcessEvent.AfterStack, "Outline")]
public sealed class PostProcessOutline : PostProcessEffectSettings //Sealed Class prevents other class to inherit from it
{
    public FloatParameter thickness = new FloatParameter{ value = 1f};
    public FloatParameter depthMin = new FloatParameter{ value = 0f};
    public FloatParameter depthMax = new FloatParameter{ value = 1f};
}
