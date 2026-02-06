using UnityEngine;
using UnityEngine.Events;

public class PlayerBattery : MonoBehaviour
{
    [Range(0, 100)] public float batteryPercentage = 100f;
    [Min(0)] public float batteryLifetimeInSecond = 300f;
    [Range(0, 100)] public float lowBatteryPercentage = 10f;
    public bool isDrainingBattery = true;
    private bool _isLowBattery = false;
    public UnityEvent OnZeroPercent;
    public UnityEvent OnLowBattery;
    void Update()
    {
        if (isDrainingBattery)
        {
            batteryPercentage -= Time.deltaTime * 100f / batteryLifetimeInSecond;
            Debug.Log(batteryPercentage);
            if (!_isLowBattery && batteryPercentage <= lowBatteryPercentage)
            {
                _isLowBattery = true;
                OnLowBattery.Invoke();
            }
        }
    }
}