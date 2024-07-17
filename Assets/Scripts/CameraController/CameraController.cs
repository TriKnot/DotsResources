using System;
using UnityEngine;

namespace CameraController
{
    public class CameraController : MonoBehaviour
    {
        public Camera Camera;

        [SerializeField] private float _lookSpeed;
        [SerializeField] private float _moveSpeed;

        private Vector2 _moveDirection;
        
        private void Awake()
        {
            // Lock mouse cursor to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void FixedUpdate()
        {            
            // Get the current position
            Vector3 currentPosition = transform.position;

            // Calculate the forward and right vectors based on the current rotation
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Remove the y component from the forward and right vectors
            forward.y = 0f;
            right.y = 0f;

            // Normalize the forward and right vectors
            forward.Normalize();
            right.Normalize();
            
            // Calculate the new position based on the input move vector
            Vector3 newPosition = currentPosition + (forward * _moveDirection.y + right * _moveDirection.x) * _moveSpeed * Time.deltaTime;

            // Apply the new position
            transform.position = newPosition;
        }

        public void OnLook(Vector2 look)
        {
            // Get the current rotation angles
            Vector3 currentRotation = transform.localEulerAngles;

            // Adjust the pitch (x-axis) and yaw (y-axis) based on the input look vector
            float pitch = currentRotation.x - look.y * _lookSpeed * Time.deltaTime;
            float yaw = currentRotation.y + look.x * _lookSpeed * Time.deltaTime;

            // Apply the new rotation
            transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
        }

        public void OnMoveStarted(Vector2 move)
        {
            _moveDirection = move;
        }
        
        public void OnMoveCancelled(Vector2 move)
        {
            _moveDirection = Vector2.zero;
        }
    }
}