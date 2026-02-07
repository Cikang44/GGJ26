using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    private PlayerBattery _playerBattery;
    private PlayerQuit _playerQuit;

    public CanvasGroup pauseMenu;
    public CanvasGroup quitMenu;
    public Button pauseButton;
    public Light2D[] globalLights;
    public float pauseDelay = 1;
    public float resumeDelay = 1;
    public float quitDelay = 2;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) Debug.LogError("Player not found");
        else
        {
            _playerMovement = player.GetComponent<PlayerMovement>();
            _playerBattery = player.GetComponent<PlayerBattery>();
            _playerQuit = player.GetComponent<PlayerQuit>();
        }
    }

    public void Pause()
    {
        StartCoroutine(PauseCoroutine());
    }
    private IEnumerator PauseCoroutine()
    {
        pauseButton.interactable = false;
        _playerMovement.enabled = false;
        _playerBattery.EnterSleepMode();

        yield return new WaitForSeconds(pauseDelay);

        pauseMenu.alpha = 1;
        pauseMenu.interactable = true;
        pauseMenu.blocksRaycasts = true;
    }
    public void Resume()
    {
        StartCoroutine(ResumeCoroutine());
    }
    private IEnumerator ResumeCoroutine()
    {
        pauseMenu.alpha = 0;
        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;

        _playerBattery.WakeUp();
        yield return new WaitForSeconds(resumeDelay);

        _playerMovement.enabled = true;
        pauseButton.interactable = true;
    }
    public void Quit()
    {
        StartCoroutine(QuitCoroutine());
    }
    private IEnumerator QuitCoroutine()
    {
        pauseMenu.alpha = 0;
        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;
        _playerQuit.Quit();

        yield return new WaitForSeconds(quitDelay);

        quitMenu.alpha = 1;
        quitMenu.interactable = true;
        quitMenu.blocksRaycasts = true;

        while (true)
        {
            yield return null;
            if (Input.anyKeyDown)
            {
                Application.Quit();
                Debug.Log("QUIT APPLICATION");
            }
        }
    }

    public void ConfigureArachnophobiaMode(bool isEnabled)
    {
        ArachnoBehaviour.ToggleArachnophobiaMode(isEnabled);
    }

    public void ToggleBrightness(bool on)
    {
        foreach (var light in globalLights)
        {
            light.enabled = on;
        }

        Screen.brightness = on ? 1f : 0.2f;
    }
}