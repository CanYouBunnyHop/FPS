using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Movement;
using UnityEngine.InputSystem;
using FPS.Settings;


namespace FPS.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class GrapplingHook : MonoBehaviour
    {
        //---------------------------------------------------------------------------
        // Grapple = Player --to--> target
        // Hook = Player <--to-- target
        //---------------------------------------------------------------------------
        [Header("Grapple Surface")]
        public float grappleSpeed;
        public float grappleMaxSpeed;
        ///<summary> how close do you need to be to the grapple target pos to cancel hook </summary>
        public float distanceThreshold;
        public float ropeStretchScale; 
        public float steeringStrength;
        public AnimationCurve steeringStrengthMult;
        private float steeringDuration;
        public float grappleAirFriction;


        [Header("Hook Enemy")]
        public float flySpeed;
        public float enemyHookDelay;
        private EnemyMovement em;
        
        //--------------------------------------------------------------------------------------------------------------------------------------

        [Header("Timings/Settings")]
        public float maxDistance;
        public float delay;
        public float fovBreak;

        [Header("References")]
        public LayerMask raycastSurface;
        public GameObject grapplePoint;
        private PlayerMovement pm;
        [SerializeField] private PlayerStateMachine pStateMachine; 
        private Transform cam;
        [SerializeField]private cooldownDataSO cdd;
        //---------------------------------------------------------------------------------------------
        //Render
        private Vector3 lerpPos;
        private LineRenderer rope;
        //---------------------------------------------------------------------------------------------
        [Header("Debug")]
        [SerializeField] private bool wishGrapHook;
        public bool WishGrapHook => wishGrapHook;
        [SerializeField] private float lowestDistance; //Updates when rope shorter than previous distance, lowest distance reached during grapple
        [SerializeField] private bool wasInAir;
        private RaycastHit hit;
        private Vector3 grappleDir;
        private float distance;
        private float initialDistance = 0;
        

        void Awake()
        {
            pm = GetComponent<PlayerMovement>();
            cam = Camera.main.transform;
            rope = GetComponent<LineRenderer>();
            rope.enabled = false;

            cdd.AwakeTimer();
        }
        public void GrappleHook_Input()
        {
            wishGrapHook = Input.GetKeyDown(KeyCode.Q) && cdd.canUseAbility;
        }
        private void LateUpdate()
        {
            //visual
            if (rope.enabled) DrawRope();
        }
        private void FixedUpdate()
        {
            //Get direction
            Vector3 grappleDisplacement = grapplePoint.transform.position - cam.position;
            grappleDir = grappleDisplacement.normalized;

            //Get distance
            distance = Vector3.Distance(cam.position, grapplePoint.transform.position);

            cdd.CoolingDown();

        }
        //testing
        public void StartGrappleHook()
        {
            cdd.isUsing = true;
            lerpPos = transform.position;

            //turn on visual
            rope.enabled = true;
            em = null;
            
            //if hit something
            if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, raycastSurface))
            {
                if (hit.collider.tag == "Enemy")
                {
                    //StartHookEnemy();
                    // set grappling hook target position
                    grapplePoint.transform.SetParent(hit.collider.gameObject.transform);
                    grapplePoint.transform.position = hit.collider.transform.position;  
                    StartCoroutine(pStateMachine.DelayExitState(pStateMachine.actionSubStates, 1, delay));
                    //pm.currentActionSubState.ExitState(pm, ref pm.currentActionSubState, pm.actionSubStates[1]);
                }
                else
                {
                    //StartGrappleSurface();
                    grapplePoint.transform.SetParent(hit.collider.gameObject.transform);
                    grapplePoint.transform.position = hit.point;
                    StartCoroutine(pStateMachine.DelayExitState(pStateMachine.actionSubStates, 2, delay));
                    //pm.currentActionSubState.ExitState(pm, ref pm.currentActionSubState, pm.actionSubStates[2]);
                    Debug.Log("Grapple Raycast Hit Surface");
                }
            }
            
            else // if hit nothing
            {
                //set visual for emptyhook
                Vector3 emptyHook = cam.forward.normalized * maxDistance;
                grapplePoint.transform.position = transform.position + emptyHook;

                Invoke(nameof(EndGrapple), delay);
            }
        }
        #region  grapple functions, start and loop
        public void StartGrappleSurface()
        {
            Debug.Log("ADD");
            wasInAir = false;
            //get initial distance
            initialDistance = Vector3.Distance(cam.position, hit.point);

            //reset value of lowest distance
            lowestDistance = initialDistance;

            //delayed switch state
            // IEnumerator DSS = pm.DelayedSwitchState(PlayerMovement.State.GrapSurface, delay);
            // StartCoroutine(DSS);
            //StartCoroutine(pm.DelayExitState(pm.actionSubStates, 2, delay));

            //reset y velocity if not grounded
            Invoke(nameof(ResetVel), delay - 0.1f);

            //set start on line renderer
            lerpPos = transform.position;

            //reset this value to 0
            steeringDuration = 0;
        }
    #region  grapple enemy(not used)
        //void StartGrappleEnemy()
        //{
        //     pm.playerVelocity = Vector3.zero;
        //     //get initial distance
        //     initialDistance = Vector3.Distance(cam.position, hit.point);

        //     // set grappling hook target position
        //     grapplePoint.transform.SetParent(hit.collider.gameObject.transform);
        //     grapplePoint.transform.position = hit.collider.transform.position;

        //     //reset value of lowest distance
        //     lowestDistance = initialDistance;

        //     //delayed switch state
        //     IEnumerator DSS = pm.DelayedSwitchState(PlayerMovement.State.GrapEnemy, delay);
        //     StartCoroutine(DSS);

        //     //reset y velocity if not grounded
        //     Invoke(nameof(ResetVel), delay - 0.1f);

        //     //set start on line renderer
        //     lerpPos = transform.position;
        //}
    #endregion
        public void StartHookEnemy()
        {
            em = hit.collider.GetComponent<EnemyMovement>();

            grapplePoint.transform.SetParent(hit.collider.gameObject.transform);
            grapplePoint.transform.position = hit.collider.transform.position;//render the rope on the center of enemy, different to surfaces

            //delayed switch state
            // IEnumerator DSS = pm.DelayedSwitchState(PlayerMovement.State.HookEnemy, delay + enemyHookDelay);
            // StartCoroutine(DSS);
            //StartCoroutine(pm.DelayExitState(pm.actionSubStates, 1, delay));
        }
        public void ExecuteGrappleSurface()
        {
            //Calc acceleration for grappling by calculating disstance
            float scale = distance / maxDistance;

            //slows player down if player has been over spinning, similar to friction
            float omDotVD = 1 - Vector3.Dot(pm.playerVelocity.normalized, grappleDir);//omDotVD = one minus dot velocity direction
            pm.playerVelocity -= (pm.playerVelocity * omDotVD) * grappleAirFriction * Time.deltaTime;

            //pull player to object, always
            Vector3 pullForce = grappleDir * scale * grappleSpeed * Time.deltaTime;
            pm.playerVelocity += pullForce;

            //steering when grappling
            pm.playerVelocity += SteerGrapple() * Time.deltaTime;

            //stop players from speeding up infinitely by capping speed
            pm.playerVelocity = Vector3.ClampMagnitude(pm.playerVelocity, grappleMaxSpeed);

            steeringDuration += Time.deltaTime;
        }

    #region  exe grappleEnemy (not used)
        // public void ExecuteGrappleEnemy()
        // {
        //     //pull player to object/
        //     pm.playerVelocity += grappleDir * grappleSpeed * Time.deltaTime;
        // }
    #endregion
        public void ExecuteHookEnemy()
        {
            //Get controller on enemy
            CharacterController cc = em.gameObject.GetComponent<CharacterController>();
            //change enemy state
            em.currentState = EnemyMovement.State.Hooked;
            //pull enemy to player
            Vector3 pullEnemy = new Vector3();
            //cc.GetComponent<EnemyMovement>().enemyVelocity = Vector3.zero;
            pullEnemy = -grappleDir * flySpeed * Time.deltaTime;

            if(cc.enabled)
            cc.Move(pullEnemy * Time.deltaTime);

        }
        #endregion 
        private Vector3 SteerGrapple()
        {
            float m = steeringStrengthMult.Evaluate(steeringDuration);
            m = Mathf.Clamp(m, 0, steeringStrengthMult.Evaluate(steeringDuration));

            Vector3 dir = cam.forward;

            //guadually slow down when steering for too long
            return cam.forward * steeringStrength * m;
        }

        #region  rope breaking 
        public void CheckRopeStretch()
        {
            if (distance < lowestDistance)
            {
                lowestDistance = distance;
            }

            //break grapple if rope stretched too much
            if (distance > lowestDistance + initialDistance * ropeStretchScale)
            {
                EndGrapple();
            }
        }
        public void CheckPlayerFov()
        {
            Vector3 displacement = grapplePoint.transform.position - cam.position;
            
            if(Vector3.Angle(displacement, cam.forward) >= fovBreak)
            {
                EndGrapple();
            }
        }
        public void CheckDistanceThreshold()
        {
            if (distance <= distanceThreshold)  //End grapple after reaching destination
            {
                EndGrapple();
            }
        }
        public void CancelHook_Input(PlayerInputSystemManager _manager)
        {
            if (_manager.grappleHook.triggered)
            {
                EndGrapple();
            }
        }
        public void CheckIfPlayerLanded()
        {
            //if player leave ground after grapple, then landed, end grapple
            if (!pm.isGrounded)
            {
                wasInAir = true;
            }
            if (wasInAir && pm.isGrounded)
            {
                EndGrapple();
            }
        }
        public void CheckObstaclesBetween()
        {
            RaycastHit hit;
            bool hitGround = Physics.Linecast(transform.position, grapplePoint.transform.position, out hit, pm.groundMask, QueryTriggerInteraction.Ignore);
            if (hitGround)
            {
                EndGrapple();
            }
        }
        public void EndGrapple()
        {
            cdd.isUsing = false;
            pStateMachine.Core_StateCheck();

            if(em != null && em.currentState == EnemyMovement.State.Hooked)
            {
                em.CheckGroundedOrInAir();
                em = null;
            }

            rope.enabled = false;
            initialDistance = 0;
            wasInAir = false;

            //Initiate Cooldown
            //cdd.cdTimer = cdd.cdTime;
            cdd.InitiateCoolDown();
            pStateMachine.currentActionSubState.ExitState(pStateMachine, ref pStateMachine.currentActionSubState, pStateMachine.actionSubStates[0]); //return to idle in action state
        }
        #endregion

        void DrawRope()
        {
            rope.SetPosition(0, transform.position);
            lerpPos = Vector3.Lerp(lerpPos, grapplePoint.transform.position, delay - 0f);
            rope.SetPosition(1, lerpPos);
        }
        private void ResetVel()
        {
            //apply equal opposite momentum
            if (pm.playerVelocity.y <= 0 && !pm.isGrounded)
            {
                pm.playerVelocity -= Vector3.up * Vector3.Dot(pm.playerVelocity, Vector3.up);
            }
        }

    }
}