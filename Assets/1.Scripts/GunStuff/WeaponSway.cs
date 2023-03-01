using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] float smooth;
    [SerializeField] float swayMultipier;
    [SerializeField] private Camera cam;
    private Quaternion modRot;

    private void Awake()
    {
        //defaultRot = transform.localRotation;
    }
    private void LateUpdate()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayMultipier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayMultipier;

        //calc target rotation
        Quaternion rotationX = Quaternion.AngleAxis(mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(-mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        //rotate
        //Child of main camera tends to jitter a lot, 
        //so move swaycontainer outside and multiply 'targetRotation' by 'cam.transform.rotation' will give the combined quaternion
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation * cam.transform.rotation, smooth * Time.deltaTime);

        transform.position = cam.transform.position;
    }
}
