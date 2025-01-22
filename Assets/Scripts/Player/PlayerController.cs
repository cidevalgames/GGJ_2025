using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 6f;
        [SerializeField] private float turnSmoothTime = .1f;

        private float _horizontal;
        private float _vertical;

        private float _turnSmoothVelocity;

        private CharacterController m_characterController;
        private Transform m_camera;

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();
            m_camera = Camera.main.transform;
        }

        private void Update()
        {
            Vector3 direction = new Vector3(_horizontal, 0, _vertical).normalized;

            if (direction.magnitude >= .1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + m_camera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                m_characterController.Move(moveDir.normalized * speed * Time.deltaTime);
            }
        }

        #region Input
        public void OnMove(InputValue value)
        {
            Vector2 axis = value.Get<Vector2>();

            _horizontal = axis.x;
            _vertical = axis.y;
        }
        #endregion
    }
}
