using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public static int lastBuildIndex;

    public float titleDelay = 2f;
    public float fadeInTime = 2f;
    public float explanationDelay = 2f;
    public float mainMenuDelay = 3f;
    public float fadeOutTime = 2f;

    public TextMeshProUGUI gameOverTitle;
    public TextMeshProUGUI gameOverExplanation;
    public Image fadeOutPanel;
    public CanvasGroup buttonContainerCanvasGroup;

    void Start()
    {
        StartCoroutine(GameOverCoroutine());
    }
    
    // Helper method to wait with skip on left click
    private IEnumerator WaitForSecondsOrSkip(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (Input.GetMouseButtonDown(0))
            {
                yield break; // Skip the wait
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
    }
    
    IEnumerator GameOverCoroutine()
    {
        float timePassed = 0;
        yield return StartCoroutine(WaitForSecondsOrSkip(titleDelay));
        while (timePassed < fadeInTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                gameOverTitle.alpha = 1f;
                break;
            }
            yield return null;
            timePassed += Time.deltaTime;
            gameOverTitle.alpha = Mathf.Clamp01(timePassed / fadeInTime);
        }

        yield return StartCoroutine(WaitForSecondsOrSkip(explanationDelay));
        timePassed = 0;
        while (timePassed < fadeInTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                gameOverExplanation.alpha = 1f;
                break;
            }
            yield return null;
            timePassed += Time.deltaTime;
            gameOverExplanation.alpha = Mathf.Clamp01(timePassed / fadeInTime);
        }

        yield return StartCoroutine(WaitForSecondsOrSkip(mainMenuDelay));
        timePassed = 0;
        while (timePassed < fadeOutTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Color color = fadeOutPanel.color;
                color.a = 1f;
                fadeOutPanel.color = color;
                break;
            }
            yield return null;
            timePassed += Time.deltaTime;
            Color color2 = fadeOutPanel.color;
            color2.a = Mathf.Clamp01(timePassed / fadeInTime);
            fadeOutPanel.color = color2;
        }

        buttonContainerCanvasGroup.interactable = true;
        yield return new WaitForSeconds(0.1f);
        timePassed = 0;
        while (timePassed < fadeOutTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                buttonContainerCanvasGroup.alpha = 1f;
                break;
            }
            yield return null;
            timePassed += Time.deltaTime;
            buttonContainerCanvasGroup.alpha = Mathf.Clamp01(timePassed / fadeInTime);
        }
        buttonContainerCanvasGroup.alpha = 1f;
    }

    public void MainMenuButtonPressed()
    {
        SceneManager.LoadScene(0);
    }

    public void RespawnButtonPressed()
    {
        SceneManager.LoadScene(lastBuildIndex);
    }
}