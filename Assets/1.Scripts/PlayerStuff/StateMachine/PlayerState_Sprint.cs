using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player.Movement;

public class PlayerState_Sprint : AbstractState<PlayerMovement>
{
    public override void EnterState(PlayerMovement _manager)
    {
        Debug.Log("State: " + this.ToString());
    }
    public override void DuringState(PlayerMovement _manager)
    {
        throw new System.NotImplementedException();
    }
    public override void ExitState(PlayerMovement _manager, ref AbstractState<PlayerMovement> _currentState, AbstractState<PlayerMovement> _targetState)
    {
        base.ExitState(_manager, ref _currentState ,_targetState);
        Debug.Log("Exit" + this.ToString());
    }
}
