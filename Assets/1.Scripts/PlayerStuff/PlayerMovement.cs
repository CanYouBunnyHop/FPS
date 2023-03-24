using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using FPS.Settings;
using FPS.Player.Movement;
using FPS.Weapon;
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
public class PlayerMovement : MonoBehaviour
{
#region references
    [Header("References")]
    
    private CharacterController controller;
    public GrapplingHook hook; //Additional movement abilities
    public LayerMask groundMask;
    [SerializeField] private PlayerInputSystemManager inputSystemManager;
    public PlayerInputSystemManager InputSystemManager => inputSystemManager;
    [SerializeField] private GunManager gm;
#endregion

//--------------------------------------------------------------------------------------------------------
#region Movement Parameters
    [Header("Settings")]
    public float gravity;
    [SerializeField] private float maxSlopeAngleToWalk;

    [Header("Jumping")]
    public float jumpForce;
    private RaycastHit onSlope;
    private Vector3 slopeMoveDir;// => Vector3.ProjectOnPlane(wishdir, onSlope.normal);
    bool called; //bool for wish jump
//--------------------------------------------------------------------------------------------------------
    [Header("Ground Move")]
    [SerializeField]internal float groundSpeed; // Moving on ground;
    [SerializeField]internal float groundAccelerate;
    [SerializeField]internal float friction;
    [SerializeField]internal float slideFriction;
    ///<summary> how much physic steps later to start friction </summary>
    [Tooltip("how much physic steps later to start friction")][SerializeField]private float lateFrictionDelay;
    public float LateFrictionDelay => lateFrictionDelay;
    [SerializeField]internal float wishSpeed;
    private float TotalGroundSpeed;

    [Header("Air Move")]
    [SerializeField]internal float airSpeed;
    [SerializeField]internal float airAccelerate;
    [SerializeField]internal float capBhopSpeed;
    [SerializeField]internal float airWishSpeed;
    //public float AirWishSpeed {get{return airWishSpeed;} set{airWishSpeed = value;}}

//------------------------------------------------------------------------------------------------------
    [Header("Crouch")]
    [SerializeField] private float crouchSpeedMult;
    public float CrouchSpeedMult => crouchSpeedMult; // {get{return crouchSpeedMult;} private set{crouchSpeedMult = value;}}
    [SerializeField] private float crouchInOutSpeed;
    [SerializeField] private float crouchDistance;
    [SerializeField] private bool wishCrouching;
    [SerializeField] private bool canCrouch;
    private float standYScale;

