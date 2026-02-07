using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    public Vector2 boundsSize = new(15, 15);
    private Camera _camera;
    private Bounds _bounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = GetComponent<Camera>();
        _bounds = new Bounds(transform.position, boundsSize);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        boundsSize.x = Mathf.Max(boundsSize.x, _camera.orthographicSize * _camera.aspect * 2);
        boundsSize.y = Mathf.Max(boundsSize.y, _camera.orthographicSize * 2);
        _bounds.max = new Vector3(boundsSize.x / 2 - _camera.orthographicSize * _camera.aspect, boundsSize.y / 2 - _camera.orthographicSize, 0);
        _bounds.min = new Vector3(boundsSize.x / 2 - _camera.orthographicSize * _camera.aspect, boundsSize.y / 2 - _camera.orthographicSize, 0) * -1;
        
        transform.position = new(
            Mathf.Clamp(transform.position.x, _bounds.min.x, _bounds.max.x),
            Mathf.Clamp(transform.position.y, _bounds.min.y, _bounds.max.y),
            transform.position.z
        );
    }

    public Bounds GetBounds()
    {
        return _bounds;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // Convert the local coordinate values into world
        // coordinates for the matrix transformation.
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boundsSize);
    }
}