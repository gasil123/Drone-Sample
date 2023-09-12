using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class DroneMovement : MonoBehaviour
{
    [SerializeField] float forwardSpeed = 5.0f;
    [SerializeField] float backwardSpeed = 2.0f;
    [SerializeField] float sidewardSpeed = 3.0f;
    [SerializeField] float rotationSpeed = 60.0f;
    [SerializeField] float ascentSpeed = 5.0f;
    [SerializeField] float descentSpeed = 3.0f;
    [SerializeField] Camera cam;

    [SerializeField] float ascentSpeedMultiplier = 3.0f;
    [SerializeField] float descentSpeedMultiplier = 3.0f;
    [SerializeField] float upwardThrustMultiplier = 3.0f;
    [SerializeField] float steadynessMultiplier = 3f;

    [SerializeField] float tiltAngle = 10f;

    [SerializeField] float maxRotorSpeed = 10f;

    private RotorController[] rotors;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private bool ascendingPressed;
    private bool descendingPressed;
    private bool isGrounded;

    [SerializeField] LayerMask layer;
    [SerializeField] PlayerInput _playerInput;

    private Vector2 _input;
    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }
    public void Ascend(InputAction.CallbackContext context)
    {
        ascendingPressed = context.ReadValueAsButton();
    }
    public void Descend(InputAction.CallbackContext context)
    {
        descendingPressed = context.ReadValueAsButton();
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        rotors = GetComponentsInChildren<RotorController>();
    }
    private void FixedUpdate()
    {
       
        HandleRotation();
        HandleMovement();
        RotateBlades();
    }
    Vector3 movementDirection;
    Quaternion tiltRotation;
    private Quaternion currentTiltRotation;
    private Quaternion targetTiltRotation;

    private void HandleMovement()
    {
        movementDirection = Vector3.Lerp(movementDirection, Vector3.zero, steadynessMultiplier * Time.fixedDeltaTime);

        float horizontalInput = _input.x;
        float verticalInput = _input.y;

        // Calculate forward and backward movement
        if (verticalInput != 0.0f)
        {
            ApplyUpwardThrust();
            movementDirection += transform.forward * (verticalInput > 0 ? forwardSpeed : -backwardSpeed);
        }

        // Calculate left and right movement
        if (horizontalInput != 0.0f)
        {
            ApplyUpwardThrust();
            movementDirection += transform.right * (horizontalInput > 0 ? sidewardSpeed : -sidewardSpeed);
        }

        // Handle ascending and descending
        if (ascendingPressed)
        {
            movementDirection += transform.up * ascentSpeed * ascentSpeedMultiplier * Time.fixedDeltaTime;
        }
        else if (descendingPressed)
        {
            movementDirection -= transform.up * descentSpeed * descentSpeedMultiplier * Time.fixedDeltaTime;
        }

        // Calculate the target rotation
        Vector3 cameraForward = cam.transform.forward;
        cameraForward.y = 0.0f;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);

        Quaternion targetRotation = Quaternion.Euler(
            verticalInput * tiltAngle,
            cameraRotation.eulerAngles.y, // Adjusted to match camera rotation
            -horizontalInput * tiltAngle
        );

        // Smoothly interpolate the tilt rotation
        currentTiltRotation = Quaternion.Slerp(currentTiltRotation, targetRotation, steadynessMultiplier * Time.fixedDeltaTime);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, currentTiltRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.velocity = movementDirection;
    }
    private void HandleRotation()
    {
        if (Grounded())
        {
            return;
        }
        Vector3 cameraForward = cam.transform.forward;
        cameraForward.y = 0.0f;
      
        if (cameraForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void ApplyUpwardThrust()
    {
        if (descendingPressed) return;
        movementDirection += transform.up * ascentSpeed * upwardThrustMultiplier * Time.fixedDeltaTime;
    }
    public bool Grounded()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, -transform.up * 0.1f);
        if (Physics.Raycast(transform.position, -transform.up, out hit, 0.1f, layer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        return isGrounded;
    }
    public void RotateBlades()
    {
        if (Grounded())
        {
            float rotorSpeed = Mathf.Lerp(rotationSpeed, 0 , 2f);
            foreach (var rotor in rotors)
            {
                rotor.SetRotorSpeed(rotorSpeed);
            }
        }
        else
        {
            float rotorSpeed = ascentSpeedMultiplier * maxRotorSpeed + MathF.Abs(verticalInput + horizontalInput + 1);
            foreach (var rotor in rotors)
            {
                rotor.SetRotorSpeed(rotorSpeed);
            }
        }
       
    }
}
