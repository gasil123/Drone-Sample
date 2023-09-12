using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] [Tooltip("Forward movement speed")] 
    private float forwardSpeed = 5.0f;
    [SerializeField] [Tooltip("Backward movement speed")] 
    private float backwardSpeed = 2.0f;
    [SerializeField] [Tooltip("Sideward movement speed")]
    private float sidewardSpeed = 3.0f;
    [SerializeField] [Tooltip("Rotation speed")]
    private float rotationSpeed = 60.0f;
    [SerializeField] [Tooltip("Ascent speed")] 
    private float ascentSpeed = 5.0f;
    [SerializeField] [Tooltip("Descent speed")] 
    private float descentSpeed = 3.0f;
    [SerializeField] 
    private Camera cam;

    [Header("Multiplier Parameters")]
    [SerializeField] [Tooltip("Multiplier for ascent speed")] 
    private float ascentSpeedMultiplier = 3.0f;
    [SerializeField] [Tooltip("Multiplier for descent speed")] 
    private float descentSpeedMultiplier = 3.0f;
    [SerializeField] [Tooltip("Multiplier for upward thrust")] 
    private float upwardThrustMultiplier = 3.0f;
    [SerializeField] [Tooltip("Steadiness multiplier")] 
    private float steadinessMultiplier = 3f;

    [Header("Tilt Parameters")]
    [SerializeField] [Tooltip("Tilt angle")]
    private float tiltAngle = 10f;
    [SerializeField] [Tooltip("Maximum rotor speed")] 
    private float maxRotorSpeed = 10f;

    [Header("Layer and Input")]
    [SerializeField] [Tooltip("Layer mask for grounding check")]
    private LayerMask layer;

    private Vector2 input;
    private bool ascendingPressed;
    private bool descendingPressed;
    private bool isGrounded;
    private Rigidbody rb;
    private Vector3 movementDirection;
    private Quaternion currentTiltRotation;
    private RotorController[] rotors;

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
    private void HandleMovement()
    {
        movementDirection = Vector3.Lerp(movementDirection, Vector3.zero, steadinessMultiplier * Time.fixedDeltaTime);

        float horizontalInput = input.x;
        float verticalInput = input.y;

        if (verticalInput != 0.0f)
        {
            ApplyUpwardThrust();
            movementDirection += transform.forward * (verticalInput > 0 ? forwardSpeed : -backwardSpeed);
        }

        if (horizontalInput != 0.0f)
        {
            ApplyUpwardThrust();
            movementDirection += transform.right * (horizontalInput > 0 ? sidewardSpeed : -sidewardSpeed);
        }

        if (ascendingPressed)
        {
            movementDirection += transform.up * ascentSpeed * ascentSpeedMultiplier * Time.fixedDeltaTime;
        }
        else if (descendingPressed)
        {
            movementDirection -= transform.up * descentSpeed * descentSpeedMultiplier * Time.fixedDeltaTime;
        }

        if (!Grounded())
        {
            Vector3 cameraForward = cam.transform.forward;
            cameraForward.y = 0.0f;
            Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);

            Quaternion targetRotation = Quaternion.Euler(
                verticalInput * tiltAngle,
                cameraRotation.eulerAngles.y,
                -horizontalInput * tiltAngle
            );
            currentTiltRotation = Quaternion.Slerp(currentTiltRotation, targetRotation, steadinessMultiplier * Time.fixedDeltaTime);
        }

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

    private bool Grounded()
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

    private void RotateBlades()
    {
        float rotorSpeed = ascentSpeedMultiplier * maxRotorSpeed + Mathf.Abs(input.x + input.y + 1);
        foreach (var rotor in rotors)
        {
            rotor.SetRotorSpeed(rotorSpeed);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    public void Ascend(InputAction.CallbackContext context)
    {
        ascendingPressed = context.ReadValueAsButton();
    }

    public void Descend(InputAction.CallbackContext context)
    {
        descendingPressed = context.ReadValueAsButton();
    }
}
