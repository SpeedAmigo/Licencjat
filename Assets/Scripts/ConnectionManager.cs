using FishNet.Managing.Scened;
using Heathen.SteamworksIntegration;
using TMPro;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;
    
    [SerializeField] private string sceneName;
    
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;

    private string _hostHex;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
        
        DontDestroyOnLoad(this);
    }
    
    public void StartHost()
    {
        var user = UserData.Get();
        _hostHex = user.ToString();

        fishySteamworks.StartConnection(true);
        fishySteamworks.StartConnection(false);
        
        //fishySteamworks.NetworkManager.SceneManager.LoadGlobalScenes(new SceneLoadData(sceneName));
        SceneManager.LoadScene(sceneName);
    }

    public void StartConnection()
    {
        _hostHex = _inputField.text;
        var hostUser = UserData.Get(_hostHex);

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
        return Instance._hostHex;
    }
}
