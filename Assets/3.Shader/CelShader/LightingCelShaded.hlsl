// Lighting in needs to be defined in code because Unity shader graph don't have nodes that support lighting
#ifndef LIGHTING_CEL_SHADED_INCLUDE //#ifndef mean if not defined 
#define LIGHTING_CEL_SHADED_INCLUDE

#ifndef SHADERGRAPH_PREVIEW //#ifndef mean if not defined 
struct SurfaceVariables //creates struct to keep all the object data that is needed to calculate custom lighting
{
    float3 normal;
    float3 view;

    float shininess;
    float smoothness;
    

}; //needs semi colon after defining struct

float3 CalculateCelShading(Light l, SurfaceVariables s) //light is urp defined struct
{
    //diffuse value = dot product of normals and the direction from the point to the light
    float diffuse = saturate(dot(s.normal, l.direction));


    //2 way of calculating Speculat light
    //specular reflection vector is calculated from light direction recflected on the normal vector
    //Specular value is calculated as such, SL = (V.R)^c
    // V = direction to the camera, R = reflection vector, c = shininess, SL = specular light

    //2nd way is SL = (V.H)^c
    //where H = direction to light + direction to viewer, this gives us halfway vector of those vectors

    float h = SafeNormalize(l.direction + s.view);
    float specular = saturate(dot(s.normal, h));
    specular = pow(specular, s.shininess);
    specular *= diffuse * s.smoothness; //this will stop specular light to reflect on the backside

    //return specular;
    return l.color * diffuse;
}
#endif

void LightingCelShaded_float(float Smoothness, float3 Normal, float3 View, out float3 Color)
{
    #if defined(SHADERGRAPH_PREVIEW)
        Normal = half3(0.5, 0.5, 0);
        View = SafeNormalize(View);
        Smoothness = 0.5f;
        Color = 1;

    #else
    //Initialize and populate SurfaceVariables (user defined struct)
        SurfaceVariables s;
        s.normal = normalize(Normal);

        s.view = SafeNormalize(View);
        s.smoothness = Smoothness;
        s.shininess = exp2(10 * Smoothness + 1);

        Light light = GetMainLight(); //urp defined way to get main light

        //Output
        Color = CalculateCelShading(light, s);
    #endif
}

#endif

//Diffuse refllection of rough surface is the "non shiny" reflection on surface
//specular reflection is smooth surface glint
//ambient reflection is how faint stacttered lights covers the model


