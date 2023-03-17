using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player.Movement;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway")]
    [SerializeField] float maxRollAngle = 45;
    [SerializeField] float smooth;
    [SerializeField] float rotSmooth;
    [SerializeField] float swayYawMultipier;
    [SerializeField] float swayPitchMultipier;
    [SerializeField] float swayRollMultipier;

    [Header("Spring")]
    [SerializeField] float springBumpiness;
    [SerializeField] float springSmooth;
    [SerializeField] float springReturnSmooth;
    private Vector3 springPos;
    private Vector3 wSpringPos;

    [Header("Bobbing")]
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobScale;
    [SerializeField] private float bobSmooth;
    [SerializeField] private AnimationCurve bobPosX;
    [SerializeField] private AnimationCurve bobPosY;
    [SerializeField]private float timer = 0;
    private Vector3 bobPos;

    [Header("Backwards")]
    [SerializeField] private float targetSwayRotX = -45;
    [SerializeField] private float targetPivotRotX = -140;
    [SerializeField] private float swingBackSmooth;
    public bool toggleBackTest; //remove later
    
    [Header("References + pivot")]
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Vector3 pivotPosOffset;
    
    Vector3 mouseAxis;
    Quaternion targetRot;

    private void Awake()
    {
        //defaultRot = transform.localRotation;
    }
    private void Update()
    {
        mouseAxis = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0);
        WeaponSwaying(out targetRot);
    }
    private void LateUpdate()
    {
        //need to move this into method
        Quaternion sideToSide = Quaternion.Slerp(transform.localRotation, cam.transform.rotation, 1- Mathf.Pow (smooth , Time.deltaTime)); //keep offset when player turns
        Quaternion roll = Quaternion.Slerp(weaponPivot.localRotation, targetRot, 1 - Mathf.Pow(rotSmooth, Time.deltaTime));

        //avoid infinite occuring with lerp
        if(Quaternion.Angle(sideToSide, cam.transform.rotation) <= 0.5f ) {sideToSide = cam.transform.rotation;} 
        if(Quaternion.Angle(roll, targetRot) <= 0.5f) {roll = targetRot;}
        

        //position
        //changed to local position, maybe has some side effects
        WeaponBob();
        WeaponSpring();

        //Swing Back
        //WeaponSwingBackwards(out Quaternion _swayConSwing, out Quaternion _pivotSwing);

        weaponPivot.localPosition = pivotPosOffset + bobPos + springPos;
        transform.localPosition = cam.transform.localPosition; //follow camera's position, since it's no longer a child of camera

        //rotation
        transform.localRotation = sideToSide;
        //do some clamping so the animations don't overshoot due to inconsistensies in frame rate
        weaponPivot.localRotation = Quaternion.Angle(weaponPivot.localRotation, targetRot) > maxRollAngle && mouseAxis.magnitude != 0 ? 
        weaponPivot.localRotation : roll;
        
    }
    private void WeaponBob()
    {
        Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if(dir.magnitude > 0 && pm.currentState == PlayerMovement.State.Grounded)
        {
            timer += bobSpeed * Time.deltaTime;
            float bX = bobPosX.Evaluate(timer);
            float bY = bobPosY.Evaluate(timer);
            Vector3 bobTarget = new Vector3(bX , bY, 0);
            bobPos = Vector3.Slerp(bobPos, bobTarget* bobScale, bobSmooth * Time.deltaTime);
        }
        else
        {
            float v = 0;
            bobPos = Vector3.Slerp(bobPos, Vector3.zero , bobSmooth * Time.deltaTime); //return if not moving
            timer = Mathf.SmoothDamp(timer, 0, ref v, bobSmooth * Time.deltaTime);
        }
    }
    ///<summary>Wepon moves down after landed from a jump</summary>
    private void WeaponSpring()
    {
        if(pm.stepInAir > 3)
        {
            wSpringPos = new Vector3(0, -springBumpiness,0);
        }
        if(pm.stepOnGround < 3 && pm.stepOnGround !=0)
        {
            wSpringPos = new Vector3(0, springBumpiness,0);
        }

        springPos = Vector3.Slerp(springPos, wSpringPos, springSmooth * Time.deltaTime);  //spring
        wSpringPos = Vector3.Lerp(wSpringPos, Vector3.zero, springReturnSmooth * Time.deltaTime); //return to default
    }
    ///<summary>Wepon lags after mouse movement</summary>
    private void WeaponSwaying(out Quaternion o_targetRotation)
    {
        float mouseX = mouseAxis.x * swayYawMultipier;
        float mouseY = mouseAxis.y * swayPitchMultipier;
        float mouseZ = mouseAxis.x * swayRollMultipier;

        //calc target rotation
        Quaternion rotationX = Quaternion.AngleAxis(mouseY, Vector3.right); // rotates (mouseY amount) on (Vector3 axis)
        Quaternion rotationY = Quaternion.AngleAxis(-mouseX, Vector3.up);
        Quaternion rotationZ = Quaternion.AngleAxis(-mouseZ, Vector3.forward);


        o_targetRotation = rotationX * rotationY * rotationZ;
    }
    
    // private void WeaponSwingBackwards(out Quaternion _swayConSwing, out Quaternion _pivotSwing) //using AnimationIK
    // {
    //     Quaternion targetRot = new Quaternion(targetSwayRotX, 0, 0, 0);
    //     Quaternion pivotTargetRot = new Quaternion(targetPivotRotX, 0, 0, 0);
    
    //     Quaternion trot = toggleBackTest? targetRot : Quaternion.identity;
    //     Quaternion prot = toggleBackTest? pivotTargetRot : Quaternion.identity;

    //     _swayConSwing = Quaternion.RotateTowards(transform.localRotation, trot, swingBackSmooth * Time.deltaTime);
    //     _pivotSwing = Quaternion.RotateTowards(weaponPivot.localRotation, prot, swingBackSmooth * Time.deltaTime);

    // }
}
