using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject network;
    public GameObject menu;
    bool isActive;
    public void ToggleMenu(bool shouldBeActive)
    {
        menu.SetActive(shouldBeActive);
    }
    void Update()
    {
        if (network.GetComponent<NetworkManager>().connectedToLobby)
        {
            ToggleMenu(false);
        }
    }
}
