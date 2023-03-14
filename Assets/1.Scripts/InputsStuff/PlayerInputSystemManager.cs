using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS.Settings.Keybinds
{
public class PlayerInputSystemManager : MonoBehaviour
{
    //[SerializeField]private PlayerInput playerInput;
    //private InputAction jumpAction;
    [SerializeField]public InputActions input;
    public InputAction jump => input.PlayerActionMap.Jump;
    public InputAction wasd => input.PlayerActionMap.WASD;

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
        jump.started += Dick; //Dick(playerInputActions.Player.Jump.started);
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
    // private void Update()
    // {
    //     ReturnGetButton(input.PlayerActionMap.Jump);

    //     if (input.PlayerActionMap.Jump.triggered)
    //     {
    //         Debug.Log("isTriggered"); // this is the same as get buttonDown, only called once
    //     }
    // }
    // public bool ReturnGetButton(InputAction _action)
    // {
    //     if(_action.ReadValue<float>() == 1) // this is the same as InputAction.IsPressed()
    //     {
    //         Debug.Log("tru" + _action.ReadValue<float>());
    //         return true;
    //     }
    //     else return false;

    //     //if(_action.ReadValueAsObject())
    // }
    public void Dick(InputAction.CallbackContext context)
    {
       
       Debug.Log(context.ReadValue<float>());

    }
    private void OnEnable()
    {
        input.PlayerActionMap.Enable();
        
    }
    private void OnDisable()
    {
        input.PlayerActionMap.Disable();
        // playerInputActions.Player.Jump.started -= Dick;
        // playerInputActions.Player.Jump.performed -= Dick;
        // playerInputActions.Player.Jump.canceled -= Dick;
    }
    //private void Update()
    // {
    //     Vector2 move = playerInputActions.Player.WASD.ReadValue<Vector2>(); //this is the new input system getaxis but no smoothing

    //     if (playerInputActions.Player.Jump.IsPressed()) //.inProgress //
    //     {

    //     }
    //     if(playerInputActions.Player.Jump.ReadValue<float>() == 1) //
    //     {

    //     }
        
    //     if(playerInputActions.Player.Jump.triggered) //
    //     {

    //     }
    // }

}

}