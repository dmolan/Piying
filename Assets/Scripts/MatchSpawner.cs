using UnityEngine;
using TMPro;

public class MatchSpawner : MonoBehaviour
{
    private const int SIZE_OF_TEXTURE = 1000;
    private const int MAX_MATCHES = 10000;
    private const float SIZE_X = 5;
    private const float SIZE_Y = 6;
    private const float SIZE_Z = 5;
    private const int PLANE_TICKS = 20;
    private const int FREQUENCY = 1;
    private const float LENGTH_OF_MATCH = 0.25f;

    public GameObject idealMatch;
    public GameObject ButtonStart;
    public GameObject ButtonStop;
    public Rigidbody matchRigidBody;
    public TextMeshProUGUI myCounterText;

    private GameObject[] matches;
    private static Color[] colorMap;

    private int spawningCyclesCounter = 0;
    private int recalculationCyclesCounter = 0;
    private int localMatches = 0;
    private int localGoodMatches = 0;
    // Number of matches that aren't visible (were cleared). Used for statistics calculation.
    private int leftoverMatches = 0;
    // Number of matches that aren't visible (were cleared) and cross the lines. Used for statistics calculation.
    private int leftoverGoodMatches = 0;
    private float frequency = 0.5f;
    private bool isOnPause = false;



    public void fillColorMap()
    {
        colorMap = new Color[SIZE_OF_TEXTURE * SIZE_OF_TEXTURE];

        for (int i = 0; i < SIZE_OF_TEXTURE; ++i)
        {
            for (int j = 0; j < SIZE_OF_TEXTURE; ++j)
            {
                if (i % (SIZE_OF_TEXTURE / PLANE_TICKS) == 0) colorMap[j + i * SIZE_OF_TEXTURE] = new Color(53 / 255f, 75 / 255f, 98 / 255f);
                else colorMap[j + i * SIZE_OF_TEXTURE] = new Color(222 / 255f, 239 / 255f, 255 / 255f);
            }
        }
    }
    
    void Start()
    {
        ButtonStop.SetActive(false);
        ButtonStart.SetActive(true);

        isOnPause = true;
        matches = new GameObject[MAX_MATCHES];

        fillColorMap();
        PlaneDisplay planeDisplay = FindObjectOfType<PlaneDisplay>();
        planeDisplay.drawTexture(TextureGenerator.textureFromColorMap(colorMap, SIZE_OF_TEXTURE, SIZE_OF_TEXTURE));
    }

    private void spawnMatch()
    {
        matches[localMatches] = Instantiate(idealMatch);
        matches[localMatches].GetComponent<Renderer>().material.color = new Color(255f/255, 255f/255, 255f/255);
        matches[localMatches].transform.position = new Vector3(UnityEngine.Random.Range(-SIZE_X+1, SIZE_X-1), UnityEngine.Random.Range(5, SIZE_Y), UnityEngine.Random.Range(-SIZE_Z+0.5f, SIZE_Z-0.5f));
        matches[localMatches].transform.Rotate(UnityEngine.Random.Range(0, 360f), UnityEngine.Random.Range(0, 360f), UnityEngine.Random.Range(0, 360f));
        ++localMatches;
    }
 
