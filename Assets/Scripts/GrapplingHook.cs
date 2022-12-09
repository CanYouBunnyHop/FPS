using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Movement;


namespace Player.Movement
{
    [RequireComponent(typeof(PlayerMovement))]
    public class GrapplingHook : MonoBehaviour
    {
        //---------------------------------------------------------------------------
        // Grapple = Player --to--> target
        // Hook = Player <--to-- target
        //---------------------------------------------------------------------------
        [Header("Grapple Surface")]
        public float maxDistance;
        public float grappleSpeed;
        public float grappleMaxSpeed;
        public float distanceThreshold;
        public float ropeStretchScale;
        public float steeringStrength;
        public LayerMask raycastSurface;

        [Header("Hook Enemy")]
        public float flySpeed;
        public float enemyHookDelay;
        private Transform cam;
        private EnemyMovement em;
        private float lowestDistance; //Updates when rope shorter than previous distance, lowest distance reached during grapple
        [Header("Timings/Other")]
        public GameObject grapplePoint;
        public float delay;
        private PlayerMovement pm;
        public cooldownData cdd;
        //---------------------------------------------------------------------------------------------
        
        private Vector3 lerpPos;
        private LineRenderer rope;
        private RaycastHit hit;
        private Vector3 grappleDir;
        private float distance;
        private bool showRope = false;
        private float initialDistance = 0;
        [SerializeField] private bool wasInAir;
        //private bool isUsing;
        

        void Awake()
        {
            pm = GetComponent<PlayerMovement>();
            cam = Camera.main.transform;
            rope = GetComponent<LineRenderer>();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) && cdd.canUseAbility)
            {
                lerpPos = transform.position;
                //visual
                showRope = true;
                rope.enabled = true;
                em = null;
                
                StartGrapple();
            }

            //visual
            if (showRope)
                DrawRope();

        }
        private void FixedUpdate()
        {
            //Get direction
            Vector3 grappleDisplacement = grapplePoint.transform.position - cam.position;
            grappleDir = grappleDisplacement.normalized;

            //Get distance
            distance = Vector3.Distance(cam.position, grapplePoint.transform.position);

            //Cool down
            if (cdd.cdTimer <= 0 && !cdd.isUsing) //if cooldown is ready and player is not using ability
            {
                cdd.canUseAbility = true;
            }
            else
            {
                cdd.canUseAbility = false;
                cdd.cdTimer -= Time.fixedDeltaTime;
            }

        }
        //testing
        private void OnDrawGizmos()
        {
            // Gizmos.DrawLine(cam.position, grapplePoint.transform.position);
        }
        public void StartGrapple()
        {
            cdd.isUsing = true;
            //if hit something
            if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, raycastSurface))
            {
                if (hit.collider.tag == "Enemy")
                {
                    StartHookEnemy();
                   
                    //cooldown
                    //cdd.cdTimer = cdd.cdTime;
                }
                else
                {
                    StartGrappleSurface();

                    //cooldown
                    //cdd.cdTimer = cdd.cdTime;
                }
            }
            else // if hit nothing
            {
                //set start on line renderer
                lerpPos = transform.position;

                //set visual for emptyhook
                Vector3 emptyHook = cam.forward.normalized * maxDistance;
                grapplePoint.transform.position = transform.position + emptyHook;

                Invoke(nameof(EndGrapple), delay);
            }
        }
        #region  grapple functions, start and loop
        void StartGrappleSurface()
        {
            wasInAir = false;
            //get initial distance
            initialDistance = Vector3.Distance(cam.position, hit.point);

            // set grappling hook target position
            grapplePoint.transform.SetParent(hit.collider.gameObject.transform);
            grapplePoint.transform.position = hit.point;

            //reset value of lowest distance
            lowestDistance = initialDistance;

            //delayed switch state
            IEnumerator DSS = pm.DelayedSwitchState(PlayerMovement.State.GrapSurface, delay);
            StartCoroutine(DSS);

            //reset y velocity if not grounded
            Invoke(nameof(ResetVel), delay - 0.1f);

            //set start on line renderer
            lerpPos = transform.position;
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
        void StartHookEnemy()
        {
            em = hit.collider.GetComponent<EnemyMovement>();

            grapplePoint.transform.SetParent(hit.collider.gameObject.transform);
            grapplePoint.transform.position = hit.collider.transform.position;//render the rope on the center of enemy, different to surfaces

            //delayed switch state
            IEnumerator DSS = pm.DelayedSwitchState(PlayerMovement.State.HookEnemy, delay + enemyHookDelay);
            StartCoroutine(DSS);
        }
        public void ExecuteGrappleSurface()
        {
            //Calc acceleration for grappling by calculating disstance
            float scale = distance / maxDistance;

            //pull player to object/
            pm.playerVelocity += grappleDir * scale * grappleSpeed * Time.deltaTime;
            //steering when grappling
            pm.playerVelocity += SteerGrapple() * Time.deltaTime;

            //stop players from speeding up infinitely by capping speed

            pm.playerVelocity.x = Mathf.Clamp(pm.playerVelocity.x, -grappleMaxSpeed, grappleMaxSpeed);
            pm.playerVelocity.y = Mathf.Clamp(pm.playerVelocity.y, -grappleMaxSpeed, grappleMaxSpeed);
            pm.playerVelocity.z = Mathf.Clamp(pm.playerVelocity.z, -grappleMaxSpeed, grappleMaxSpeed);
        }

         #region  exe grappleEnemy (not used)
        public void ExecuteGrappleEnemy()
        {
            //pull player to object/
            pm.playerVelocity += grappleDir * grappleSpeed * Time.deltaTime;
        }
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
        Vector3 SteerGrapple()
        {
            Vector3 dir = cam.forward;
            return cam.forward * steeringStrength;
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

        //End grapple after reaching destination
        public void CheckDistanceAfterGrapple()
        {
            if (distance <= distanceThreshold)
            {
                EndGrapple();
            }
        }
        public void CancelHookInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
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
            pm.CheckGroundedOrInAir();

            if(em != null && em.currentState == EnemyMovement.State.Hooked)
            {
                em.CheckGroundedOrInAir();
                em = null;
            }

            showRope = false;
            rope.enabled = false;
            initialDistance = 0;
            wasInAir = false;

            //Initiate Cooldown
            cdd.cdTimer = cdd.cdTime;
        }
        #endregion

        void DrawRope()
        {
            rope.SetPosition(0, transform.position);
            lerpPos = Vector3.Lerp(lerpPos, grapplePoint.transform.position, delay - 0f);
            rope.SetPosition(1, lerpPos);
        }
        public void ResetVel()
        {
            //apply equal opposite momentum
            if (pm.playerVelocity.y <= 0 && !pm.isGrounded)
            {

                pm.playerVelocity -= Vector3.up * Vector3.Dot(pm.playerVelocity, Vector3.up);
            }
        }

    }
}