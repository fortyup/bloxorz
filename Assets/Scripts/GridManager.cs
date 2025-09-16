using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;                // Largeur de la grille
    public int height = 5;               // Hauteur de la grille
    public GameObject tilePrefab;        // Prefab de la tile

    private GameObject[,] tiles;         // Matrice des tiles
    private TileType[,] tileTypes;       // Type de chaque tile

    public enum TileType { Normal, Hole, Switch, Goal }

    void Start()
    {
        GenerateGrid();
    }

    // Génération de la grille
    void GenerateGrid()
    {
        tiles = new GameObject[width, height];
        tileTypes = new TileType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.name = $"Tile_{x}_{z}";
                tiles[x, z] = tile;

                // Initialisation par défaut
                tileTypes[x, z] = TileType.Normal;
            }
        }
    }

    // Méthode pour définir une tile spéciale
    public void SetTileType(int x, int z, TileType type)
    {
        if (x >= 0 && x < width && z >= 0 && z < height)
        {
            tileTypes[x, z] = type;

            // Optionnel : changer visuellement la tile selon son type
            Renderer rend = tiles[x, z].GetComponent<Renderer>();
            switch (type)
            {
                case TileType.Normal:
                    rend.material.color = Color.white;
                    break;
                case TileType.Hole:
                    rend.material.color = Color.black;
                    break;
                case TileType.Switch:
                    rend.material.color = Color.yellow;
                    break;
                case TileType.Goal:
                    rend.material.color = Color.green;
                    break;
            }
        }
    }

    // Vérifier si une tile existe
    public bool IsTileValid(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < height && tiles[x, z] != null;
    }

    // Récupérer le type d’une tile
    public TileType GetTileType(int x, int z)
    {
        if (IsTileValid(x, z))
            return tileTypes[x, z];
        return TileType.Hole; // Considérer comme vide si hors grille
    }
}