    void Update()
    {
        if ((!isOnPause && spawningCyclesCounter >= (1/frequency/frequency) * 5) && (localMatches + leftoverMatches < MAX_MATCHES))
        {
            spawnMatch();
            spawningCyclesCounter = 0;
        }
        else ++spawningCyclesCounter;

        if (recalculationCyclesCounter % (10 * FREQUENCY) == 0 && localMatches+leftoverMatches < MAX_MATCHES)
        {
            localGoodMatches = checkMatchesPosition();
            
            // Write statistics in text field
            myCounterText.text = (localMatches+leftoverMatches).ToString() +  " / " + (localGoodMatches+leftoverGoodMatches).ToString() 
            + " ≈ " + (1f * (localMatches+leftoverMatches) / (localGoodMatches+leftoverGoodMatches)).ToString("F6");
        }
        ++recalculationCyclesCounter;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            spawnMatch();
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C))
        {
            clearMatches();
        }
        if (Input.GetKeyDown(KeyCode.R)) 
        {
            resetMatches();
        }
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.K))
        {
            if (isOnPause) setPauseOff();
            else setPauseOn();
        }
    }

    private float findDeltaToMiddles(float angle)
    {
        float angleLocal = Mathf.Abs(angle);
        return Mathf.Min(Mathf.Abs(angleLocal - 45), Mathf.Abs(angleLocal - 135), Mathf.Abs(angleLocal - 225), Mathf.Abs(angleLocal - 315));
    }

    public int checkMatchesPosition()
    {
        int succesessMatches = 0;
        for (int i = 0; i < localMatches; ++i)
        {
            if (matches[i].transform.position.y < 2) matches[i].GetComponent<Renderer>().material.color = new Color(250f/255, 30f/255, 0f/255);

            for (int j = -PLANE_TICKS / 2; j <= PLANE_TICKS / 2; ++j)
            {
                float tmpX = matches[i].transform.eulerAngles.x;
                while (tmpX < 0) tmpX += 360;
                // near 0 degrees or near 180 degrees or near -180 degrees
                if (((tmpX >= 0 && tmpX <= 45) || (tmpX >= 315 && tmpX <= 360)) || (tmpX >= 135 && tmpX <= 225))
                {
                    if (Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) <= 
                    Mathf.Abs(Mathf.Sin(matches[i].transform.eulerAngles.y * Mathf.PI / 180)) * LENGTH_OF_MATCH / 2) 
                    {
                        succesessMatches++;
                        if (matches[i].transform.position.y < 2) matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
                        break;
                    }
                }
                // near 90 degrees
                else if (tmpX > 45 && tmpX < 135)
                {
                    if (Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) <= 
                    Mathf.Abs(Mathf.Cos(Mathf.Abs(matches[i].transform.eulerAngles.y - matches[i].transform.eulerAngles.z) * Mathf.PI / 180)) * LENGTH_OF_MATCH / 2) 
                    {
                        succesessMatches++;
                        matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
                        break;
                    }
                }
                // near 270 degrees
                else if (tmpX > 225 && tmpX < 315)
                {
                    if (Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) <= 
                    Mathf.Abs(Mathf.Cos(Mathf.Abs(matches[i].transform.eulerAngles.y + matches[i].transform.eulerAngles.z) * Mathf.PI / 180)) * LENGTH_OF_MATCH / 2) 
                    {
                        succesessMatches++;
                        matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
                        break;
                    }
                }
                else
                {
                    Debug.Log("UNSOLVED CASE! " + tmpX);
                }
            }
        }

        return succesessMatches;
    }

    public void setPauseOn()
    {
        isOnPause = true;
        ButtonStart.SetActive(true);
        ButtonStop.SetActive(false);
    }

    public void setPauseOff()
    {
        isOnPause = false;
        ButtonStart.SetActive(false);
        ButtonStop.SetActive(true);
    }

    public void changeFrequency(float newFrequency)
    {
        frequency = newFrequency;
    }

    public void myTestOfRotationX()
    {
        int i = localMatches-1;
        // matches[i].transform.Rotate(90, 0, 0);
        matches[i].transform.eulerAngles = new Vector3(90, 0, 0);

        Quaternion myQ = new Quaternion(0, 0.70710f, 0, 0.70710f);

        Debug.Log( 
        "anX: " + matches[i].transform.eulerAngles.x +
        "     anY: " + matches[i].transform.eulerAngles.y +
        "     anZ: " + matches[i].transform.eulerAngles.z +
        "     quW: " + matches[i].transform.rotation.w +
        "     quX: " + matches[i].transform.rotation.x +
        "     quY: " + matches[i].transform.rotation.y +
        "     quZ: " + matches[i].transform.rotation.z +
        "     delta: " + Quaternion.Angle(matches[i].transform.rotation, myQ)
        );
    }

    public void writeCoordinates()
    {
        int i = localMatches - 1;
        for (int j = -PLANE_TICKS / 2; j <= PLANE_TICKS / 2; ++j)
        {
            float tmpX = matches[i].transform.eulerAngles.x;
            while (tmpX < 0) tmpX += 360;
            // near 0 degrees or near 180 degrees or near -180 degrees
            if (((tmpX >= 0 && tmpX <= 45) || (tmpX >= 315 && tmpX <= 360)) || (tmpX >= 135 && tmpX <= 225))
            {
                if (Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) <= 
                Mathf.Abs(Mathf.Sin(matches[i].transform.eulerAngles.y * Mathf.PI / 180)) * 0.5f / 2) 
                {
                    // if (i == localMatches-1) Debug.Log
                    // (
                    //     "angX: " + matches[i].transform.eulerAngles.x +
                    //     "     angY: " + matches[i].transform.eulerAngles.y +
                    //     "     angZ: " + matches[i].transform.eulerAngles.z +
                    //     "     posX: " + matches[i].transform.position.x +
                    //     "     posY: " + matches[i].transform.position.y +
                    //     "     posZ: " + matches[i].transform.position.z + '\n' +
                    //     tmpX + ". Case 0, 180, 360:" + 
                    //     Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) + " <= " + Mathf.Abs(Mathf.Sin(matches[i].transform.eulerAngles.y * Mathf.PI / 180)) * 0.5f / 2
                    // );
                    
                    if (matches[i].transform.position.y < 3) matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
                    break;
                }
            }
            // near 90 degrees
            else if (tmpX > 45 && tmpX < 135)
            {
                if (Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) <= 
                Mathf.Abs(Mathf.Cos(Mathf.Abs(matches[i].transform.eulerAngles.y - matches[i].transform.eulerAngles.z) * Mathf.PI / 180)) * 0.5f / 2) 
                {
                    // if (i == localMatches-1) Debug.Log
                    // (
                    //     "angX: " + matches[i].transform.eulerAngles.x +
                    //     "     angY: " + matches[i].transform.eulerAngles.y +
                    //     "     angZ: " + matches[i].transform.eulerAngles.z +
                    //     "     posX: " + matches[i].transform.position.x +
                    //     "     posY: " + matches[i].transform.position.y +
                    //     "     posZ: " + matches[i].transform.position.z + '\n' +
                    //     tmpX + ". Case 90:" + 
                    //     Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) + "<= " +
                    //     Mathf.Abs(Mathf.Cos(Mathf.Abs(matches[i].transform.eulerAngles.y - matches[i].transform.eulerAngles.z) * Mathf.PI / 180)) * 0.5f / 2
                    // );
                    
                    if (matches[i].transform.position.y < 3) matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
                    break;
                }
            }
            // near 270 degrees
            else if (tmpX > 225 && tmpX < 315)
            {
                if (Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) <= 
                Mathf.Abs(Mathf.Cos(Mathf.Abs(matches[i].transform.eulerAngles.y + matches[i].transform.eulerAngles.z) * Mathf.PI / 180)) * 0.5f / 2) 
                {
                    // if (i == localMatches-1) Debug.Log
                    // (
                    //     "angX: " + matches[i].transform.eulerAngles.x +
                    //     "     angY: " + matches[i].transform.eulerAngles.y +
                    //     "     angZ: " + matches[i].transform.eulerAngles.z +
                    //     "     posX: " + matches[i].transform.position.x +
                    //     "     posY: " + matches[i].transform.position.y +
                    //     "     posZ: " + matches[i].transform.position.z + '\n' +
                    //     tmpX + ". Case 90:" + 
                    //     Mathf.Abs(matches[i].transform.position.z - j*(1.0f * 2 * SIZE_Z / PLANE_TICKS)) + "<=" + 
                    //     Mathf.Abs(Mathf.Cos(Mathf.Abs(matches[i].transform.eulerAngles.y + matches[i].transform.eulerAngles.z) * Mathf.PI / 180)) * 0.5f / 2
                    // );

                    
                    if (matches[i].transform.position.y < 3) matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
                    break;
                }
            }
            else
            {
                Debug.Log("UNSOLVED CASE! " + tmpX);
            }
        }
    }

    public void myTestOfRotationY()
    {
        int i = localMatches-1;
        matches[i].transform.Rotate(0, 90, 0);

        Quaternion myQ = new Quaternion(0, 0.70710f, 0, 0.70710f);

        Debug.Log( 
        "anX: " + matches[i].transform.eulerAngles.x +
        "     anY: " + matches[i].transform.eulerAngles.y +
        "     anZ: " + matches[i].transform.eulerAngles.z +
        "     quW: " + matches[i].transform.rotation.w +
        "     quX: " + matches[i].transform.rotation.x +
        "     quY: " + matches[i].transform.rotation.y +
        "     quZ: " + matches[i].transform.rotation.z +
        "     delta: " + Quaternion.Angle(matches[i].transform.rotation, myQ)
        );
    }

    public void myTestOfRotationZ()
    {
        int i = localMatches-1;
        // matches[i].transform.Rotate(0, 0, 90);
        matches[i].transform.eulerAngles = new Vector3(0, 0, 90);

        Quaternion myQ = new Quaternion(0, 0.70710f, 0, 0.70710f);

        Debug.Log( 
        "anX: " + matches[i].transform.eulerAngles.x +
        "     anY: " + matches[i].transform.eulerAngles.y +
        "     anZ: " + matches[i].transform.eulerAngles.z +
        "     quW: " + matches[i].transform.rotation.w +
        "     quX: " + matches[i].transform.rotation.x +
        "     quY: " + matches[i].transform.rotation.y +
        "     quZ: " + matches[i].transform.rotation.z +
        "     delta: " + Quaternion.Angle(matches[i].transform.rotation, myQ)
        );
    }

    public void quitApplication()
    {
        Application.Quit();
    }

    public void clearMatches()
    {
        // Destroy all matches
        for (int i = 0; i < localMatches; ++i)
        {
            Destroy(matches[i]);
        }

        // Add matches that were destroyed in global matches counter
        leftoverGoodMatches += localGoodMatches;
        leftoverMatches += localMatches;

        // Set local matches counter to zero
        localMatches = 0;
        localGoodMatches = 0;
    }

    public void resetMatches()
    {
        clearMatches();
        leftoverMatches = 0;
        leftoverGoodMatches = 0;
        myCounterText.text = "";
    }
}



