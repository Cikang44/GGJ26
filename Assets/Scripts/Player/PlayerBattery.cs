using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerBattery : MonoBehaviour
{
    [Range(0, 100)] public float batteryPercentage = 100f;
    [Min(0)] public float batteryLifetimeInSecond = 300f;
    [Range(0, 100)] public float lowBatteryPercentage = 10f;
    public bool isDrainingBattery = true;
    private bool _isLowBattery = false;
    public UnityEvent OnZeroPercent = new();
    public UnityEvent OnLowBattery = new();
    void Start()
    {
        GameOverScreen.lastBuildIndex = SceneManager.GetActiveScene().buildIndex;
        OnZeroPercent.AddListener(() => SceneManager.LoadScene("Game Over By No Battery"));
        GetComponent<HealthBehaviour>().OnDeath.AddListener(() => SceneManager.LoadScene("Game Over By Dying"));
    }
    void Update()
    {
        if (isDrainingBattery)
        {
            batteryPercentage -= Time.deltaTime * 100f / batteryLifetimeInSecond;
            if (!_isLowBattery && batteryPercentage <= lowBatteryPercentage)
            {
                _isLowBattery = true;
                OnLowBattery.Invoke();
            }
            if (batteryPercentage <= 0)
            {
                batteryPercentage = 0;
                OnZeroPercent.Invoke();
                isDrainingBattery = false;
            }
        }
    }
    public void ChargeBattery(float percentage)
    {
        batteryPercentage += percentage;
        if (batteryPercentage > 100f) batteryPercentage = 100f;
    }

    public void EnterSleepMode()
    {
        isDrainingBattery = false;
    }

    public void WakeUp()
    {
        isDrainingBattery = true;
    }
}