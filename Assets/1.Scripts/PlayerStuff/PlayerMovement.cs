using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using FPS.Settings.Keybinds;
//-------------------------------------------------------------------------------------------------------
// Remember To change the input manager under project settings for adding counter-strafing
//
// Horizontal && Vertical axis:
//      gravity = 8     (this makes movement less slippery, but still slides)
//      sensitivity = 1 
//      Snap = false    (this is so the input gets a gradual change, important if you want counter strafe
//-------------------------------------------------------------------------------------------------------
namespace FPS.Player.Movement
{
//[RequireComponent(typeof(PlayerInputSystemManager))]
public class PlayerMovement : MonoBehaviour
{
#region references
    [Header("References")]
    public float gravity;
    private CharacterController controller;
    public GrapplingHook hook; //Additional movement abilities
    public LayerMask groundMask;
    [SerializeField] private PlayerInputSystemManager inputSystemManager;
#endregion

//--------------------------------------------------------------------------------------------------------
#region Movement Parameters
    [Header("Jumping")]
    public float jumpForce;
    private RaycastHit onSlope;
    private Vector3 slopeMoveDir;// => Vector3.ProjectOnPlane(wishdir, onSlope.normal);
    bool called; //bool for wish jump
//--------------------------------------------------------------------------------------------------------
    [Header("Ground Move")]
    public float groundSpeed; // Moving on ground;
    public float groundAccelerate;
    public float friction;
    ///<summary> how much physic steps later to start friction </summary>
    [Tooltip("how much physic steps later to start friction")][SerializeField]private float lateFrictionDelay;
    [SerializeField]private float wishSpeed;
    private float TotalGroundSpeed;

    [Header("Air Move")]
    public float airSpeed;
    public float airAccelerate;
    public float capBhopSpeed;
    private float airWishSpeed;

//------------------------------------------------------------------------------------------------------
    [Header("Crouch")]
    [SerializeField] private float crouchSpeedMult;
    [SerializeField] private float crouchInOutSpeed;
    [SerializeField] private float crouchDistance;
    [SerializeField] private bool wishCrouching;
    //public bool isCrouching => wishCrouching && isGrounded;
    private float standYScale;

    [Header("Sprint")]
    [SerializeField] private float sprintSpeedMult;
    [SerializeField] private bool wishSprinting;
#endregion
//------------------------------------------------------------------------------------------------------
#region enums
    public enum State
    {
        Grounded,
        InAir,
        GrapSurface,
        HookEnemy,
    }
    public enum GroundSubState
    {
        Walk,
        Sprint,
        Crouch,
        Slide,
    }
    public AbstractState<PlayerMovement> currentGroundSubStateV2;
    ///<summary> 0, walk | 1, crouch | 2, sprint </summary>///
    public Dictionary<int, AbstractState<PlayerMovement>> groundSubStates = new Dictionary<int, AbstractState<PlayerMovement>>()
    {
        
        {0, new PlayerState_Walk()}, 
        {1, new PlayerState_Crouch()}, 
        {2, new PlayerState_Sprint()}
    };

#endregion
#region Snap On Ground
    [Header("Snap On Ground")] [Tooltip("if the hit normal is less than this, abort snap, for steep angles")]
    [SerializeField]private float minGroundDotProduct;
    [SerializeField]private float snapMaxVel; //abort snap if playervel is greater
#endregion
//------------------------------------------------------------------------------------------------------
#region Debug variables
    [Header("Debug")]
    public State currentState;
    public GroundSubState curGroundSubState; //new
    //public GrappleState currentGrappleState;
    public Vector3 playerVelocity;
    public bool isGrounded {get; private set;}
    [SerializeField]public int stepInAir {get; private set;}
    [SerializeField]public int stepOnGround {get; private set;}
    [SerializeField]public int stepSinceJumped {get; private set;} //assign this to anything that will knock player up
    [SerializeField]public int stepSinceKockback;
    [SerializeField]private float currentSpeed; //not actual speed, dot product of playervel and wish vel
    private Vector3 wishdir;
    [SerializeField]private bool wishJump = false;
    
    public Text text;//UI remove later

    [Header("Debug Gizmo")]
    [SerializeField] private float snapGroundRayLength;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private float groundCheckRadius = 0.3f;

//--------------------------------------------------------------------------------------------------------
    public Vector3 playerZXVel => new Vector3(playerVelocity.x, 0, playerVelocity.z); 
    private bool isOnSlope => Physics.Raycast(transform.position, Vector3.down, out onSlope, 0.6f, groundMask) ? true : false;
    [SerializeField] Coroutine jumpBufferTimer; 
#endregion
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        hook = GetComponent<GrapplingHook>();

