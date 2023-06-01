using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public UdpReceiver udpReceive;
    public GameSystem gameSystem;

    private int previousCommand = 0;
    private int currentCommand = 0;

    private float playerXPosition;

    [HideInInspector]
    public Animator AnimC;
    [HideInInspector]
    public bool isHitObstacle = false;

    // Start is called before the first frame update
    void Start()
    {
        AnimC = GetComponent<Animator>();

        playerXPosition = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        int[] data = udpReceive.receivedArray;

        if (data.Length <= 0 || data.Contains(9))
        {
            return;
        }

        if (data[0] == 1)
        {
            // Move character forward in Z axis
            // transform.Translate(0, 0, 0.03f);
            // transform.position = new Vector3(playerXPosition + float.Parse(points[2]), 0, 0) * Time.deltaTime;

            if (isHitObstacle)
            {
                return;
            }
            if (!gameSystem.currentGamePlay)
            {
                return;
            }

            transform.Translate(0, 0, 0.02f);
            // transform.position = new Vector3(playerXPosition + float.Parse(points[2]), 0, 0) * Time.deltaTime;

            if (data[2] == 1 && previousCommand == 0)
            {
                currentCommand = 1;
                AnimC.SetBool("Jump", true);
                transform.Translate(0, 0.05f, 0.02f);
            }
            else if (data[2] == 2 && previousCommand == 0)
            {
                currentCommand = 2;
                AnimC.SetBool("Slide", true);
                transform.Translate(0, -0.02f, 0.02f);
            }
            else if (data[2] == 0 && previousCommand == 2)
            {
                currentCommand = 0;
                transform.Translate(0, 0.02f, 0.02f);
            }
            else
            {
                currentCommand = 0;
                AnimC.SetBool("Jump", false);
                AnimC.SetBool("Slide", false);
            }
            previousCommand = currentCommand;
        }
        else
        {
            print("No detected body");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            isHitObstacle = true;
            gameSystem.currentGamePlay = false;
        }
    }
}