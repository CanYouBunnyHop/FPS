using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FPS.Weapon;
public class PlayerCamera : MonoBehaviour
{

    [Header("Sensitivity")]
    [Range(1,100)]
    public float mouseSensitivity;

    [Header("References")]
    [SerializeField] private Transform body; // set here the player transform 
    [SerializeField] private GunManager gm;
    [SerializeField] private Transform campos;
    [SerializeField] private Transform camMainCamPivot;
    Vector2 mouse;
    
    [Header("Debug")]
    [SerializeField] private float xRot;
    [SerializeField] private float yRot;
    [Header("Screen Shake")]
    [SerializeField] private float shakeStrength;
    [SerializeField] private float shakeSpeed;
    [SerializeField] private Vector3 shakePos;
    private float timer;
    
    [SerializeField] private AnimationCurve shakeWeight = new AnimationCurve
    (
        new Keyframe(0,0, Mathf.Deg2Rad * 0, Mathf.Deg2Rad * 720),
        new Keyframe(0.2f, 1f),
        new Keyframe(1f, 0f)
    );

    [Header("Rotation Vectors")]
    [SerializeField] private Vector3 mouseRot;
    public Vector3 recoilRot;
    public Vector3 targetRot;

   
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()

    {
        //move camera to player head, camera jitter if its a child of player
        camMainCamPivot.position = campos.position;
        transform.localPosition = shakePos;

        //shakePos = Vector3.SmoothDamp(shakePos, Vector3.zero, ref shakePos, 2 * Time.deltaTime);

        //inputs
        mouse.x = Input.GetAxis("Mouse X");
        mouse.y = Input.GetAxis("Mouse Y");

        //calc mouse rotation
        xRot-=mouse.y; 
        yRot+=mouse.x;
        mouseRot = new Vector3(xRot, yRot, 0); 
        xRot = Mathf.Clamp(xRot, -90 -gm.currentBehavior.dX, 90 + gm.currentBehavior.dX); //clamp x rotation (vertical) + recoil's position in mind

        //some logic is done in gunBehavior
        targetRot = Vector3.Slerp(targetRot, Vector3.zero, gm.currentBehavior.gunData.returnSpeed * Time.deltaTime); //return rotation
        recoilRot = Vector3.Slerp(recoilRot, targetRot, gm.currentBehavior.gunData.recoilSpeed * Time.fixedDeltaTime);//recoil rotation //fixed because this affects gun recoil

        //clamp to avoid infinite lerp
        //if(Vector3.Angle(targetRot, Vector3.zero) <= 0.01f) targetRot = Vector3.zero;

        camMainCamPivot.localRotation = Quaternion.Euler(recoilRot + mouseRot);
        body.rotation = Quaternion.Euler(0,recoilRot.y + mouseRot.y,0);
        
        //recoilRestRot = new Vector2(xRot,yRot);
        //test
        if(Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(ScreenShake(shakeStrength, 1f));
        }
    }
    private void LateUpdate()
    {
        //transform.position = posAfterShake;
    }
    private void FixedUpdate()
    {
         mouse.x *= Time.smoothDeltaTime * mouseSensitivity *1000;
         mouse.y *= Time.smoothDeltaTime * mouseSensitivity *1000;

    }
    public IEnumerator ScreenShake(float _strength, float _duration)
    {
        timer = 0;
        float totalDuration = _duration * shakeSpeed;

        while(timer < totalDuration)
        {
            timer += shakeSpeed * Time.deltaTime;

            float progress = timer / totalDuration;

            shakePos = (GetPerlinVec3() * _strength) * shakeWeight.Evaluate(progress);
            yield return null;
        }
        // if(timer >= _duration)
        // {
        //     shakePos = Vector3.zero;
        //     Debug.Log("zeroo");
        // }


        float GetPerlinFloat(float seed)
        {
            return (Mathf.PerlinNoise(seed, timer) - 0.5f) * 2; // perlin noise returns 0 to 1, we want -1 to 1
        }
        Vector3 GetPerlinVec3()
        {
            return new Vector3( GetPerlinFloat(1), GetPerlinFloat(10), 0);
        }
        
    }
    
}