    [Header("Sprint")]
    [SerializeField] private float sprintSpeedMult;
    public float SprintSpeedMult => sprintSpeedMult;
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
    public AbstractState<PlayerMovement> currentCoreState;
    ///<summary> 0, Grounded | 1, InAir </summary>///
    public Dictionary<int, AbstractState<PlayerMovement>> coreStates = new Dictionary<int, AbstractState<PlayerMovement>>()
    {
        {0, new CoreState_Grounded()},
        {1, new CoreState_InAir()},
    };
    public AbstractState<PlayerMovement> currentGroundSubState;
    ///<summary> 0, Walk | 1, Crouch | 2, Sprint | 3, Slide </summary>///
    public Dictionary<int, AbstractState<PlayerMovement>> groundSubStates = new Dictionary<int, AbstractState<PlayerMovement>>()
    {
        {0, new GroundSubState_Walk()}, 
        {1, new GroundSubState_Crouch()}, 
        {2, new GroundSubState_Sprint()},
        {3, new GroundSubState_Slide()},
    };
    public AbstractState<PlayerMovement> currentActionSubState;
    ///<summary> 0, Idle | 1, HookEnemy |2, GrappleSurface| 3, Reloading | 4, WeaponSwitching </summary>///
    public Dictionary<int, AbstractState<PlayerMovement>> actionSubStates = new Dictionary<int, AbstractState<PlayerMovement>>()
    {
        {0, new ActionSubState_Idle()},
        {1, new ActionSubState_HookEnemy()},
        {2, new ActionSubState_GrappleSurface()},
        {3, new ActionSubState_Reloading()},
        {4, new ActionSubState_WeaponSwitching()},
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
    //public State currentState;
    public Vector3 playerVelocity;
    public bool isGrounded {get; private set;}
    [SerializeField]public int stepInAir {get; private set;}
    [SerializeField]public int stepOnGround {get; private set;}
    [SerializeField]public int stepSinceJumped {get; private set;} //assign this to anything that will knock player up
    [SerializeField]public int stepSinceKockback;
    [SerializeField]public float currentSpeed {get; private set;} //not actual speed, dot product of playervel and wish vel
    private Vector3 wishdir;
    [SerializeField]private bool wishJump = false;
    
    public Text text;//UI remove later

    [Header("Debug Gizmo")]
    [SerializeField] private float snapGroundRayLength;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private float groundCheckRadius = 0.3f;

//--------------------------------------------------------------------------------------------------------
    public Vector3 playerZXVel => new Vector3(playerVelocity.x, 0, playerVelocity.z); 
    [SerializeField]private bool isOnSlope;// => Physics.Raycast(transform.position, Vector3.down, out onSlope, 0.6f, groundMask) ? true : false;
    [SerializeField] Coroutine jumpBufferTimer; 
#endregion
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        hook = GetComponent<GrapplingHook>();

        //currentState = State.InAir;
        //curGroundSubState = GroundSubState.Walk;
        StartCoroutine(LateFixedUpdate());

        standYScale = transform.localScale.y; //get player's y scale which is the standing's height

        currentCoreState = coreStates[1];
        //currentCoreState.EnterState(this);

        currentGroundSubState = groundSubStates[0];
        //currentGroundSubState.EnterState(this);

        currentActionSubState = actionSubStates[0];
        //currentActionSubState.EnterState(this);
        
    }
#region Inputs
    internal void Direction_Input(float _GroundOrAirSpeed)
    {
        //get WASD input into wish direction
        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();


        wishSpeed =  wishdir.magnitude;
        wishSpeed *= _GroundOrAirSpeed;
    }
    
