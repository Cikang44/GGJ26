using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class Door : MonoBehaviour
{
    private bool _hasOpened = false;
    private Interactable _interactable;
    public GameObject trapdoor;
    private GameObject _player;
    private Collider2D _playerCollider;
    private SpriteRenderer _trapdoorSpriteRenderer;
    private Animator _trapdoorAnimator;
    public AudioSource openSound;
    public AudioSource closeSound;
    void Start()
    {
        _interactable = GetComponent<Interactable>();
        _trapdoorSpriteRenderer = trapdoor.GetComponent<SpriteRenderer>();
        _trapdoorAnimator = trapdoor.GetComponent<Animator>();

        _player = GameObject.FindWithTag("Player");
        _playerCollider = _player.GetComponent<Collider2D>();
    }
    public void Open()
    {
        if (!_hasOpened)
        {
            Debug.Log("OPENED!");
            _trapdoorSpriteRenderer.enabled = true;
            _trapdoorAnimator.Play("Trapdoor_Open");
            _hasOpened = true;
            StartCoroutine(EnterTrapdoorCoroutine());
        }
    }
    public void Enter()
    {
        if (_hasOpened)
        {
            Debug.Log("Entered!");
        }
    }

    private IEnumerator EnterTrapdoorCoroutine()
    {
        _trapdoorSpriteRenderer.enabled = true;
        _trapdoorAnimator.Play("Trapdoor_Open");
        openSound.Play();
        yield return new WaitForSeconds(0.4f);
        _player.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        SceneTransitionManager.Instance.NextScene();
    }
}