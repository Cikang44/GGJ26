using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class CollectableButton : MonoBehaviour
{
    public static int collectedButtonCount = 0;
    private Interactable _interactable;
    void Start()
    {
        collectedButtonCount = 0;
        _interactable = GetComponent<Interactable>();
        _interactable.OnInteract.AddListener(PickUp);
    }
    public void PickUp()
    {
        collectedButtonCount++;
        gameObject.SetActive(false);
    }
}