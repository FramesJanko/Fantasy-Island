using TMPro;
using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject network;
    public GameObject menu;
    bool menuActive = true;
    bool lobbyNameSet;
    public TMP_Text lobbyName;
    public void ToggleMenu(bool shouldBeActive)
    {
        menu.SetActive(shouldBeActive);
    }
    void Update()
    {
        if (network.GetComponent<NetworkManager>().connectedToLobby)
        {
            if (menuActive)
            {
                menuActive = false;
                ToggleMenu(false);
            }
            if (!lobbyNameSet)
            {
                lobbyNameSet = true;
                lobbyName.text = network.GetComponent<NetworkManager>().connectedLobby;
            }
        }
    }
}
