using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject membersScreen;
    public GameObject mapScreen;

    void Start()
    {
        // At the start, the menu will show up first, while the others are set unactive
        startScreen.SetActive(true);
        mapScreen.SetActive(false);
        membersScreen.SetActive(false);
    }

    // Open member list
    public void OnMemberButtonClick()
    {
        startScreen.SetActive(false);
        membersScreen.SetActive(true);
    }

    // Return to start menu
    public void OnReturnButtonClick()
    {
        startScreen.SetActive(true);
        mapScreen.SetActive(false);
        membersScreen.SetActive(false);
    }

    // Open map selector
    public void OnMapButtonClick()
    {
        startScreen.SetActive(false);
        mapScreen.SetActive(true);
    }

    // Select map 1
    public void OnMapOneClick()
    {
        SceneManager.LoadScene(1);
    }
}
