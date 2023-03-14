using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player.Movement;

public class TestPlayerStateManager : MonoBehaviour
{
    // // Start is called before the first frame update
    // public AbstractState currentState;
    // private PlayerMovement pm;
    // //states
    // public PlayerState_Walk walk;
    // public PlayerState_Crouch crouch;
    // void Awake()
    // {
    //     //get player movement
    //     pm = GetComponent<PlayerMovement>();

    //     //create instances of states
    //     walk = new PlayerState_Walk(pm);
    //     crouch = new PlayerState_Crouch(pm);


    //     //Set Initial State
    //     currentState = walk;
    //     currentState.EnterState(this);
    // }

    // // Update is called once per frame
    // void Update()
    // {
    
    // }
    // void FixedUpdate()
    // {
    //     currentState.DuringState(this);
    // }
    // public void SwitchState(AbstractState _stateToSwitchTo)
    // {
    //     currentState = _stateToSwitchTo;
    //     _stateToSwitchTo.EnterState(this);
    // }
}
