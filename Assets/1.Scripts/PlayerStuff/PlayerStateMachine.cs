using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player;
using FPS.Weapon;
using System;

public class PlayerStateMachine : MonoBehaviour
{
    [SerializeField] PlayerMovement pm;
    public PlayerMovement Pm => pm;
    [SerializeField] GrapplingHook hook;
    public GrapplingHook Hook => hook;
    [SerializeField] GunManager gm;
    public GunManager Gm => gm;

    public AbstractState<PlayerStateMachine> currentCoreState;
    ///<summary> 0, Grounded | 1, InAir </summary>///
    public Dictionary<int, AbstractState<PlayerStateMachine>> coreStates = new Dictionary<int, AbstractState<PlayerStateMachine>>()
    {
        {0, new CoreState_Grounded()},
        {1, new CoreState_InAir()},
    };
    public AbstractState<PlayerStateMachine> currentGroundSubState;
    ///<summary> 0, Walk | 1, Crouch | 2, Sprint | 3, Slide </summary>///
    public Dictionary<int, AbstractState<PlayerStateMachine>> groundSubStates = new Dictionary<int, AbstractState<PlayerStateMachine>>()
    {
        {0, new GroundSubState_Walk()}, 
        {1, new GroundSubState_Crouch()}, 
        {2, new GroundSubState_Sprint()},
        {3, new GroundSubState_Slide()},
    };
    public AbstractState<PlayerStateMachine> currentActionSubState;
    ///<summary> 0, Idle | 1, HookEnemy |2, GrappleSurface| 3, Reloading | 4, WeaponSwitching </summary>///
    public Dictionary<int, AbstractState<PlayerStateMachine>> actionSubStates = new Dictionary<int, AbstractState<PlayerStateMachine>>()
    {
        {0, new ActionSubState_Idle()},
        {1, new ActionSubState_HookEnemy()},
        {2, new ActionSubState_GrappleSurface()},
        {3, new ActionSubState_Reloading()},
        {4, new ActionSubState_WeaponSwitching()},
    };
    [Header("Debug")]
    public string curCoreState;
    public string curGroundSubState;
    public string curActionSubState;
    void Awake()
    {
        currentCoreState = coreStates[1];
        //currentCoreState.EnterState(this);

        currentGroundSubState = groundSubStates[0];
        //currentGroundSubState.EnterState(this);

        currentActionSubState = actionSubStates[0];
        //currentActionSubState.EnterState(this);

        StartCoroutine(LateFixedUpdate());
    }
    void Update()
    {
        //Debug
        curCoreState = currentCoreState.ToString();
        curGroundSubState = currentGroundSubState.ToString();
        curActionSubState = currentActionSubState.ToString();
        
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
    }
    void FixedUpdate()
    {
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
#region State Checks
    ///<summary> Check Current Core states (grounded || inAir) </summary>
    public void Core_StateCheck()
    {
        if(pm.isGrounded) //while grounded
        {
            if(currentCoreState != coreStates[0])
            currentCoreState.ExitState(this, ref currentCoreState, coreStates[0]);

            //count steps                
            pm.stepInAir = 0;
            pm.stepOnGround += 1;
        }
        else // while in air
        {
            if(currentCoreState != coreStates[1])
            currentCoreState.ExitState(this, ref currentCoreState, coreStates[1]);
            
            //count steps
            pm.stepOnGround = 0;
            pm.stepInAir += 1;
        }
    }
    ///<summary> Check Current grounded sub states (walk || crouch || sprint || slide) </summary>///
    public void Ground_SubStateCheck()//ref float o_CrouchMult, ref float o_SprintMult)
    {
        if(!pm.CanCrouch && !pm.WishSprinting && currentGroundSubState is not GroundSubState_Walk) //walk
        {
            currentGroundSubState.ExitState(this, ref currentGroundSubState, groundSubStates[0]);
        } 

        if(pm.CanCrouch && currentGroundSubState is not GroundSubState_Crouch) //crouch
        { 
            currentGroundSubState.ExitState(this, ref currentGroundSubState, groundSubStates[1]);
        }

        if(pm.WishSprinting && currentGroundSubState is not GroundSubState_Sprint) //sprint
        {
            currentGroundSubState.ExitState(this, ref currentGroundSubState, groundSubStates[2]);
        }
    } 
    ///<summary> Check Current Action sub states () </summary>///
    public void Action_SubstateCheck()
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

//-----------------------------------------------------------------------------------------------------------------------------------------------------------
//                              ||
//                 States       ||
//                              ||
//-----------------------------------------------------------------------------------------------------------------------------------------------------------
#region Player Core States (best defined as states that drastically changes how player's physics is calculated)
public class CoreState_Grounded : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        _manager.Core_StateCheck();
    }
    public override void UpdateState(PlayerStateMachine _manager)
    {
        _manager.Pm.Jump_Input();
        _manager.Pm.Crouch_Input();
        _manager.Pm.Sprint_Input();
        _manager.Hook.GrappleHook_Input();

        //_manager.Ground_SubStateCheck();
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        _manager.Core_StateCheck();
        _manager.Pm.Check_CrouchingStanding();

       // _manager.Ground_PhysicsSubStateMachine();
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _CurrentState, AbstractState<PlayerStateMachine> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class CoreState_InAir : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        
    }
    public override void UpdateState(PlayerStateMachine _manager)
    {
        _manager.Pm.Direction_Input(_manager.Pm.airSpeed); //air wish spd
        _manager.Pm.Jump_Input();
        _manager.Pm.Crouch_Input();
        _manager.Pm.hook.GrappleHook_Input();

        // if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        // {
        //    // _manager.AirPhysics();
        // }
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        {
            _manager.Pm.AirPhysics(); //move it to sub state machine when air sub states are implemented
            _manager.Core_StateCheck();
            _manager.Pm.SnapOnGround();
        }
            
        _manager.Pm.CapBhopSpeed();
        _manager.Pm.Check_CrouchingStanding();
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _CurrentState, AbstractState<PlayerStateMachine> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
#endregion

#region Grounded Sub States
public class GroundSubState_Walk : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        Debug.Log("GroundSubState: " + this.ToString());
    }
    public override void UpdateState(PlayerStateMachine  _manager)
    {
        _manager.Pm.Direction_Input(_manager.Pm.groundSpeed);
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        //if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        _manager.Pm.GroundPhysics(1);
        
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        if(_manager.Pm.stepOnGround > _manager.Pm.LateFrictionDelay)
        _manager.Pm.ApplyFriction(_manager.Pm.friction);
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _currentState, AbstractState<PlayerStateMachine> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log("GroundSubStateExit" + this.ToString());
    }
}
public class GroundSubState_Crouch : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        Debug.Log("GroundSubStateState: " + this.ToString());
    }
    public override void UpdateState(PlayerStateMachine  _manager)
    {
        _manager.Pm.Direction_Input(_manager.Pm.groundSpeed);
        
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        //if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        _manager.Pm.GroundPhysics(_manager.Pm.CrouchSpeedMult);
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        if(_manager.Pm.stepOnGround > _manager.Pm.LateFrictionDelay)
        _manager.Pm.ApplyFriction(_manager.Pm.friction);
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _currentState, AbstractState<PlayerStateMachine> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log("GroundSubStateExit" + this.ToString());
    }
}
public class GroundSubState_Sprint : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        Debug.Log("GroundSubState: " + this.ToString());
    }
    public override void UpdateState(PlayerStateMachine  _manager)
    {
        _manager.Pm.Direction_Input(_manager.Pm.groundSpeed);
        
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        //if(_manager.currentActionSubState is not ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        _manager.Pm.GroundPhysics(_manager.Pm.SprintSpeedMult);
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        if(_manager.Pm.stepOnGround > _manager.Pm.LateFrictionDelay)
        _manager.Pm.ApplyFriction(_manager.Pm.friction);
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _currentState, AbstractState<PlayerStateMachine> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log("GroundSubStateExit" + this.ToString());
    }
}
public class GroundSubState_Slide : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        Debug.Log($"Enter State: {this.ToString()}");
    }
    public override void UpdateState(PlayerStateMachine  _manager)
    {
        
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        if(_manager.Pm.stepOnGround > _manager.Pm.LateFrictionDelay)
        _manager.Pm.ApplyFriction(_manager.Pm.slideFriction);
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _currentState, AbstractState<PlayerStateMachine> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log($"Exit State: {this.ToString()}");
    }
}
#endregion