        currentState = State.InAir;
        curGroundSubState = GroundSubState.Walk;
        StartCoroutine(LateFixedUpdate());

        standYScale = transform.localScale.y; //get player's y scale which is the standing's height

        currentGroundSubStateV2 = groundSubStates[0];
        currentGroundSubStateV2.EnterState(this);
    }
#region Inputs
    void Direction_Input(float _GroundOrAirSpeed, out float _wishSpeed)
    {
        //get WASD input into wish direction
        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);
		wishdir.Normalize();

        _wishSpeed =  wishdir.magnitude;
        _wishSpeed *= _GroundOrAirSpeed;
    }
    
    void Jump_Input()
    {
         //Inputs for jump
        if(inputSystemManager.jump.triggered) //if(Input.GetKeyDown(KeyCode.Space)||Input.GetAxis("Mouse ScrollWheel") < 0f )
        {
        
            wishJump = true;
            //Hook cancel with jump
            if(currentState == State.GrapSurface || currentState == State.HookEnemy)
            {
                //when not grounded, another jump input will cancel hook
                if(!isGrounded)
                hook.EndGrapple();
            }

        }

        
        if(wishJump && jumpBufferTimer == null)
        {
            jumpBufferTimer = StartCoroutine(JumpBufferTimer());
        }

        IEnumerator JumpBufferTimer()
        {
            //Debug.Log("started");
            yield return new WaitForSecondsRealtime(0.1f);

            if(!isGrounded) wishJump = false;

            jumpBufferTimer = null;
        }
    }
    
    void Crouch_Input()
    {   
        wishCrouching = Input.GetKey(KeyCode.LeftControl) && isGrounded;
        
    }
    void Sprint_Input()
    {
        wishSprinting = Input.GetKey(KeyCode.LeftShift) && !wishCrouching && isGrounded;
    }
    
#endregion


