using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
///<summary> Abstract class of where all states objects are derived from, "T" is stateMachine </summary>///
public abstract class AbstractState<T>
{
    //protected PlayerMovement pm;
    ///<summary> Run once when state is initially switched </summary>///
    public abstract void EnterState(T _manager);
    ///<summary> handles state physics in FixedUpdate </summary>///
    public abstract void DuringState(T _manager);
    ///<summary> handles state switching, extra fuctionalities when exited</summary>///
    public virtual void ExitState(T _manager, ref AbstractState<T> _CurrentState ,AbstractState<T> _TargetState)
    {
        _CurrentState = _TargetState;
        _TargetState.EnterState(_manager);
    }

    // protected AbstractState(PlayerMovement _pm)
    // {
    //     pm = _pm;
    // }
}

//for copy pasting

    // public override void EnterState(TestPlayerStateManager _manager)
    // {
    //     throw new System.NotImplementedException();
    // }
    // public override void UpdateState(TestPlayerStateManager _manager)
    // {
    //     throw new System.NotImplementedException();
    // }
    // public override void FixedUpdateState(TestPlayerStateManager _manager)
    // {
    //     throw new System.NotImplementedException();
    // }
