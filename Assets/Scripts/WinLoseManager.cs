using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Win/Lose manager centralisé. Attache ce script à un GameObject de la scène (ex: GameManager).
// Configure les GameObjects UI (winUI/loseUI) et les UnityEvents dans l'inspecteur.
public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager I { get; private set; }

    [Header("UI")]
    public GameObject winUI;
    public GameObject loseUI;

    [Header("Behavior")]
    public float delayBeforeAction = 1.2f;
    public bool disableLevelManagerOnEnd = true;

    [Header("Auto scene handling (optionnel)")]
    public bool autoLoadNextScene = false;
    public int nextSceneIndexOffset = 1; // charge la scène actuelle + offset
    public bool autoReloadOnLose = false;

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    bool ended = false;

    void Awake()
    {
        if (I == null) I = this;
        else if (I != this) Destroy(gameObject);
    }

    // Méthode publique à appeler quand le joueur gagne
    public void TriggerWin()
    {
        if (ended) return;
        ended = true;
        StartCoroutine(HandleWin());
    }

    // Méthode publique à appeler quand le joueur perd
    public void TriggerLose()
    {
        if (ended) return;
        ended = true;
        StartCoroutine(HandleLose());
    }

    IEnumerator HandleWin()
    {
        // Désactive LevelManager si demandé
        if (disableLevelManagerOnEnd && LevelManager.I != null)
            LevelManager.I.enabled = false;

        // Affiche UI
        if (winUI != null) winUI.SetActive(true);

        // Déclenche event pour jouer sons/particules via l'éditeur
        onWin?.Invoke();

        // Attendre un petit délai pour animations
        yield return new WaitForSeconds(delayBeforeAction);

        if (autoLoadNextScene)
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + nextSceneIndexOffset;
            // S'assure que l'index est valide (si non, ne fait rien)
            if (nextIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextIndex);
        }
    }

    IEnumerator HandleLose()
    {
        if (disableLevelManagerOnEnd && LevelManager.I != null)
            LevelManager.I.enabled = false;

        if (loseUI != null) loseUI.SetActive(true);

        onLose?.Invoke();

        yield return new WaitForSeconds(delayBeforeAction);

        if (autoReloadOnLose)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Méthodes utilitaires optionnelles
    public void NextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + nextSceneIndexOffset;
        if (nextIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Reset du flag (utile si tu réinitialises la scène manuellement depuis l'éditeur)
    public void ResetState()
    {
        ended = false;
        if (winUI != null) winUI.SetActive(false);
        if (loseUI != null) loseUI.SetActive(false);
        if (disableLevelManagerOnEnd && LevelManager.I != null)
            LevelManager.I.enabled = true;
    }
}
