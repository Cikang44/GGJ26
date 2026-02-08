using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    private PlayerBattery _playerBattery;
    private PlayerQuit _playerQuit;
    private Collider2D _playerCollider;
    private CameraMovement _cameraMovement;

    public CanvasGroup pauseMenu;
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
            _playerCollider = player.GetComponent<Collider2D>();
        }
        _cameraMovement = Camera.main.GetComponent<CameraMovement>();
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
        _playerMovement.boostHeight = 100;
        _playerMovement.Boost(_playerMovement.transform.position);
        _playerCollider.enabled = false;
        if (_cameraMovement != null) _cameraMovement.enabled = false;

        yield return new WaitForSeconds(quitDelay);

        SceneManager.LoadScene("Game Over By Quitting");
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