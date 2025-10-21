using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private float rollSpeed = 1f; // Vitesse d'animation
    private bool isRolling = false;
    private Vector3 moveDir;

    public void OnMove(InputValue movementValue)
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

        // Calculer les dimensions réelles du rectangle en fonction de son orientation actuelle
        Bounds bounds = GetComponent<Renderer>().bounds;
        float widthX = bounds.size.x; // Dimension réelle sur l'axe X
        float heightY = bounds.size.y; // Dimension réelle sur l'axe Y (hauteur)
        float depthZ = bounds.size.z; // Dimension réelle sur l'axe Z

        Debug.Log($"Dimensions réelles - X: {widthX}, Y: {heightY}, Z: {depthZ}");

        // Déterminer la distance de déplacement et l'axe de rotation
        Vector3 rotationAxis;
        Vector3 anchorOffset;

        // Calculer le pivot en fonction de la direction du mouvement
        if (Mathf.Abs(dir.x) > 0) // Déplacement gauche/droite
        {
            rotationAxis = Vector3.forward * -Mathf.Sign(dir.x);
            // Le pivot est à l'arête latérale : décalage en X et descente en Y
            anchorOffset = new Vector3(widthX / 2 * Mathf.Sign(dir.x), -heightY / 2, 0);
        }
        else // Déplacement avant/arrière
        {
            rotationAxis = Vector3.right * Mathf.Sign(dir.z);
            // Le pivot est à l'arête avant/arrière : décalage en Z et descente en Y
            anchorOffset = new Vector3(0, -heightY / 2, depthZ / 2 * Mathf.Sign(dir.z));
        }

        // Point de pivot (arête au sol)
        Vector3 anchor = transform.position + anchorOffset;

        // Animation de rotation
        float remainingAngle = 90f;

        while (remainingAngle > 0)
        {
            float deltaAngle = Mathf.Min(remainingAngle, rollSpeed * 90f);
            transform.RotateAround(anchor, rotationAxis, deltaAngle);

            remainingAngle -= deltaAngle;
            
            // Afficher les coordonnées du player à chaque frame
            Debug.Log($"Position du player: {transform.position}");

            yield return null;
        }

        isRolling = false;
    }
}