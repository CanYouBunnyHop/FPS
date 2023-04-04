using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using FPS.Settings;
using FPS.Weapon;
//-------------------------------------------------------------------------------------------------------
// Remember To change the input manager under project settings for adding counter-strafing
//
// Horizontal && Vertical axis:
//      gravity = 8     (this makes movement less slippery, but still slides)
//      sensitivity = 1 
//      Snap = false    (this is so the input gets a gradual change, important if you want counter strafe
//
// ToDo:
//      Smooth transition for WASD for new input manager
//-------------------------------------------------------------------------------------------------------
namespace FPS.Player
{
public class PlayerMovement : MonoBehaviour
{
#region references
    [Header("References")]
    public GrapplingHook hook; //Additional movement abilities
    public LayerMask groundMask;
    [SerializeField] private PlayerInputSystemManager inputSystemManager; //public PlayerInputSystemManager InputSystemManager => inputSystemManager;
    [SerializeField] private GunManager gm;
    private CharacterController controller;
#endregion

//--------------------------------------------------------------------------------------------------------
#region Movement Parameters
    [Header("Settings")]
    public float gravity;
    [SerializeField] private float maxSlopeAngleToWalk;
    [SerializeField] private float feetRayLength;

    [Header("Jumping")]
    public float jumpForce;
    private RaycastHit onSlope;
//--------------------------------------------------------------------------------------------------------
    [Header("Ground Move")]
    [SerializeField]public float groundSpeed; // Moving on ground;
    [SerializeField]public float groundAccelerate;
    [SerializeField]public float friction;
    [SerializeField]private float curGroundSpeedMult = 1;
    ///<summary> This changes in player ground sub state's enter state, curGroundSpeedMult is lerp to target </summary>
    [HideInInspector]public float targetGroundSpeedMult = 1;

    ///<summary> how much physic steps later to start friction </summary>
    [Tooltip("how much physic steps later to start friction")][SerializeField]private float lateFrictionDelay;
    public float LateFrictionDelay => lateFrictionDelay;
    [SerializeField]public float wishSpeed;
    [SerializeField]private Vector3 wishdir;
    private Vector3 slopeMoveDir;

    [Header("Air Move")]
    [SerializeField]public float airSpeed;
    [SerializeField]public float airAccelerate;
    [SerializeField]public float capBhopSpeed;
    [SerializeField]public float airWishSpeed;
    //public float AirWishSpeed {get{return airWishSpeed;} set{airWishSpeed = value;}}

//------------------------------------------------------------------------------------------------------
    [Header("Crouch")]
    [SerializeField] private float crouchSpeedMult; public float CrouchSpeedMult => crouchSpeedMult; // {get{return crouchSpeedMult;} private set{crouchSpeedMult = value;}}
    [SerializeField] private float crouchInOutSpeed;
    [SerializeField] private float crouchDistance;
    [SerializeField] private bool wishCrouching;
    [SerializeField] private bool canCrouch; public bool CanCrouch => canCrouch;
    private float standYScale;
    [Header("Slide")]
    [SerializeField] private float minSpeedToSlide;
    [SerializeField] private float minSpeedToStopSlide;
    [SerializeField] private float slideSpeedBoost;
    [SerializeField] public float slideFriction;
    [SerializeField] private Vector3 slideDir;
    [SerializeField] private bool canSlide; public bool CanSlide => canSlide;

    [Header("Sprint")]
    [SerializeField] private float sprintAngleThreshold;
    [SerializeField] private float sprintSpeedMult; public float SprintSpeedMult => sprintSpeedMult;
    [SerializeField] private bool wishSprinting; //public bool WishSprinting => wishSprinting;
    [SerializeField] private bool canSprint; public bool CanSprint => canSprint;
#endregion
//------------------------------------------------------------------------------------------------------
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
    [SerializeField]public int stepInAir;// {get; private set;}
    [SerializeField]public int stepOnGround;// {get; private set;}
    [SerializeField]public int stepSinceJumped;// {get; private set;} //assign this to anything that will knock player up
    [SerializeField]public int stepSinceKockback;
    [SerializeField]public float currentSpeed {get; private set;} //not actual speed, dot product of playervel and wish vel
    [SerializeField]private bool wishJump = false;
    
