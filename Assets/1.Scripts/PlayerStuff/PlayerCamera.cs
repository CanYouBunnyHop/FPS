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
    [SerializeField]private Transform body; // set here the player transform 
    [SerializeField]private GunManager gm;
    [SerializeField]private Transform campos;
    Vector2 mouse;
    
    [Header("Debug")]
    public float xRot;
    public float yRot;

    [Header("Rotation Vectors")]
    [SerializeField]private Vector3 mouseRot;
    public Vector3 recoilRot;
    public Vector3 targetRot;

    private Vector3 vel = Vector3.zero;

   
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()

    {
        //move camera to player head, camera jitter if its a child of player
        transform.position = campos.position;

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
        
        
        recoilRot = Vector3.Slerp(recoilRot, targetRot, gm.currentBehavior.gunData.recoilSpeed * Time.fixedDeltaTime);  //recoil rotation
        

        transform.localRotation = Quaternion.Euler(recoilRot + mouseRot);
        body.rotation = Quaternion.Euler(0,recoilRot.y + mouseRot.y,0);
        
        //recoilRestRot = new Vector2(xRot,yRot);
    }
    private void LateUpdate()
    {
    
    }
    private void FixedUpdate()
    {
         mouse.x *= Time.smoothDeltaTime * mouseSensitivity *1000;
         mouse.y *= Time.smoothDeltaTime * mouseSensitivity *1000;

    }
}
