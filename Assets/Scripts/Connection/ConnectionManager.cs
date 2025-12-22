using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using TMPro;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;
    
    [SerializeField] private string sceneName;
    
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private LobbyManager overlayManager;
    
    //private string _lobbyData;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
        
        DontDestroyOnLoad(this);
        
        //lobbyManager.evtCreated.AddListener(OnLobbyCreated);
        //lobbyManager.evtEnterSuccess.AddListener(StartConnection);
        
        Overlay.Client.EventGameLobbyJoinRequested.AddListener(JoinLobby);
        lobbyManager.evtEnterSuccess.AddListener(OnLobbyEntered);
    }

    private void JoinLobby(LobbyData lobbyData, UserData userData)
    {
        lobbyManager.Join(lobbyData);

        //var currentPlayers = lobbyManager.Lobby.MemberCount;
        //var maxPlayers = lobbyManager.Lobby.MaxMembers;
        
        /*if (lobbyManager.Lobby.Full)
        {
            Debug.Log("Game is full");
            lobbyManager.Leave();
            return;
        }*/
        
        
        /*if (!userData.IsValid)
        {
            Debug.LogError("hostUser is not valid");
            return;
        }
        
        fishySteamworks.SetClientAddress(userData.id.ToString());
        fishySteamworks.StartConnection(false);
        
        SceneManager.LoadScene(sceneName);*/
    }

    private void OnLobbyEntered(LobbyData lobbyData)
    {
        if (lobbyData.MemberCount > lobbyData.MaxMembers)
        {
            lobbyManager.Leave();
            Debug.Log("Server if full");
            return;
        }
        
        Debug.Log($"Lobby Entered! players: {lobbyData.Members}, Max players: {lobbyData.MaxMembers}");
        
        var hostUser = lobbyData.Owner.user;
        
        if (!hostUser.IsValid)
        {
            Debug.LogError("hostUser is not valid");
            return;
        }
        
        fishySteamworks.SetClientAddress(hostUser.id.ToString());
        fishySteamworks.StartConnection(false);
        
        SceneManager.LoadScene(sceneName);
    }

    /*private void OnLobbyCreated(LobbyData lobbyData)
    {
        _lobbyData = lobbyData.HexId;
    }*/
    
    public void StartHost()
    {
        fishySteamworks.StartConnection(true);
        fishySteamworks.StartConnection(false);
        
        SceneManager.LoadScene(sceneName);
    }

    /*public void TryJoinLobby()
    {
        var lobbyId = inputField.text.Trim();
        
        lobbyManager.Join(lobbyId);
    }*/

    /*private void StartConnection(LobbyData lobbyData) 
    {
        var hostHex = lobbyData.Owner.user.HexId;
        var hostUser = UserData.Get(hostHex);

        if (!hostUser.IsValid)
        {
            Debug.LogError("hostUser is not valid");
            return;
        }
        
        fishySteamworks.SetClientAddress(hostUser.id.ToString());
        fishySteamworks.StartConnection(false);
        
        SceneManager.LoadScene(sceneName);
    }*/

    public void StopConnection()
    {
        lobbyManager.Leave();
    }
    
    /*public static string GetHostHex()
    {
        return Instance._lobbyData;
    }*/

    public void ExitGame()
    {
        Application.Quit();
    }
}