#region Updateloops Statemachine / gizmo
    void Update()
    {
        controller.Move(playerVelocity * Time.deltaTime); //since controller don't use unity's physics update, we can getaway with update
#region Input StateMachine (Update)
        switch(currentState)
        {
            case State.Grounded:
                Ground_InputSubStateMachine(); //sub stateMachine
                Jump_Input();
                Crouch_Input();
                Sprint_Input();

                //Ground_InputSubStateMachine();
            break;

            case State.InAir:
                Direction_Input(airSpeed, out airWishSpeed);
                Jump_Input();
                Crouch_Input();
            break;

            case State.GrapSurface:
                Direction_Input(airSpeed, out airWishSpeed);
                Jump_Input();
                hook.CancelHookInput();
            break;

            case State.HookEnemy:
                Direction_Input(groundSpeed, out wishSpeed);
                hook.CancelHookInput();
            break;
        }
#endregion
        //subStateMachine
        void Ground_InputSubStateMachine()
        {
            // switch(curGroundSubState)
            // {
            //     case GroundSubState.Walk:
            //     Direction_Input(groundSpeed, out wishSpeed);
            //     break;

            //     case GroundSubState.Sprint:
            //     Direction_Input(groundSpeed, out wishSpeed);
            //     break;

            //     case GroundSubState.Crouch:
            //     Direction_Input(groundSpeed, out wishSpeed);
            //     break;

            //     case GroundSubState.Slide:
            //     break;
            // }
            switch(currentGroundSubStateV2)
            {
                case var state when state == groundSubStates[0]: //pattern matching (walk)
                Direction_Input(groundSpeed, out wishSpeed);
                break;

                case var state when state == groundSubStates[1]: //pattern matching (crouch)
                Direction_Input(groundSpeed, out wishSpeed);
                break;

                case var state when state == groundSubStates[2]: //pattern matching (sprint)
                Direction_Input(groundSpeed, out wishSpeed);
                break;
            }

        }

        //UI remove later
        string dec = Convert.ToString(wishSpeed);
        text.text = "currentspeed: "+ playerZXVel.magnitude;
    }
    void FixedUpdate()
    {
        currentSpeed = Vector3.Dot(wishdir, playerVelocity);

        //Check grounded
        isGrounded = Physics.CheckSphere(transform.position + groundCheckOffset , groundCheckRadius , groundMask);
        
        //roundup small numbers avoid small movements
        if(Mathf.Abs(playerZXVel.magnitude) < 0.1) //Horizontal velocity rounded
        {
            playerVelocity.x = 0;
            playerVelocity.z = 0;
        }

        //count steps //stay in fixedUpdate
        stepSinceJumped += 1;
        stepSinceKockback += 1;

        //jumping buffer, runs outside of state handler currently, 
        //reset y vel avoid jump boost on after running on slope
        if (wishJump && isGrounded)
        {
            playerVelocity.y = 0;
            playerVelocity.y += jumpForce;
            stepSinceJumped = 0;
        }

        //calc total ground speed
        Check_GroundSubState();
        //TotalGroundSpeed = 
        Debug.Log(stepInAir);
        

#region PhysicsStateHandler(old)
        //state Physics handling
        switch(currentState)
        {
            case State.Grounded:
            {
                //GroundPhysics(currentSpeed); //unlike controller.Move(), physics have to be in fixed update for some reason, buggy in Update()
                Check_GroundedOrInAir();      //Maybe because most of my physics calculation is already in fixedUpdate
                //
                Check_CrouchingStanding();                             // Maybe if everything is in Update() movement will be smoother, (not a big difference)

                Ground_PhysicsSubStateMachine();
            }
            break;
            case State.InAir:
            {
                AirPhysics(currentSpeed);
                Check_GroundedOrInAir();
                CapBhopSpeed();

                SnapOnGround();
                Check_CrouchingStanding();
            }
            break;
            case State.GrapSurface:
            {
            
                hook.CheckDistanceAfterGrapple();
                hook.CheckRopeStretch();
                hook.CheckPlayerFov();
                hook.CheckIfPlayerLanded();
                hook.ExecuteGrappleSurface();
                //for better air steering with grapple
                //if(!isGrounded)AirPhysics(currentSpeed);
            
            }
            break;
            case State.HookEnemy:
            {
                hook.ExecuteHookEnemy();
                hook.CheckDistanceAfterGrapple();
                hook.CheckObstaclesBetween();
                if(isGrounded)
                {
                    GroundPhysics(1, currentSpeed);
                }
                else
                {
                    AirPhysics(currentSpeed);
                    CapBhopSpeed();
                }
            }
            break;
        }
        //subStateMachine
        void Ground_PhysicsSubStateMachine()
        {
            // switch(curGroundSubState)
            // {
            //     case GroundSubState.Walk:
            //     GroundPhysics(1,currentSpeed);
            //     break;

            //     case GroundSubState.Sprint:
            //     GroundPhysics(sprintSpeedMult, currentSpeed);
            //     break;

            //     case GroundSubState.Crouch:
            //     GroundPhysics(crouchSpeedMult, currentSpeed);
            //     break;

            //     case GroundSubState.Slide:
            //     break;

            // }
            switch(currentGroundSubStateV2)
            {
                case var state when state == groundSubStates[0]: //pattern matching (walk)
                GroundPhysics(1,currentSpeed);
                break;
                
                case var state when state == groundSubStates[1]: //pattern matching (crouch)
                GroundPhysics(crouchSpeedMult, currentSpeed);
                break;

                case var state when state == groundSubStates[2]: //pattern matching (sprint)
                GroundPhysics(sprintSpeedMult, currentSpeed);
                break;
            }

        }

#endregion
        
    }   
    IEnumerator LateFixedUpdate()
    {
        while(true)
        {
            yield return new WaitForFixedUpdate(); //apply after fixedUpdate

            switch(currentState)
            {
                case State.Grounded:
                {
                    if(stepOnGround > lateFrictionDelay)
                    ApplyFriction();
                }
                break;
                case State.InAir:
                {
                   //SnapOnGround(); //Has to later than fixedUpdate, or else, will constantly snap after jumping
                }
                break;
                case State.GrapSurface:
                {
                    
                
                }
                break;
                case State.HookEnemy:
                {
                    
                }
                break;
            }
        }
    }
    void OnDrawGizmos()
    {
        //Gizmos.DrawLine(groundCheck.position, Vector3.forward);
        Gizmos.DrawRay(transform.position, Vector3.down * snapGroundRayLength);

        Gizmos.color = new Color(1,1,1,0.5f);
        Gizmos.DrawSphere(transform.position + groundCheckOffset, groundCheckRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + new Vector3(0,1,0), Vector3.down * 0.9f);
    }
#endregion

