using System;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    [Header("Character Input Values")]

    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool crouch;
    public bool dodge;

    public void OnMove(InputAction.CallbackContext value)
    {
        MoveInput(value.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext value)
    {
        LookInput(value.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext value)
    {
        JumpInput(value.action.triggered);
    }

    public void OnSprint(InputAction.CallbackContext value)
    {
        SprintInput(value.action.triggered);
    }

    public void OnCrouch(InputAction.CallbackContext value)
    {
        CrouchInput(value.action.triggered);
    }

    public void OnDodge(InputAction.CallbackContext value)
    {
        DodgeInput(value.started);
    }


    private void MoveInput(Vector2 moveInput)
    {
        move = moveInput;
    }

    private void LookInput(Vector2 lookInput)
    {
        look = lookInput;
    }

    private void JumpInput(bool jumpInput)
    {
        jump = jumpInput;
    }

    private void SprintInput(bool sprintInput)
    {
        sprint = sprintInput;
    }

    private void CrouchInput(bool crouchInput)
    {
        crouch = crouchInput;
    }

    private void DodgeInput(bool dodgeInput)
    {
        dodge = dodgeInput;
    }



}