    internal void Jump_Input()
    {
         //Inputs for jump
        if(inputSystemManager.jump.triggered) //if(Input.GetKeyDown(KeyCode.Space)||Input.GetAxis("Mouse ScrollWheel") < 0f )
        {
            wishJump = true;
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
    
    internal void Crouch_Input()
    {   
        wishCrouching = inputSystemManager.crouch.IsPressed() && isGrounded ; //Input.GetKey(KeyCode.LeftControl)
    }
    internal void Sprint_Input()
    {
        wishSprinting = inputSystemManager.sprint.IsPressed() && !wishCrouching && isGrounded; //Input.GetKey(KeyCode.LeftShift)
    }
    
#endregion


#region Updateloops Statemachine / gizmo
    void Update()
    {
        IfOnSlope();
        controller.Move(playerVelocity * Time.deltaTime); //since controller don't use unity's physics update, we can getaway with update
       
        

        //new state machine
        currentCoreState.UpdateState(this);
        switch(currentCoreState) //using switch case maybe? because I feel its more readable than putting it in state Classes
        {
            case var x when x is CoreState_Grounded:
            Action_SubstateCheck();
            Ground_SubStateCheck();
            currentGroundSubState.UpdateState(this);
            break;
            case var x when x is CoreState_InAir:
            
            break;
        }
#region Input StateMachine (Update)
        // switch(currentState)
        // {
        //     case State.Grounded:
        //         Ground_InputSubStateMachine(); //sub stateMachine
        //         Jump_Input();
        //         Crouch_Input();
        //         Sprint_Input();
        //         hook.GrappleHook_Input();

        //         Ground_SubStateCheck();
        //     break;

        //     case State.InAir:
        //         Direction_Input(airSpeed); //air wish spd
        //         Jump_Input();
        //         Crouch_Input();
        //         hook.GrappleHook_Input();
        //     break;

            // case State.GrapSurface:
            //     Direction_Input(airSpeed); //air wish spd
            //     Jump_Input();
            //     hook.CancelHook_Input(inputSystemManager.grappleHook);
            // break;

            // case State.HookEnemy:
            //     Direction_Input(groundSpeed);
            //     hook.CancelHook_Input(inputSystemManager.grappleHook);
            // break;
        //}
#endregion

        //UI remove later
        string dec = Convert.ToString(wishSpeed);
        text.text = "currentspeed: "+ playerZXVel.magnitude;

        //remove later, for testing
        if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(currentCoreState.ToString() + currentGroundSubState.ToString());
        }

    }
    void FixedUpdate()
    {
        currentSpeed = Vector3.Dot(wishdir, playerVelocity);

        //Check grounded
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit ghit ,groundCheckRadius);
        float slopeAngle = Vector3.Angle(ghit.normal, Vector3.up);
        isGrounded = Physics.CheckSphere(transform.position + groundCheckOffset , groundCheckRadius , groundMask) && slopeAngle < maxSlopeAngleToWalk;
        
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
        
        //new state machine
        currentCoreState.DuringState(this);

        switch(currentCoreState)//using switch case maybe? because I feel its more readable?
        {
            case var x when x is CoreState_Grounded:
            currentGroundSubState.DuringState(this);
            break;

            case var x when x is CoreState_InAir:
            break;
        }
        currentActionSubState.DuringState(this);

#region PhysicsStateHandler(old)
        //state Physics handling
        // switch(currentState)
        // {
        //     case State.Grounded:
        //     {
        //         //GroundPhysics(currentSpeed); //moved to sub state machine
        //         Core_StateCheck();
        //         Check_CrouchingStanding();                       

        //         Ground_PhysicsSubStateMachine();
        //         //Ground_SubStateCheck();
                
        //     }
        //     break;
        //     case State.InAir:
        //     {
        //         AirPhysics(currentSpeed);
        //         Core_StateCheck();
        //         CapBhopSpeed();

        //         SnapOnGround();
        //         Check_CrouchingStanding();
        //     }
        //     break;
            // case State.GrapSurface:
            // {
            //     hook.CheckDistanceThreshold();
            //     hook.CheckRopeStretch();
            //     hook.CheckPlayerFov();
            //     hook.CheckIfPlayerLanded();
            //     hook.ExecuteGrappleSurface();
            // }
            // break;
            // case State.HookEnemy: //move hook enemy into method, this is not a core state
            // {
            //     hook.ExecuteHookEnemy();
            //     hook.CheckDistanceThreshold();
            //     hook.CheckObstaclesBetween();
            //     if(isGrounded)
            //     {
            //         GroundPhysics(1, currentSpeed);
            //     }
            //     else
            //     {
            //         AirPhysics(currentSpeed);
            //         CapBhopSpeed();
            //     }
            // }
            // break;
        //}
#endregion
    }
    IEnumerator LateFixedUpdate()
    {
        while(true)
        {
            yield return new WaitForFixedUpdate(); //apply after fixedUpdate

            switch(currentCoreState)//using switch case maybe? because I feel its more readable?
            {
                case var x when x is CoreState_Grounded:
                currentGroundSubState.LateDuringState(this);
                break;

                case var x when x is CoreState_InAir:
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
    internal void GroundPhysics(float _mult)//_mult = sprint or crouch multiplier
    {
            wishSpeed *= _mult;
            float addSpeed = wishSpeed - currentSpeed; //addspeed is the capped speed
            float accelSpeed = wishSpeed * friction * Time.deltaTime * groundAccelerate; // without deltatime, player accelerate almost instantly
            
             if (accelSpeed > addSpeed) //cap speed
            {
                accelSpeed = addSpeed;
            }

            if(!isOnSlope)
            {
                playerVelocity.z += accelSpeed * wishdir.z;
                playerVelocity.x += accelSpeed * wishdir.x;
            }
            else
            {
                slopeMoveDir = Vector3.ProjectOnPlane(wishdir, onSlope.normal);
                playerVelocity += accelSpeed * slopeMoveDir;
            }

            //gravity magnetism?
            // if(wishdir != Vector3.zero)
            // playerVelocity +=  (onSlope.normal * gravity) * Time.deltaTime;
    }
    internal void AirPhysics()
    {
            float addSpeed = airWishSpeed - currentSpeed;
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
    internal void CapBhopSpeed()
    {
        //capping bhop speed 
        playerVelocity.z = Mathf.Clamp(playerVelocity.z, -capBhopSpeed, capBhopSpeed);
        playerVelocity.x = Mathf.Clamp(playerVelocity.x, -capBhopSpeed, capBhopSpeed);
    }
    ///<summary> The physics update for switching between Crouching and standing, 
    ///also pushes player upwards if player is stuck in ground. </summary>
    internal void Check_CrouchingStanding()
    {
        //check if anything is blocking player to stand up
        Vector3 headPos = new Vector3(transform.position.x, transform.position.y + controller.height - 0.2f, transform.position.z );
        bool somethingAbove = Physics.CheckSphere(headPos, 0.3f, groundMask);
        //Debug.DrawLine(headPos, headPos + Vector3.up*0.3f, Color.red);
        //Debug.Log(somethingAbove);

        canCrouch = wishCrouching || somethingAbove;

        Vector3 targetScale = canCrouch ?
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

    ///<summary> Check Current Core states (grounded || inAir) </summary>
    public void Core_StateCheck()
    {
        if(isGrounded) //while grounded
        {
            //currentState = State.Grounded;
            
            if(currentCoreState != coreStates[0])
            currentCoreState.ExitState(this, ref currentCoreState, coreStates[0]);

            //count steps                
            stepInAir = 0;
            stepOnGround += 1;
        }
        else // while in air
        {
            //currentState = State.InAir;

            if(currentCoreState != coreStates[1])
            currentCoreState.ExitState(this, ref currentCoreState, coreStates[1]);
            
            //count steps
            stepOnGround = 0;
            stepInAir += 1;
        }
    }
    public void ResetYVel()
    {
        //playerVelocity.y = 0;
        Debug.Log("reee");
    }

    
    ///<summary> Check Current grounded sub states (walk || crouch || sprint || slide) </summary>///
    internal void Ground_SubStateCheck()//ref float o_CrouchMult, ref float o_SprintMult)
    {
        if(!canCrouch && !wishSprinting && currentGroundSubState is not GroundSubState_Walk) //walk
        {
            currentGroundSubState.ExitState(this, ref currentGroundSubState, groundSubStates[0]);
        } 

        if(canCrouch && currentGroundSubState is not GroundSubState_Crouch) //crouch
        { 
            currentGroundSubState.ExitState(this, ref currentGroundSubState, groundSubStates[1]);
        }

        if(wishSprinting && currentGroundSubState is not GroundSubState_Sprint) //sprint
        {
            currentGroundSubState.ExitState(this, ref currentGroundSubState, groundSubStates[2]);
        }
    } 
    ///<summary> Check Current Action sub states () </summary>///
    internal void Action_SubstateCheck()
    {
        if(hook.WishGrapHook && currentActionSubState is not ActionSubState_HookEnemy or ActionSubState_GrappleSurface)
        {
            hook.StartGrappleHook(); //the action check for grappling hook is in grappling hook
            Debug.Log("Called ADD");
        }
        //if(gm.WishReload)

        //if(gm.WishSwitchGun)
    }

#endregion

#region friction/equal opposite force
    internal void ApplyFriction(float _friction)
    {
        //Vector2 vec = new Vector2(playerVelocity.x, playerVelocity.z);
        //float speed = vec.magnitude;
        float speed  = playerVelocity.magnitude;

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

            // if((controller.collisionFlags & CollisionFlags.CollidedBelow) != 0)
            // playerVelocity.y -= hit.normal.y * Vector3.Dot(hit.moveDirection, hit.normal);

            //when grounded apply the raycast normal direction, works on flat and slope surface
            // if(isGrounded)
            // {
            //     Vector3 oppositeForce = onSlope.normal * Vector3.Dot(playerVelocity, onSlope.normal);
            //     playerVelocity -= oppositeForce; ///reset yvel
            //     playerVelocity.y = Mathf.Lerp(playerVelocity.y, -1f, 0.5f/Time.deltaTime); 

            //     //need to apply opposite normal force instead
            //     //Vector3 slopeNormal = -onSlope.normal;
            //     //magnetism = slopeNormal;
            //     //playerVelocity += magnetism.normalized  * Time.deltaTime;
            //     playerVelocity += -onSlope.normal.normalized  * Time.deltaTime;
                
            //     ////////////////////////////////////////////////
            //     // Vector3 slopeNormal = -onSlope.normal.normalized;
            //     // magnetism = Vector3.MoveTowards(playerVelocity, slopeNormal, 0.1f);
                    
            //     // playerVelocity += magnetism.normalized  * Time.deltaTime;

            //     // if(Physics.Raycast(transform.position, -onSlope.normal.normalized , out RaycastHit hitGround, 0.25f, groundMask))
            //     // {
            //     //     //Vector3 targetPos = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            //     // }
            //}
        }
    }
#endregion

#region slope physics
    //handles ground movement when player is on slopes, prevents skipping down slopes 
    void IfOnSlope() 
    {
        isOnSlope  = Physics.Raycast(transform.position, Vector3.down, out onSlope, 0.5f, groundMask) ? true : false;
    }
    internal void SnapOnGround()
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
    // public IEnumerator DelayedSwitchState(State _currentState, float delay)
    // {
    //     yield return new WaitForSecondsRealtime(delay);
    //     currentState = _currentState;
    //     yield return null;
    // }
    public IEnumerator DelayExitState<T>(Dictionary<int, AbstractState<T>> _dicToUse, int _targetState ,float delay)
    {
        string stateName = _dicToUse[_targetState].ToString();
        string stateCategory = stateName.Remove(stateName.IndexOf("_") + 1); //remove everything after the first "_"

        yield return new WaitForSeconds(delay);

        switch (stateCategory)
        {
            case string x when x == "CoreState_":
            currentCoreState.ExitState(this, ref currentCoreState, coreStates[_targetState]);
            break;

            case string x when x == "GroundSubState_":
            currentGroundSubState.ExitState(this, ref currentGroundSubState, groundSubStates[_targetState]);
            break;

            case string x when x == "ActionSubState_":
            currentActionSubState.ExitState(this, ref currentActionSubState, actionSubStates[_targetState]);
            break;
        }
    }
}
}


//-----------------------------------------------------------------------------------------------------------------------------------------------------------
//                              ||
//                 States       ||
//                              ||
//-----------------------------------------------------------------------------------------------------------------------------------------------------------
#region Player Core States (best defined as states that drastically changes how player's physics is calculated)
public class CoreState_Grounded : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        _manager.Core_StateCheck();
        _manager.ResetYVel();
    }
    public override void UpdateState(PlayerMovement _manager)
    {
        _manager.Jump_Input();
        _manager.Crouch_Input();
        _manager.Sprint_Input();
        _manager.hook.GrappleHook_Input();

        //_manager.Ground_SubStateCheck();
    }
    public override void DuringState(PlayerMovement _manager)
    {
        _manager.Core_StateCheck();
        _manager.Check_CrouchingStanding();

       // _manager.Ground_PhysicsSubStateMachine();
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _CurrentState, AbstractState<PlayerMovement> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class CoreState_InAir : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        
    }
    public override void UpdateState(PlayerMovement _manager)
    {
        _manager.Direction_Input(_manager.airSpeed); //air wish spd
        _manager.Jump_Input();
        _manager.Crouch_Input();
        _manager.hook.GrappleHook_Input();

         if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        {
            _manager.AirPhysics();
        }
    }
    public override void DuringState(PlayerMovement _manager)
    {
        if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        {
           // _manager.AirPhysics(); //move it to sub state machine when air sub states are implemented
            _manager.Core_StateCheck();
            _manager.SnapOnGround();
        }
            
        _manager.CapBhopSpeed();

       
        _manager.Check_CrouchingStanding();
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        throw new NotImplementedException();
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _CurrentState, AbstractState<PlayerMovement> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
#endregion

#region Grounded Sub States
public class GroundSubState_Walk : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        Debug.Log("GroundSubState: " + this.ToString());
    }
    public override void UpdateState(PlayerMovement  _manager)
    {
        _manager.Direction_Input(_manager.groundSpeed);
        _manager.GroundPhysics(1);
    }
    public override void DuringState(PlayerMovement _manager)
    {
        //if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        if(_manager.stepOnGround > _manager.LateFrictionDelay)
        _manager.ApplyFriction(_manager.friction);
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _currentState, AbstractState<PlayerMovement> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log("GroundSubStateExit" + this.ToString());
    }
}
public class GroundSubState_Crouch : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        Debug.Log("GroundSubStateState: " + this.ToString());
    }
    public override void UpdateState(PlayerMovement  _manager)
    {
        _manager.Direction_Input(_manager.groundSpeed);
        _manager.GroundPhysics(_manager.CrouchSpeedMult);
    }
    public override void DuringState(PlayerMovement _manager)
    {
        //if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        if(_manager.stepOnGround > _manager.LateFrictionDelay)
        _manager.ApplyFriction(_manager.friction);
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _currentState, AbstractState<PlayerMovement> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log("GroundSubStateExit" + this.ToString());
    }
}
public class GroundSubState_Sprint : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        Debug.Log("GroundSubState: " + this.ToString());
    }
    public override void UpdateState(PlayerMovement  _manager)
    {
        _manager.Direction_Input(_manager.groundSpeed);
        _manager.GroundPhysics(_manager.SprintSpeedMult);
    }
    public override void DuringState(PlayerMovement _manager)
    {
        //if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        if(_manager.stepOnGround > _manager.LateFrictionDelay)
        _manager.ApplyFriction(_manager.friction);
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _currentState, AbstractState<PlayerMovement> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log("GroundSubStateExit" + this.ToString());
    }
}
public class GroundSubState_Slide : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        Debug.Log($"Enter State: {this.ToString()}");
    }
    public override void UpdateState(PlayerMovement  _manager)
    {
        
    }
    public override void DuringState(PlayerMovement _manager)
    {
        
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        if(_manager.stepOnGround > _manager.LateFrictionDelay)
        _manager.ApplyFriction(_manager.slideFriction);
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _currentState, AbstractState<PlayerMovement> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log($"Exit State: {this.ToString()}");
    }
}
#endregion

