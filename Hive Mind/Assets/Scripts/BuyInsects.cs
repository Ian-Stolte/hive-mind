using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyInsects : MonoBehaviour
{
    public void BuyInsect(int index)
    {
        GameObject.Find("Game Manager").GetComponent<GameManager>().BuyInsect(index);
    }
}
