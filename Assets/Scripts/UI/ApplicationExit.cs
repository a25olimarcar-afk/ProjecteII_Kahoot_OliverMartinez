using UnityEngine;

public class ApplicationExit : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Closing Kahoot Game...");
        Application.Quit(); //salir del juego

        //salir tambien si estamos dentro del editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}