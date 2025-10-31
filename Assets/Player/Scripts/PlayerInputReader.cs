using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    #region Input Values
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool crouch;
    public bool dodge;
    public bool primaryFire;
    public bool secondaryFire;

    private bool nextPressed;
    private bool previousPressed;
    #endregion

    #region Input Callbacks
    public void OnMove(InputAction.CallbackContext value)
    {
        move = value.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext value)
    {
        look = value.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext value)
    {
        jump = value.action.triggered;
    }

    public void OnSprint(InputAction.CallbackContext value)
    {
        sprint = value.action.triggered;
    }

    public void OnCrouch(InputAction.CallbackContext value)
    {
        crouch = value.action.triggered;
    }

    public void OnDodge(InputAction.CallbackContext value)
    {
        dodge = value.started;
    }

    public void OnPrimaryFire(InputAction.CallbackContext value)
    {
        primaryFire = value.action.triggered;
    }

    public void OnSecondryFire(InputAction.CallbackContext value)
    {
        secondaryFire = value.action.triggered;
    }

    public void OnPrevious(InputAction.CallbackContext value)
    {
        // Only trigger once when button is first pressed
        if (value.performed)
        {
            previousPressed = true;
        }
    }

    public void OnNext(InputAction.CallbackContext value)
    {
        // Only trigger once when button is first pressed
        if (value.performed)
        {
            nextPressed = true;
        }
    }
    #endregion

    #region Public Methods - Consumable Inputs
    /// <summary>
    /// Check if next weapon input was pressed and consume it.
    /// Returns true only once per button press.
    /// </summary>
    public bool ConsumeNextInput()
    {
        if (nextPressed)
        {
            nextPressed = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if previous weapon input was pressed and consume it.
    /// Returns true only once per button press.
    /// </summary>
    public bool ConsumePreviousInput()
    {
        if (previousPressed)
        {
            previousPressed = false;
            return true;
        }
        return false;
    }
    #endregion
}