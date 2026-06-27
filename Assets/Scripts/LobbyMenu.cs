using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    MenuToggle parentCanvas;
    string[] _lobbyList;
    public Button[] lobbyButtons;
    // public Button joinLobby;
    public TMP_Text title;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentCanvas = GetComponentInParent<MenuToggle>();
        _lobbyList = parentCanvas.network.lobbyList;
    }

    // Update is called once per frame
    void Update()
    {
        _lobbyList = parentCanvas.network.lobbyList;
        for(int i = 0; i < _lobbyList.Length; i++)
        {
            if(_lobbyList[i] != null)
            {
                title.gameObject.SetActive(true);
                lobbyButtons[i].gameObject.SetActive(true);
                lobbyButtons[i].GetComponentInChildren<TMP_Text>().text = _lobbyList[i];
                // joinLobby.gameObject.SetActive(true);

            }
            else
            {
                lobbyButtons[i].gameObject.SetActive(false);
            }
        }
    }
}
