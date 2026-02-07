using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class Door : MonoBehaviour
{
    private bool _hasOpened = false;
    public int requiredButtonCount = 1;
    private Interactable _interactable;
    void Start()
    {
        _interactable = GetComponent<Interactable>();
    }
    public void Open()
    {
        if (!_hasOpened && CollectableButton.collectedButtonCount >= requiredButtonCount)
        {
            Debug.Log("OPENED!");
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