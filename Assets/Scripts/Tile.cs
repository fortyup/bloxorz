using UnityEngine;

public enum TileType { Normal, Hole, Goal, Switch, Fragile, Bridge }

public class Tile : MonoBehaviour
{
    public Vector2Int coord;
    public TileType type;
    public int switchId = -1; // si type == Switch

    // Optionnel: visuel, collider etc.
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Couleurs attitrées par type
    private Color ColorForType(TileType t)
    {
        switch (t)
        {
            case TileType.Hole:    return new Color(0.1f, 0.1f, 0.12f); // sombre
            case TileType.Goal:    return new Color(0.18f, 0.8f, 0.28f); // vert
            case TileType.Switch:  return new Color(0.95f, 0.85f, 0.12f); // jaune
            case TileType.Fragile: return new Color(1f, 0.5f, 0.0f); // orange
            case TileType.Bridge:  return new Color(0.55f, 0.35f, 0.2f); // marron
            case TileType.Normal:
            default:               return new Color(0.85f, 0.85f, 0.85f); // gris clair
        }
    }

    private void ApplyColor(Color c)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = c;
            return;
        }

        if (meshRenderer != null)
        {
            // Use MaterialPropertyBlock to change color without instantiating or modifying the material asset.
            // This is safe for prefabs and works in edit mode.
            try
            {
                var mpb = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(mpb);
                // Common color property names: _Color (Standard), _BaseColor (URP/HDRP)
                mpb.SetColor("_Color", c);
                mpb.SetColor("_BaseColor", c);
                meshRenderer.SetPropertyBlock(mpb);
            }
            catch (System.Exception)
            {
                // As a last resort (very old/edge cases), try to set sharedMaterial if available and not null.
                if (meshRenderer.sharedMaterial != null)
                {
                    meshRenderer.sharedMaterial.color = c;
                }
            }
        }
    }

    public void UpdateVisuals()
    {
        // Récupérer automatiquement les composants si non fournis
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponent<Renderer>();

        ApplyColor(ColorForType(type));
    }

    private void Awake()
    {
        UpdateVisuals();
    }

    private void OnValidate()
    {
        // Mise à jour dans l'éditeur quand on change le type
        UpdateVisuals();
    }
    
    // Public wrapper for external callers (LevelManager, editors) to refresh visuals
    public void RefreshVisuals()
    {
        UpdateVisuals();
    }
    
}