//-------------------------------------------------------------------
//------------------------------ARCHIVE------------------------------
//-------------------------------------------------------------------

// Function to generate image with random while/black dots:
/*
    public void saveImage()
    {
        int sizeOfTextureX = 1000;
        int sizeOfTextureY = 1000;
        // Construct empty array
        Color[] greyArray = new Color[sizeOfTextureY * sizeOfTextureX];
        // Fill grey map
        for (int y = 0; y < sizeOfTextureY; ++y)
        {
            for (int x = 0; x < sizeOfTextureX; ++x)
            {
                float randomMultiplier = UnityEngine.Random.Range(0f, 1f);
                greyArray[x + y * sizeOfTextureX] = new Color(randomMultiplier, randomMultiplier, randomMultiplier);
            }
                }
                }
                else
                {
                    Debug.Log("UNSOLVED CASE! " + tmpX);
                }
*/

/*
                float tmpX = Mathf.Abs(matches[i].transform.eulerAngles.x);
                if (((tmpX >= 0 && tmpX <= 45) || (tmpX >= 315 && tmpX <= 360)) || (tmpX >= 135 && tmpX <= 225))
                {
                    float tmp = Mathf.Min(
                        Mathf.Min(
                            findSmallestDiffAngle(matches[i].transform.eulerAngles.x), 
                            findSmallestDiffAngle(matches[i].transform.eulerAngles.y)
                        ), 
                        findSmallestDiffAngle(matches[i].transform.eulerAngles.z));

                    if (matches[i].transform.eulerAngles.x > matches[i].transform.eulerAngles.y && matches[i].transform.eulerAngles.x > matches[i].transform.eulerAngles.z)
        }
                else
                {
                    Debug.Log("UNSOLVED CASE! " + tmpX);
                }
*/

