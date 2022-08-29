using UnityEngine;

public class Player_MouseLook : MonoBehaviour
{
    [Range(1f, 100f)]
    [SerializeField] float sensitivity = 100f;
    [SerializeField] Transform Camera;
    float horizontal, vertical;

    float xRotation = 0f;


    void Start()
    {
        Debug.Log("Player MouseLook - Initialized");
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        horizontal = Input.GetAxis("Mouse X") * sensitivity;
        vertical = Input.GetAxis("Mouse Y") * sensitivity;

        xRotation -= vertical;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * horizontal);
    }
}