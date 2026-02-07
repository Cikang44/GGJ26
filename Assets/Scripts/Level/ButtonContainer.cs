using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable))]
public class ButtonContainer : MonoBehaviour
{
    public UnityEvent OnClick = new();
    private bool _hasButton = false;
    private Interactable _interactable;
    public Sprite withButtonSprite;
    void Start()
    {
        _interactable = GetComponent<Interactable>();
        _interactable.OnInteract.AddListener(AttachButton);
    }

    public void AttachButton()
    {
        if (CollectableButton.collectedButtonCount > 0 && !_hasButton)
        {
            CollectableButton.collectedButtonCount--;
            _interactable.OnInteract.RemoveListener(AttachButton);
            _interactable.OnInteract.AddListener(Click);
            _hasButton = true;
            GetComponent<SpriteRenderer>().sprite = withButtonSprite;
        }
    }
    public void Click()
    {
        OnClick.Invoke();
    }
}