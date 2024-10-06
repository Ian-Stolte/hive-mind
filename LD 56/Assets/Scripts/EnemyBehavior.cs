using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private GameObject insects;

    [SerializeField] private float speed;
    
    public float maxHealth;
    public float health;

    [SerializeField] private Color stunnedColor;
    private float stunTimer;

    void Start()
    {
        health = maxHealth;
        insects = GameObject.Find("Insects");
    }

    void Update()
    {
        float minDistance = 9999;
        foreach (Transform child in insects.transform)
        {
            float dist = Vector2.Distance(child.position, transform.position);
            if (dist < 1.5f)
            {
                InsectBehavior.types type = child.GetComponent<InsectBehavior>().type;
                if (!GameObject.Find("Scene Loader").GetComponent<SceneLoader>().inTransition)
                    GameObject.Find("Game Manager").GetComponent<GameManager>().insectCounts[(int)type]--;
                Destroy(child.gameObject);
                GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Ant Die");
            }
            else if (dist < minDistance)
            {
                minDistance = dist;
                target = child.gameObject;
            }
        }

        stunTimer -= Time.deltaTime;
        if (stunTimer > 0)
            GetComponent<SpriteRenderer>().color = stunnedColor;
        else
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
    }

    void FixedUpdate()
    {
        if (stunTimer < 0)
        {
            if (target != null)
            {
                Vector3 dir = target.transform.position - transform.position;
                dir = new Vector3(dir.x, dir.y, 0);
                dir = Vector3.Normalize(dir);
                transform.position += speed*dir/60;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle-90));
            }
        }
    }

    public void LoseHealth()
    {
        Transform healthBar = transform.GetChild(1);
        healthBar.localScale = new Vector3(health/maxHealth, healthBar.localScale.y, healthBar.localScale.z);
        healthBar.localPosition = new Vector3(-0.5f + 0.5f*health/maxHealth, healthBar.localPosition.y, healthBar.localPosition.z);
        if (health <= 0)
        {
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Beetle Die");
            foreach (Transform child in insects.transform)
            {
                if (child.GetComponent<InsectBehavior>().attackTarget == gameObject)
                {
                    child.GetComponent<InsectBehavior>().attackTarget = null;
                    child.GetComponent<InsectBehavior>().mode = "IDLE";
                    child.GetComponent<SpriteRenderer>().color = child.GetComponent<InsectBehavior>().idleColor; 
                }
            }
            Destroy(gameObject);
        }
        stunTimer = 0.25f;
    }
}
