using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour
{
    public NetworkManager network;
    public GameObject menu;
    bool menuActive = true;
    bool lobbyNameSet;
    public TMP_Text lobbyName;
    public InputAction sendHostMessage;
    public void ToggleMenu(bool shouldBeActive)
    {
        menu.SetActive(shouldBeActive);
    }
    void Start()
    {
        sendHostMessage.Enable();
    }
    void Update()
    {
        if (network.connectedToLobby)
        {
            if (menuActive)
            {
                menuActive = false;
                ToggleMenu(false);
            }
            if (!lobbyNameSet)
            {
                lobbyNameSet = true;
                if(network.is_host == 0)
                {
                    Debug.Log(network.connectedLobby + " " + network.is_host.ToString() + " " + network.host_number.ToString() + " " + network.client_number.ToString());
                    lobbyName.text = network.connectedLobby + " " + network.is_host.ToString() + " " + network.host_number.ToString() + " " + network.client_number.ToString();
                }
                else
                {
                    Debug.Log(network.connectedLobby + " " + network.is_host.ToString());
                    lobbyName.text = network.connectedLobby + " " + network.is_host.ToString();
                }
            }
            if (network.is_host == 1 && sendHostMessage.WasPressedThisFrame())
            {
                network.FormHostMessage(network.host_number);
            }
        }
    }
}
