using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public static bool isOnePlayerGame = true;

    public static int livesPlayerOne;
    public static int livesPlayerTwo;

    public static int playerOnePelletsConsumed = 0;
    public static int playerTwoPelletsConsumed = 0;

    public Text playerTextOne;
    public Text playerTextTwo;
    public Text playerSelector;

    void Start()
    {
        isOnePlayerGame = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.UpArrow))
        {
            if(!isOnePlayerGame)
            {
                isOnePlayerGame = true;
                playerSelector.transform.localPosition = new Vector3(playerSelector.transform.localPosition.x, playerTextOne.transform.localPosition.y, playerSelector.transform.localPosition.z);
            }
        }
        else if(Input.GetKeyUp(KeyCode.DownArrow))
        {
            if (isOnePlayerGame)
            {
                isOnePlayerGame = false;
                playerSelector.transform.localPosition = new Vector3(playerSelector.transform.localPosition.x, playerTextTwo.transform.localPosition.y, playerSelector.transform.localPosition.z);
            }
        }
        else if (Input.GetKeyUp(KeyCode.Return))
        {
            livesPlayerOne = 3;
            livesPlayerTwo = 3;

            if(isOnePlayerGame)
            {
                livesPlayerTwo = 0;
            }

            SceneManager.LoadScene("Level1");
        }
    }
}
