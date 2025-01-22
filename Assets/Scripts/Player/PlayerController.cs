using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 6f;
        [SerializeField] private float jumpForce = 3f;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;
        [SerializeField] private float turnSmoothTime = .1f;

        private float _horizontal;
        private float _vertical;

        private float _turnSmoothVelocity;

        private bool _isSprinting = false;
        private bool _isJumping = false;
        private bool _isGrounded = true;

        private float _vSpeed = 0f;

        private Vector3 _velocity;

        private CharacterController m_characterController;
        private Transform m_camera;
        private Animator m_animator;

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();
            m_camera = Camera.main.transform;
            m_animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            Vector3 direction = new Vector3(_horizontal, 0, _vertical).normalized;

            if (direction.magnitude >= .1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + m_camera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);

                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                float currentSpeed = _isSprinting ? speed * sprintSpeedMultiplier : speed;

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                moveDir.y = _vSpeed;

                _velocity = moveDir.normalized * currentSpeed * Time.deltaTime;
            }

            if (m_characterController.isGrounded)
            {
                _vSpeed = 0f;
                m_animator.SetBool("isGrounded", true);
            }
            else
            {
                _vSpeed = Physics.gravity.y * Time.deltaTime;
            }

            _velocity.y = _vSpeed;

            m_characterController.Move(_velocity);
        }

        #region Input
        public void OnMove(InputValue value)
        {
            Vector2 axis = value.Get<Vector2>();

            _horizontal = axis.x;
            _vertical = axis.y;

            m_animator.SetBool("isMoving", axis.magnitude >= .1f);
        }

        public void OnSprint(InputValue value)
        {
            _isSprinting = !_isSprinting;

            m_animator.SetBool("isSprinting", _isSprinting);
        }

        public void OnJump(InputValue value)
        {
            Debug.Log($"Jump value: {value.Get<float>()}");

            if (value.Get<float>() <= 0)
                return;
            if (!m_characterController.isGrounded)
                return;

            _vSpeed += jumpForce;

            _isJumping = true;

            //m_animator.SetBool("isJumping", _isJumping);
        }
        #endregion
    }
}