/*
                float tmpX = Mathf.Abs(matches[i].transform.eulerAngles.x);
                if (((tmpX >= 0 && tmpX <= 45) || (tmpX >= 315 && tmpX <= 360)) || (tmpX >= 135 && tmpX <= 225))
                {
                    float tmp = Mathf.Min(
                        Mathf.Min(
                            findSmallestDiffAngle(matches[i].transform.eulerAngles.x), 
                            findSmallestDiffAngle(matches[i].transform.eulerAngles.y)
                        ), 
                        findSmallestDiffAngle(matches[i].transform.eulerAngles.z));

                    if (matches[i].transform.eulerAngles.x > matches[i].transform.eulerAngles.y && matches[i].transform.eulerAngles.x > matches[i].transform.eulerAngles.z)

        string saveFilePath = "C://Oleg//Pictures//GreyTest//Myfile.bmp";
        Texture2D texture2D = TextureGenerator.textureFromColorMap(greyArray, 1000, 1000);
        saveTextureToBMP.saveTextureToBMP(saveFilePath, texture2D);
    }

    public SaveTextureToBMP saveTextureToBMP;
    if (Input.GetKeyDown(KeyCode.T))
    {
        saveImage();
    }
*/

// An attempt of rotation-check with one angle 90/-90, second 180/-180 and third gives rotation to Z axis
/*
    float deltaX = findDeltaToMiddles(matches[i].transform.localEulerAngles.x);
    float deltaY = findDeltaToMiddles(matches[i].transform.localEulerAngles.y);
    float deltaZ = findDeltaToMiddles(matches[i].transform.localEulerAngles.z);
    float bestAngle = 0;

    bool xWasBest = false, yWasBest = false, zWasBest = false;
    if (deltaX < deltaY && deltaX < deltaZ) 
    {
        bestAngle = matches[i].transform.localEulerAngles.x;
        xWasBest = true;
    }
    if (deltaY < deltaX && deltaY < deltaZ) 
    {
        bestAngle = matches[i].transform.localEulerAngles.y;
        yWasBest = true;
    }
    if (deltaZ < deltaX && deltaZ < deltaY) 
    {
        bestAngle = matches[i].transform.localEulerAngles.z;
        zWasBest = true;
    }

    float angleX = Mathf.Abs(matches[i].transform.localEulerAngles.x);
    float angleY = Mathf.Abs(matches[i].transform.localEulerAngles.y);
    float angleZ = Mathf.Abs(matches[i].transform.localEulerAngles.z);

    if ((xWasBest && 
            (angleY >= 269 && angleY <= 271 && angleZ >= 179 && angleZ <= 181) 
            || 
            (angleZ >= 269 && angleZ <= 271 && angleY >= 179 && angleY <= 181)
        ) ||
        (yWasBest && 
            (angleX >= 269 && angleX <= 271 && angleZ >= 179 && angleZ <= 181) 
            || 
            (angleZ >= 269 && angleZ <= 271 && angleX >= 179 && angleX <= 181)
        ) ||
        (zWasBest && 
            (angleX >= 269 && angleX <= 271 && angleY >= 179 && angleY <= 181) 
            || 
            (angleY >= 269 && angleY <= 271 && angleX >= 179 && angleX <= 181)
        )
    )
    {
        if (1000*Mathf.Abs(matches[i].transform.position.z - j) <= 
        1000*Mathf.Abs(Mathf.Cos(bestAngle * Mathf.PI / 180)) * 0.5f / 2) 
        {
            if (i == localMatches - 1) Debug.Log(
            "COS(): " + Mathf.Sin(bestAngle * Mathf.PI / 180) + 
            "     bestAngle: " + bestAngle + 
            // "     anW: " + matches[i].transform.rotation.w +
            // "     anX: " + matches[i].transform.rotation.x +
            // "     anY: " + matches[i].transform.rotation.y +
            // "     anZ: " + matches[i].transform.rotation.z +
            "     anX: " + matches[i].transform.eulerAngles.x +
            "     anY: " + matches[i].transform.eulerAngles.y +
            "     anZ: " + matches[i].transform.eulerAngles.z +
            "     Position.x: " + matches[i].transform.localPosition.x + 
            "     Position.y: " + matches[i].transform.localPosition.y + 
            "     Position.z: " + matches[i].transform.localPosition.z);

            // Debug.Log("ALL MATHCHES:");
            // for (int k = 0; k < localMatches; ++k)
            // {
            //     Debug.Log(k + ": Position.z: " + matches[k].transform.position.z);
            // }

            succesessMatches++;
            matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
            // if (matches[i].transform.position.y < 3) matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
            break;
        }
    }
    else if (1000*Mathf.Abs(matches[i].transform.position.z - j) <= 
        1000*Mathf.Abs(Mathf.Sin(bestAngle * Mathf.PI / 180)) * 0.5f / 2) 
    {
        if (i == localMatches - 1) Debug.Log(
            "SIN(): " + Mathf.Sin(bestAngle * Mathf.PI / 180) + 
            "     bestAngle: " + bestAngle + 
            // "     anW: " + matches[i].transform.rotation.w +
            // "     anX: " + matches[i].transform.rotation.x +
            // "     anY: " + matches[i].transform.rotation.y +
            // "     anZ: " + matches[i].transform.rotation.z +
            "     anX: " + matches[i].transform.eulerAngles.x +
            "     anY: " + matches[i].transform.eulerAngles.y +
            "     anZ: " + matches[i].transform.eulerAngles.z +
            "     Position.x: " + matches[i].transform.localPosition.x + 
            "     Position.y: " + matches[i].transform.localPosition.y + 
            "     Position.z: " + matches[i].transform.localPosition.z);

        succesessMatches++;
        matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
        // if (matches[i].transform.position.y < 3) matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
        break;
    }
*/

