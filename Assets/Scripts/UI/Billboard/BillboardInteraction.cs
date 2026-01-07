using UnityEngine;

public class BillboardInteraction : MonoBehaviour
{
    public GameObject panel;
    public KeyCode interactKey = KeyCode.F;

    private bool playerInRange;
    private bool panelOpen;

    void Update()
    {
        if (!playerInRange || panelOpen)
            return;

        if (Input.GetKeyDown(interactKey))
            OpenPanel();
    }

    void OpenPanel()
    {
        panelOpen = true;
        panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseFromButton()
    {
        panelOpen = false;
        panel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null)
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null)
            playerInRange = false;
    }
}
