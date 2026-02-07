using System.Collections;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    private Renderer _renderer;
    private Collider2D _collider;

    public bool canAppearAgain = true;
    public float disappearDelay = 3f;
    public float appearDelay = 3f;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
    }

    public void Disappear()
    {
        StartCoroutine(DisappearCoroutine());
    }

    private IEnumerator DisappearCoroutine()
    {
        yield return new WaitForSeconds(disappearDelay);
        if (_renderer != null) _renderer.enabled = false;
        if (_collider != null) _collider.enabled = false;
        if (canAppearAgain) Appear();
    }

    public void Appear()
    {
        StartCoroutine(AppearCoroutine());
    }

    private IEnumerator AppearCoroutine()
    {
        yield return new WaitForSeconds(appearDelay);
        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
    }
}