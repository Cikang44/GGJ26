using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    private PlayerBattery _playerBattery;
    private PlayerQuit _playerQuit;

    public CanvasGroup pauseMenu;
    public CanvasGroup quitMenu;
    public Button pauseButton;
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
        Debug.Log("ZA WARUDO!");
        _playerMovement.enabled = false;
        _playerBattery.EnterSleepMode();
        pauseButton.interactable = false;
        pauseMenu.alpha = 1;
        pauseMenu.interactable = true;
        pauseMenu.blocksRaycasts = true;
    }
    public void Resume()
    {
        Debug.Log("TOKI O TOMARE");
        _playerMovement.enabled = true;
        _playerBattery.WakeUp();
        pauseButton.interactable = true;
        pauseMenu.alpha = 0;
        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;
    }
    public void Quit()
    {
        pauseMenu.alpha = 0;
        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;

        StartCoroutine(QuitCoroutine());
    }
    private IEnumerator QuitCoroutine()
    {
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
}