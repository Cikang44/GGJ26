using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private PlayerBattery _playerBattery;
    private HealthBehaviour _playerHealth;

    public Sprite batteryHighSprite;
    public Sprite batteryMediumSprite;
    public Sprite batteryLowSprite;
    public Sprite batteryEmptySprite;

    public Image batteryImage;
    public TextMeshProUGUI batteryPercentageText;

    public Sprite heartGoodSprite;
    public Sprite heartBrokenSprite;
    public RectTransform heartsParent;
    private Image[] _hearts;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) Debug.LogError("Player not found");
        else
        {
            _playerBattery = player.GetComponent<PlayerBattery>();
            _playerHealth = player.GetComponent<HealthBehaviour>();
        }
        _hearts = heartsParent.GetComponentsInChildren<Image>();
        _playerHealth.OnHealed.AddListener(UpdateHeartFromHeal);
        _playerHealth.OnDamaged.AddListener(UpdateHeartFromDamage);
    }

    void Update()
    {
        UpdateBattery();
    }

    private void UpdateHeartFromHeal(int heal)
    {
        for (int i = Mathf.Max(0, _playerHealth.health - heal); i < _playerHealth.health && i < _hearts.Length; i++)
        {
            _hearts[i].sprite = heartGoodSprite;
        }
    }
    private void UpdateHeartFromDamage(int damage)
    {
        for (int i = Mathf.Min(_hearts.Length - 1, _playerHealth.health + damage - 1); i >= _playerHealth.health && i >= 0; i--)
        {
            _hearts[i].sprite = heartBrokenSprite;
        }
    }

    private void UpdateBattery()
    {
        batteryPercentageText.text = Mathf.CeilToInt(_playerBattery.batteryPercentage).ToString() + "%";
        if (_playerBattery.batteryPercentage > 2f / 3f)
        {
            batteryImage.sprite = batteryHighSprite;
        }
        else if (_playerBattery.batteryPercentage > 1f / 3f)
        {
            batteryImage.sprite = batteryMediumSprite;
        }
        else if (_playerBattery.batteryPercentage > 0)
        {
            batteryImage.sprite = batteryLowSprite;
        }
        else
        {
            batteryImage.sprite = batteryEmptySprite;
        }
    }
}