    public Text text;//UI remove later

    [Header("Debug Gizmo")]
    [SerializeField] private float snapGroundRayLength;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private float groundCheckRadius = 0.3f;

//--------------------------------------------------------------------------------------------------------
    public Vector3 playerZXVel => new Vector3(playerVelocity.x, 0, playerVelocity.z); 
    //[SerializeField]private bool isOnSlope;// => Physics.Raycast(transform.position, Vector3.down, out onSlope, 0.6f, groundMask) ? true : false;
    [SerializeField] Coroutine jumpBufferTimer; 
#endregion
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        hook = GetComponent<GrapplingHook>();

        //currentState = State.InAir;
        //curGroundSubState = GroundSubState.Walk;

        standYScale = transform.localScale.y; //get player's y scale which is the standing's height
    }
#region Inputs
    public void Direction_Input(float _GroundOrAirSpeed)
    {
        //get WASD input into wish direction
        var axis = inputSystemManager.wasd.ReadValue<Vector2>();
        wishdir =  new Vector3(axis.x, 0, axis.y);
        //wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();

        wishSpeed =  wishdir.magnitude;
        wishSpeed *= _GroundOrAirSpeed;
    }
    
    public void Jump_Input()
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

            wishJump = false;

            jumpBufferTimer = null;
        }
    }
    
    public void Crouch_Input()
    {   
        wishCrouching = inputSystemManager.crouch.IsPressed(); //Input.GetKey(KeyCode.LeftControl)
    }
    public void Sprint_Input()
    {
        wishSprinting = inputSystemManager.sprint.IsPressed(); //Input.GetKey(KeyCode.LeftShift)
    }
    
#endregion


