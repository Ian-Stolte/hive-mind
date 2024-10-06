using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject target;
    [SerializeField] private float speed;
    [SerializeField] private float attackDmg;

    void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }
        else
        {
            Vector3 dir = target.transform.position - transform.position;
            dir = new Vector3(dir.x, dir.y, 0);
            dir = Vector3.Normalize(dir);
            transform.position += dir*speed/60;

            if (Vector2.Distance(transform.position, target.transform.position) < 1)
            {
                EnemyBehavior enemy = target.GetComponent<EnemyBehavior>();
                enemy.health -= attackDmg;
                enemy.LoseHealth();
                Destroy(gameObject);
            }
        }
    }
}
