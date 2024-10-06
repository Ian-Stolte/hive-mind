using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyBehavior : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private Color runColor;
    [SerializeField] private Color idleColor;

    [SerializeField] private float speed;
    [SerializeField] private float foodAmount;
    
    void Start()
    {
        player = GameObject.Find("Player");
    }

    void Update()
    {
        float dist = Vector2.Distance(player.transform.position, transform.position);
        if (dist <= 5)
        {
            //run
            GetComponent<SpriteRenderer>().color = runColor;
            GetComponent<Rigidbody2D>().velocity = speed * Vector3.Normalize(transform.position - player.transform.position);
        }
        else
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<SpriteRenderer>().color = idleColor;
        }

        if (dist <= 1.5f)
        {
            player.GetComponent<PlayerMovement>().hunger = Mathf.Min(player.GetComponent<PlayerMovement>().maxHunger, player.GetComponent<PlayerMovement>().hunger + foodAmount);
            Destroy(gameObject);
        }
    }
}
