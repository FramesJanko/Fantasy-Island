using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour
{
    NetworkManager network;
    public GameObject menu;
    public TMP_Text lobbyName;
    public InputAction sendHostMessage;
    public void ToggleMenu(bool shouldBeActive)
    {
        menu.SetActive(shouldBeActive);
    }
    void Start()
    {
        network = NetworkManager.Instance;
        network.OnLobbyJoined += HandleLobbyJoined;
        // Cover the race where the lobby was joined before this subscribed.
        if (network.connectedToLobby)
        {
            HandleLobbyJoined();
        }
        sendHostMessage.Enable();
    }
    void OnDestroy()
    {
        if (network != null)
        {
            network.OnLobbyJoined -= HandleLobbyJoined;
        }
    }
    void HandleLobbyJoined()
    {
        ToggleMenu(false);
        if (network.is_host == 0)
        {
            lobbyName.text = network.connectedLobby + " " + network.is_host + " " + network.host_number + " " + network.client_number;
        }
        else
        {
            lobbyName.text = network.connectedLobby + " " + network.is_host;
        }
        Debug.Log(lobbyName.text);
    }
    void Update()
    {
        // Per-frame input still belongs here; only the one-time join reaction moved to the event.
        if (network.connectedToLobby && network.is_host == 1 && sendHostMessage.WasPressedThisFrame())
        {
            network.RequestSpawn(SpawnId.Player, Vector3.zero);
        }
    }
}
