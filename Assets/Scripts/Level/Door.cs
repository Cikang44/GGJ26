using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class Door : MonoBehaviour
{
    private bool _hasOpened = false;
    private Interactable _interactable;
    public GameObject trapdoor;
    private SpriteRenderer _trapdoorSpriteRenderer;
    private Animator _trapdoorAnimator;
    void Start()
    {
        _interactable = GetComponent<Interactable>();
        _trapdoorSpriteRenderer = trapdoor.GetComponent<SpriteRenderer>();
        _trapdoorAnimator = trapdoor.GetComponent<Animator>();
    }
    public void Open()
    {
        if (!_hasOpened)
        {
            Debug.Log("OPENED!");
            _trapdoorSpriteRenderer.enabled = true;
            _trapdoorAnimator.Play("Trapdoor_Open");
            _interactable.OnInteract.AddListener(Enter);
            _hasOpened = true;
        }
    }
    public void Enter()
    {
        if (_hasOpened)
        {
            Debug.Log("Entered!");
        }
    }
}