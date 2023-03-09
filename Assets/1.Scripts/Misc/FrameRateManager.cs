using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.EditorTools;

public class FrameRateManager : MonoBehaviour 
{
    [SerializeField] private Text FrameRateText;
    private float deltaTime;
    [Range(30, 300)] [SerializeField] private int cappedFrameRate;
    void Awake () 
    {
        SetMaxFrameRate();
    }
    public void SetMaxFrameRate()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = cappedFrameRate;
    }
    void Update () 
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        FrameRateText.text = Mathf.Ceil (fps).ToString ();
    }
}

[CustomEditor(typeof(FrameRateManager))] //CustomEditor attribute informs Unity which component it should act as an editor for.
public class FrameRateManagerEditor : Editor //class must inherit from Editor
{
    SerializedProperty frameRate;
    private void OnEnable()
    {
        frameRate = serializedObject.FindProperty("frameRate"); //get properties with this
    }
    //public override On
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        FrameRateManager fm = (FrameRateManager)target; //Editor.target is the current object being inspected

        DrawDefaultInspector();

        EditorGUILayout.Space(10); //after default inspector, seperate by this space
        //EditorGUILayout.Separator();

        //https://docs.unity3d.com/Manual/class-GUISkin.html GUISkin Doc
        Rect r = EditorGUILayout.BeginHorizontal(GUI.skin.label);       //label is the default background color it seems

        Rect sr = new Rect( r.width/2 , r.y , r.width/2, r.height);
        EditorGUILayout.Space(25); //button space, the hieght of button

        EditorGUILayout.EndHorizontal();
            
        bool isClicked = GUI.Button(sr, "Confirm Settings");
        if(isClicked)
        {
            fm.SetMaxFrameRate();
            Debug.Log("New Framerate Settings has been set");
        }
        
    }
}

