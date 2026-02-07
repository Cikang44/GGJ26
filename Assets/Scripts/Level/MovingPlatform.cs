using UnityEngine;
using UnityEditor;
using System.Linq;

public class MovingPlatform : MonoBehaviour
{
    public Transform[] paths;
    public float movingSpeed = 3f;

    private int _currentTargetIndex = 0;
    private bool _reverseDirection = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        paths.Prepend(transform);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = paths[_currentTargetIndex].position - transform.position;
        moveDirection.z = 0;
        Debug.Log(movingSpeed * Time.deltaTime + ", " + moveDirection.magnitude);
        if (movingSpeed * Time.deltaTime >= moveDirection.magnitude)
        {
            transform.position = paths[_currentTargetIndex].position;
            if (_currentTargetIndex == paths.Length - 1) _reverseDirection = true;
            if (_currentTargetIndex == 0) _reverseDirection = false;
            _currentTargetIndex += _reverseDirection ? -1 : 1;
        }
        else
        {
            Vector3 moveOffset = moveDirection.normalized * (movingSpeed * Time.deltaTime);
            transform.position += moveOffset;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, paths[0].position);
        for (int i = 0; i < paths.Length - 1; i++)
        {
            Gizmos.DrawLine(paths[i].position, paths[i + 1].position);
        }
    }
}