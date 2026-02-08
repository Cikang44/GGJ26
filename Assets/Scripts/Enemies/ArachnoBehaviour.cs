using UnityEngine;

public class ArachnoBehaviour : MonoBehaviour
{
    static bool _isArachnophobiaModeEnabled = false;

    public Animator animator;
    public Enemy enemy;
    public EnemySimpleMovement enemySimpleMovement;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
        enemySimpleMovement = GetComponent<EnemySimpleMovement>();
    }

    public static void ToggleArachnophobiaMode(bool isEnabled)
    {
        _isArachnophobiaModeEnabled = isEnabled;
        var arachnoBehaviours = FindObjectsByType<ArachnoBehaviour>(FindObjectsSortMode.None);
        for (int i = 0; i < arachnoBehaviours.Length; i++)
        {
            arachnoBehaviours[i].animator.SetBool("ArachnophobiaMode", isEnabled);
            if (isEnabled)
            {
                arachnoBehaviours[i].enemySimpleMovement.moveSpeed = 5f;
                arachnoBehaviours[i].enemy.damage = 1;
            }
            else
            {
                arachnoBehaviours[i].enemySimpleMovement.moveSpeed = 2f;
                arachnoBehaviours[i].enemy.damage = 0;
            }
        }
    }
}