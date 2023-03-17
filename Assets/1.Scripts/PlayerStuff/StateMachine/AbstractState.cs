using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary> Abstract class of where all states objects are derived from, "T" is stateMachine </summary>///
public abstract class AbstractState<T> //: IState<T>
{
    //protected PlayerMovement pm;
    ///<summary> Run once when state is initially switched </summary>///
    public abstract void EnterState(T _manager);
    ///<summary> Run every frame, in Update, use it for input reading </summary>///
    public abstract void UpdateState(T _manager);
    ///<summary> handles state physics in FixedUpdate </summary>///
    public abstract void DuringState(T _manager);
    public abstract void LateDuringState(T _manager);

    ///<summary> handles state switching, extra fuctionalities when exited</summary>///
    public virtual void ExitState(T _manager, ref AbstractState<T> _CurrentState ,AbstractState<T> _TargetState)
    {
        _CurrentState = _TargetState;
        _TargetState.EnterState(_manager);
    }
}

// public interface IState<T>
// {
//     public void EnterState(T _manager);
//     public void UpdateState(T _manager);
//     public void DuringState(T _manager);
//     public void LateDuringState(T _manager);
// }
