using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] float smooth;
    [SerializeField] float swayMultipier;
    private Quaternion defaultRot;

    private void Awake()
    {
        defaultRot = transform.localRotation;
    }
    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayMultipier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayMultipier;

        //calc target rotation
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        //rotate
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}
