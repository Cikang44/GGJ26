using UnityEngine;

public class DialogCommandHelper : MonoBehaviour
{
    [Yarn.Unity.YarnCommand("timescale")]
    public static void Timescale(float scale)
    {
        Time.timeScale = scale;
    }
}