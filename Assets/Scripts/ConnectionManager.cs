using FishNet.Managing.Scened;
using Heathen.SteamworksIntegration;
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
    
    private string _lobbyData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
        
        DontDestroyOnLoad(this);
        
        lobbyManager.evtCreated.AddListener(OnLobbyCreated);
        lobbyManager.evtEnterSuccess.AddListener(StartConnection);
        
        Heathen.SteamworksIntegration.API.Overlay.Client.EventGameLobbyJoinRequested.AddListener(JoinLobby);
    }

    private void JoinLobby(LobbyData lobbyData, UserData userData)
    {
        lobbyManager.Join(lobbyData);
        
        if (!userData.IsValid)
        {
            Debug.LogError("hostUser is not valid");
            return;
        }
        
        fishySteamworks.SetClientAddress(userData.id.ToString());
        fishySteamworks.StartConnection(false);
        
        SceneManager.LoadScene(sceneName);
    }

    private void OnLobbyCreated(LobbyData lobbyData)
    {
        _lobbyData = lobbyData.HexId;
    }

    public void StartHost()
    {
        fishySteamworks.StartConnection(true);
        fishySteamworks.StartConnection(false);
        
        SceneManager.LoadScene(sceneName);
    }

    public void TryJoinLobby()
    {
        var lobbyId = inputField.text;
        
        lobbyManager.Join(lobbyId);
    }

    private void StartConnection(LobbyData lobbyData) 
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
    }
    
    public static string GetHostHex()
    {
        return Instance._lobbyData;
    }
}
