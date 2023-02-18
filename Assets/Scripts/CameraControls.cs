/*
 * This code is executed only at the runtime.
 * It's purpose is to get input from mouse and rotate camera in accordance.
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

    // public TMP_InputField inputFieldDefaultDistanceToMap, inputFieldDefaultAngleOfView, inputFieldRotationSensitivity, 
    // inputFieldZoomingSensitivity, inputFieldRotationSmoothTime, inputFieldDistanceLerpTime;



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

    /*
    public void changeMouseSensitivity(string newMouseSensitivity)
    {
        if (newMouseSensitivity != "")
        {
            rotationSensitivity = float.Parse(newMouseSensitivity);
            if (rotationSensitivity > 99) 
            {
                rotationSensitivity = 99;
                inputFieldRotationSensitivity.text = "99";
            }
            if (rotationSensitivity < 0) 
            {
                rotationSensitivity = 0;
                inputFieldRotationSensitivity.text = "0";
            }
        }
    }

    public void changeScrollingSpeed(string newScrollingSpeed)
    {
        if (newScrollingSpeed != "")
        {
            zoomingSensitivity = float.Parse(newScrollingSpeed);
            if (zoomingSensitivity > 9999) 
            {
                zoomingSensitivity = 9999;
                inputFieldZoomingSensitivity.text = "9999";
            }
            if (zoomingSensitivity < 0) 
            {
                zoomingSensitivity = 0;
                inputFieldZoomingSensitivity.text = "0";
            }
        }
    }

    public void changeAngleOfView(string newAngleOfView)
    {
        if (newAngleOfView != "")
        {
            deafultAngleOfView = float.Parse(newAngleOfView);
            rotationX = float.Parse(newAngleOfView);
            if (rotationX > 90) 
            {
                rotationX = 90;
                inputFieldDefaultAngleOfView.text = "90";
            }
            if (rotationX < 10) 
            {
                rotationX = 0;
                inputFieldDefaultAngleOfView.text = "0";
            }
        }
    }

    public void changeCurrentDistance(string newCurrentDistance)
    {
        if (newCurrentDistance != "")
        {
            defaultDistanceToMap = float.Parse(newCurrentDistance);
            nextDistance = float.Parse(newCurrentDistance);
            if (nextDistance > 9999) 
            {
                nextDistance = 9999;
                inputFieldDefaultDistanceToMap.text = "9999";
            }
            if (nextDistance < 0) 
            {
                nextDistance = 0;
                inputFieldDefaultDistanceToMap.text = "0";
            }
        }
    }

    public void changeRotationSmoothTime(string newRotationSmoothTime)
    {
        if (newRotationSmoothTime != "")
        {
            rotationSmoothTime = float.Parse(newRotationSmoothTime);
            if (rotationSmoothTime > 999) 
            {
                rotationSmoothTime = 999;
                inputFieldRotationSmoothTime.text = "999";
            }
            if (rotationSmoothTime < 0) 
            {
                rotationSmoothTime = 0;
                inputFieldRotationSmoothTime.text = "0";
            }
        }
    }

    public void changeDistanceLerpTime(string newDistanceLerpTime)
    {
        if (newDistanceLerpTime != "")
        {
            distanceLerpTime = float.Parse(newDistanceLerpTime);
            if (distanceLerpTime > 99) 
            {
                distanceLerpTime = 99;
                inputFieldDistanceLerpTime.text = "99";
            }
            if (distanceLerpTime < 0)
            {
                distanceLerpTime = 0;
                inputFieldDistanceLerpTime.text = "0";
            }
        }
    }
    */
}
