using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform body; // set here the player transform 

    public float mouseSensitivity;
    public Transform campos;
    Vector2 mouse;
    float xRot;
    float yRot;

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
     
        
        transform.rotation = Quaternion.Euler(xRot,yRot,0);
        body.rotation = Quaternion.Euler(0,yRot,0);
        
       
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
