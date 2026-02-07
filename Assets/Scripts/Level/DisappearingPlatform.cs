using System;
using System.Collections;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Collider2D _collider;

    public bool canAppearAgain = true;
    public float disappearDelay = 3f;
    public float appearDelay = 3f;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    public void Disappear()
    {
        StartCoroutine(DisappearCoroutine());
    }

    private IEnumerator DisappearCoroutine()
    {
        float timePassed = 0f;
        while (timePassed < disappearDelay)
        {
            yield return null;
            timePassed += Time.deltaTime;
            Color color = _renderer.color;
            color.a = Mathf.Cos(timePassed * Mathf.PI * 2) / 3f + 0.6667f;
            _renderer.color = color;
        }
        if (_collider != null) _collider.enabled = false;
        timePassed = 0f;
        while (timePassed < 0.25f)
        {
            yield return null;
            timePassed += Time.deltaTime;
            Color color = _renderer.color;
            color.a = (0.25f - timePassed) / 0.25f;
            _renderer.color = color;
        }
        if (_renderer != null) _renderer.enabled = false;
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
        float timePassed = 0f;
        while (timePassed < 0.5f)
        {
            yield return null;
            timePassed += Time.deltaTime;
            Color color = _renderer.color;
            color.a = timePassed / 0.5f;
            _renderer.color = color;
        }
        if (_collider != null) _collider.enabled = true;
    }
}