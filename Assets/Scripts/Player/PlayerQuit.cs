using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerQuit : MonoBehaviour
{
    public TextMeshProUGUI quitMessage;

    void Start()
    {
        if (quitMessage != null) quitMessage.gameObject.SetActive(false);
    }

    public void Quit()
    {
        if (quitMessage != null) quitMessage.gameObject.SetActive(true);
    }
}
