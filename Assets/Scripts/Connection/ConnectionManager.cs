using System.Collections;
using System.Collections.Generic;
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
    
    [SerializeField] private Animator transitionAnimator;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
        
        DontDestroyOnLoad(this);
        
        Overlay.Client.EventGameLobbyJoinRequested.AddListener(JoinLobby);
        lobbyManager.evtEnterSuccess.AddListener(OnLobbyEntered);
    }

    private void JoinLobby(LobbyData lobbyData, UserData userData)
    {
        lobbyManager.Join(lobbyData);
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
        
        /*fishySteamworks.SetClientAddress(hostUser.id.ToString());
        fishySteamworks.StartConnection(false);*/
        
        //SceneManager.LoadScene(sceneName);
        //SceneManager.LoadSceneAsync(sceneName);
        StartCoroutine(LoadLevel(false, hostUser.id.ToString()));
    }
    
    public void StartHost()
    {
        //SceneManager.LoadScene(sceneName);
        //SceneManager.LoadSceneAsync(sceneName);
        StartCoroutine(LoadLevel(true, null));
    }

    private IEnumerator LoadLevel(bool asServer, string clientData)
    {
        transitionAnimator.gameObject.SetActive(true);
        transitionAnimator.SetTrigger("End");
        
        yield return new WaitForSeconds(1f);
        
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        if (asServer)
        {
            fishySteamworks.StartConnection(true);
            fishySteamworks.StartConnection(false);
        }
        else
        {
            fishySteamworks.SetClientAddress(clientData);
            fishySteamworks.StartConnection(false);
        }
        
        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
        
        yield return new WaitForSeconds(0.1f);
        
        transitionAnimator.SetTrigger("Start");
        
        yield return new WaitForSeconds(1f);
        
        transitionAnimator.gameObject.SetActive(false);
    }
    
    public void StopConnection()
    {
        lobbyManager.Leave();
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
