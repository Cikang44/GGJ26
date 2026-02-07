using UnityEngine;

public class LaserEnemyBehaviour : MonoBehaviour
{
    public GameObject laserPrefab;
    public float laserInterval = 3f;
    private float _laserTimer;

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
        Instantiate(laserPrefab, transform.position, Quaternion.identity);
    }
}