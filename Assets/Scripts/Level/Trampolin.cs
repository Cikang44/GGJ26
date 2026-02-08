using UnityEngine;

public class Trampolin : MonoBehaviour
{
    public bool canBeThrowed = true;
    public AudioSource failSound;

    private Rigidbody2D _rigidbody;
    private Collider2D[] _colliders;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _colliders = GetComponents<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (var collider in _colliders)
            {
                collider.enabled = false;
            }
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            float angle = Random.Range(60, 120) * Mathf.Deg2Rad;
            _rigidbody.AddForce(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 500);
            _rigidbody.AddTorque(Random.Range(-720, 720) * Mathf.Deg2Rad);

            if (failSound != null) failSound.Play();
        }
    }

    void Update()
    {
        if (transform.position.y < -100)
        {
            Destroy(gameObject);
        }
    }
}
