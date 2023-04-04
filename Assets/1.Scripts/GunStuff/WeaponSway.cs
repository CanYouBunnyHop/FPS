using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player;
using FPS.Settings;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway")]
    [SerializeField] float maxRollAngle = 45;
    [SerializeField] float smooth;
    [SerializeField] float rotSmooth;
    [SerializeField] float rotSpeed;
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

    //[Header("Backwards")]
    //[SerializeField] private float targetSwayRotX = -45;
    //[SerializeField] private float targetPivotRotX = -140;
    //[SerializeField] private float swingBackSmooth;
    //public bool toggleBackTest; //remove later
    
    [Header("References + pivot")]
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private PlayerStateMachine pStateMachine;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform camPivot;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Vector3 pivotPosOffset;
    [SerializeField] private PlayerInputSystemManager pism;
    [SerializeField]Vector2 moveAxis;
    [SerializeField]Vector3 mouseAxis;
    [SerializeField]Quaternion targetRot;
    [SerializeField]Quaternion currentRot;
    bool highEnough = false;

    private void Awake()
    {
        //defaultRot = transform.localRotation;
    }
    private void Update()
    {
        mouseAxis = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0);
        moveAxis = pism.wasd.ReadValue<Vector2>();
    }
    private void LateUpdate()
    {
        WeaponBob();
        WeaponSpring();
        WeaponSwaying_SideRoll();

        //need to move this into method
        Quaternion sideToSide = Quaternion.Slerp(transform.localRotation, camPivot.transform.rotation, 1- Mathf.Pow (smooth , Time.deltaTime)); //keep offset when player turns
        currentRot = Quaternion.Slerp(currentRot, targetRot, 1 - Mathf.Pow(rotSmooth, Time.deltaTime)); //Quaternion.Slerp(current, target, 1 - Mathf.Pow(rsmooth, Time.deltaTime));

        //avoid infinite occuring with lerp
        if(Quaternion.Angle(sideToSide, camPivot.transform.rotation) <= 0.01f ) {sideToSide = camPivot.transform.rotation;} 
        if(Quaternion.Angle(currentRot, targetRot) <= 0.01f) {currentRot = targetRot;}
        

        //position
        //changed to local position, maybe has some side effects
        
        //Swing Back
        //WeaponSwingBackwards(out Quaternion _swayConSwing, out Quaternion _pivotSwing);

        weaponPivot.localPosition = pivotPosOffset + bobPos + springPos;
        transform.localPosition = camPivot.transform.localPosition; //follow camera's position, since it's no longer a child of camera

        //rotation
        transform.localRotation = sideToSide;
        weaponPivot.localRotation = Quaternion.Lerp(weaponPivot.localRotation, currentRot, 1 - Mathf.Pow(rotSpeed, Time.deltaTime)); //Quaternion.Slerp(current, target, 1 - Mathf.Pow(rsmooth, Time.deltaTime));
        
    }
    private void WeaponBob()
    {
        Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if(moveAxis.magnitude > 0 && pStateMachine.currentCoreState is CoreState_Grounded && pStateMachine.currentGroundSubState is not GroundSubState_Slide)
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
            highEnough = true;
        }
        if(pm.stepOnGround < 3 && pm.stepOnGround !=0 && highEnough)
        {
            wSpringPos = new Vector3(0, springBumpiness,0);
            highEnough = false;
        }

        springPos = Vector3.Slerp(springPos, wSpringPos, springSmooth * Time.deltaTime);  //spring
        wSpringPos = Vector3.Lerp(wSpringPos, Vector3.zero, springReturnSmooth * Time.deltaTime); //return to default
    }
    ///<summary>Wepon lags after mouse movement</summary>
    private void WeaponSwaying_SideRoll()
    {
        
        mouseAxis = Vector3.ClampMagnitude(mouseAxis, 3);

        float mouseX = mouseAxis.x * swayYawMultipier; 
        float mouseY = mouseAxis.y * swayPitchMultipier; 
        //float mouseZ = mouseAxis.x * swayRollMultipier;

        //calc target rotation
        Quaternion rotationX = Quaternion.AngleAxis(mouseY, Vector3.right); // rotates (mouseY amount) on (Vector3 axis)
        Quaternion rotationY = Quaternion.AngleAxis(-mouseX, Vector3.up);

        //calc sideMove target rotation
        float sideMove = moveAxis.x * swayRollMultipier;
        Quaternion rotationZ = Quaternion.AngleAxis(-sideMove, Vector3.forward);
        //targetRot *= rotationZ;

        targetRot = rotationX * rotationY * rotationZ;
    }
}
