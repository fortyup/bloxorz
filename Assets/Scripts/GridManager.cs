using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;                // Largeur de la grille
    public int height = 5;               // Hauteur de la grille
    public GameObject tilePrefab;        // Prefab de la tile

    [Header("Random Generation Settings")]
    public bool useRandomGeneration = true;     // Active la génération aléatoire
    public int randomSeed = -1;                 // Seed pour la génération (-1 pour seed aléatoire)
    
    [Header("Tile Probabilities (0-100)")]
    [Range(0, 100)] public float normalTileProbability = 85f;
    [Range(0, 100)] public float holeProbability = 15f;
    
    [Header("Generation Constraints")]
    public int goalTiles = 1;                   // Nombre de tiles Goal (fixé à 1)
    public int minNormalTiles = 9;              // Nombre minimum de tiles normales pour la jouabilité

    private GameObject[,] tiles;         // Matrice des tiles
    private TileType[,] tileTypes;       // Type de chaque tile

    public enum TileType { Normal, Hole, Goal }

    void Start()
    {
        GenerateGrid();
    }

    // Génération de la grille
    void GenerateGrid()
    {
        // Initialiser la seed si spécifiée
        if (randomSeed != -1)
        {
            Random.InitState(randomSeed);
        }

        tiles = new GameObject[width, height];
        tileTypes = new TileType[width, height];

        // Première passe : créer tous les GameObjects et initialiser comme Normal
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.name = $"Tile_{x}_{z}";
                tiles[x, z] = tile;
                tileTypes[x, z] = TileType.Normal;
            }
        }

        // Générer aléatoirement si activé
        if (useRandomGeneration)
        {
            GenerateRandomTileTypes();
        }

        // Appliquer les couleurs selon les types
        ApplyTileVisuals();
    }

    // Génération aléatoire des types de tiles
    void GenerateRandomTileTypes()
    {
        int totalTiles = width * height;
        
        // Normaliser les probabilités pour qu'elles totalisent 100%
        float totalProbability = normalTileProbability + holeProbability;
        float normalizedNormal = normalTileProbability / totalProbability * 100f;
        float normalizedHole = holeProbability / totalProbability * 100f;

        // Génération aléatoire pour chaque tile (sans Goal pour l'instant)
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float randomValue = Random.Range(0f, 100f);
                TileType newType = TileType.Normal;

                if (randomValue < normalizedHole)
                {
                    newType = TileType.Hole;
                }
                else
                {
                    newType = TileType.Normal;
                }

                tileTypes[x, z] = newType;
            }
        }

        // Placer exactement une tile Goal à une position aléatoire
        PlaceGoalTile();

        // Assurer les contraintes minimales
        EnsurePlayabilityConstraints();
    }

    // Placer une tile Goal à une position aléatoire
    void PlaceGoalTile()
    {
        var availablePositions = new System.Collections.Generic.List<Vector2Int>();
        
        // Trouver toutes les positions disponibles (non-Hole)
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (tileTypes[x, z] == TileType.Normal)
                {
                    availablePositions.Add(new Vector2Int(x, z));
                }
            }
        }

        // Placer la tile Goal à une position aléatoire
        if (availablePositions.Count > 0)
        {
            Vector2Int randomPos = availablePositions[Random.Range(0, availablePositions.Count)];
            tileTypes[randomPos.x, randomPos.y] = TileType.Goal;
        }
        else
        {
            // Si toutes les tiles sont des trous, forcer une tile Goal en (0,0)
            tileTypes[0, 0] = TileType.Goal;
        }
    }

    // S'assurer que la grille respecte les contraintes de jouabilité
    void EnsurePlayabilityConstraints()
    {
        int goalCount = CountTileType(TileType.Goal);
        int normalCount = CountTileType(TileType.Normal);

        // S'assurer qu'il y a exactement une tile Goal
        if (goalCount == 0)
        {
            // Convertir une tile normale en Goal
            ConvertRandomTileToType(TileType.Goal, TileType.Normal);
        }
        else if (goalCount > 1)
        {
            // Convertir les Goals supplémentaires en tiles normales
            while (CountTileType(TileType.Goal) > 1)
            {
                ConvertRandomTileOfType(TileType.Goal, TileType.Normal);
            }
        }

        // Assurer le nombre minimum de tiles normales
        while (CountTileType(TileType.Normal) < minNormalTiles)
        {
            ConvertRandomTileToType(TileType.Normal, TileType.Hole);
        }
    }

    // Compter le nombre de tiles d'un type donné
    int CountTileType(TileType targetType)
    {
        int count = 0;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (tileTypes[x, z] == targetType)
                    count++;
            }
        }
        return count;
    }

    // Convertir une tile aléatoire vers un type spécifique
    void ConvertRandomTileToType(TileType newType, params TileType[] fromTypes)
    {
        var availablePositions = new System.Collections.Generic.List<Vector2Int>();
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (System.Array.Exists(fromTypes, type => type == tileTypes[x, z]))
                {
                    availablePositions.Add(new Vector2Int(x, z));
                }
            }
        }

        if (availablePositions.Count > 0)
        {
            Vector2Int randomPos = availablePositions[Random.Range(0, availablePositions.Count)];
            tileTypes[randomPos.x, randomPos.y] = newType;
        }
    }

    // Convertir une tile aléatoire d'un type spécifique vers un autre type
    void ConvertRandomTileOfType(TileType fromType, TileType toType)
    {
        var availablePositions = new System.Collections.Generic.List<Vector2Int>();
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (tileTypes[x, z] == fromType)
                {
                    availablePositions.Add(new Vector2Int(x, z));
                }
            }
        }

        if (availablePositions.Count > 0)
        {
            Vector2Int randomPos = availablePositions[Random.Range(0, availablePositions.Count)];
            tileTypes[randomPos.x, randomPos.y] = toType;
        }
    }

    // Appliquer les visuels selon les types de tiles
    void ApplyTileVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                SetTileVisual(x, z, tileTypes[x, z]);
            }
        }
    }

    // Définir le visuel d'une tile selon son type
    void SetTileVisual(int x, int z, TileType type)
    {
        if (tiles[x, z] != null)
        {
            Renderer rend = tiles[x, z].GetComponent<Renderer>();
            switch (type)
            {
                case TileType.Normal:
                    rend.material.color = Color.white;
                    tiles[x, z].SetActive(true);
                    break;
                case TileType.Hole:
                    // Désactiver la tile pour qu'elle ne s'affiche pas
                    tiles[x, z].SetActive(false);
                    break;
                case TileType.Goal:
                    rend.material.color = Color.green;
                    tiles[x, z].SetActive(true);
                    break;
            }
        }
    }

    // Méthode pour définir une tile spéciale (conservée pour compatibilité)
    public void SetTileType(int x, int z, TileType type)
    {
        if (x >= 0 && x < width && z >= 0 && z < height)
        {
            tileTypes[x, z] = type;
            SetTileVisual(x, z, type);
        }
    }

    // Régénérer la grille avec une nouvelle seed aléatoire
    public void RegenerateRandomGrid()
    {
        // Détruire les tiles existantes
        DestroyExistingTiles();
        
        // Générer une nouvelle seed aléatoire
        randomSeed = Random.Range(0, int.MaxValue);
        
        // Régénérer la grille
        GenerateGrid();
    }

    // Régénérer la grille avec une seed spécifique
    public void RegenerateGridWithSeed(int seed)
    {
        // Détruire les tiles existantes
        DestroyExistingTiles();
        
        randomSeed = seed;
        GenerateGrid();
    }

    // Détruire toutes les tiles existantes
    void DestroyExistingTiles()
    {
        if (tiles != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (tiles[x, z] != null)
                    {
                        DestroyImmediate(tiles[x, z]);
                    }
                }
            }
        }
    }

    // Obtenir la seed actuelle
    public int GetCurrentSeed()
    {
        return randomSeed;
    }

    // Afficher des informations sur la grille générée
    public void PrintGridInfo()
    {
        int normalCount = CountTileType(TileType.Normal);
        int holeCount = CountTileType(TileType.Hole);
        int goalCount = CountTileType(TileType.Goal);
        
        Debug.Log($"Grille générée avec seed: {randomSeed}");
        Debug.Log($"Normal: {normalCount}, Holes: {holeCount}, Goals: {goalCount}");
    }

    // Vérifier si une tile existe et est active (pas un trou)
    public bool IsTileValid(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < height && 
               tiles[x, z] != null && tileTypes[x, z] != TileType.Hole;
    }

    // Récupérer le type d’une tile
    public TileType GetTileType(int x, int z)
    {
        if (IsTileValid(x, z))
            return tileTypes[x, z];
        return TileType.Hole; // Considérer comme vide si hors grille
    }
}