#region Source Engine Code translated to Unity C# / valve physics calculation
    void GroundPhysics(float _mult, float _currentSpeed)//_mult = sprint or crouch multiplier
    {
            wishSpeed *= _mult;
            float addSpeed = wishSpeed - _currentSpeed; //addspeed is the capped speed
            float accelSpeed = wishSpeed * friction * Time.deltaTime * groundAccelerate; // without deltatime, player accelerate almost instantly
            
             if (accelSpeed > addSpeed) //cap speed
            {
                accelSpeed = addSpeed;
            }
            
            //WASD movement
            if(!isOnSlope)
            {
                //if not on slope use wishDir
                playerVelocity.z += accelSpeed* wishdir.z;
                playerVelocity.x += accelSpeed* wishdir.x;
            }
            else
            {
                //when player is on slope use slope dir
                slopeMoveDir = Vector3.ProjectOnPlane(wishdir, onSlope.normal);
                playerVelocity += accelSpeed * slopeMoveDir;
            }
            
    }
    void AirPhysics(float _currentSpeed)
    {
            float addSpeed = airWishSpeed - _currentSpeed;
            addSpeed = Mathf.Clamp(addSpeed, 0, Mathf.Infinity); // make sure you don't and slow down when input same direction midair
            float accelSpeed = airWishSpeed * Time.deltaTime * airAccelerate; // without deltatime, player accelerate almost instantly
            
            //Debug.Log("aird "+ addSpeed);
            if(accelSpeed > addSpeed)
            {
                accelSpeed = addSpeed;
            }
             //WASD movement

            playerVelocity.z += accelSpeed* wishdir.z;
            playerVelocity.x += accelSpeed* wishdir.x;

            //gravity
            playerVelocity.y += gravity * Time.deltaTime;
    }
#endregion

#region Ground air check + cap bhop speed + crouch
    void CapBhopSpeed()
    {
        //capping bhop speed 
        playerVelocity.z = Mathf.Clamp(playerVelocity.z, -capBhopSpeed, capBhopSpeed);
        playerVelocity.x = Mathf.Clamp(playerVelocity.x, -capBhopSpeed, capBhopSpeed);
    }

    public void Check_GroundedOrInAir()
    {
        if(isGrounded) //while grounded
        {
            currentState = State.Grounded;

            //count steps                
            stepInAir = 0;
            stepOnGround += 1;
        }
        else // while in air
        {
            currentState = State.InAir;
            
            //count steps
            stepOnGround = 0;
            stepInAir += 1;
        }
    }

    ///<Summary> The physics update for switching between Crouching and standing, 
    ///also pushes player upwards if player is stuck in ground. </Summary>
    private void Check_CrouchingStanding()
    {
        Vector3 targetScale = wishCrouching ?  //if holding inputs or not
            targetScale = new Vector3(1, standYScale - crouchDistance, 1) :      //crouched
            targetScale = new Vector3(1, standYScale, 1);                       //stand

        if(transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, crouchInOutSpeed * Time.deltaTime);
        }

        //push player up if he is underground
        RaycastHit[] hits = Physics.RaycastAll(transform.position + new Vector3(0,1,0), Vector3.down , 0.9f, groundMask);
        if(hits.Length > 0)
        {   
            playerVelocity.y += Mathf.Lerp(playerVelocity.y, crouchInOutSpeed , crouchInOutSpeed * Time.deltaTime); 
        }
    }

    void Check_GroundSubState()//ref float o_CrouchMult, ref float o_SprintMult)
    {
        if(!wishCrouching && !wishSprinting && currentGroundSubStateV2 != groundSubStates[0]) //walk
        {
            //curGroundSubState = GroundSubState.Walk; //CurGroundSubState.Exit( target state = walk);
            currentGroundSubStateV2.ExitState(this, ref currentGroundSubStateV2, groundSubStates[0]);
            //currentGroundSubStateV2 = groundSubStates[0];
        } 

        if(wishCrouching && currentGroundSubStateV2 != groundSubStates[1]) //crouch
        { 
            //curGroundSubState = GroundSubState.Crouch;
            currentGroundSubStateV2.ExitState(this, ref currentGroundSubStateV2, groundSubStates[1]);
            //currentGroundSubStateV2 = groundSubStates[1];
        }

        if(wishSprinting && currentGroundSubStateV2 != groundSubStates[2]) //sprint
        {
            //curGroundSubState = GroundSubState.Sprint;
            currentGroundSubStateV2.ExitState(this, ref currentGroundSubStateV2, groundSubStates[2]);
            //currentGroundSubStateV2 = groundSubStates[2];
        }
    } 

#endregion

