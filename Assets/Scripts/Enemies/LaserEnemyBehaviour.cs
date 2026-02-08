using UnityEngine;

public class LaserEnemyBehaviour : MonoBehaviour
{
    public GameObject laserPrefab;
    public float laserInterval = 1f;
    public float initialDelay = 1f / 6 * 5f;
    private float _laserTimer;

    private void Start()
    {
        _laserTimer = -initialDelay + laserInterval;
    }

    private void FixedUpdate()
    {
        _laserTimer += Time.fixedDeltaTime;
        if (_laserTimer >= laserInterval)
        {
            ShootLaser();
            _laserTimer = 0f;
        }
    }

    private void ShootLaser()
    {
        Instantiate(laserPrefab, transform.position, transform.localScale.x > 0 ? Quaternion.Euler(0, 0, 180) : Quaternion.identity);
    }
}