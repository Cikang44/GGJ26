using UnityEngine;

public class Bobbing : MonoBehaviour
{
    [SerializeField] private float bobbingHeight = 0.5f;
    [SerializeField] private float bobbingSpeed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}