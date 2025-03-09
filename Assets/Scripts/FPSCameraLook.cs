using UnityEngine;

public class FPSCameraLook : MonoBehaviour
{
    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public bool lockCursor = true;

    [Header("Constraints")]
    public float maxLookUpAngle = 90f;
    public float maxLookDownAngle = 90f;

    private float xRotation = 0f;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Handle vertical rotation (looking up and down)
        xRotation -= mouseY; // Subtract because mouse Y is inverted
        xRotation = Mathf.Clamp(xRotation, -maxLookDownAngle, maxLookUpAngle);

        // Apply vertical rotation to camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Handle horizontal rotation (looking left and right)
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    public void ToggleCursorLock(bool lockState)
    {
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }
}