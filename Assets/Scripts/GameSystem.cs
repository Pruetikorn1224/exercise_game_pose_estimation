using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSystem : MonoBehaviour
{
    [HideInInspector] public bool currentGamePlay = false;

    public PlayerController playerController;
    public UdpReceiver udpReceive;

    public GameObject beginScreen;
    public GameObject noPythonScreen;
    public GameObject gameOverScreen;
    public GameObject scoreText;
    public TextMeshProUGUI score_text;

    public int score = 0;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        currentGamePlay = false;
    }

    // Update is called once per frame
    void Update()
    {
        int[] data = udpReceive.receivedArray;

        // If there is no data or the data has kill code -> 9
        if (data.Length <= 0 || data.Contains(9))
        {
            currentGamePlay = false;
            noPythonScreen.SetActive(true);
            return;
        }

        if (playerController.isHitObstacle)
        {
            currentGamePlay = false;
            gameOverScreen.SetActive(true);

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                currentGamePlay = true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
            }
            return;
        }

        // If system receives data and currently the game is not running
        if (data.Length > 0 && !currentGamePlay)
        {
            StartCoroutine(CountDown());

            currentGamePlay = true;
            noPythonScreen.SetActive(false);
        }

        timer += Time.deltaTime;
        score = (int)timer;

        score_text.text = "Score: " + score.ToString();
    }

    IEnumerator CountDown()
    {
        int countDownTimer = 5;

        beginScreen.SetActive(true);
        scoreText.SetActive(false);
        for (int i = countDownTimer; i > 0; i--)
        {
            TextMeshProUGUI begin_text = GameObject.Find("time").GetComponent<TextMeshProUGUI>();
            begin_text.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        score = 0;
        beginScreen.SetActive(false);
        scoreText.SetActive(true);
    }
}
