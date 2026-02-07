using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public Animator animator;
    public bool transitionOnStart = true; // Apakah perlu ada transisi saat masuk scene ini
    public float transitionTime = 1f; // Dalam detik
    private bool _isInTransition = false;
    void Awake()
    {
        // Kalo udah ada transition manager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ga usah buat lagi, pake yang lama
        }
        else
        {
            // Set instance
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transitionOnStart)
        {
            TransitionIn();
        }
    }

    public void TransitionIn()
    {
        animator.Play("Fade Out");
    }
    public void TransitionOut()
    {
        animator.Play("Fade In");
    }
    public void GoToScene(string name)
    {
        if (_isInTransition)
        { // Jangan ganti scene lagi kalo udah lagi transisi
            Debug.LogWarning("Already in transition, can't load scene!");
            return;
        }
        TransitionOut(); // Transisi dulu
        StartCoroutine(LoadScene(name, transitionTime)); // Baru load scene baru

    }
    public void GoToScene(int index)
    {
        if (_isInTransition)
        { // Jangan ganti scene lagi kalo udah lagi transisi
            Debug.LogWarning("Already in transition, can't load scene!");
            return;
        }
        TransitionOut(); // Transisi dulu
        StartCoroutine(LoadScene(index, transitionTime)); // Baru load scene baru

    }
    public void ReloadScene()
    {
        if (_isInTransition)
        { // Jangan ganti scene lagi kalo udah lagi transisi
            Debug.LogWarning("Already in transition, can't load scene!");
            return;
        }
        TransitionOut(); // Transisi dulu
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex, transitionTime)); // Baru load scene baru
    }
    public void NextScene()
    {
        if (_isInTransition)
        { // Jangan ganti scene lagi kalo udah lagi transisi
            Debug.LogWarning("Already in transition, can't load scene!");
            return;
        }
        TransitionOut(); // Transisi dulu
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1, transitionTime)); // Baru load scene baru
    }
    IEnumerator LoadScene(string name, float timeInSeconds)
    {
        if (Time.timeScale != 1) yield return new WaitForSecondsRealtime(timeInSeconds);
        else yield return new WaitForSeconds(timeInSeconds);
        Time.timeScale = 1;
        yield return SceneManager.LoadSceneAsync(name);
        TransitionIn(); // Transisi setelah loading
    }
    IEnumerator LoadScene(int index, float timeInSeconds)
    {
        if (Time.timeScale != 1) yield return new WaitForSecondsRealtime(timeInSeconds);
        else yield return new WaitForSeconds(timeInSeconds);
        Time.timeScale = 1;
        yield return SceneManager.LoadSceneAsync(index);
        TransitionIn(); // Transisi setelah loading
    }
}
