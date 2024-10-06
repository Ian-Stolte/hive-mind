using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMove : MonoBehaviour
{
    [SerializeField] private float speedMin;
    [SerializeField] private float speedMax;

    private float currentSpeed;
    [SerializeField] private float speedChangeDelay;
    private float speedChangeTimer;

    void Start()
    {
        currentSpeed = Random.Range(speedMin, speedMax);
        speedChangeTimer = Random.Range(0f, speedChangeDelay);
    }

    void Update()
    {
        speedChangeTimer -= Time.deltaTime;
        if (speedChangeTimer < 0)
        {
            speedChangeTimer = speedChangeDelay;
            bool randomChange = true;
            foreach (Transform child in transform.parent)
            {
                float dist = child.GetComponent<RectTransform>().anchoredPosition.x - GetComponent<RectTransform>().anchoredPosition.x;
                if (dist > 0 && dist < 60)
                {
                    currentSpeed = speedMin;
                    randomChange = false;
                }
                else if (dist < 0 && dist > -60)
                {
                    currentSpeed = speedMax;
                    randomChange = false;
                }
            }
            if (randomChange)
                currentSpeed = Random.Range(speedMin, speedMax);
        }

        GetComponent<RectTransform>().anchoredPosition += Vector2.right*currentSpeed*Time.deltaTime;
        if (GetComponent<RectTransform>().anchoredPosition.x > 1000)
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-1000, GetComponent<RectTransform>().anchoredPosition.y);
        }
    }
}
