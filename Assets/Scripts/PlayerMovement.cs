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

            yield return null;
        }

        isRolling = false;

        // Après le mouvement, vérifier si la face inférieure (centre bas) est sur une tuile Goal.
        // Calculer le point 'face' : position XZ du transform, avec Y au dessous du renderer (au contact du sol).
        var rend = GetComponent<Renderer>();
        if (rend != null && LevelManager.I != null)
        {
            Bounds b = rend.bounds;

            // point au centre bas (foot point)
            Vector3 footPoint = new Vector3(transform.position.x, b.min.y, transform.position.z);

            // Déterminer si le bloc est 'debout' : empreinte XZ ~ 1x1 (face) et hauteur Y supérieure
            // Ajuster tolérance si nécessaire selon les dimensions réelles du modèle
            const float sizeTarget = 1f;
            const float tol = 0.3f; // tolérance pour X et Z
            bool isFootprint1x1 = Mathf.Abs(b.size.x - sizeTarget) < tol && Mathf.Abs(b.size.z - sizeTarget) < tol;
            bool isTall = b.size.y > 1.05f; // assez haut pour être debout

            if (isFootprint1x1 && isTall)
            {
                Tile tileUnder = LevelManager.I.GetTileAtWorldPoint(footPoint);
                if (tileUnder != null && tileUnder.type == TileType.Goal)
                {
                    LevelManager.I.Win();
                }
            }
        }
    }

    IEnumerator AnimateMove(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
}