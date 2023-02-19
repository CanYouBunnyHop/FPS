using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
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
    [Header("References")]
    public float gravity;
    private CharacterController controller;
    public GrapplingHook hook; //Additional movement abilities
    public Transform headCheck;
    public Transform groundCheck;
    public LayerMask groundMask;

//--------------------------------------------------------------------------------------------------------
    [Header("Jumping")]
    public float jumpForce;
    private RaycastHit onSlope;
    private Vector3 slopeMoveDir;
    bool called; //bool for wish jump
//--------------------------------------------------------------------------------------------------------
    [Header("Ground Move")]
    public float groundSpeed; // Moving on ground;
    public float groundAccelerate;
    public float friction;
    private float wishSpeed;

    [Header("Air Move")]
    public float airSpeed;
    public float airAccelerate;
    public float capBhopSpeed;
    private float airWishSpeed;
//------------------------------------------------------------------------------------------------------
    public enum State
    {
        Grounded,
        InAir,
        GrapSurface,
        HookEnemy,
    }
    [Header("Snap On Ground")]
    public float minGroundDotProduct;
    public float snapMaxVel; //abort snap if playervel is greater
//------------------------------------------------------------------------------------------------------
    [Header("Debug")]
    public State currentState;
    public Vector3 playerVelocity;
    public Vector3 playerZXVel;
    [SerializeField]private int stepSinceGrounded;
    [SerializeField]public int stepSinceJumped; //assign this to anything that will knock player up
    [SerializeField]public int stepSinceKockback;
    [SerializeField]private float currentSpeed; //not actual speed, dot product of playervel and wish vel
    private Vector3 wishdir;
    [SerializeField]private bool wishJump = false;
    public bool isGrounded;
    [SerializeField] private bool isOnSlope;
    public Text text;//UI remove later
//--------------------------------------------------------------------------------------------------------
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        hook = GetComponent<GrapplingHook>();
        currentState = State.InAir;
        StartCoroutine(LateFixedUpdate());
       
        //controller.stepOffset
    }
#region Inputs
    void DirectionInputs()
    {
        //get WASD input into wish direction
        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);
		wishdir.Normalize();
        
        wishSpeed = wishdir.magnitude;
        wishSpeed*= groundSpeed;

        airWishSpeed = wishdir.magnitude;
        airWishSpeed*= airSpeed;
    }
    
    void JumpInput()
    {
         //Inputs for jump
        if(Input.GetKeyDown(KeyCode.Space)||Input.GetAxis("Mouse ScrollWheel") < 0f )
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
        if(wishJump && called == false)
        {
            called = true;
            Invoke(nameof(WJF), 0.1f); //sets Wish jump false with delay
        }
    }
#endregion