#region Action Sub States (best defined as state that will limit character's arm action, character only has 2 arms)
public class ActionSubState_Idle : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        
    }
    public override void UpdateState(PlayerStateMachine _manager)
    {
        
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _CurrentState, AbstractState<PlayerStateMachine> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_HookEnemy : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        _manager.Hook.StartHookEnemy();
    }
    public override void UpdateState(PlayerStateMachine _manager)
    {
        _manager.Pm.Direction_Input(_manager.Pm.groundSpeed);
        _manager.Pm.Jump_Input();
        //_manager.hook.CancelHook_Input(_manager.InputSystemManager.grappleHook);
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        _manager.Hook.CheckDistanceThreshold();
        _manager.Hook.CheckObstaclesBetween();

        _manager.Hook.ExecuteHookEnemy();
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _CurrentState, AbstractState<PlayerStateMachine> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_GrappleSurface : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        _manager.Hook.StartGrappleSurface();
    }
    public override void UpdateState(PlayerStateMachine _manager)
    {
        _manager.Pm.Direction_Input(_manager.Pm.airSpeed);
        _manager.Pm.Jump_Input();
        //_manager.hook.CancelHook_Input(_manager.InputSystemManager.grappleHook);
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        _manager.Hook.CheckDistanceThreshold();
        _manager.Hook.CheckRopeStretch();
        _manager.Hook.CheckPlayerFov();
        _manager.Hook.CheckIfPlayerLanded();

        _manager.Hook.ExecuteGrappleSurface();
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        throw new NotImplementedException();
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _CurrentState, AbstractState<PlayerStateMachine> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_Reloading : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        
    }
    public override void UpdateState(PlayerStateMachine _manager)
    {
        
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
    
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _CurrentState, AbstractState<PlayerStateMachine> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}
public class ActionSubState_WeaponSwitching : AbstractState<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine _manager)
    {
        
    }
    public override void UpdateState(PlayerStateMachine _manager)
    {
        
    }
    public override void DuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void LateDuringState(PlayerStateMachine _manager)
    {
        
    }
    public override void ExitState(PlayerStateMachine _manager, ref AbstractState<PlayerStateMachine> _CurrentState, AbstractState<PlayerStateMachine> _TargetState)
    {
        base.ExitState(_manager, ref _CurrentState, _TargetState);
    }
}

#endregion