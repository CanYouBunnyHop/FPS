using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    private Vector3 mouseRot;
    private Vector3 recoilRot;
    public Vector3 targetRot;
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
        xRot = Mathf.Clamp(xRot, -90 -X, 90 + X);

        mouseRot = new Vector3(xRot, yRot, 0);
        targetRot += new Vector3(-X,Y, 0);
       // float xTotal = xRot-X;
       //float yTotal = yRot+Y;
     
        targetRot = Vector3.Lerp(targetRot, Vector3.zero, 10f * Time.deltaTime);
        recoilRot = Vector3.Slerp(recoilRot, targetRot, 10f * Time.deltaTime);
        
        //transform.localRotation = Quaternion.Euler(xTotal,yTotal,0);
        //body.rotation = Quaternion.Euler(0,yTotal,0);

        transform.localRotation = Quaternion.Euler(recoilRot + mouseRot);
        body.rotation = Quaternion.Euler(0,recoilRot.y + mouseRot.y,0);
        
        //recoilRestRot = new Vector2(xRot,yRot);
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
