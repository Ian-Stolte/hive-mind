using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalFood;
    [SerializeField] private int hungerDelay;
    [SerializeField] private int hungerTimer;

    private int enemyHealth;

    [SerializeField] private bool[] levelsSpawned = new bool[4];
    public int timesExplored;
    
    public int[] insectCounts;
    [SerializeField] private int[] costs;
    [SerializeField] private GameObject[] insectPrefabs;
    [SerializeField] private Color purchasableColor;
    [SerializeField] private Color tooExpensiveColor;

    private float foodSpawnTimer;
    [SerializeField] private float foodBaseDelay;
    [SerializeField] private float foodDelayMultiplier;
    [SerializeField] private GameObject foodPrefab;

    private float enemySpawnTimer;
    [SerializeField] private float enemyDelay;
    [SerializeField] private GameObject enemyPrefab;

    private bool gameOver;

    //Tutorial
    private GameObject exploreButton;
    [SerializeField] private bool skipTutorial;
    private bool boughtInsect;
    private bool firstFight = true;
    private bool firstHunger = true;
    [HideInInspector] public bool inTutorial;
    public int foodDeposited;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Battle")
        {
            foodSpawnTimer = 2;
            //difficulty based on # times explored
            timesExplored++;
            if (timesExplored == 1)
            {
                foodBaseDelay = 8;
                foodDelayMultiplier = 2.5f;
            }
            else if (timesExplored < 6)
            {
                foodBaseDelay = 6;
                foodDelayMultiplier = 2f;
            }
            else
            {
                foodBaseDelay = 4;
                foodDelayMultiplier = 1.5f;
            }
            if (timesExplored < 3)
            {
                enemySpawnTimer = 15;
                enemyDelay = 40;
                enemyHealth = 12;
            }
            else if (timesExplored < 5)
            {
                enemySpawnTimer = 5;
                enemyDelay = 30;
                enemyHealth = 20;
            }
            else if (timesExplored < 8)
            {
                enemySpawnTimer = 5;
                enemyDelay = 20;
                enemyHealth = 25;
            }
            else
            {
                enemyHealth = 40;
            }
            
            //choose level setup
            int choices = GameObject.Find("Areas").transform.childCount;
            int chosenArea = Random.Range(0, choices);
            while(levelsSpawned[chosenArea])
            {
                chosenArea = Random.Range(0, choices);
            }
            levelsSpawned[chosenArea] = true;
            //reset count of levels we've done
            bool allLevelsDone = true;
            for (int i = 0; i < levelsSpawned.Length; i++)
            {
                if (!levelsSpawned[i])
                    allLevelsDone = false;
            }
            if (allLevelsDone)
            {
                for (int i = 0; i < levelsSpawned.Length; i++)
                {
                    levelsSpawned[i] = false;
                }
            }
            for (int i = 0; i < choices; i++)
            {
                GameObject.Find("Areas").transform.GetChild(i).gameObject.SetActive(i == chosenArea);
            }
            if (firstFight)
            {
                firstFight = false;
                if (!skipTutorial)
                    StartCoroutine(Tutorial());
            }
            if (timesExplored >= 3 && foodDeposited >= 3)
                GameObject.Find("Food Deposit").transform.GetChild(0).gameObject.SetActive(false);
            SpawnInsects();
            GameObject.Find("Food Count").GetComponent<TMPro.TextMeshProUGUI>().text = "Food: " + totalFood;
        }
        if (scene.name == "Colony")
        {
            exploreButton = GameObject.Find("Explore Button");
            exploreButton.SetActive(boughtInsect);
            hungerTimer--;
            if (hungerTimer == 0)
            {
                GameObject.Find("Hunger Warning").GetComponent<CanvasGroup>().alpha = 1;
                if (firstHunger)
                {
                    if (!skipTutorial)
                        StartCoroutine(Tutorial());
                    firstHunger = false;
                }
            }
            else
            {
                GameObject.Find("Hunger Warning").GetComponent<CanvasGroup>().alpha = 0;
            }
            if (hungerTimer == -1)
            {
                hungerTimer = hungerDelay;
                GameObject.Find("Hunger Report").GetComponent<CanvasGroup>().alpha = 1;
                GameObject.Find("Hunger Report").GetComponent<CanvasGroup>().blocksRaycasts = true;
                int numInsects = 0;
                for (int i = 0; i < insectCounts.Length; i++)
                {
                    numInsects += insectCounts[i];
                }
                if (totalFood > numInsects)
                {
                    totalFood -= numInsects;
                    GameObject.Find("Hunger Report Text").GetComponent<TMPro.TextMeshProUGUI>().text = "The colony was fed.";
                }
                else
                {
                    numInsects -= totalFood;
                    totalFood = 0;
                    int[] insectLosses = new int[insectCounts.Length];
                    while (numInsects > 0)
                    {
                        if (insectCounts[1] > 0)
                        {
                            insectCounts[1]--;
                            insectLosses[1]++;
                        }
                        else if (insectCounts[0] > 1)
                        {
                            insectCounts[0]--;
                            insectLosses[0]++;
                        }
                        else
                        {
                            gameOver = true;
                            StartCoroutine(GameObject.Find("Scene Loader").GetComponent<SceneLoader>().LoadScene("Game Over"));
                            //show reason for death in game over screen
                        }
                        numInsects--;
                    }
                    GameObject.Find("Hunger Report Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Part of the colony starved:\n  - " + insectLosses[0] + " Gatherers\n  - " + insectLosses[1] + " Soldiers";                    
                }
                StartCoroutine(HideHungerReport());
            }
            UpdateColonyCounts();
        }
    }

    private IEnumerator HideHungerReport()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
        GameObject.Find("Hunger Report").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("Hunger Report").GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            skipTutorial = true;
        else if (Input.GetKeyDown(KeyCode.T))
        {
            firstFight = true;
            firstHunger = true;
            skipTutorial = false;
        }

        if (SceneManager.GetActiveScene().name == "Colony")
        {
            Transform parent = GameObject.Find("Ant Types").transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (totalFood >= costs[i])
                {
                    parent.GetChild(i).GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().color = purchasableColor;
                    parent.GetChild(i).GetChild(3).GetComponent<Button>().interactable = true;
                }
                else
                {
                    parent.GetChild(i).GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().color = tooExpensiveColor;
                    parent.GetChild(i).GetChild(3).GetComponent<Button>().interactable = false;
                }
            }
        }

        else if (SceneManager.GetActiveScene().name == "Battle" && !inTutorial)
        {
            //Check game over
            if ((GameObject.Find("Insects").transform.childCount == 0 || (insectCounts[0] == 0 && totalFood < 2)) && !gameOver)
            {
                gameOver = true;
                StartCoroutine(GameObject.Find("Scene Loader").GetComponent<SceneLoader>().LoadScene("Game Over"));
            }


            //Spawn food
            foodSpawnTimer -= Time.deltaTime;
            if (foodSpawnTimer < 0)
            {
                foodSpawnTimer = Mathf.Max(foodBaseDelay + GameObject.FindGameObjectsWithTag("Food").Length*foodDelayMultiplier - insectCounts[0], 2);
                foodBaseDelay++;
                Vector2 position = Vector2.zero;
                bool safeSpawn = false;           
                while (!safeSpawn)
                {
                    position = new Vector2(Random.Range(-11f, 11f), Random.Range(-6f, 6f));
                    safeSpawn = true;
                    foreach (GameObject g in GameObject.FindGameObjectsWithTag("Obstacle"))
                    {
                        if (g.GetComponent<BoxCollider2D>().OverlapPoint(position))
                        {
                            safeSpawn = false;
                            break;
                        }
                    }
                }
                Instantiate(foodPrefab, position, Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), GameObject.Find("Food").transform);
            }


            //Spawn enemies
            enemySpawnTimer -= Time.deltaTime;
            if (enemySpawnTimer < 0)
            {
                enemySpawnTimer = enemyDelay;
                if (enemyDelay > 6)
                    enemyDelay = Mathf.Max(7, enemyDelay*0.7f);
                else if (enemyDelay == 6)
                    enemyDelay = 2;
                else if (enemyDelay == 2)
                    enemyDelay = 12;
                GameObject enemy = Instantiate(enemyPrefab, new Vector2(Random.Range(-8f, 8f), 8), Quaternion.identity, GameObject.Find("Enemies").transform);
                enemy.GetComponent<EnemyBehavior>().health = enemyHealth;
                enemyHealth += 3;
            }
        }
    }

    public void RestartValues()
    {
        totalFood = 3;
        gameOver = false;
        insectCounts = new int[]{1, 1};
        hungerTimer = 3;
    }

    public void BuyInsect(int index)
    {
        boughtInsect = true;
        exploreButton.SetActive(true);
        totalFood -= costs[index];
        insectCounts[index]++;
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Buy Ants");
        UpdateColonyCounts();
    }

    private void UpdateColonyCounts()
    {
        Transform parent = GameObject.Find("Ant Types").transform;
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).GetComponent<TMPro.TextMeshProUGUI>().text = parent.GetChild(i).name + ": <u>" + insectCounts[i] + "</u>";
        }
        GameObject.Find("Food Count").GetComponent<TMPro.TextMeshProUGUI>().text = "Food: <u>" + totalFood + "</u>";
    }

    private void SpawnInsects()
    {
        for (int i = 0; i < insectCounts.Length; i++)
        {
            for (int j = 0; j < insectCounts[i]; j++)
            {
                Vector2 position = Vector2.zero;
                bool safeSpawn = false;           
                while (!safeSpawn)
                {
                    position = new Vector2(Random.Range(-8f, 8f), Random.Range(-6f, 0));
                    safeSpawn = true;
                    if (position.x < -7 && position.y < -5)
                        safeSpawn = false;
                    foreach (GameObject g in GameObject.FindGameObjectsWithTag("Obstacle"))
                    {
                        if (g.GetComponent<BoxCollider2D>().OverlapPoint(position))
                        {
                            safeSpawn = false;
                            break;
                        }
                    }
                    foreach (Transform child in GameObject.Find("Insects").transform)
                    {
                        if (child.GetComponent<CircleCollider2D>().OverlapPoint(position))
                        {
                            safeSpawn = false;
                            break;
                        }
                    }
                }
                GameObject insect = Instantiate(insectPrefabs[i], position, Quaternion.identity, GameObject.Find("Insects").transform);
                insect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-90, 90));
            }
        }
    }
    
    private IEnumerator Tutorial()
    {
        inTutorial = true;
        GameObject.Find("Tutorial").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("Tutorial").GetComponent<CanvasGroup>().blocksRaycasts = true;
        foreach (Transform child in GameObject.Find("Tutorial").transform)
        {
            if (!skipTutorial)
            {
                yield return new WaitForSeconds(0.2f);
                child.gameObject.SetActive(true);
                if (child.transform.childCount > 1)
                {
                    yield return new WaitForSeconds(1);
                    child.GetChild(1).gameObject.SetActive(true);
                }
                yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.S));
                child.gameObject.SetActive(false);
            }
        }
        inTutorial = false;
        GameObject.Find("Tutorial").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("Tutorial").GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
