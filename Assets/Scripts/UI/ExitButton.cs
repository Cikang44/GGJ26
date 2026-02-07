using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    public DragTransform daButton;
    public Transform daSocketButtonPosition;
    public Button daSocketButton;
    public Sprite fixedSocketSprite;

    void Update()
    {
        Vector2 socketPos = daSocketButtonPosition.position;
        Vector2 buttonPos = daButton.transform.position;
        if ((socketPos - buttonPos).magnitude < 1 && !daButton.dragging)
        {
            daSocketButton.image.sprite = fixedSocketSprite;
            daSocketButton.interactable = true;
            daButton.gameObject.SetActive(false);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
