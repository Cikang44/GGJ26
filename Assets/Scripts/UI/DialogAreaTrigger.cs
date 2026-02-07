using UnityEngine;

public class DialogAreaTrigger : MonoBehaviour {
    public string node;
    private bool _triggered = false;
    private void OnTriggerEnter2D(Collider2D other) {
        if (_triggered) return;
        _triggered = true;
        
        if (other.CompareTag("Player")) {
            var dialogueRunner = FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
            if (dialogueRunner != null) {
                dialogueRunner.StartDialogue(node);
            }
        }
    }
}