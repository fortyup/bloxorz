using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager I { get; private set; }

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int width = 7;
    [SerializeField] private int depth = 7;

    private List<Tile> tiles = new List<Tile>();

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        // Probabilité qu'une case soit un trou (0 = aucun trou, 1 = que des trous)
        float holeChance = 0.15f;

        // Choisit une case goal aléatoire (une seule par niveau)
        int goalX = Random.Range(0, width);
        int goalZ = Random.Range(0, depth);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // éviter qu'une case goal soit un trou
                bool isHole = Random.value < holeChance && !(x == goalX && z == goalZ);
                if (isHole)
                {
                    // Pas d'instance = trou
                    continue;
                }

                Vector3 pos = new Vector3(x, 0, z);

                if (tilePrefab == null)
                    continue;

                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

                bool isGoal = (x == goalX && z == goalZ);

                // Nom lisible dans la hiérarchie (marque la case goal)
                tile.name = isGoal ? $"Tile_{x}_{z}_GOAL" : $"Tile_{x}_{z}";

                // Initialiser les coordonnées si le script Tile est dessus
                Tile t = tile.GetComponent<Tile>();
                if (t != null)
                {
                    t.coord = new Vector2Int(x, z);

                    if (isGoal)
                    {
                        // Tenter de marquer la tuile comme goal via reflection (compatible avec plusieurs signatures possibles)
                        var type = t.GetType();

                        // champs publics communs
                        var field = type.GetField("isGoal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                 ?? type.GetField("IsGoal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                 ?? type.GetField("goal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                 ?? type.GetField("Goal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (field != null)
                        {
                            field.SetValue(t, true);
                        }
                        else
                        {
                            // propriétés publiques
                            var prop = type.GetProperty("isGoal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                   ?? type.GetProperty("IsGoal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                   ?? type.GetProperty("goal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                   ?? type.GetProperty("Goal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            if (prop != null && prop.CanWrite)
                                prop.SetValue(t, true, null);
                            else
                            {
                                // méthode SetGoal(bool) si existante
                                var method = type.GetMethod("SetGoal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                if (method != null)
                                    method.Invoke(t, new object[] { true });
                            }
                        }

                        // Optionnel : donner un retour visuel en couleur (si un Renderer est présent)
                        var rend = tile.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            // utilise material (créera une instance au runtime)
                            rend.material.color = Color.green;
                        }
                    }
                }

                // Enregistrer la tuile (les trous ne sont pas enregistrés)
                LevelManager.I?.RegisterTile(t);
            }
        }
    }

    public void RegisterTile(Tile t)
    {
        if (t == null) return;
        tiles.Add(t);
    }
}
