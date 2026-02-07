using UnityEngine;

public class ArachnoBehaviour : MonoBehaviour
{
    static bool _isArachnophobiaModeEnabled = false;

    public Sprite spiderSprite;
    public Sprite catSprite;

    public SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public static void ToggleArachnophobiaMode(bool isEnabled)
    {
        _isArachnophobiaModeEnabled = isEnabled;
        for (int i = 0; i < FindObjectsByType<ArachnoBehaviour>(FindObjectsSortMode.None).Length; i++)
        {
            FindObjectsByType<ArachnoBehaviour>(FindObjectsSortMode.None)[i].spriteRenderer.sprite = isEnabled ?
                FindObjectsByType<ArachnoBehaviour>(FindObjectsSortMode.None)[i].catSprite :
                FindObjectsByType<ArachnoBehaviour>(FindObjectsSortMode.None)[i].spiderSprite;
        }
    }
}