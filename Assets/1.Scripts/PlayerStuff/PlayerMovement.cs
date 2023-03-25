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
//--------------------------------------------------------------------------------------------------------
    [Header("Ground Move")]
    [SerializeField]public float groundSpeed; // Moving on ground;
    [SerializeField]public float groundAccelerate;
    [SerializeField]public float friction;
    [SerializeField]public float slideFriction;
    ///<summary> how much physic steps later to start friction </summary>
    [Tooltip("how much physic steps later to start friction")][SerializeField]private float lateFrictionDelay;
    public float LateFrictionDelay => lateFrictionDelay;
    [SerializeField]public float wishSpeed;
    private float TotalGroundSpeed;

    [Header("Air Move")]
    [SerializeField]public float airSpeed;
    [SerializeField]public float airAccelerate;
    [SerializeField]public float capBhopSpeed;
    //[SerializeField]public float airWishSpeed;
    //public float AirWishSpeed {get{return airWishSpeed;} set{airWishSpeed = value;}}

//------------------------------------------------------------------------------------------------------
    [Header("Crouch")]
    [SerializeField] private float crouchSpeedMult;
    public float CrouchSpeedMult => crouchSpeedMult; // {get{return crouchSpeedMult;} private set{crouchSpeedMult = value;}}
    [SerializeField] private float crouchInOutSpeed;
    [SerializeField] private float crouchDistance;
    [SerializeField] private bool wishCrouching;
    [SerializeField] private bool canCrouch; public bool CanCrouch => canCrouch;
    private float standYScale;

    [Header("Sprint")]
    [SerializeField] private float sprintSpeedMult;
    public float SprintSpeedMult => sprintSpeedMult;
    [SerializeField] private bool wishSprinting; public bool WishSprinting => wishSprinting;
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
    private Vector3 wishdir;
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
        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
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
        wishCrouching = inputSystemManager.crouch.IsPressed() && isGrounded ; //Input.GetKey(KeyCode.LeftControl)
    }
    public void Sprint_Input()
    {
        wishSprinting = inputSystemManager.sprint.IsPressed() && !wishCrouching && isGrounded; //Input.GetKey(KeyCode.LeftShift)
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
    public void GroundPhysics(float _mult)//_mult = sprint or crouch multiplier
    {
            wishSpeed *= _mult;
            float addSpeed = wishSpeed - currentSpeed; //addspeed is the capped speed
            float accelSpeed = wishSpeed * friction * Time.deltaTime * groundAccelerate; // without deltatime, player accelerate almost instantly
            
             if (accelSpeed > addSpeed) //cap speed
            {
                accelSpeed = addSpeed;
            }

            //Modified here so player can walk up and down slopes without speed penalty or tumbling
            Physics.Raycast(transform.position, Vector3.down, out onSlope, 0.6f, groundMask);
            var slopeMoveDir = Vector3.ProjectOnPlane(wishdir, onSlope.normal);
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
#endregion

#region Ground air check + cap bhop speed + crouch
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
        //check if anything is blocking player to stand up
        Vector3 bodyPos = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        bool somethingAbove = Physics.Raycast(bodyPos, Vector3.up, controller.height - 0.11f, groundMask);
        Debug.DrawLine(bodyPos, bodyPos + Vector3.up*(controller.height - 0.11f), Color.red);
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
            Debug.Log("player underground");
        }
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
            Debug.Log("snaping to ground now"+ dot);
        }
    }
#endregion
}
}