using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    public float maxHunger;
    public float hunger;
    [SerializeField] private float hungerLoss;
    [SerializeField] private GameObject hungerBar;
    [SerializeField] private GameObject preyObj;

    [SerializeField] private float spawnDelay;
    [SerializeField] private float spawnTimer;

    void Start()
    {
        hunger = maxHunger;
    }

    void Update()
    {
        hungerBar.GetComponent<Image>().fillAmount = hunger/maxHunger;
        //Debug.Log(Input.GetAxis("Horizontal") + " : " + Input.GetAxis("Vertical"));
        GetComponent<Rigidbody2D>().velocity = new Vector2(speed*Input.GetAxisRaw("Horizontal"), speed*Input.GetAxisRaw("Vertical"));
        hunger -= Time.deltaTime * hungerLoss;

        //Spawn
        spawnTimer -= Time.deltaTime;
        if (spawnTimer < 0)
        {
            spawnTimer = spawnDelay;
            Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
            dir = Vector3.Normalize(dir);
            float dist = Random.Range(3, 10);
            Instantiate(preyObj, transform.position + dir*dist, Quaternion.identity);
        }
    }
}
