/*
 * Gets keyboard input, spawns, counts and writes statistics about matches in TextMeshProUGUI text field 
 */

using UnityEngine;
using TMPro;

public class MatchSpawner : MonoBehaviour
{
    private const int SIZE_OF_TEXTURE = 1000;
    private const int MAX_MATCHES = 100000;
    private const int FREQUENCY = 1;
    private const int PLANE_TICKS = 10;
    private const float LENGTH_OF_MATCH = 0.5f;

    // Size of plane on X axis and limits from -CONST to CONST in which match spawns on X axis
    private const float SIZE_X = 4.5f;
    // Position on which match spawns on Y axis
    private const float SIZE_Y = 5f;
    // Size of plane on Z axis
    private const float SIZE_Z = 5f;

    public GameObject idealMatch;
    public GameObject ButtonStart;
    public GameObject ButtonStop;
    public Rigidbody matchRigidBody;
    public TextMeshProUGUI myCounterText;
    public TextMeshProUGUI myCounterAndGenerateText;

    private GameObject[] matches;
    private static Color[] colorMap;

    private int spawningCyclesCounter = 0;
    private int recalculationCyclesCounter = 0;
    private int frequency = 3;
    private bool isOnPause = false;

    // Number of matches from last clearing
    private int localMatches = 0;
    // Number of matches from last clearing that crosses one of the lines
    private int localGoodMatches = 0;
    // Number of matches that aren't visible (were cleared). Used for statistics calculation
    private int leftoverMatches = 0;
    // Number of matches that aren't visible (were cleared) and cross the lines. Used for statistics calculation
    private int leftoverGoodMatches = 0;



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

    void Update()
    {
        if ((!isOnPause && spawningCyclesCounter >= 5) && (localMatches + leftoverMatches < MAX_MATCHES))
        {
            for (int i = 0; i < frequency; ++i)
            {
                createNewRandomMatch();
            }
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
            createNewRandomMatch();
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            checkMatchesPositionAndGenerate();
        }
    }

    // Calculate number of line-crossing matches
    public int checkMatchesPosition()
    {
        int crossingLineMatches = 0;
        for (int i = 0; i < localMatches; ++i)
        {
            // Ddistance between every two neighbor lines on plane, crossing of which we are checking
            float unitSegment = (2f*SIZE_Z / PLANE_TICKS);
            float positionOnZAxis = Mathf.Abs(matches[i].transform.position.z);
            // Smallest distance from match's center to any of the lines
            float remainder = positionOnZAxis - Mathf.FloorToInt(positionOnZAxis / unitSegment) * unitSegment;

            // If x <= sin(a)* L / 2, then match is crossing the line
            if ((2f*remainder >= unitSegment ? unitSegment - remainder : remainder)
            <= Mathf.Abs(Mathf.Sin(matches[i].transform.eulerAngles.y * Mathf.PI / 180)) * LENGTH_OF_MATCH / 2) 
            {
                // Paint match in blue, if it crosses the line
                if (matches[i].transform.position.y < 2) matches[i].GetComponent<Renderer>().material.color = new Color(0f/255, 60f/255, 230f/255);
                ++crossingLineMatches;
            }
            else
            {
                // Paint match in red, if it does not cross the line
                if (matches[i].transform.position.y < 2) matches[i].GetComponent<Renderer>().material.color = new Color(250f/255, 30f/255, 0f/255);
            }
        }

        return crossingLineMatches;
    }

    // Mthematically run calculation on huge number of matches (result only in text field)
    public void checkMatchesPositionAndGenerate()
    {
        int matchesToGenerate = 1000000;
        int succesessMatches = 0;
        for (int i = 0; i < matchesToGenerate; ++i)
        {
            float positionOnZAxis = UnityEngine.Random.Range(-4.5f, 4.5f);
            float angleToZAxis = UnityEngine.Random.Range(0f, 360f);
            
            positionOnZAxis = Mathf.Abs(positionOnZAxis*2f) + 0.5f;
            float unitSegment = (2f*SIZE_Z / PLANE_TICKS);
            float remainder = positionOnZAxis - Mathf.Floor(positionOnZAxis / unitSegment) * unitSegment;

            if ((2f*remainder > unitSegment ? remainder - unitSegment / 2f : remainder) 
            <= Mathf.Abs(Mathf.Cos(angleToZAxis * Mathf.PI / 180)) * LENGTH_OF_MATCH / 2) 
            {
                ++succesessMatches;
            }
        }

        myCounterAndGenerateText.text = matchesToGenerate + " / " + succesessMatches + " ≈ " + (1.0f * matchesToGenerate / succesessMatches).ToString("F6");
    }

    // Fill 2D color map with values, that will be used for plain's texture
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
    
    // Create new match with random rotation and position in XZ plane (the height is constant)
    private void createNewRandomMatch()
    {
        matches[localMatches] = Instantiate(idealMatch);
        // Paint match in white before all calculations
        matches[localMatches].GetComponent<Renderer>().material.color = new Color(255f/255, 255f/255, 255f/255);
        matches[localMatches].transform.position = 
        new Vector3(UnityEngine.Random.Range(-SIZE_X, SIZE_X), SIZE_Y, UnityEngine.Random.Range(-SIZE_Z + 0.5f, SIZE_Z - 0.5f));
        matches[localMatches].transform.Rotate(UnityEngine.Random.Range(0, 360f), 0, 0);
        ++localMatches;
    }



    /*-----------------------------------------------*/
    /* TECHNICAL FUNCTIONS (for buttons and sliders) */
    /*-----------------------------------------------*/

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
        frequency = (int)newFrequency;
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
        myCounterAndGenerateText.text = "";
    }
}
