using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class arcadeScript : MonoBehaviour
{

    // Prefabs:
    public GameObject blockPrefab;


    // Other Public GameObjects:
    public Camera mainCamera;
    public GameObject scoreText, comboText;

    // Private GameObjects:
    private GameObject selectedBlock;

    // Gameplay arrays:
    private List<GameObject>[] columnGOs;
    private List<int>[] columnCOLs;
    private List<GameObject> scrapyard;
    private int[,] neighbourGrid;

    private Color[] colours = new Color[] { Color.yellow, Color.green, Color.red, Color.blue, Color.magenta, Color.cyan, Color.black };
    private int[] scores;

    // Gameplay variables:

    private int boardWidth, boardHeight, colourCount, eliminationThreshold;
    private int defaultBoardWidth = 9, defaultBoardHeight = 16, defaultColourCount = 4, defaultEliminationThreshold = 5;
    private float horizontalAdjustment, verticalAdjustment;
    private float movementCheckTicker;

    private int neighbourCount;

    private int playerSelectionX, playerSelectionY;

    private int cascadeCounter, scoreMultiplier;

    private bool blocksExist, readyToDelete, movingAtTheMoment;

    // Use this for initialization
    void Start()
    {

        // Load previous settings, if any:

        loadSettings();

        // Prepare game space:

        prepBoard();

        // Adjust camera to fit playing area:

        initialiseCameraSize();

        // Instantiate board:

        generateBoard();

    }

    // Update is called once per frame
    void Update()
    {
        movementCheckTicker += Time.deltaTime;

        if (movementCheckTicker > 0.2f)
        {
            moreScore();

            movingAtTheMoment = false;
            movementCheckTicker = 0;
            for (int i = 0; i < boardWidth; i++)
            {
                foreach (GameObject go in columnGOs[i])
                {
                    if (go.GetComponent<blockScript>().amIFalling())
                    {
                        movingAtTheMoment = true;
                    }
                }
            }
            if (!movingAtTheMoment)
            {
                if (checkForElimination())
                {
                    movingAtTheMoment = true;
                }
                else
                {
                    for (int i = 0; i < scores.Length; i++)
                    {
                        PlayerPrefs.SetInt("Score" + i, scores[i]);
                    }
                }
            }
        }

        // touch controls:

        if (Application.platform == RuntimePlatform.Android && Input.GetTouch(0).phase == TouchPhase.Ended && !movingAtTheMoment)
        {
            if (!readyToDelete)
            {
                if (selectedBlock != null)
                {
                    selectedBlock.GetComponent<Renderer>().material.color = colours[selectedBlock.GetComponent<blockScript>().ColourID];
                }


                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.gameObject.tag == "Block")
                    {
                        selectedBlock = hit.transform.gameObject;
                        selectedBlock.GetComponent<Renderer>().material.color = Color.white;
                        readyToDelete = true;

                    }
                }

            }
            else if (selectedBlock != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.collider == null)
                    {
                        selectedBlock.GetComponent<Renderer>().material.color = colours[selectedBlock.GetComponent<blockScript>().ColourID];
                        selectedBlock = null;
                        readyToDelete = false;

                    }
                    else
                    {
                        cascadeCounter = 0;
                        scoreMultiplier = 1;
                        moreScore();
                        movementCheckTicker = 0;
                        playerRemoveBlock(selectedBlock.GetComponent<blockScript>().getCoord(0), selectedBlock.GetComponent<blockScript>().getCoord(1));
                        readyToDelete = false;
                        movingAtTheMoment = true;
                        

                    }
                }
            }
        }


        // mouse controls:

        if (Input.GetMouseButtonDown(0) && !movingAtTheMoment)
        {
            if (!readyToDelete)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {

                    if (hit.transform.gameObject.tag == "Block")
                    {
                        if (selectedBlock != null && selectedBlock != hit.transform.gameObject)
                        {
                            selectedBlock.GetComponent<Renderer>().material.color = colours[selectedBlock.GetComponent<blockScript>().ColourID];
                        }
                        selectedBlock = hit.transform.gameObject;
                        selectedBlock.GetComponent<Renderer>().material.color = Color.white;
                        readyToDelete = true;

                    }
                }

            }
            else if (selectedBlock != null)
            {
                cascadeCounter = 0;
                scoreMultiplier = 1;
                moreScore();
                movementCheckTicker = 0;
                playerRemoveBlock(selectedBlock.GetComponent<blockScript>().getCoord(0), selectedBlock.GetComponent<blockScript>().getCoord(1));
                readyToDelete = false;
                movingAtTheMoment = true;
                

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (selectedBlock != null)
            {
                selectedBlock.GetComponent<Renderer>().material.color = colours[selectedBlock.GetComponent<blockScript>().ColourID];
                selectedBlock = null;
                readyToDelete = false;
            }
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            PlayerPrefs.Save();
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerPrefs.Save();
            Application.Quit();
        }
    }

    // Load the player's saved settings, if any:

    private void loadSettings()
    {
        if (PlayerPrefs.HasKey("Width"))
        {
            boardWidth = PlayerPrefs.GetInt("Width");
        }
        else
        {
            boardWidth = defaultBoardWidth;
        }

        if (PlayerPrefs.HasKey("Height"))
        {
            boardHeight = PlayerPrefs.GetInt("Height");
        }
        else
        {
            boardHeight = defaultBoardHeight;
        }

        if (PlayerPrefs.HasKey("ColourCount"))
        {
            colourCount = PlayerPrefs.GetInt("ColourCount");
        }
        else
        {
            colourCount = defaultColourCount;
        }

        if (PlayerPrefs.HasKey("EliminationThreshold"))
        {
            eliminationThreshold = PlayerPrefs.GetInt("EliminationThreshold");
        }
        else
        {
            eliminationThreshold = defaultEliminationThreshold;
        }

        
    }

    // (re)generates the column structure of the gamespace:

    private void prepBoard()
    {
        columnGOs = new List<GameObject>[boardWidth];
        columnCOLs = new List<int>[boardWidth];
        scrapyard = new List<GameObject>();

        scores = new int[colours.Length];
        
        for (int i = 0; i < boardWidth; i++)
        {
            columnGOs[i] = new List<GameObject>();
            columnCOLs[i] = new List<int>();
        }

        if (boardWidth % 2 == 0)
        {
            horizontalAdjustment = 0.5f;
        }
        else
        {
            horizontalAdjustment = 0.5f;
        }

        if (boardHeight % 2 == 0)
        {
            verticalAdjustment = 0.5f;
        }
        else
        {
            verticalAdjustment = 0.5f;
        }

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                int colID = Random.Range(0, colourCount);
                columnCOLs[i].Add(colID);
            }
        }

        while (checkForElimination()) ;
    }

    // sets the camera size to encompass the playing area:

    private void initialiseCameraSize()
    {
        int oSize = 0;

        if (boardHeight / 2 >= boardWidth)
        {
            oSize = boardHeight / 2 + 2;
        }
        else
        {
            oSize = boardWidth + 1;
        }

        mainCamera.orthographicSize = oSize;
    }

    // (re)generates the actual playing field:

    private void generateBoard()
    {
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                GameObject newBlock = Instantiate(blockPrefab, new Vector3(i - boardWidth * 0.5f + horizontalAdjustment, 0, j - boardHeight * 0.5f + verticalAdjustment), Quaternion.identity) as GameObject;

                int colID = columnCOLs[i][j];
                columnGOs[i].Add(newBlock);
                newBlock.GetComponent<blockScript>().ColourID = colID;
                newBlock.GetComponent<blockScript>().Colour = colours[colID];
                newBlock.GetComponent<blockScript>().setCoords(i, j);
                newBlock.GetComponent<blockScript>().setGC(gameObject);
            }
        }

        blocksExist = true;
        loadScores();
    }

    // runs the 

    // checks the grid with recursiveNeighbourCheck(int, int) and tags them for deletion if larger than eliminationThreshold
    // with recursiveNeighbourTag(int, int)

    private bool checkForElimination()
    {
        bool removeAtAll = false;

        neighbourGrid = new int[boardWidth, boardHeight];

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                neighbourCount = 1;
                recursiveNeighbourCheck(i, j);
                if (neighbourCount >= eliminationThreshold)
                {
                    recursiveNeighbourTag(i, j);
                    removeAtAll = true;
                }
            }
        }
        if (removeAtAll)
        {
            removeBlocks();
        }
        return removeAtAll;
    }

    // checks to see if there are any zones to clear out from the board:

    private void recursiveNeighbourCheck(int x, int y)
    {
        if (neighbourGrid[x, y] == 0)
        {
            neighbourGrid[x, y] = 1;
        }
        if (x > 0 && neighbourGrid[x - 1, y] == 0 && columnCOLs[x][y] == columnCOLs[x - 1][y])
        {
            neighbourCount++;
            recursiveNeighbourCheck(x - 1, y);
        }
        if (x < boardWidth - 1 && neighbourGrid[x + 1, y] == 0 && columnCOLs[x][y] == columnCOLs[x + 1][y])
        {
            neighbourCount++;
            recursiveNeighbourCheck(x + 1, y);
        }
        if (y > 0 && neighbourGrid[x, y - 1] == 0 && columnCOLs[x][y] == columnCOLs[x][y - 1])
        {
            neighbourCount++;
            recursiveNeighbourCheck(x, y - 1);
        }
        if (y < boardHeight - 1 && neighbourGrid[x, y + 1] == 0 && columnCOLs[x][y] == columnCOLs[x][y + 1])
        {
            neighbourCount++;
            recursiveNeighbourCheck(x, y + 1);
        }
    }

    // tags neighbours for deletion:

    private void recursiveNeighbourTag(int x, int y)
    {

        neighbourGrid[x, y] = 2;

        if (x > 0 && neighbourGrid[x - 1, y] < 2 && columnCOLs[x][y] == columnCOLs[x - 1][y])
        {
            recursiveNeighbourTag(x - 1, y);
        }
        if (x < boardWidth - 1 && neighbourGrid[x + 1, y] < 2 && columnCOLs[x][y] == columnCOLs[x + 1][y])
        {
            recursiveNeighbourTag(x + 1, y);
        }
        if (y > 0 && neighbourGrid[x, y - 1] < 2 && columnCOLs[x][y] == columnCOLs[x][y - 1])
        {
            recursiveNeighbourTag(x, y - 1);
        }
        if (y < boardHeight - 1 && neighbourGrid[x, y + 1] < 2 && columnCOLs[x][y] == columnCOLs[x][y + 1])
        {
            recursiveNeighbourTag(x, y + 1);
        }

    }

    // clears groups tagged by checkForElimination() from the board:

    private void removeBlocks()
    {
        if (blocksExist)
        {
            cascadeCounter++;
            moreScore();
        }
        for (int i = 0; i < boardWidth; i++)
        {
            int downShift = 0;
            for (int j = 0; j < boardHeight; j++)
            {

                if (neighbourGrid[i, j] == 2)
                {
                    scores[columnCOLs[i][j - downShift]] += scoreMultiplier;
                    columnCOLs[i].RemoveAt(j - downShift);
                    if (blocksExist)
                    {
                        columnGOs[i][j - downShift].transform.position = new Vector3(100, 100, 100);
                        scrapyard.Add(columnGOs[i][j - downShift]);
                        columnGOs[i].RemoveAt(j - downShift);
                    }
                    downShift++;
                }
                else
                {
                    if (blocksExist && downShift > 0)
                    {
                        if (!columnGOs[i][j - downShift].GetComponent<blockScript>().amIFalling())
                        {
                            columnGOs[i][j - downShift].GetComponent<blockScript>().dropMe((float)downShift);
                        }
                        else
                        {
                            columnGOs[i][j - downShift].GetComponent<blockScript>().dropMeMore((float)downShift);
                        }
                        columnGOs[i][j - downShift].GetComponent<blockScript>().changeCoords(0, -downShift);
                    }
                }
            }
        }
        

        repopulateBoard();
    }

    // drops blocks to their new places after elimination:
    // OBSOLETE: handled by repopulateBoard() and blocks' dropMe()


    // handles player selecting a block:



    // handles player removing a block:

    private void playerRemoveBlock(int x, int y)
    {
        neighbourGrid = new int[boardWidth, boardHeight];

        recursiveNeighbourTag(x, y);

        moreScore();

        removeBlocks();
    }

    // repopulates the board after deletion:

    private void repopulateBoard()
    {
        for (int i = 0; i < boardWidth; i++)
        {
            int totalDrop = 0;
            int difference = columnCOLs[i].Count - boardHeight;
            while (columnCOLs[i].Count < boardHeight)
            {
                int colID = Random.Range(0, colourCount);
                columnCOLs[i].Add(colID);
                totalDrop++;

                if (blocksExist)
                {
                    GameObject newBlock;
                    if (scrapyard.Count > 0)
                    {
                        newBlock = scrapyard[0];
                        newBlock.transform.position = new Vector3(i - boardWidth * 0.5f + horizontalAdjustment, 0, totalDrop + 1 + boardHeight * 0.5f + verticalAdjustment);
                        scrapyard.RemoveAt(0);
                    }
                    else
                    {
                        newBlock = Instantiate(blockPrefab, new Vector3(i - boardWidth * 0.5f + horizontalAdjustment, 0, totalDrop + 1 + boardHeight * 0.5f + verticalAdjustment), Quaternion.identity) as GameObject;
                    }
                    columnGOs[i].Add(newBlock);
                    newBlock.GetComponent<blockScript>().ColourID = colID;
                    newBlock.GetComponent<blockScript>().Colour = colours[colID];
                    newBlock.GetComponent<blockScript>().dropMe(2 - difference);
                    newBlock.GetComponent<blockScript>().setCoords(i, boardHeight + difference + totalDrop - 1);
                    newBlock.GetComponent<blockScript>().setGC(gameObject);
                }
            }

        }
    }

    // keeps track of score after every drop:

    private void moreScore()
    {
        if (cascadeCounter < 5)
        {
            comboText.GetComponent<TextMesh>().text = "Waves: " + cascadeCounter;
            scoreMultiplier = 1;
        }
        else if (cascadeCounter < 10)
        {
            comboText.GetComponent<TextMesh>().text = "CASCADE! x" + cascadeCounter;
            scoreMultiplier = 2;
        }
        else if (cascadeCounter < 15)
        {
            comboText.GetComponent<TextMesh>().text = "DOUBLE CASCADE! x" + cascadeCounter;
            scoreMultiplier = 3;
        }
        else if (cascadeCounter < 20)
        {
            comboText.GetComponent<TextMesh>().text = "TRIPLE CASCADE! x" + cascadeCounter;
            scoreMultiplier = 4;
        }
        else
        {
            comboText.GetComponent<TextMesh>().text = "SUPER CASCADE! x" + cascadeCounter;
            scoreMultiplier = 5;
        }
        scoreText.GetComponent<TextMesh>().text = "Scores: Y:" + scores[0] + " G:" + scores[1] + " R:" + scores[2] + " B:" + scores[3];
    }

    private void loadScores()
    {
        scores = new int[colours.Length];

        for (int i = 0; i < scores.Length; i++)
        {
            if (PlayerPrefs.HasKey("Score" + i))
            {
                scores[i] = PlayerPrefs.GetInt("Score" + i);
            }
        }
    }
    // handles special tile creation:



    // handles special tile effects:
}

