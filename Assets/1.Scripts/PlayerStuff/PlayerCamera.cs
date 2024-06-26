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
    [SerializeField] private Transform camPivot;
    Vector2 mouse;
    
    [Header("Debug")]
    [SerializeField] private float xRot;
    [SerializeField] private float yRot;
    [Header("Screen Shake")]
    [SerializeField] private float shakeStrength;
    [SerializeField] private float shakeRotStrength;
    [SerializeField] private float shakeSpeed;
    [SerializeField] private Vector3 shakePos;
    [SerializeField] private Quaternion shakeRot = Quaternion.identity;
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
    [Header("Camera Animation")]
    [SerializeField] private Vector3 anim_ShakePos;
    [SerializeField] private Quaternion anim_ShakeRot;


   
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        shakePos = Vector3.zero;
        shakeRot = Quaternion.identity;
    }
    void Update()

    {
        //move camera parent to player head, camera jitter if its a child of player
        camPivot.position = campos.position;
        //move camera to the camera shake posisiton and shake rotation
        transform.localPosition = shakePos + anim_ShakePos;
        transform.localRotation = (shakeRot * anim_ShakeRot);

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

        camPivot.localRotation = Quaternion.Euler(recoilRot + mouseRot);
        body.rotation = Quaternion.Euler(0,recoilRot.y + mouseRot.y,0);
        
        //recoilRestRot = new Vector2(xRot,yRot);
        //test
        if(Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(PerlinScreenShake(shakeStrength, shakeRotStrength, 0.5f, shakeSpeed));
        }
    }
    private void LateUpdate()
    {
        //transform.position = posAfterShake;
    }
    private void FixedUpdate()
    {
         mouse.x *= Time.smoothDeltaTime * mouseSensitivity * 1000;
         mouse.y *= Time.smoothDeltaTime * mouseSensitivity * 1000;

    }

    ///<summary> 
    ///Randomized screen shake, good for explosion, earthquake. 
    ///Randomised position is not great on gun animations, use animation curve instead.
    ///</summary> 
    public IEnumerator PerlinScreenShake(float _posStrength, float _rotStrength, float _duration, float _shakeSpeed)
    {
        timer = 0;
        float totalDuration = _duration * _shakeSpeed;

        float X = Random.Range(0, 100);
        float Y = Random.Range(0, 100);
        float Z = Random.Range(0, 100);

        float rX = Random.Range(0, 100);
        float rY = Random.Range(0, 100);
        float rZ = Random.Range(0, 100);

        while (timer < totalDuration)
        {
            timer += _shakeSpeed * Time.deltaTime;

            float progress = timer / totalDuration;

            shakePos = GetPerlinVec3(X, Y, Z) * _posStrength * shakeWeight.Evaluate(progress);
            shakeRot = Quaternion.Euler(GetPerlinVec3(rX,rY,rZ)* _rotStrength * shakeWeight.Evaluate(progress));
            yield return null;
        }

        float GetPerlinFloat(float seed)
        {
            return (Mathf.PerlinNoise(seed, timer) - 0.5f) * 2; // perlin noise returns 0 to 1, we want -1 to 1
        }
        Vector3 GetPerlinVec3(float x, float y, float z)
        {
            return new Vector3(GetPerlinFloat(x), GetPerlinFloat(y), GetPerlinFloat(z));
        }

    }
}
