using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started!");

        // InputSystem.RegisterInteraction<RectangleGestureInteraction>();

        var keyboard = InputSystem.AddDevice<Mouse>();
        var mouseRectangleMovement = new InputAction("RectangleMovement", binding: "<Pointer>/position", interactions: "RectangleGesture");

        mouseRectangleMovement.Enable();
        mouseRectangleMovement.performed += ctx =>
        {
            Debug.Log("My mouse has drawn a rectangle!: ");
        };

        mouseRectangleMovement.canceled += ctx =>
        {
            Debug.Log("Canceled!");
        };

    }
}
