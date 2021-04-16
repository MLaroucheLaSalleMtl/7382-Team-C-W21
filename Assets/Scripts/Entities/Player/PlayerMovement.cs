using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


// Require component so it is added automatically when script is assigned
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // Player
    [SerializeField] private Player _player;
    
    // Animator component
    private Animator _animator;
    
    // States
    public bool canMove;
    
    // Instance of PlayerInput
    private PlayerInput _playerInput;

    private Transform _cameraTransform;
    
    public CharacterController controller;
    public float speed = 6f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    
    // Movement variables
    private int locomotionHash;
    //private float locomotion = 0;
    
    // PlayerInput values
    private Vector2 currentMovement;
    private bool movementPressed;
    private bool runPressed;

    // Delegates
    public float GetAxis(string axisName)
    {
        switch (axisName)
        {
            case "Mouse X" when Input.GetMouseButton(1):
                return UnityEngine.Input.GetAxis("Mouse X");
            
            case "Mouse X":
                return 0;
            
            case "Mouse Y" when Input.GetMouseButton(1):
                return UnityEngine.Input.GetAxis("Mouse Y");
            
            case "Mouse Y":
                return 0;
            
            default:
                return UnityEngine.Input.GetAxis(axisName);
        }
    }

    void HandleMovement(Vector3 direction)
    {
        if (direction.magnitude >= 0.1f)
        {
            // Rotation
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Movement
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * (speed * Time.deltaTime));
        }

        // If we start moving
        if (movementPressed)
        {
            _animator.SetFloat(locomotionHash, 1f);
        }
        
        // If we stop moving
        if (!movementPressed)
        {
            _animator.SetFloat(locomotionHash, 0f);
        }

        // If moving and pressing run
        if (movementPressed && runPressed)
        {
            _animator.SetFloat(locomotionHash, 2f);
        }
        
        // If running but not pressing run
        if (movementPressed && !runPressed)
        {
            _animator.SetFloat(locomotionHash, 1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!_player.isLocalPlayer)
            return;

        canMove = true;
        
        // Cache our camera transform
        _cameraTransform = Camera.main.transform;

        // Initialize PlayerInput script
        _playerInput = new PlayerInput();

        // We're pressing movement buttons
        _playerInput.CharacterControls.Move.performed += ctx =>
        {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
            //Debug.Log("X: " + currentMovement.x + " Y: " + currentMovement.y);
        };

        // Stopped pressing movement buttons
        _playerInput.CharacterControls.Move.canceled += ctx =>
        {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = false;
        };

        // Pressing Run
        _playerInput.CharacterControls.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();

        // Stopped pressing Run
        _playerInput.CharacterControls.Run.canceled += ctx => runPressed = ctx.ReadValueAsButton();
        
        _animator = GetComponent<Animator>();
        locomotionHash = Animator.StringToHash("locomotion");
        
        // Enable controllers
        _playerInput.CharacterControls.Enable();
        
        // Enable camera rotation
        CinemachineCore.GetInputAxis = GetAxis;
    }

    // Update is called once per frame
    void Update()
    {
        // If we're not the local player, return
        // That stops the player from moving on the character selection screen
        //Debug.Log("Local player? " + _player.isLocalPlayer.ToString());
        if (!_player.isLocalPlayer)
            return;

        Vector3 moveDirection = new Vector3(currentMovement.x, 0f, currentMovement.y).normalized;
        
        if (canMove)
            HandleMovement(direction: moveDirection);

        //_player._freeLookCamera.m_XAxis = _lastX;
        //_player._freeLookCamera.m_YAxis = _lastY;

        // Move camera while the right click is pressed, and when is dropped, place the camera to that position
        if (Input.GetMouseButton(1))
        {
            //_lastX = _player._freeLookCamera.m_XAxis;
            //_lastY = _player._freeLookCamera.m_YAxis;
            GetAxis("Mouse X");
            GetAxis("Mouse Y");
        }
    }
}
