using UnityEngine;

public class LaserBehaviour : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 5f;
    private float _lifeTimer = 0f;

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, 0.1f);
        if ((bool)(hit.collider?.CompareTag("Ground")))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponent<HealthBehaviour>();
        if (health != null && other.CompareTag("Player"))
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }   
}