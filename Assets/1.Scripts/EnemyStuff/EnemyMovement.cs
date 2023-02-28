using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Movement{
public class EnemyMovement : MonoBehaviour
{
    public Transform groundCheck;
    public LayerMask groundMask;
    public float friction;
    public float gravity;
    public bool agentUpdatePos;//stun bool
    private CharacterController controller;
    public NavMeshAgent agent;
    public Vector3 enemyVelocity;
    public State currentState;
    private bool isGrounded;
    private bool readyNavmesh;
    [SerializeField]
    private Transform player;
    public bool canResetVel = true; // to rest vel once
     public enum State
    {
        Grounded,
        InAir,
        Hooked,
    }
    //get agent
   /* public NavMeshAgent Agent
    {
        get{return agent;}
    }*/
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.detectCollisions = false;
       // agent = GetComponent<NavMeshAgent>();
       // Invoke(nameof(DelayAwake), 0.1f);
    }

    
    void FixedUpdate()
    {
        //so i can check in inspector for updatePosition
        agent.enabled = agentUpdatePos;
        controller.enabled = !agentUpdatePos;

        switch(currentState)
        {
            case State.Grounded:
            {
                CheckGroundedOrInAir();
                GroundBehavior();
                ApplyFriction();

                if(enemyVelocity.magnitude < 0.1f)
                {
                    agentUpdatePos = true;
                    ResetVel(); // only called once via bool
                    if(agent.enabled)
                    agent.destination = player.position;
                }
                else
                {
                    agentUpdatePos = false;
                }
            }
            break;
            case State.InAir:
            {
                CheckGroundedOrInAir();
                AirBehavior();
                //while in air
                agentUpdatePos = false;
                canResetVel = true;
            }
            break;
            case State.Hooked:
            {
                //while hooked
                agentUpdatePos = false;
                enemyVelocity = Vector3.zero;
                canResetVel = true;
            }
            break;
        }
        //Check grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask);
        
        if(controller.enabled)
        controller.Move(enemyVelocity * Time.deltaTime);
       
    }

    void DelayAwake()
    {
       // agent.enabled = true;
    }
    void GroundBehavior()
    {
       // enemyVelocity = Vector3.zero;
    }
    void AirBehavior()
    {
        //gravity
        enemyVelocity.y += gravity * Time.deltaTime;
        
    }
    void HookedBehavior()
    {

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
    public void DelayCheckGorA(float delay)
    {
        Invoke(nameof(CheckGroundedOrInAir), delay);
    }
     void ApplyFriction()
    {
        Vector2 vec = new Vector2( enemyVelocity.x,  enemyVelocity.z);
        float speed = vec.magnitude;

        if (speed != 0) // To avoid divide by zero errors
        {
            float drop = speed * friction * Time.deltaTime;
            enemyVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //when using int, it can check collsions, when using layer it doesnt check at all
       if(hit.gameObject.layer == 0)
        {
        //have to use hit.move direction for y rather than playervelocity, otherwise the check is buggy
         //  enemyVelocity.z -= hit.normal.z * Vector3.Dot(enemyVelocity, hit.normal);
         //  enemyVelocity.x -= hit.normal.x * Vector3.Dot(enemyVelocity, hit.normal);
         //  enemyVelocity.y -= hit.normal.y * Vector3.Dot(hit.moveDirection, hit.normal);
           enemyVelocity -= hit.normal * Vector3.Dot(enemyVelocity, hit.normal);
           //Debug.Log(hit.moveLength);
        }
    }
    //only called once
    void ResetVel()
    {
        if(canResetVel)
        {
            currentState = State.Grounded;
            enemyVelocity = Vector3.zero;
            canResetVel = false;
        }
    }
}}
