using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    public Button[] lobbyButtons;
    // public Button joinLobby;
    public TMP_Text title;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Instance.OnLobbyListUpdated += HandleLobbyListUpdated;
        // Render whatever list we may already have.
        HandleLobbyListUpdated();
    }

    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnLobbyListUpdated -= HandleLobbyListUpdated;
        }
    }

    void HandleLobbyListUpdated()
    {
        string[] lobbyList = NetworkManager.Instance.lobbyList;
        if (lobbyList == null)
        {
            return;
        }
        for(int i = 0; i < lobbyList.Length; i++)
        {
            if(lobbyList[i] != null)
            {
                title.gameObject.SetActive(true);
                lobbyButtons[i].gameObject.SetActive(true);
                lobbyButtons[i].GetComponentInChildren<TMP_Text>().text = lobbyList[i];
                // joinLobby.gameObject.SetActive(true);
            }
            else
            {
                lobbyButtons[i].gameObject.SetActive(false);
            }
        }
    }
}
