using UnityEngine;


public class Parallax : MonoBehaviour
{
    Transform cam;
    Vector3 previousCamPos;

    public float distanceX = 0f;
    public float distanceY = 0f;

    public float smoothingX = 1f;
    public float smoothingY = 1f;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (distanceX != 0f)
        {
            float parallaxX = (previousCamPos.x - cam.position.x) * distanceX;
            Vector3 backgroundTargetPosX = new Vector3(transform.position.x + parallaxX,
                                                      transform.position.y,
                                                      transform.position.z);


            transform.position = Vector3.Lerp(transform.position, backgroundTargetPosX, smoothingX * Time.deltaTime);
        }

        if (distanceY != 0f)
        {
            float parallaxY = (previousCamPos.y - cam.position.y) * distanceY;
            Vector3 backgroundTargetPosY = new Vector3(transform.position.x,
                                                       transform.position.y + parallaxY,
                                                       transform.position.z);

            transform.position = Vector3.Lerp(transform.position, backgroundTargetPosY, smoothingY * Time.deltaTime);
        }

        previousCamPos = cam.position;
    }
}