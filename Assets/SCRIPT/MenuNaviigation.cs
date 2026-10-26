using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuNaviigation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void goToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }
}
