using UnityEngine;

public class RotorController : MonoBehaviour
{
    public float maxRotationSpeed = 1000.0f;

    private float rotorRotationSpeed = 1.0f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotorRotationSpeed * Time.deltaTime);
    }
    public void SetRotorSpeed(float speed)
    {
        rotorRotationSpeed = Mathf.Clamp(speed, 0.0f, maxRotationSpeed);
    }
}
