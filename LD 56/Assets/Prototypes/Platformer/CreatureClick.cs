using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureClick : MonoBehaviour
{
    [SerializeField] Color[] colors;
    private string[] modes = new string[]{"IDLE", "JUMP", "MOVERIGHT"/*, "IDLE", "MOVELEFT"*/};
    private int currentMode;

    private void OnMouseDown()
    {
        //Debug.Log("GameObject clicked: " + gameObject.name);
        currentMode = (currentMode+1)%modes.Length;
        Debug.Log(modes[currentMode]);
        GetComponent<SpriteRenderer>().color = colors[currentMode];
    }

    [SerializeField] float jumpDelay;
    [SerializeField] float jumpForce;
    [SerializeField] private float jumpTimer;
    [SerializeField] float speed;

    void Update()
    {
        jumpTimer -= Time.deltaTime;
        if (modes[currentMode] == "JUMP" && jumpTimer < 0)
        {
            GetComponent<Rigidbody2D>().AddForce(jumpForce*Vector2.up);
            jumpTimer = jumpDelay;
        }
        if (modes[currentMode] == "MOVERIGHT")
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(speed, GetComponent<Rigidbody2D>().velocity.y);
        }
        else if (modes[currentMode] == "MOVELEFT")
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(-speed, GetComponent<Rigidbody2D>().velocity.y);
        }
        else
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
        }



        /*if (Input.GetMouseButtonDown(0) && MouseOverlap())
        {
            Debug.Log("Left button!");
        }
        else if (Input.GetMouseButtonDown(1) && MouseOverlap())
        {
            Debug.Log("Right button!");
        }*/
    }

    /*private bool MouseOverlap()
    {
        Camera cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform a raycast
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the collider hit is the one we are interested in
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }
        return false;
    }*/
}
