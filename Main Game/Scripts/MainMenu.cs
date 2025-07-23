using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public void ExitButton() 
    {
        Application.Quit();
        Debug.Log("Game closed");
    }

    public void StartGame ()
    {
        SceneManager.LoadScene("Game");
    }
}
