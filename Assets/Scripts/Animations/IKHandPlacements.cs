using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandPlacements : MonoBehaviour
{
    [SerializeField]private Animator anim;
    [SerializeField]private Transform handLTarget;
    [SerializeField]private Transform handRTarget;
     private void OnAnimatorIK(int layerIndex)
   {
        if(anim)
        {   
            anim.SetIKPosition(AvatarIKGoal.LeftHand, handLTarget.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, handRTarget.position);

            anim.SetIKRotation(AvatarIKGoal.LeftHand, handLTarget.rotation);
            anim.SetIKRotation(AvatarIKGoal.RightHand, handRTarget.rotation);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);

            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        }
   }
}
