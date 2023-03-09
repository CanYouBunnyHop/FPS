using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player.Movement;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway")]
    [SerializeField] float smooth;
    [SerializeField] float rotSmooth;
    [SerializeField] float swayYawMultipier;
    [SerializeField] float swayPitchMultipier;
    [SerializeField] float swayRollMultipier;

    [Header("Spring")]
    [SerializeField] private float springBumpiness;
    [SerializeField] float springSmooth;
    [SerializeField] float springReturnSmooth;
    private Vector3 movePos;
    private Vector3 wSpringPos;

    [Header("Bobbing")]
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobScale;
    [SerializeField] private float bobSmooth;
    [SerializeField] private AnimationCurve bobPosX;
    [SerializeField] private AnimationCurve bobPosY;
    [SerializeField]private float timer = 0;
    private Vector3 bobPos;
    
    //movement
    [Header("References + pivot")]
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Vector3 pivotPosOffset;
    private void Awake()
    {
        //defaultRot = transform.localRotation;
    }
    private void Update()
    {
        //rotate
        //Child of main camera tends to jitter a lot, 
        //so move swaycontainer outside and multiply 'targetRotation' by 'cam.transform.rotation' will give the combined quaternion
        WeaponSwaying(out Quaternion o_targetRot);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, cam.transform.rotation, smooth * Time.deltaTime); //keep offset when player turns
        weaponPivot.localRotation = Quaternion.Slerp(weaponPivot.localRotation, o_targetRot, rotSmooth * Time.deltaTime);

        //changed to local position, maybe has some side effects
        WeaponBob();
        weaponPivot.localPosition = pivotPosOffset + bobPos;
        WeaponSpring();
        transform.localPosition = cam.transform.localPosition + movePos;
    }
    private void LateUpdate()
    {
        

        
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

        movePos = Vector3.Slerp(movePos, wSpringPos, springSmooth * Time.deltaTime);  //spring
        wSpringPos = Vector3.Lerp(wSpringPos, Vector3.zero, springReturnSmooth * Time.deltaTime); //return to default
    }
    ///<summary>Wepon lags after mouse movement</summary>
    private void WeaponSwaying(out Quaternion o_targetRotation)
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayYawMultipier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayPitchMultipier;
        float mouseZ = Input.GetAxisRaw("Mouse X") * swayRollMultipier;

        //calc target rotation
        Quaternion rotationX = Quaternion.AngleAxis(mouseY, Vector3.right); // rotates (mouseY amount) on (Vector3 axis)
        Quaternion rotationY = Quaternion.AngleAxis(-mouseX, Vector3.up);
        Quaternion rotationZ = Quaternion.AngleAxis(-mouseZ, Vector3.forward);

        o_targetRotation = rotationX * rotationY * rotationZ;
    }
}