#region Updateloops Statemachine / gizmo
    void Update()
    {
        controller.Move(playerVelocity * Time.deltaTime); //since controller don't use unity's physics update, we can getaway with update

        //UI remove later
        string dec = Convert.ToString(wishSpeed);
        text.text = "currentspeed: "+ playerZXVel.magnitude;
    }
    void FixedUpdate()
    {
        currentSpeed = Vector3.Dot(wishdir, playerVelocity);

        //Check grounded
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit ghit , 1);
        float slopeAngle = Vector3.Angle(ghit.normal, Vector3.up);
        isGrounded = Physics.CheckSphere(transform.position + groundCheckOffset , groundCheckRadius , groundMask) && slopeAngle < maxSlopeAngleToWalk;
        Debug.Log(slopeAngle);
        
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

        curGroundSpeedMult = Mathf.Lerp(curGroundSpeedMult, targetGroundSpeedMult, 4 * Time.deltaTime);
        curGroundSpeedMult = Mathf.Abs(curGroundSpeedMult - targetGroundSpeedMult) < 0.01f ? targetGroundSpeedMult : curGroundSpeedMult;

        //Vector3 yRot = new Vector3(0,transform.rotation.y,0);
        
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
    public void GroundPhysics()//_mult = sprint or crouch multiplier
    {       
            wishSpeed *= curGroundSpeedMult;
            float addSpeed = wishSpeed - currentSpeed; //addspeed is the capped speed
            float accelSpeed = wishSpeed * friction * Time.deltaTime * groundAccelerate; // without deltatime, player accelerate almost instantly
            
            if (accelSpeed > addSpeed) //cap speed
            {
                accelSpeed = addSpeed;
            }

            //Modified here so player can walk up and down slopes without speed penalty or tumbling
            Physics.Raycast(transform.position, Vector3.down, out onSlope, 0.6f, groundMask);
            slopeMoveDir = Vector3.ProjectOnPlane(wishdir, onSlope.normal);
            playerVelocity += accelSpeed * slopeMoveDir;
    }
    public void AirPhysics()
    {
            float addSpeed = wishSpeed - currentSpeed;
            addSpeed = Mathf.Clamp(addSpeed, 0, Mathf.Infinity); // make sure you don't and slow down when input same direction midair
            float accelSpeed = wishSpeed * Time.deltaTime * airAccelerate; // without deltatime, player accelerate almost instantly
            
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
    //custom made
    public void SlidingPhysics()
    {
        //get slide dir
        //make sure not to slide up hill
        //get slopeMoveDir but not with wishSpeed involved

        //Method 1, only works if player starts with wishdir    //bug when shift key and crouch key is held down
        //ground physics disabled
        // wishdir = Vector3.Lerp(wishdir, Vector3.zero, slideFriction * Time.deltaTime); //behavior works good on flat ground
        // var slopeDir =  Vector3.ProjectOnPlane(playerVelocity.normalized, onSlope.normal);
        // slideDir = (playerZXVel.magnitude >= minSpeedToStopSlide)? slopeDir : Vector3.zero;

        // playerVelocity += slideDir.normalized * Time.deltaTime;

        //Method 2
        //ground physics still enabled (maybe)
        slideDir = Vector3.Lerp(slideDir, Vector3.zero, slideFriction * Time.deltaTime);
        playerVelocity = Vector3.Lerp(playerVelocity, Vector3.zero, Time.deltaTime);

        if(playerZXVel.magnitude > minSpeedToStopSlide)
        {
            playerVelocity +=  slideSpeedBoost * slideDir * Time.deltaTime;
            Debug.Log("////////////////////// slide physics");
        }
        

       


        
        
    }
    public void StartSlide()
    {
        slideDir = playerVelocity.normalized;
    }
#endregion

#region cap bhop speed + ground substate checks
    public void CapBhopSpeed()
    {
        //capping bhop speed 
        playerVelocity.z = Mathf.Clamp(playerVelocity.z, -capBhopSpeed, capBhopSpeed);
        playerVelocity.x = Mathf.Clamp(playerVelocity.x, -capBhopSpeed, capBhopSpeed);
    }
    
    ///<summary> The physics update for switching between Crouching and standing, 
    ///also pushes player upwards if player is stuck in ground. </summary>
    public void Check_CrouchingStanding()
    {
        canCrouch = (wishCrouching || Check_SomethingAbove()); // <================ 

        Vector3 targetScale = canCrouch ?
            targetScale = new Vector3(1, standYScale - crouchDistance, 1) :      //crouched
            targetScale = new Vector3(1, standYScale, 1);                       //stand

        if(transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, crouchInOutSpeed * Time.deltaTime);
        }

        ////pushes player upwards if hit below, for stepping up stairs, floating collider
        RaycastHit[] hits = Physics.RaycastAll(transform.position + new Vector3(0,1,0), Vector3.down , feetRayLength, groundMask);
        if(hits.Length > 0 && isGrounded)
        {   
            Mathf.SmoothDamp(playerVelocity.y, 50, ref playerVelocity.y, 1f, 10);
            Debug.Log("player underground");
        }
    }
    public void Check_Sprinting()
    {
        canSprint = Vector3.Angle(wishdir, transform.forward) <= sprintAngleThreshold && wishSprinting && !wishCrouching && isGrounded;
    }

    public void Check_Sliding()
    {
        if(canCrouch && playerZXVel.magnitude >= minSpeedToSlide && canSlide == false) canSlide = true;
        //canSlide = canCrouch && playerZXVel.magnitude >= minSpeedToSlide; //start slide
        else if(canCrouch && playerZXVel.magnitude < minSpeedToStopSlide && canSlide == true || !canCrouch ) canSlide = false;
    }
    
    private bool Check_SomethingAbove()
    {
        //check if anything is blocking player to stand up
        Vector3 bodyPos = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        return Physics.Raycast(bodyPos, Vector3.up, controller.height - 0.11f, groundMask);
    }
#endregion

#region friction/equal opposite force
    public void ApplyFriction(float _friction)
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


            //pushes player upwards if hit below, for stepping up stairs
            if((isGrounded && (controller.collisionFlags & CollisionFlags.Below) != 0))
            {
                Mathf.SmoothDamp(playerVelocity.y, playerVelocity.y + 25, ref playerVelocity.y, 1f, 10);
            }
        }
    }
#endregion

#region slope physics
    //handles ground movement when player is on slopes, prevents skipping down slopes 
    public void SnapOnGround()
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
            //Debug.Log("snaping to ground now"+ dot);
        }
    }
#endregion
}
}