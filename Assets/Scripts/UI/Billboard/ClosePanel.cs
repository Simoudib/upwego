using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    public GameObject panel;

    public void Close()
    {
        panel.SetActive(false);
        Time.timeScale = 1f; // Resume game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
