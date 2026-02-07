using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    public Collider2D detectorCollider;
    public bool isGrounded = false;
    private int _groundCount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Moving Platform") || collision.CompareTag("Disappearing Platform"))
        {
            isGrounded = true;
            _groundCount++;
            if (collision.CompareTag("Moving Platform"))
            {
                transform.SetParent(collision.transform, true);
            }
            else if (collision.CompareTag("Disappearing Platform"))
            {
                collision.GetComponent<DisappearingPlatform>().Disappear();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Moving Platform") || collision.CompareTag("Disappearing Platform"))
        {
            _groundCount--;
            isGrounded = _groundCount > 0;
            if (collision.CompareTag("Moving Platform"))
            {
                transform.SetParent(null, true);
            }
        }
    }
}