#region friction/equal opposite force
    void ApplyFriction()
    {
        Vector2 vec = new Vector2(playerVelocity.x, playerVelocity.z);
        float speed = vec.magnitude;

        if (speed != 0) // To avoid divide by zero errors
        {
            float drop = speed * friction * Time.deltaTime;
            playerVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }
    }
    //Apply equal opposite force after hitting something, like a wall, resets momentum. For some reason it don't work on ceiling
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(groundMask == (groundMask | (1 << hit.gameObject.layer)) && wishdir.magnitude == 0 || !isGrounded)
        {                                                           // using bitwise operators equivalent of 
        //                                                          // if(hit.gameObject.layer == groundMask), 
        //                                                          // if using '==' would actually works
        //                                                          // shift operator example
        //                                                          // A = 1, which is '0000 0001' in 8 bit
        //                                                          // A << 2 = '0000 0100' which is '4'
        //
        // layerMask is an integer formatted as a bitmask where every '1' represents a layer to include 
        // and every '0' represents a layer to exclude.
        // if GroundMask = '0000 1001' = 'layer 0' + 'layer 3', and hit.layer = 3(index)  = '0000 1000'(layerMask)
        // even doh layer 3 is true in the inspector
        // because '0000 1001' != '0000 1000', it won't have the desired results
        // if('0000 1001' == '0000 1001' | ('0000 0001' << '3')) after applying the shift which then is
        // if('0000 1001' == '0000 1001' | ('0000 1000') then applying the 'or' bitwise operator
        // if('0000 1001' == '0000 1001')   in this case, the satement is true, because layer 3 is included

        //have to use hit.move direction for y rather than playervelocity, otherwise the check is buggy
           playerVelocity.z -= hit.normal.z * Vector3.Dot(playerVelocity, hit.normal);
           playerVelocity.x -= hit.normal.x * Vector3.Dot(playerVelocity, hit.normal);

            //stay on ceiling just a bit longer, otherwise slight touch with the ceiling will start going down fast
            if(playerVelocity.y > 2 && (controller.collisionFlags & CollisionFlags.Above) != 0)
            playerVelocity.y -= hit.normal.y * Vector3.Dot(hit.moveDirection, hit.normal);

            //when grounded apply the raycast normal direction, works on flat and slope surface
            // if(isGrounded)
            // {
            //     //Vector3 oppositeForce = onSlope.normal * Vector3.Dot(playerVelocity, onSlope.normal);
            //     //playerVelocity -= oppositeForce; ///reset yvel
            //     //playerVelocity.y = Mathf.Lerp(playerVelocity.y, -1f, 0.5f/Time.deltaTime); 

            //     //need to apply opposite normal force instead
            //     //Vector3 slopeNormal = -onSlope.normal;
            //     //magnetism = slopeNormal;
            //     //playerVelocity += magnetism.normalized  * Time.deltaTime;
                
            //     ////////////////////////////////////////////////
            //     // Vector3 slopeNormal = -onSlope.normal.normalized;
            //     // magnetism = Vector3.MoveTowards(playerVelocity, slopeNormal, 0.1f);
                    
            //     // playerVelocity += magnetism.normalized  * Time.deltaTime;

            //     if(Physics.Raycast(transform.position, -onSlope.normal.normalized , out RaycastHit hitGround, 0.25f, groundMask))
            //     {
            //         //Vector3 targetPos = new Vector3(transform.position.x, hit.point.y, transform.position.z);

                    
            //     }
            //}
        }
    }
#endregion

#region jump physics
    //private void QueueJump() => playerVelocity.y += jumpForce;
    
    //wish jump false for invoking
    // private void WJF()
    // {
    //     if(!isGrounded)
    //     wishJump = false;

    //     called = false;
    // }
#endregion

#region slope physics
    //handles ground movement when player is on slopes, prevents skipping down slopes 
    // bool IfOnSlope()
    // {
    //     return Physics.Raycast(groundCheck.position, Vector3.down, out onSlope, 0.6f, groundMask) ? true : false;
    // }
    void SnapOnGround()
    {
        //bool r = Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, snapGroundRayLength, groundMask);
       // Debug.Log("bruh"+ hit.normal);
        
        if (stepInAir > 2 || stepSinceJumped <= 3 || stepSinceKockback < 5) //abort when just jumped or havn't been in air long enough
		return ;

        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, snapGroundRayLength, groundMask))
        return;

        if (hit.normal.y < minGroundDotProduct) //if surface is too steep, return
		return;

        if(playerZXVel.magnitude > snapMaxVel)//abort if playervel is too fast
        return;

        float speed = playerVelocity.magnitude;
		float dot = Vector3.Dot(playerVelocity, hit.normal);
        

        if (dot > 0f)
        {
            playerVelocity = (playerVelocity - hit.normal * dot).normalized * speed;
            Debug.Log("snaping to ground now"+ dot);
        }
		
    }
#endregion
    
    //switch state
    public IEnumerator DelayedSwitchState(State _currentState, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        currentState = _currentState;
        yield return null;
    }
    
}
}

