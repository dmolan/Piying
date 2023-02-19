/*
 * Gets input from mouse and rotates/zooms camera in accordance.
 */
 
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public Camera cameraObject;
    public Transform target;
    
    public float currentDistanceToMap;
    public float defaultDistanceToMap = 50f;
    public float deafultAngleOfView = 60f;

    // Mouse controls
    public float rotationSensitivity = 5f;
    public float zoomingSensitivity = 1f;

    // Parameters for smoothly changing camera rotation/zoom
    public float rotationSmoothTime = 60f;
    public float distanceLerpTime = 10f;

    public Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    public float nextDistance;

    public float rotationY;
    public float rotationX;

    private bool isMousePressed = false;



    void Start()
    {
        rotationX = deafultAngleOfView;
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            nextDistance = currentDistanceToMap - Input.GetAxis("Mouse ScrollWheel") * zoomingSensitivity;
        }

        // If Right Mouse Button is UNPRESSED, stop calculating which rotation to make
        if (Input.GetMouseButtonUp(1))
        {
            isMousePressed = false;
        }
        // If Right Mouse Button is PRESSED, start calculating which rotation to make
        else if (Input.GetMouseButtonDown(1) || isMousePressed)
        {
            isMousePressed = true;

            float mouseX = 0, mouseY = 0;
            // If Control is pressed rotate vertically, else horizontally
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                mouseY = Input.GetAxis("Mouse Y") * rotationSensitivity;
            }
            else
            {
                mouseX = Input.GetAxis("Mouse X") * rotationSensitivity;
            }

            rotationY += mouseX;
            rotationX += mouseY;
            if (rotationX < 10) rotationX = 10;
            if (rotationX > 90) rotationX = 90;
        }

        Vector3 nextRotation = new Vector3(rotationX, rotationY);

        // Apply damping between rotation changes
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, Time.deltaTime*rotationSmoothTime);
        cameraObject.transform.localEulerAngles = currentRotation;

        // Substract forward vector of the GameObject to point its forward vector to the target
        cameraObject.transform.position = target.position - cameraObject.transform.forward * currentDistanceToMap;

        // Lineary interpolate distance from target
        if (nextDistance > 0)
        {
            currentDistanceToMap = Mathf.Lerp(currentDistanceToMap, nextDistance, Time.deltaTime*distanceLerpTime);
        }
    }
}
