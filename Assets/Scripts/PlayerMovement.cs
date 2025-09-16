using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        // Use the new Input System
        if ((UnityEngine.InputSystem.Keyboard.current.zKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.wKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.upArrowKey.wasPressedThisFrame))
        {
            Debug.Log("Avancer");
        }
        if ((UnityEngine.InputSystem.Keyboard.current.sKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.downArrowKey.wasPressedThisFrame))
        {
            Debug.Log("Reculer");
        }
        if ((UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.leftArrowKey.wasPressedThisFrame))
        {
            Debug.Log("Gauche");
        }
        if ((UnityEngine.InputSystem.Keyboard.current.dKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.rightArrowKey.wasPressedThisFrame))
        {
            Debug.Log("Droite");
        }
    }
}
