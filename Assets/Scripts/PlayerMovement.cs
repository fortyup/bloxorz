using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private float rollSpeed = 1f; // Vitesse d’animation
    private bool isRolling = false;
    private Vector3 moveDir;

    void OnMove(InputValue movementValue)
    {
        if (isRolling) return;

        Vector2 input = movementValue.Get<Vector2>();
        moveDir = new Vector3(input.x, 0, input.y);

        if (moveDir != Vector3.zero)
            StartCoroutine(Roll(moveDir));
    }

    private IEnumerator Roll(Vector3 dir)
    {
        isRolling = true;

        Vector3 anchor = transform.position + (Vector3.down + dir) * 0.5f;
        Vector3 axis = Vector3.Cross(Vector3.up, dir);

        for (int i = 0; i < 90; i++)
        {
            transform.RotateAround(anchor, axis, 1f * rollSpeed);
            yield return null;
        }

        // Corrige les imprécisions de position ET de rotation
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y,
            Mathf.Round(transform.position.z)
        );
        
        // Corrige les imprécisions de rotation
        Vector3 eulerAngles = transform.eulerAngles;
        transform.eulerAngles = new Vector3(
            Mathf.Round(eulerAngles.x / 90f) * 90f,
            Mathf.Round(eulerAngles.y / 90f) * 90f,
            Mathf.Round(eulerAngles.z / 90f) * 90f
        );

        isRolling = false;
    }
}
