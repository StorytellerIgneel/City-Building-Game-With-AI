using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame(){
        SceneManager.LoadSceneAsync(1); //either scene name or build index
    }

    public void QuitGame(){
        Debug.Log("Quitting the game..."); //optional log message   
        Application.Quit(); //quit the application immediately
    }
}
