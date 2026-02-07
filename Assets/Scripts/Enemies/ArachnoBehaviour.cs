using UnityEngine;

public class ArachnoBehaviour : MonoBehaviour
{
    static bool _isArachnophobiaModeEnabled = false;

    public Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public static void ToggleArachnophobiaMode(bool isEnabled)
    {
        _isArachnophobiaModeEnabled = isEnabled;
        for (int i = 0; i < FindObjectsByType<ArachnoBehaviour>(FindObjectsSortMode.None).Length; i++)
        {
            FindObjectsByType<ArachnoBehaviour>(FindObjectsSortMode.None)[i].animator.SetBool("ArachnophobiaMode", isEnabled);
        }
    }
}