#region Action Sub States (best defined as state that will limit character's arm action, character only has 2 arms)
public class ActionSubState_Idle : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        
    }
    public override void UpdateState(PlayerMovement _manager)
    {
        
    }
    public override void DuringState(PlayerMovement _manager)
    {
        
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _CurrentState, AbstractState<PlayerMovement> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_HookEnemy : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        _manager.hook.StartHookEnemy();
    }
    public override void UpdateState(PlayerMovement _manager)
    {
        _manager.Direction_Input(_manager.groundSpeed);
        _manager.Jump_Input();
        //_manager.hook.CancelHook_Input(_manager.InputSystemManager.grappleHook);
    }
    public override void DuringState(PlayerMovement _manager)
    {
        _manager.hook.CheckDistanceThreshold();
        _manager.hook.CheckObstaclesBetween();

        _manager.hook.ExecuteHookEnemy();
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _CurrentState, AbstractState<PlayerMovement> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_GrappleSurface : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        _manager.hook.StartGrappleSurface();
    }
    public override void UpdateState(PlayerMovement _manager)
    {
        _manager.Direction_Input(_manager.airSpeed);
        _manager.Jump_Input();
        //_manager.hook.CancelHook_Input(_manager.InputSystemManager.grappleHook);
    }
    public override void DuringState(PlayerMovement _manager)
    {
        _manager.hook.CheckDistanceThreshold();
        _manager.hook.CheckRopeStretch();
        _manager.hook.CheckPlayerFov();
        _manager.hook.CheckIfPlayerLanded();

        _manager.hook.ExecuteGrappleSurface();
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        throw new NotImplementedException();
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _CurrentState, AbstractState<PlayerMovement> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_Reloading : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        
    }
    public override void UpdateState(PlayerMovement _manager)
    {
        
    }
    public override void DuringState(PlayerMovement _manager)
    {
        
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
    
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _CurrentState, AbstractState<PlayerMovement> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_WeaponSwitching : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        
    }
    public override void UpdateState(PlayerMovement _manager)
    {
        
    }
    public override void DuringState(PlayerMovement _manager)
    {
        
    }
    public override void LateDuringState(PlayerMovement _manager)
    {
        
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _CurrentState, AbstractState<PlayerMovement> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}

#endregion