using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Input
{
    public class InputHandler : MonoBehaviour
    {
        private InputActions _inputActions;
        
        public UnityEvent<Vector2> OnLook = new ();
        public UnityEvent<Vector2> OnMoveStarted = new ();
        public UnityEvent<Vector2> OnMoveCancelled = new ();

        private void OnEnable()
        {
            _inputActions = new InputActions();
            _inputActions.Player.Look.performed += OnLookInput;
            _inputActions.Player.Move.started += OnMoveInputStarted;
            _inputActions.Player.Move.canceled += OnMoveInputCancelled;
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Look.performed -= OnLookInput;
            _inputActions.Player.Move.started -= OnMoveInputStarted;
            _inputActions.Player.Move.canceled -= OnMoveInputCancelled;
            _inputActions.Disable();
        }

        private void OnLookInput(InputAction.CallbackContext ctx)
        {
            OnLook.Invoke(ctx.ReadValue<Vector2>());
        }
        
        private void OnMoveInputStarted(InputAction.CallbackContext ctx)
        {
            OnMoveStarted.Invoke(ctx.ReadValue<Vector2>());
        }
        
        private void OnMoveInputCancelled(InputAction.CallbackContext obj)
        {
            OnMoveStarted.Invoke(Vector2.zero);
        }
        
    }
}