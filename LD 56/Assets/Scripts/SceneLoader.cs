using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [HideInInspector] public bool inTransition;

    public void StartGame()
    {
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press");
        StartCoroutine(LoadScene("Colony"));
    }
    
    public void GoToFight()
    {
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press");
       StartCoroutine(LoadScene("Battle"));
    }

    public void RestartGame()
    {
        GameObject.Find("Game Manager").GetComponent<GameManager>().RestartValues();
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press");
        StartCoroutine(LoadScene("Colony"));
    }

    public void Flee()
    {
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Flee");
        StartCoroutine(LoadScene("Colony"));
    }

    public IEnumerator LoadScene(string name)
    {
        inTransition = true;
        GameObject.Find("Fader").GetComponent<Animator>().Play("TransitionOut");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(name);
    }
}
