using UnityEngine;
using Cinemachine;

public class SwitchCams : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCameraBase[] cam;
    [SerializeField] int priorityBoostAmount = 5;
    bool change = false;
    private Coroutine Coroutine;
    private void Start()
    {
        ChangePriorities();
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.V))
        {
            ChangePriorities();
        }
    }
    public void ChangePriorities()
    {
        change = !change;

        if (change)
        {
            cam[0].Priority += priorityBoostAmount; 
            cam[1].Priority -= priorityBoostAmount; 
        }
        if (!change)
        {
            cam[0].Priority -= priorityBoostAmount;
            cam[1].Priority += priorityBoostAmount;
        }
    }
}