#region Updateloops Statemachine
    void Update()
    {
        controller.Move(playerVelocity * Time.deltaTime); //since controller don't use unity's physics update, we can getaway with update

        //Check slope
        CheckOnSlope();

        //state INPUTS handling
        switch(currentState)
        {
            case State.Grounded:
            {
                DirectionInputs();
                JumpInput();
                //previously in FixedUpdate
                

            }
            break;
            case State.InAir:
            {
                DirectionInputs();
                JumpInput();
                //previously in FixedUpdate
                
            }
            break;
            case State.GrapSurface:
            {
                DirectionInputs();
                JumpInput();
                hook.CancelHookInput();
                //previously in FixedUpdate
                
            }
            break;
            case State.HookEnemy:
            {
                DirectionInputs();
                hook.CancelHookInput();
                //previously in FixedUpdate
                
            }
            break;
        }
        
        //UI remove later
        string dec = Convert.ToString(wishSpeed);
        text.text = "currentspeed: "+ playerZXVel.magnitude;
        //
    }
    void FixedUpdate()
    {
        //Check grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.3f, groundMask);

        //Always keep moving
        //roundup small numbers avoid small movements
        if(Math.Abs(playerVelocity.x)< 0.01)
        playerVelocity.x = 0;

        if(Math.Abs(playerVelocity.z)< 0.01)
        playerVelocity.z = 0;
        

        currentSpeed = Vector3.Dot(wishdir, playerVelocity);
        playerZXVel = new Vector3(playerVelocity.x, 0, playerVelocity.z);

        //test debug 
        //Debug.Log(currentSpeed);

        //count steps //stay in fixedUpdate
        stepSinceJumped += 1;
        stepSinceKockback += 1;

        //state Physics handling
        switch(currentState)
        {
            case State.Grounded:
            {
                GroundPhysics(currentSpeed); //unlike controller.Move(), physics have to be in fixed update for some reason, buggy in Update()
                CheckGroundedOrInAir();      //Maybe because most of my physics calculation is already in fixedUpdate                       
                //count steps                // Maybe if everything is in Update() movement will be smoother, (not a big difference)
                stepSinceGrounded = 0;
            }
            break;
            case State.InAir:
            {
                AirPhysics(currentSpeed);
                CheckGroundedOrInAir();
                CapBhopSpeed();
                
                //count steps
                stepSinceGrounded += 1;
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
                    GroundPhysics(currentSpeed);
                }
                else
                {
                    AirPhysics(currentSpeed);
                    CapBhopSpeed();
                }
            }
            break;
        }

        //jumping buffer, runs outside of state handler currently, 
        //reset y vel avoid jump boost on after running on slope
        
        if (wishJump && isGrounded)
        {
            playerVelocity.y = 0;
        }
       
    }   
    IEnumerator LateFixedUpdate()
    {
        while(true)
        {
            yield return new WaitForFixedUpdate(); //apply this after 1 physics timestep late

            switch(currentState)
            {
                case State.Grounded:
                {
                    ApplyFriction();
                }
                break;
                case State.InAir:
                {
                    SnapOnGround();
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

            //jump here
            if (wishJump && isGrounded)
            {
                QueueJump();
                stepSinceJumped = 0;
            }
        }
    }
#endregion

#region Source Engine Code translated to Unity C# / valve physics calculation
    void GroundPhysics(float _currentSpeed)
    {
            float addSpeed = wishSpeed - _currentSpeed;
            float accelSpeed = wishSpeed * friction * Time.deltaTime * groundAccelerate; // without deltatime, player accelerate almost instantly
            
             if (accelSpeed > addSpeed) //cap speed
            {
              accelSpeed = addSpeed;
            }
            
            //WASD movement
            if(!isOnSlope)
            {
                //if not on sloper use wishDir
                playerVelocity.z += accelSpeed* wishdir.z;
                playerVelocity.x += accelSpeed* wishdir.x;
            }
            else
            {
                //when player is on slope
                slopeMoveDir = Vector3.ProjectOnPlane(wishdir, onSlope.normal);
                playerVelocity += accelSpeed* slopeMoveDir;
            }
            
            
            //Reset y vel when on ground, but with onSlope's normal 
            //So when applying opposite equal force, the angle of surface is calculated
            if(!wishJump && isGrounded)
            ResetYVel();

            //apply friction when on ground but not for the first frame
            //if(lateFriction)      
            //ApplyFriction();  //decaprecated, moved to latefixedupdate
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

#region Ground air check + cap bhop speed
    void CapBhopSpeed()
    {
        //capping bhop speed 
        playerVelocity.z = Mathf.Clamp(playerVelocity.z, -capBhopSpeed, capBhopSpeed);
        playerVelocity.x = Mathf.Clamp(playerVelocity.x, -capBhopSpeed, capBhopSpeed);
    }

    public void CheckGroundedOrInAir()
    {
         if(isGrounded) //while grounded
        {
            currentState = State.Grounded;
        }
        else // while in air
        {
            currentState = State.InAir;
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
        //when using int, it can check collsions, when using layer it doesnt check at all
       if(hit.gameObject.layer == 0)
        {
        //have to use hit.move direction for y rather than playervelocity, otherwise the check is buggy
           playerVelocity.z -= hit.normal.z * Vector3.Dot(playerVelocity, hit.normal);
           playerVelocity.x -= hit.normal.x * Vector3.Dot(playerVelocity, hit.normal);

           if(playerVelocity.y > 2)//stay on ceiling just a bit longer, otherwise slight touch with the ceiling will start going down fast
           playerVelocity.y -= hit.normal.y * Vector3.Dot(hit.moveDirection, hit.normal);

           Debug.Log(Vector3.Dot(hit.moveDirection, hit.normal));
        }
    }
    //when grounded apply the raycast normal direction, works on flat and slope surface
    public void ResetYVel()
    {
        playerVelocity -= onSlope.normal * Vector3.Dot(playerVelocity, onSlope.normal);
    }
#endregion

#region jump physics
    void QueueJump()
    {
        playerVelocity.y += jumpForce;
        //playerVelocity.y = Mathf.Lerp(playerVelocity.y, jumpForce, 0.2f);
    }
    //wish jump false for invoking
    public void WJF()
    {
        if(!isGrounded)
        wishJump = false;
        called = false;
    }
#endregion

#region slope physics
    //handles ground movement when player is on slopes, prevents skipping down slopes 
    void CheckOnSlope()
    {
        if(Physics.Raycast(groundCheck.position, Vector3.down, out onSlope, 0.6f, groundMask))
        {
            //when hitting raycasting into the ground, if the normal is not up, must be on slope
            if(onSlope.normal!= Vector3.up)
            {
                isOnSlope = true;
            }
            else
            {
                isOnSlope = false;
            }
        }
        else isOnSlope = false;
    }
    void SnapOnGround()
    {
        if (stepSinceGrounded > 1 || stepSinceJumped <= 2 || stepSinceKockback < 5)//abort when just jumped or havn't been in air long enough
		return ;

        if (!Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, 0.8f, groundMask))
		return;

        if (hit.normal.y < minGroundDotProduct)
		return;

        if(playerZXVel.magnitude > snapMaxVel)//abort if playervel is too fast
        return;

        float speed = playerVelocity.magnitude;
		float dot = Vector3.Dot(playerVelocity, hit.normal);
        Debug.Log("snaping to ground now"+dot);

        if (dot > 0f)
		playerVelocity = (playerVelocity - hit.normal * dot).normalized * speed;
		//return true;
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

