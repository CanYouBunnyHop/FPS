 using UnityEngine;
 using System.Collections;
 using UnityEngine.UI;
 
 public class ShowFPS : MonoBehaviour {
     public Text fpsText;
     public float deltaTime;
     public int cappedFps;
      void Awake () 
     {
         QualitySettings.vSyncCount = 0;  // VSync must be disabled
         Application.targetFrameRate = cappedFps;
     }
     void Update () 
     {
         deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
         float fps = 1.0f / deltaTime;
         fpsText.text = Mathf.Ceil (fps).ToString ();
     }
 }
