using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour
{
    private static int boardHeight = 36;
    private static int boardWidth = 28;

    private bool didStartDeath = false;
    private bool didStartConsumed = false;

    public static int playerOneLevel = 1;
    public static int playerTwoLevel = 1;

    public int totalPellets = 0;
    public static int playerOneScore = 0;
    public static int playerTwoScore = 0;

    public static bool isPlayerOneUp = true;
    public static bool shouldBlink = false;

    public float blinkIntervalTime = 0.1f;
    public float blinkIntervalTimer = 0;

    public AudioClip backgroundAudioNormal;
    public AudioClip backgroundAudioFrightened;
    public AudioClip backgroundAudioPacManDeath;
    public AudioClip consumedGhostAudioClip;

    public Sprite mazeBlue;
    public Sprite mazeWhite;

    public Text playerText;
    public Text readyText;

    public Text highScoreText;
    public Text playerOneUp;
    public Text playerTwoUp;
    public Text playerOneScoreText;
    public Text playerTwoScoreText;
    public Image playerLives2;
    public Image playerLives3;

    public Text consumedGhostScoreText;

    public GameObject[,] board = new GameObject[boardWidth, boardHeight];

    private bool didIncrementLevel = false;

    // Start is called before the first frame update
    void Start()
    {
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject o in objects)
        {
            Vector2 pos = o.transform.position;
            if (o.name != "PacMan" && o.name != "Nodes" && o.name != "NonNodes" && o.name != "Maze" && o.name != "Pellets" && o.name != "bottom_left_corner_single" && o.tag != "Ghost" && o.tag != "GhostHome" && o.tag != "UIElements")
            {
                if (o.GetComponent<Tile>() != null)
                {
                    if (o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isSuperPellet)
                    {
                        totalPellets++;
                    }
                }

                board[(int)pos.x, (int)pos.y] = o;
            }
        }

        if (isPlayerOneUp)
        {
            if (playerOneLevel == 1)
            {
                GetComponent<AudioSource>().Play();
            }
        }
        else
        {
            if (playerTwoLevel == 1)
            {
                GetComponent<AudioSource>().Play();
            }
        }

        StartGame();
    }

    void Update()
    {
        UpdateUI();
        CheckPelletsConsumed();
        CheckShouldBlink();
    }

    void UpdateUI()
    {
        playerOneScoreText.text = playerOneScore.ToString();
        playerTwoScoreText.text = playerTwoScore.ToString();

        if (isPlayerOneUp)
        {
            if (GameMenu.livesPlayerOne == 3)
            {
                playerLives3.enabled = true;
                playerLives2.enabled = true;
            }
            else if (GameMenu.livesPlayerOne == 2)
            {
                playerLives3.enabled = false;
                playerLives2.enabled = true;
            }
            else if (GameMenu.livesPlayerOne == 1)
            {
                playerLives3.enabled = false;
                playerLives2.enabled = false;
            }
        }
        else
        {
            if (GameMenu.livesPlayerTwo == 3)
            {
                playerLives3.enabled = true;
                playerLives2.enabled = true;
            }
            else if (GameMenu.livesPlayerTwo == 2)
            {
                playerLives3.enabled = false;
                playerLives2.enabled = true;
            }
            else if (GameMenu.livesPlayerTwo == 1)
            {
                playerLives3.enabled = false;
                playerLives2.enabled = false;
            }
        }
    }

    void CheckPelletsConsumed()
    {
        if (isPlayerOneUp)
        {
            if (totalPellets == GameMenu.playerOnePelletsConsumed)
            {
                PlayerWin(1);
            }
        }
        else
        {
            if (totalPellets == GameMenu.playerTwoPelletsConsumed)
            {
                PlayerWin(2);
            }
        }
    }

    void PlayerWin(int playerNum)
    {
        if (playerNum == 1)
        {
            if (!didIncrementLevel)
            {
                didIncrementLevel = true;
                playerOneLevel++;
                StartCoroutine(ProcessWin(2));
            }
        }
        else
        {
            if (!didIncrementLevel)
            {
                didIncrementLevel = true;
                playerTwoLevel++;
                StartCoroutine(ProcessWin(2));
            }
        }
    }

    IEnumerator ProcessWin(float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = false;
        pacMan.transform.GetComponent<Animator>().enabled = false;

        transform.GetComponent<AudioSource>().Stop();

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
            ghost.transform.GetComponent<Animator>().enabled = false;
        }

        yield return new WaitForSeconds(delay);

        StartCoroutine(BlinkBoard(2));
    }

    IEnumerator BlinkBoard(float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        // Blink board
        shouldBlink = true;

        yield return new WaitForSeconds(delay);

        // Restart the game at the next level
        shouldBlink = false;
        StartNextLevel();
    }

    private void CheckShouldBlink()
    {
        if (shouldBlink)
        {
            if (blinkIntervalTimer < blinkIntervalTime)
            {
                blinkIntervalTimer += Time.deltaTime;
            }
            else
            {
                blinkIntervalTimer = 0;
                if (GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite == mazeBlue)
                {
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeWhite;
                }
                else
                {
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeBlue;
                }
            }
        }
    }

    private void StartNextLevel()
    {
        StopAllCoroutines();

        if(isPlayerOneUp)
        {
            ResetPelletsForPlayer(1);
            GameMenu.playerOnePelletsConsumed = 0;
        }
        else
        {
            ResetPelletsForPlayer(2);
            GameMenu.playerTwoPelletsConsumed = 0;
        }

        GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeBlue;

        didIncrementLevel = false;

        StartCoroutine(ProcessStartNextLevel(1));
    }

    IEnumerator ProcessStartNextLevel(float delay)
    {
        playerText.transform.GetComponent<Text>().enabled = true;
        readyText.transform.GetComponent<Text>().enabled = true;

        if (GameMenu.isOnePlayerGame)
        {
            StartCoroutine(StartBlinking(playerOneUp));
        }
        else
        {
            if (isPlayerOneUp)
            {
                StartCoroutine(StartBlinking(playerOneUp));
            }
            else
            {
                StartCoroutine(StartBlinking(playerTwoUp));
            }
        }

        RedrawBoard();

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessRestartShowObjects(1));
    }

    public void StartGame()
    {
        if (GameMenu.isOnePlayerGame)
        {
            playerTwoUp.GetComponent<Text>().enabled = false;
            playerTwoScoreText.GetComponent<Text>().enabled = false;
        }
        else
        {
            playerText.transform.GetComponent<Text>().text = "PLAYER 1";
            playerTwoUp.GetComponent<Text>().enabled = true;
            playerTwoScoreText.GetComponent<Text>().enabled = true;
        }

        if(GameMenu.isOnePlayerGame)
        {
            StartCoroutine(StartBlinking(playerOneUp));
        }
        else
        {
            if (isPlayerOneUp)
            {
                StartCoroutine(StartBlinking(playerOneUp));
            }
            else
            {
                StartCoroutine(StartBlinking(playerTwoUp));
            }
        }

        // Hide all ghosts
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }

        // Hide pac-man
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        pacMan.transform.GetComponent<PacMan>().canMove = false;

        StartCoroutine(ShowObjectsAfter(2.25f));
    }

    public void StartConsumed(Ghost consumedGhost)
    {
        if (!didStartConsumed)
        {
            didStartConsumed = true;

            // Pause all the ghosts
            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }

            // Pause pac-man
            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;

            // Hide pac-man
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            // Hide consumed ghosts
            consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = false;

            // Stop background music
            transform.GetComponent<AudioSource>().Stop();

            Vector2 pos = consumedGhost.transform.position;

            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

            consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

            consumedGhostScoreText.GetComponent<Text>().enabled = true;

            // Play the consumed sound
            transform.GetComponent<AudioSource>().PlayOneShot(consumedGhostAudioClip);

            // Wait for the audio clip to finish
            StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }

    IEnumerator StartBlinking(Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);

        blinkText.GetComponent<Text>().enabled = !blinkText.GetComponent<Text>().enabled;
        StartCoroutine(StartBlinking(blinkText));
    }

    IEnumerator ProcessConsumedAfter(float delay, Ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);

        // Hide the score
        consumedGhostScoreText.GetComponent<Text>().enabled = false;

        // Show pac-man
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        // Show consumed ghost
        consumedGhost.GetComponent<SpriteRenderer>().enabled = true;

        // Resume all ghosts
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }

        // Resume pac-man
        pacMan.transform.GetComponent<PacMan>().canMove = true;

        // Start background music
        transform.GetComponent<AudioSource>().Play();

        didStartConsumed = false;
    }

    IEnumerator ShowObjectsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        playerText.transform.GetComponent<Text>().enabled = false;

        StartCoroutine(StartGameAfter(2));
    }

    IEnumerator StartGameAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = true;

        readyText.transform.GetComponent<Text>().enabled = false;

        transform.GetComponent<AudioSource>().clip = backgroundAudioNormal;
        transform.GetComponent<AudioSource>().Play();
    }

    public void StartDeath()
    {
        if (!didStartDeath)
        {
            StopAllCoroutines();

            if (GameMenu.isOnePlayerGame)
            {
                playerOneUp.GetComponent<Text>().enabled = true;
                playerTwoUp.GetComponent<Text>().enabled = false;
            }
            else
            {
                playerOneUp.GetComponent<Text>().enabled = true;
                playerTwoUp.GetComponent<Text>().enabled = true;
            }

            didStartDeath = true;

            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }

            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;

            pacMan.transform.GetComponent<Animator>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            StartCoroutine(ProcessDeathAfter(2));
        }
    }

    IEnumerator ProcessDeathAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        StartCoroutine(ProcessDeathAnimation(1.9f));
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.localScale = new Vector3(1, 1, 1);
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);

        pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<PacMan>().deathAnimation;
        pacMan.transform.GetComponent<Animator>().enabled = true;

        transform.GetComponent<AudioSource>().clip = backgroundAudioPacManDeath;
        transform.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessRestart(1));
    }

    IEnumerator ProcessRestart(float delay)
    {
        if(isPlayerOneUp)
        {
            GameMenu.livesPlayerOne -= 1;
        }
        else
        {
            GameMenu.livesPlayerTwo -= 1;
        }

        if (!GameMenu.isOnePlayerGame)
        {
            if (GameMenu.livesPlayerOne == 0 && GameMenu.livesPlayerTwo == 0)
            {
                playerText.transform.GetComponent<Text>().enabled = true;

                readyText.transform.GetComponent<Text>().text = "GAME OVER";
                readyText.transform.GetComponent<Text>().color = Color.red;

                readyText.transform.GetComponent<Text>().enabled = true;

                GameObject pacMan = GameObject.Find("PacMan");
                pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

                transform.GetComponent<AudioSource>().Stop();

                StartCoroutine(ProcessGameOver(2));
            }
            else if (GameMenu.livesPlayerOne == 0 || GameMenu.livesPlayerTwo == 0)
            {
                if (GameMenu.livesPlayerOne == 0)
                {
                    playerText.transform.GetComponent<Text>().text = "PLAYER 1";
                }
                else if (GameMenu.livesPlayerTwo == 0)
                {
                    playerText.transform.GetComponent<Text>().text = "PLAYER 2";
                }

                readyText.transform.GetComponent<Text>().text = "GAME OVER";
                readyText.transform.GetComponent<Text>().color = Color.red;

                readyText.transform.GetComponent<Text>().enabled = true;
                playerText.transform.GetComponent<Text>().enabled = true;

                GameObject pacMan = GameObject.Find("PacMan");
                pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

                transform.GetComponent<AudioSource>().Stop();

                yield return new WaitForSeconds(delay);

                if (!GameMenu.isOnePlayerGame)
                {
                    isPlayerOneUp = !isPlayerOneUp;
                }

                if (isPlayerOneUp)
                {
                    StartCoroutine(StartBlinking(playerOneUp));
                }
                else
                {
                    StartCoroutine(StartBlinking(playerTwoUp));
                }

                RedrawBoard();

                if (isPlayerOneUp)
                {
                    playerText.transform.GetComponent<Text>().text = "PLAYER 1";
                }
                else
                {
                    playerText.transform.GetComponent<Text>().text = "PLAYER 2";
                }

                readyText.transform.GetComponent<Text>().text = "READY!";
                readyText.transform.GetComponent<Text>().color = new Color(240f / 255f, 207f / 255f, 101f / 255f);

                yield return new WaitForSeconds(delay);

                StartCoroutine(ProcessRestartShowObjects(2));
            }
            else
            {
                playerText.transform.GetComponent<Text>().enabled = true;
                readyText.transform.GetComponent<Text>().enabled = true;

                GameObject pacMan = GameObject.Find("PacMan");
                pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

                transform.GetComponent<AudioSource>().Stop();

                if (!GameMenu.isOnePlayerGame)
                {
                    isPlayerOneUp = !isPlayerOneUp;
                }

                if (isPlayerOneUp)
                {
                    StartCoroutine(StartBlinking(playerOneUp));
                }
                else
                {
                    StartCoroutine(StartBlinking(playerTwoUp));
                }

                if (!GameMenu.isOnePlayerGame)
                {
                    if (isPlayerOneUp)
                    {
                        playerText.transform.GetComponent<Text>().text = "PLAYER 1";
                    }
                    else
                    {
                        playerText.transform.GetComponent<Text>().text = "PLAYER 2";
                    }
                }

                RedrawBoard();

                yield return new WaitForSeconds(delay);

                StartCoroutine(ProcessRestartShowObjects(1));
            }
        }
        else
        {
            if (GameMenu.livesPlayerOne == 0)
            {
                playerText.transform.GetComponent<Text>().enabled = true;

                readyText.transform.GetComponent<Text>().text = "GAME OVER";
                readyText.transform.GetComponent<Text>().color = Color.red;

                readyText.transform.GetComponent<Text>().enabled = true;

                GameObject pacMan = GameObject.Find("PacMan");
                pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

                transform.GetComponent<AudioSource>().Stop();

                StartCoroutine(ProcessGameOver(2));
            }
            else
            {
                playerText.transform.GetComponent<Text>().enabled = true;
                readyText.transform.GetComponent<Text>().enabled = true;

                GameObject pacMan = GameObject.Find("PacMan");
                pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

                transform.GetComponent<AudioSource>().Stop();

                yield return new WaitForSeconds(delay);

                StartCoroutine(ProcessRestartShowObjects(1));
            }
        }
    }

    IEnumerator ProcessGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene("GameMenu");

        // Reset score
        playerOneScore = 0;
        playerTwoScore = 0;

        // Reset first player
        isPlayerOneUp = true;
    }

    IEnumerator ProcessRestartShowObjects(float delay)
    {
        playerText.transform.GetComponent<Text>().enabled = false;

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
            ghost.transform.GetComponent<Animator>().enabled = true;
            ghost.transform.GetComponent<Ghost>().MoveToStartingPosition();
        }

        GameObject pacMan = GameObject.Find("PacMan");

        pacMan.transform.GetComponent<Animator>().enabled = false;
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
        pacMan.transform.GetComponent<PacMan>().MoveToStartingPosition();

        yield return new WaitForSeconds(delay);

        Restart();
    }

    public void Restart()
    {
        int playerLevel = 0;

        if(isPlayerOneUp)
        {
            playerLevel = playerOneLevel;
        }
        else
        {
            playerLevel = playerTwoLevel;
        }

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().SetDifficultyForLevel(playerLevel);
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().SetDifficultyForLevel(playerLevel);

        readyText.transform.GetComponent<Text>().enabled = false;

        pacMan.transform.GetComponent<PacMan>().Restart();

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().Restart();
        }

        transform.GetComponent<AudioSource>().clip = backgroundAudioNormal;
        transform.GetComponent<AudioSource>().Play();

        didStartDeath = false;
    }

    void ResetPelletsForPlayer(int playerNum)
    {
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject o in objects)
        {
            if (o.GetComponent<Tile>() != null)
            {
                if (o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isSuperPellet)
                {
                    if (playerNum == 1)
                    {
                        o.GetComponent<Tile>().didConsumePlayerOne = false;
                    }
                    else
                    {
                        o.GetComponent<Tile>().didConsumePlayerTwo = false;
                    }
                }
            }
        }
    }

    void RedrawBoard()
    {
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject o in objects)
        {
            if (o.GetComponent<Tile>() != null)
            {
                if (o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isSuperPellet)
                {
                    if(isPlayerOneUp)
                    {
                        if(o.GetComponent<Tile>().didConsumePlayerOne)
                        {
                            o.GetComponent<SpriteRenderer>().enabled = false;
                        }
                        else
                        {
                            o.GetComponent<SpriteRenderer>().enabled = true;
                        }
                    }
                    else
                    {
                        if (o.GetComponent<Tile>().didConsumePlayerTwo)
                        {
                            o.GetComponent<SpriteRenderer>().enabled = false;
                        }
                        else
                        {
                            o.GetComponent<SpriteRenderer>().enabled = true;
                        }
                    }
                }
            }
        }
    }
}
