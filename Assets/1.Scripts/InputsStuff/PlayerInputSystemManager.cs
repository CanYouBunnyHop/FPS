using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS.Settings
{
public class PlayerInputSystemManager : MonoBehaviour
{
    //[SerializeField]private PlayerInput playerInput;
    //private InputAction jumpAction;
    [SerializeField]public InputActions input;

    //Input Actions
    public InputAction jump => input.PlayerActionMap.Jump;
    public InputAction wasd => input.PlayerActionMap.WASD;
    public InputAction crouch => input.PlayerActionMap.Crouch;
    public InputAction backAiming => input.PlayerActionMap.BackAiming;
    public InputAction sprint => input.PlayerActionMap.Sprint;
    public InputAction grappleHook => input.PlayerActionMap.GrapplingHook;

    //mouse input actions
    public InputAction fire => input.PlayerActionMap.Fire;
    public InputAction ads => input.PlayerActionMap.ADS;
    public InputAction cycleUp => input.PlayerActionMap.CycleWeaponUp;
    public InputAction cycleDown => input.PlayerActionMap.CycleWeaponDown;

    private void Awake()
    {
        input = new InputActions();
        //currentActionMap = input.PlayerActionMap; 

        //event delegate context
        //started = down
        //performed = hold
        //canceld = up

        //input.PlayerActionMap.Jump.started += Dick;

        //example event delegates //or "actions" which are Unity made ready to use delegates
        //jump.started += Dick; //Dick(playerInputActions.Player.Jump.started);
        //playerInputActions.Player.Jump.performed += Dick;
        //playerInputActions.Player.Jump.canceled += Dick;
        
        //actions.Player
        

        //inputSetting Interactions
        //press point = how far down do you press it, don't work on keyboard
        //holdTime = time held down long enough to it performs the action

        //inputSetting processors
        //process value after the action

        //button modifier
        //button = binding, modifier = the other key that will change the value when pressed with button.

        //player input makes somethings easier

        //player input manager is for local multiplayer
        
    }
    public void SetActiveAction(InputAction _action, bool _enable)
    {
        if(_enable)
        _action.Enable();
        else
        _action.Disable();
    }
    private void OnEnable()
    {
        input.PlayerActionMap.Enable();
        
    }
    private void OnDisable()
    {
        input.PlayerActionMap.Disable();
    }
}

}