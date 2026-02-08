using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    public Transform[] pathTransforms;
    public List<Vector3> _pathNodes = new();
    public float movingSpeed = 3f;
    public float reverseDelay = 2f;

    public AudioSource movingSound;
    public float fullVolumeDistance = 1f;
    public float noVolumeDistance = 10f;
    private Transform _playerTransform;

    public int _currentTargetIndex = 0;
    private bool _reverseDirection = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _pathNodes.Add(transform.position);
        foreach (var tf in pathTransforms)
        {
            _pathNodes.Add(tf.position);
        }
        if (movingSound != null)
        {
            movingSound.Play();
            movingSound.loop = true;
        }
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = _pathNodes[_currentTargetIndex] - transform.position;
        moveDirection.z = 0;
        if (movingSpeed * Time.deltaTime >= moveDirection.magnitude)
        {
            transform.position = _pathNodes[_currentTargetIndex];
            if (_currentTargetIndex == _pathNodes.Count - 1 && !_reverseDirection)
            {
                StartCoroutine(ReverseTimer());
                _reverseDirection = true;
            }
            if (_currentTargetIndex == 0 && _reverseDirection)
            {
                StartCoroutine(ReverseTimer());
                _reverseDirection = false;
            }
            _currentTargetIndex += _reverseDirection ? -1 : 1;
        }
        else
        {
            Vector3 moveOffset = moveDirection.normalized * (movingSpeed * Time.deltaTime);
            transform.position += moveOffset;
        }


        Vector2 playerOffset = transform.position - _playerTransform.position;
        float distanceToPlayer = playerOffset.magnitude;
        if (distanceToPlayer <= fullVolumeDistance)
        {
            movingSound.volume = 1;
        }
        else if (distanceToPlayer <= noVolumeDistance)
        {
            movingSound.volume = 1 - (noVolumeDistance - fullVolumeDistance - distanceToPlayer) / (noVolumeDistance - fullVolumeDistance);
        }
        else
        {
            movingSound.volume = 0;
        }
    }

    private IEnumerator ReverseTimer()
    {
        float originalSpeed = movingSpeed;
        movingSpeed = 0;
        yield return new WaitForSeconds(reverseDelay);
        movingSpeed = originalSpeed;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            Gizmos.DrawLine(transform.position, pathTransforms[0].position);
            for (int i = 0; i < pathTransforms.Length - 1; i++)
            {
                Gizmos.DrawLine(pathTransforms[i].position, pathTransforms[i + 1].position);
            }
        }
        else
        {
            for (int i = 0; i < _pathNodes.Count - 1; i++)
            {
                Gizmos.DrawLine(_pathNodes[i], _pathNodes[i + 1]);
            }
        }
    }
}