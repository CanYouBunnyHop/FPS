using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform body; // set here the player transform 

    public float mouseSensitivity;
    public Transform campos;
    Vector2 mouse;
    public float xRot;
    public float yRot;
    ///<summary>
    ///X = Y recoil
    ///</summary>
    [Header("GunRecoil")]
    public float X = 0;

    ///<summary>
    ///Y = X recoil
    ///</summary>
    public float Y = 0;

    ///<summary>
    ///xrot = y up down, yrot = x left right
    ///</summary>
    public Vector2 recoilRestRot;

    private Vector3 velocity = Vector3.zero;
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
        xRot-=mouse.y;
        yRot+=mouse.x;
        xRot = Mathf.Clamp(xRot, -90, 90);
     
        
        transform.rotation = Quaternion.Euler(xRot-X,yRot+Y,0);
        body.rotation = Quaternion.Euler(0,yRot,0);
        
        recoilRestRot = new Vector2(xRot,yRot);
    }
    private void LateUpdate()
    {
    
    }
     private void FixedUpdate()
    {
         mouse.x *= Time.smoothDeltaTime * mouseSensitivity;
         mouse.y *= Time.smoothDeltaTime * mouseSensitivity;

    }
}
