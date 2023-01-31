using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootPlacements : MonoBehaviour
{
   [SerializeField]private Animator anim;
   [SerializeField]private float yOffset;
   [SerializeField]private float detectionRange;
   [SerializeField]private LayerMask groundMask;
   [SerializeField]private Transform footR, footL;
   [SerializeField]private float speed, rotSpeed;
  
   
   
   private Vector3 footPosTarget;
   private Quaternion footRotTarget;
   //private Vector3 footRMoveZX, _footPosMoveZX;
   //private RaycastHit hit, hit2;


   private void Update()
   {
        
        
   }
   private void OnAnimatorIK(int layerIndex)
   {
       if(anim)
       {
            //left foot
            IKFootPlacement(AvatarIKGoal.LeftFoot, footL);

            //right foot
            IKFootPlacement(AvatarIKGoal.RightFoot, footR);
       }
   }
   private void IKFootPlacement(AvatarIKGoal _aIKGoal, Transform _footPos)
   {

            RaycastHit checkHit;
            Ray checkRay = new Ray(_footPos.position + Vector3.up, Vector3.down); // do a raycast downwards first 
           
                Ray ray = Physics.Raycast(checkRay, out checkHit, 2, groundMask)? 
                new Ray(_footPos.position + Vector3.up, -Vector3.Reflect(Vector3.down, checkHit.normal)) :  //new raycast adjusted with check normal
                new Ray(_footPos.position + Vector3.up, Vector3.down);

            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, detectionRange , groundMask))
            {
                float displacement = _footPos.position.y - hit.point.y;
                Mathf.Clamp(displacement, 0, displacement);

                anim.SetIKPositionWeight(_aIKGoal, 1 - displacement); //set foot weights
                anim.SetIKRotationWeight(_aIKGoal, 1 );


                footPosTarget = new Vector3(hit.point.x, hit.point.y + yOffset ,  hit.point.z);
            }

        //Position Update
        Vector3 footMove = anim.GetIKPosition(_aIKGoal); 
        Vector3 currentFootMove = new Vector3(footMove.x, footPosTarget.y * checkHit.normal.y ,footMove.z); //get IK position
        currentFootMove = Vector3.MoveTowards(currentFootMove, footPosTarget, speed * Time.deltaTime);  //Move towards the IK goal 
        anim.SetIKPosition(_aIKGoal, currentFootMove); //set the IK position to currentFootMove

        //Rotation Update
        Quaternion currentFootRot = anim.GetIKRotation(_aIKGoal);
        footRotTarget = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal); //set the target rotation
        currentFootRot = Quaternion.RotateTowards(currentFootRot,footRotTarget, rotSpeed * Time.deltaTime); //rotate current rotation towards target rotation
        anim.SetIKRotation(_aIKGoal, currentFootRot); //set the IK Rotation to currentFootRot

   }
}
