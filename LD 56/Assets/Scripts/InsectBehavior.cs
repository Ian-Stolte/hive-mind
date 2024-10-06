using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InsectBehavior : MonoBehaviour
{
    public string mode = "IDLE";
    private bool activeInsect;
    private bool carryingFood;
    [SerializeField] private GameObject foodSprite;
    //[SerializeField] private GameObject foodIncrement;

    public enum types
    {
        GATHERER,
        SOLDIER
    }
    public types type;

    //Colors
    [SerializeField] private Color waitColor;
    public Color idleColor;
    [SerializeField] private Color attackColor;

    private Vector3 moveTarget;
    [SerializeField] private float speed;

    [SerializeField] private float attackDelay;
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackTimer;
    [HideInInspector] public GameObject attackTarget;
    [SerializeField] private GameObject attackObj;


    private void InsectClicked()
    {
        if (!GameObject.Find("Game Manager").GetComponent<GameManager>().inTutorial)
        {
            mode = "IDLE";
            StartCoroutine(SetWait());
        }
    }

    private IEnumerator SetWait()
    {
        yield return null;
        foreach (Transform child in transform.parent)
        {
            child.GetComponent<InsectBehavior>().activeInsect = false;
            if (child.GetComponent<InsectBehavior>().mode != "ATTACK")
                child.GetComponent<SpriteRenderer>().color = idleColor;
        }
        activeInsect = true;
        GetComponent<SpriteRenderer>().color = waitColor;
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Command Mode");
    }

    void Update()
    {
        //Check if clicked
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                    InsectClicked();
            }
        }

        //Selecting target of action
        if (activeInsect && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool otherInsect = false;
            foreach (Transform child in transform.parent)
            {
                if (child.GetComponent<CircleCollider2D>().OverlapPoint(mousePos))
                {
                    otherInsect = true; 
                }
            }
            if (!otherInsect)
            {
                attackTarget = null;
                foreach (Transform child in GameObject.Find("Enemies").transform)
                {
                    if (child.GetComponent<BoxCollider2D>().OverlapPoint(mousePos) && type == types.SOLDIER)
                    {
                        mode = "ATTACK";
                        GetComponent<SpriteRenderer>().color = attackColor;
                        //face target
                        Vector3 dir = child.position - transform.position;
                        dir = Vector3.Normalize(dir);
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle-90));
                        attackTarget = child.gameObject;
                        break;
                    }
                }
                if (attackTarget == null)
                {
                    mode = "MOVE";
                    GetComponent<SpriteRenderer>().color = idleColor;
                    moveTarget = mousePos;
                }
            }
        }

        //Movement
        if (mode == "MOVE")
        {
            MoveToward(moveTarget);
            if (Vector2.Distance(moveTarget, transform.position) < 0.5f)
            {
                mode = "IDLE";
                GetComponent<SpriteRenderer>().color = idleColor;
            }
        }
        else 
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }


        //Attack
        attackTimer -= Time.deltaTime;
        if (mode == "ATTACK")
        {
            if (Vector3.Distance(attackTarget.transform.position, transform.position) > attackDistance)
            {
                MoveToward(attackTarget.transform.position);
            }
            else if (attackTimer < 0)
            {
                attackTimer = attackDelay;
                GetComponent<Animator>().Play("InsectAttack");
                GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Ant Attack");
                float rads = (transform.eulerAngles.z-90) * Mathf.Deg2Rad;
                Vector3 dirFacing = new Vector3(Mathf.Cos(rads), Mathf.Sin(rads), 0);
                dirFacing = Vector3.Normalize(dirFacing);
                GameObject attack = Instantiate(attackObj, transform.position + dirFacing*0.5f, Quaternion.identity);
                attack.GetComponent<Attack>().target = attackTarget;
            }
        }


        //Pick up food
        if (Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Food")) && type == types.GATHERER && !carryingFood)
        {
            GameObject food = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Food")).gameObject;
            Destroy(food);
            carryingFood = true;
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Grab Food");
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Food Deposit")) && type == types.GATHERER && carryingFood)
        {
            GameObject.Find("Game Manager").GetComponent<GameManager>().totalFood++; //set based on amount of food
            GameObject.Find("Game Manager").GetComponent<GameManager>().foodDeposited++;
            //GameObject incrementObj = Instantiate(foodIncrement, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
            //incrementObj.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(transform.position);
            GameObject.Find("Food Count").GetComponent<TMPro.TextMeshProUGUI>().text = "Food: " + GameObject.Find("Game Manager").GetComponent<GameManager>().totalFood;
            carryingFood = false;
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Deposit Food");
            transform.GetChild(0).gameObject.SetActive(false);
            //place leaf on deposit
            GameObject foodDepot = GameObject.Find("Food Deposit");
            Vector3 offset = Vector3.zero;
            for (int i = 0; i < 5; i++)
            {
                offset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-0.5f, 0.5f), 0);
                bool touchingLeaf = false;
                foreach (Transform child in foodDepot.transform)
                {
                    if (Vector2.Distance(child.position, foodDepot.transform.position + offset) < 1f/i)
                        touchingLeaf = true;
                }
                if (!touchingLeaf)
                    break;
            }
            GameObject foodVisual = Instantiate(foodSprite, foodDepot.transform.position + offset, Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), foodDepot.transform);
        }


        //Idle actions
        if (mode == "IDLE" && !activeInsect)
        {
            if (type == types.SOLDIER)
            {
                foreach (Transform child in transform.parent)
                {
                    //Auto-attack
                    if (Vector2.Distance(child.position, transform.position) < 3 && child.GetComponent<InsectBehavior>().attackTarget != null)
                    {
                        int randomNum = Random.Range(0, 1000);
                        if (randomNum == 0)
                        {
                            attackTarget = child.GetComponent<InsectBehavior>().attackTarget;
                            mode = "ATTACK";
                            GetComponent<SpriteRenderer>().color = attackColor;
                            //face target
                            Vector3 dir = attackTarget.transform.position - transform.position;
                            dir = Vector3.Normalize(dir);
                            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle-90));
                        }
                    }
                }
            }
            else if (type == types.GATHERER && !carryingFood)
            {
                foreach (Transform food in GameObject.Find("Food").transform)
                {
                    if (Vector2.Distance(food.position, transform.position) < 5)
                    {
                        int randomNum = Random.Range(0, 1000);
                        if (randomNum == 0)
                        {
                            bool validTarget = true;
                            foreach (Transform child in transform.parent)
                            {
                                if (Vector2.Distance(food.position, child.GetComponent<InsectBehavior>().moveTarget) < 0.5f)
                                    validTarget = false;
                            }
                            if (validTarget)
                            {
                                moveTarget = food.position;
                                mode = "MOVE";
                            }
                        }
                    }
                }
            }
            //else if (type == types.GATHERER && carryingFood)
            // ...move toward the deposit (given LOS?)
        }
    }

    void MoveToward(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir = new Vector3(dir.x, dir.y, 0);
        dir = Vector3.Normalize(dir);
        bool safeMove = true;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            if (g.GetComponent<BoxCollider2D>().OverlapPoint(transform.position + dir*speed*Time.deltaTime + dir*0.5f))
            {
                safeMove = false;
                break;
            }
        }
        if (safeMove)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle-90));
            transform.position += dir*speed*Time.deltaTime;
        }
        else
        {
            mode = "IDLE";
            GetComponent<SpriteRenderer>().color = idleColor;
        }
    }
}