// An attempt of Quaternion rotation-check
/*
    Quaternion myQ = new Quaternion(0, 0.70710f, 0, 0.70710f);

    float deltaAngle = Quaternion.Angle(matches[i].transform.rotation, idealMatch.transform.rotation);
    if (1000*Mathf.Abs(matches[i].transform.position.z - j) <= 
        1000*Mathf.Abs(Mathf.Cos(deltaAngle * Mathf.PI / 180)) * 0.5f / 2) 
    {
        if (i == localMatches - 1) Debug.Log( 
        // "anX: " + matches[i].transform.eulerAngles.x +
        // "     anY: " + matches[i].transform.eulerAngles.y +
        // "     anZ: " + matches[i].transform.eulerAngles.z +
        // "     quW: " + matches[i].transform.rotation.w +
        // "     quX: " + matches[i].transform.rotation.x +
        // "     quY: " + matches[i].transform.rotation.y +
        // "     quZ: " + matches[i].transform.rotation.z +
        "     delta: " + deltaAngle +
        "     eulerX: " + matches[i].transform.eulerAngles.x +
        "     localEulerX: " + matches[i].transform.localEulerAngles.x +
        "     Cos(): " + Mathf.Cos(deltaAngle * Mathf.PI / 180)
        );
        succesessMatches++;
        matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
        break;
    }
*/