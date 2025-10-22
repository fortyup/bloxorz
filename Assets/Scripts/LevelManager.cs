using System.Collections.Generic;
using UnityEngine;
// Import PlayerInput type from the new Input System if available
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    public static LevelManager I { get; private set; }
    public GameObject winTextObject;
    public GameObject loseTextObject;

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int width = 7;
    [SerializeField] private int depth = 7;
    [SerializeField] private int maxGenerateAttempts = 100;
    private Vector2Int startCoord;

    private List<Tile> tiles = new List<Tile>();

    // Représente l'état du bloc via deux coordonnées de tuiles (égales si debout)
    private struct State { public Vector2Int a, b; public State(Vector2Int a, Vector2Int b) { this.a = a; this.b = b; } }

    void Awake()
    {
        I = this;
        startCoord = GetStartCoordFromPlayer();
    }

    void Start()
    {
        GenerateLevel();
        winTextObject.SetActive(false);
        loseTextObject.SetActive(false);
    }

    private void GenerateLevel()
    {
        // Probabilité qu'une case soit un trou (0 = aucun trou, 1 = que des trous)
        float holeChance = 0.15f;

        int attempt = 0;
        while (attempt++ < maxGenerateAttempts)
        {
            // Nettoyer l'éventuel niveau précédent
            ClearLevel();

            // Choisit une case goal aléatoire (une seule par niveau)
            int goalX = Random.Range(0, width);
            int goalZ = Random.Range(0, depth);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // éviter qu'une case goal ou la case de départ soient des trous
                    bool isHole = Random.value < holeChance && !(x == goalX && z == goalZ) && !(x == startCoord.x && z == startCoord.y);
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

                        // Définir le type de la tuile (utile pour la détection de victoire)
                        t.type = isGoal ? TileType.Goal : TileType.Normal;

                        if (isGoal)
                        {
                            // Marquage visuel simple : colorer si Renderer présent
                            var rend = tile.GetComponent<Renderer>();
                            if (rend != null)
                                rend.material.color = Color.green;
                        }

                        // Enregistrer la tuile (les trous ne sont pas enregistrés)
                        LevelManager.I?.RegisterTile(t);
                    }
                }
            }

            // Vérifier la solvabilité en simulant le bloc (position debout initiale)
            if (IsLevelSolvable(startCoord))
            {
                return;
            }
            else
            {
                // Sinon, tenter à nouveau (les tiles instanciés seront détruits en début de boucle)
                Debug.Log($"Attempt {attempt} produced unsolvable level (start={startCoord.x},{startCoord.y} goal={goalX},{goalZ}), regenerating...");
            }
        }

        Debug.LogWarning($"GenerateLevel: could not produce a solvable level after {maxGenerateAttempts} attempts.");
    }

    private void ClearLevel()
    {
        // Destroy all child tile GameObjects
        var children = new List<GameObject>();
        foreach (Transform child in transform)
            children.Add(child.gameObject);

        foreach (var c in children)
        {
            if (Application.isPlaying)
                Destroy(c);
            else
                DestroyImmediate(c);
        }

        tiles.Clear();
    }

    private Vector2Int GetStartCoordFromPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var t = GetTileAtWorldPoint(player.transform.position);
            if (t != null)
                return t.coord;
        }

        // Fallback raisonnable : coin (1,1) — on suppose que la scène contient cette tuile
        return new Vector2Int(1, 1);
    }

    // BFS solver pour le bloc de taille 1x2x1 (etat initial debout sur startCoord)
    private bool IsLevelSolvable(Vector2Int startCoord)
    {
        // Vérifier que la tuile de départ existe
        var startTile = GetTileAtCoord(startCoord);
        if (startTile == null) return false;

        // Etat représenté par deux coordonnées (peuvent être égales si debout)

        var q = new Queue<State>();
        var visited = new HashSet<string>();

        State Normalize(State s)
        {
            if (s.a.x > s.b.x || (s.a.x == s.b.x && s.a.y > s.b.y))
            {
                return new State(s.b, s.a);
            }
            return s;
        }

        string Key(State s) => $"{s.a.x},{s.a.y}|{s.b.x},{s.b.y}";

        State start = new State(startCoord, startCoord);
        start = Normalize(start);
        q.Enqueue(start);
        visited.Add(Key(start));

        // directions: left, right, up, down in grid coordinates (x,z)
        var dirs = new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();

            // victory: debout sur une tuile goal
            if (cur.a == cur.b)
            {
                var t = GetTileAtCoord(cur.a);
                if (t != null && t.type == TileType.Goal)
                    return true;
            }

            // Générer tous les mouvements possibles
            foreach (var d in dirs)
            {
                State next = MoveState(cur, d);
                next = Normalize(next);

                // vérifier que toutes les tuiles occupées existent
                var t1 = GetTileAtCoord(next.a);
                var t2 = GetTileAtCoord(next.b);
                if (t1 == null || t2 == null) continue;

                var key = Key(next);
                if (visited.Contains(key)) continue;
                visited.Add(key);
                q.Enqueue(next);
            }
        }

        return false;

        // fonction locale pour calculer l'état après mouvement
        State MoveState(State s, Vector2Int dir)
        {
            // shorthand
            var a = s.a;
            var b = s.b;

            // debout
            if (a == b)
            {
                int x = a.x, z = a.y;
                if (dir.x == 1 && dir.y == 0) // right
                    return new State(new Vector2Int(x + 1, z), new Vector2Int(x + 2, z));
                if (dir.x == -1 && dir.y == 0) // left
                    return new State(new Vector2Int(x - 2, z), new Vector2Int(x - 1, z));
                if (dir.x == 0 && dir.y == 1) // down
                    return new State(new Vector2Int(x, z + 1), new Vector2Int(x, z + 2));
                if (dir.x == 0 && dir.y == -1) // up
                    return new State(new Vector2Int(x, z - 2), new Vector2Int(x, z - 1));
            }

            // lying horizontally (same x)
            if (a.x == b.x && Mathf.Abs(a.y - b.y) == 1)
            {
                int x = a.x;
                int minY = Mathf.Min(a.y, b.y);
                // left
                if (dir.x == -1 && dir.y == 0)
                    return new State(new Vector2Int(x, minY - 1), new Vector2Int(x, minY - 1)); // standing
                // right
                if (dir.x == 1 && dir.y == 0)
                    return new State(new Vector2Int(x, minY + 2), new Vector2Int(x, minY + 2)); // standing
                // up
                if (dir.x == 0 && dir.y == -1)
                    return new State(new Vector2Int(x - 1, minY), new Vector2Int(x - 1, minY + 1));
                // down
                if (dir.x == 0 && dir.y == 1)
                    return new State(new Vector2Int(x + 1, minY), new Vector2Int(x + 1, minY + 1));
            }

            // lying vertically (same y)
            if (a.y == b.y && Mathf.Abs(a.x - b.x) == 1)
            {
                int z = a.y;
                int minX = Mathf.Min(a.x, b.x);
                // up
                if (dir.x == 0 && dir.y == -1)
                    return new State(new Vector2Int(minX - 1, z), new Vector2Int(minX - 1, z)); // standing
                // down
                if (dir.x == 0 && dir.y == 1)
                    return new State(new Vector2Int(minX + 2, z), new Vector2Int(minX + 2, z)); // standing
                // left
                if (dir.x == -1 && dir.y == 0)
                    return new State(new Vector2Int(minX, z - 1), new Vector2Int(minX + 1, z - 1));
                // right
                if (dir.x == 1 && dir.y == 0)
                    return new State(new Vector2Int(minX, z + 1), new Vector2Int(minX + 1, z + 1));
            }

            // fallback (invalid) — return an unreachable position
            return new State(new Vector2Int(-1000, -1000), new Vector2Int(-1000, -1000));
        }
    }

    public void RegisterTile(Tile t)
    {
        if (t == null) return;
        tiles.Add(t);
    }

    // Retourne la tuile aux coordonnées données (ou null si trou/absente)
    public Tile GetTileAtCoord(Vector2Int coord)
    {
        return tiles.Find(t => t != null && t.coord == coord);
    }

    // Vérifie si une tuile Goal se trouve sous/à l'intérieur des bounds fournis
    public bool IsGoalInBounds(Bounds bounds)
    {
        int minX = Mathf.FloorToInt(bounds.min.x);
        int maxX = Mathf.FloorToInt(bounds.max.x);
        int minZ = Mathf.FloorToInt(bounds.min.z);
        int maxZ = Mathf.FloorToInt(bounds.max.z);

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                var t = GetTileAtCoord(new Vector2Int(x, z));
                if (t != null && t.type == TileType.Goal)
                    return true;
            }
        }

        return false;
    }

    // Retourne la tuile située à la position monde donnée (considérant la grille 1x1)
    // Le point monde sera converti en coordonnées entières de tuile via Floor.
    public Tile GetTileAtWorldPoint(Vector3 worldPoint)
    {
        int x = Mathf.FloorToInt(worldPoint.x + 0.5f);
        int z = Mathf.FloorToInt(worldPoint.z + 0.5f);
        return GetTileAtCoord(new Vector2Int(x, z));
    }

    // Appelée quand le joueur atteint l'objectif
    public void Win()
    {
        // Bloquer entrées du joueur : désactiver les scripts de contrôle s'il existe un GameObject tagué "Player"
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            foreach (var mb in player.GetComponents<MonoBehaviour>())
            {
                if (mb == this) continue; // précaution
                mb.enabled = false;
            }

            var rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        // Désactiver les composants "PlayerInput" (nouveau input system) s'ils existent
        // Utiliser la nouvelle API typée pour cibler directement PlayerInput (plus sûr que comparer les noms)
        try
        {
            foreach (var pi in Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            {
                if (pi != null)
                    pi.enabled = false;
            }
        }
        catch (System.Exception)
        {
            // Si PlayerInput n'est pas présent/accessible dans ce projet, fall back to reflection-based approach
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb.GetType().Name == "PlayerInput")
                    mb.enabled = false;
            }
        }

        // Afficher le curseur et le déverrouiller
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Optionnel : stopper le temps de jeu pour geler tout (UI doit utiliser unscaled time si animations)
        Time.timeScale = 0f;
        winTextObject.gameObject.SetActive(true);
    }

    // Appelée quand le joueur perd (block partiellement ou totalement dans le vide)
    public void Lose()
    {
        // Afficher le texte de défaite
        loseTextObject.SetActive(true);

        // Optionnel : stopper le temps de jeu pour geler tout (UI doit utiliser unscaled time si animations)
        Time.timeScale = 0f;
    }
}
