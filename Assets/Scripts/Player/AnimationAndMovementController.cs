using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    private CharacterController m_characterController;
    private Animator m_animator;
    private PlayerControls m_playerControls;
    private Transform m_camera;

    private int _isWalkingHash;
    private int _isRunningHash;

    private float _turnSmoothVelocity;

    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private Vector3 _currentRunMovement;

    private bool _isMovementPressed;
    private bool _isRunPressed;

    // Constants
    [SerializeField] private float rotationFactorPerFrame = 15f;
    [SerializeField] private float turnSmoothTime = .1f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runMultiplier = 3f;
    [SerializeField] private int zero = 0;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravity = -.05f;

    // Jumping variables
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 1.5f;
    [SerializeField] private float maxJumpHeight = 4f;
    [SerializeField] private float maxJumpTime = .75f;
    private float _initialJumpVelocity;
    private bool _isJumpPressed = false;
    private bool _isJumping = false;
    private int _isJumpingHash;
    private bool _isJumpAnimating = false;

    private void Awake()
    {
        m_playerControls = new PlayerControls();
        m_characterController = GetComponent<CharacterController>();
        m_animator = GetComponentInChildren<Animator>();
        m_camera = Camera.main.transform;

        // Set the parameter hash references
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");

        // Set the player input callbacks
        m_playerControls.MyPlayer.Move.started += OnMovementInput;
        m_playerControls.MyPlayer.Move.canceled += OnMovementInput;
        m_playerControls.MyPlayer.Move.performed += OnMovementInput;

        m_playerControls.MyPlayer.Jump.started += OnJump;
        m_playerControls.MyPlayer.Jump.canceled += OnJump;

        m_playerControls.MyPlayer.Sprint.started += OnSprint;
        m_playerControls.MyPlayer.Sprint.canceled += OnSprint;

        SetupJumpVariables();
    }

    private void OnEnable()
    {
        // Enable the character controls action map
        m_playerControls.MyPlayer.Enable();
    }

    private void OnDisable()
    {
        // Disable the character controls action map
        m_playerControls.MyPlayer.Disable();
    }

    private void Update()
    {
        HandleRotation();
        HandleAnimation();

        if (_isRunPressed)
        {
            m_characterController.Move(_currentRunMovement * Time.deltaTime);
        }
        else
        {
            m_characterController.Move(_currentMovement * Time.deltaTime);
        }

        HandleGravity();
        HandleJump();
    }

    private void HandleAnimation()
    {
        // Get parameter values from animator
        bool isWalking = m_animator.GetBool(_isWalkingHash);
        bool isRunning = m_animator.GetBool(_isRunningHash);

        // Start walking if movement pressed is true and not already walking
        if (_isMovementPressed && !isWalking)
        {
            m_animator.SetBool(_isWalkingHash, true);
        }
        // Stop walking if isMovementPressed is false and not already walking
        else if (!_isMovementPressed && isWalking)
        {
            m_animator.SetBool(_isWalkingHash, false);
        }

        if ((_isMovementPressed && _isRunPressed) && !isRunning)
        {
            m_animator.SetBool(_isRunningHash, true);
        }
        else if ((!_isMovementPressed || !_isRunPressed) && isRunning)
        {
            m_animator.SetBool(_isRunningHash, false);
        }
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0f;
        positionToLookAt.z = _currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
        }
    }

    private void HandleJump()
    {
        if (!_isJumping && m_characterController.isGrounded && _isJumpPressed)
        {
            m_animator.SetBool(_isJumpingHash, true);
            _isJumpAnimating = true;

            _isJumping = true;

            float previousYVelocity = _currentMovement.y;
            float newYVelocity = (_currentMovement.y + _initialJumpVelocity);
            float nextYVelocity = (previousYVelocity + newYVelocity) * jumpForce;

            _currentMovement.y = nextYVelocity;
            _currentRunMovement.y = nextYVelocity;
        }
        else if (!_isJumpPressed && _isJumping && m_characterController.isGrounded)
        {
            _isJumping = false;
        }
    }

    private void HandleGravity()
    {
        bool isFalling = _currentMovement.y <= 0f || !_isJumpPressed;
        float fallMultiplier = 2f;

        // Apply proper gravity if the player is grounded or not
        if (m_characterController.isGrounded)
        {
            m_animator.SetBool(_isJumpingHash, false);
            _isJumpAnimating = false;

            _currentMovement.y = groundedGravity;
            _currentRunMovement.y = groundedGravity;
        }
        else if (isFalling)
        {
            float previousYVelocity = _currentMovement.y;
            float newYVelocity = _currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * .5f, -20f);

            _currentMovement.y = nextYVelocity;
            _currentRunMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = _currentMovement.y;
            float newYVelocity = _currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;

            _currentMovement.y = nextYVelocity;
            _currentRunMovement.y = nextYVelocity;
        }
    }

    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    #region Input methods
    private void OnSprint(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();

        //Debug.Log($"Is jump pressed: {_isJumpPressed}");
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();

        _currentMovement.x = _currentMovementInput.x * walkSpeed;
        _currentMovement.z = _currentMovementInput.y * walkSpeed;
        
        _currentRunMovement.x = _currentMovementInput.x * walkSpeed * runMultiplier;
        _currentRunMovement.z = _currentMovementInput.y * walkSpeed * runMultiplier;

        _isMovementPressed = _currentMovementInput.x != zero || _currentMovementInput.y != zero;
    }
    #endregion
}
