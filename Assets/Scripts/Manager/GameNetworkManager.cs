using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using Assets.Scripts.Systems;
using Unity.Netcode.Transports.UTP;

namespace Assets.Scripts.NetCode
{
    public class GameNetworkManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> initNetworkPrefabInGame = new List<GameObject>();
        //[SerializeField][Tooltip("Load the same map when is active")] private bool DebugMode = false;
        [SerializeField][Tooltip("Start the direct connection ip in p2p client")] private bool clientDebugStartDirectGame = false;
        [SerializeField][Tooltip("Start host connection p2p server")] private bool serverDebugStartDirectGame = false;
        [SerializeField][Tooltip("IP connection")] private string clientDebugDirectIP = "127.0.0.1";
        [SerializeField][Tooltip("Server Log")] private LogLevel ServerBuildlogLevel = LogLevel.Developer;
        public static GameNetworkManager Instance { get; private set; } = null;
        public Action<ulong> Disconected;
        public Lobby? CurrentLobby { get; private set; } = null;
        private FacepunchTransport _transport = null;
        private UnityTransport _unityTransport = null;
        private bool isConnectedToLobby = false;
        private bool isFindingGame = false;        
        private SteamId OpponentSteamId { get; set; }
        public static bool IsConnectedToLobby { get => Instance.isConnectedToLobby; }
        public static bool IsFindingGame { get => Instance.isFindingGame; }
        private int _p2pmemberConnect = 0;
        private int _p2pmemberMax = 0;
        private bool loadNetworkScene = false;
        private bool normalMode = false;
        private int endLoadCount = 0;
        public int port;

        public static int MemberConnected { get => Instance._p2pmemberConnect; }
        public static int MemberMax { get => Instance._p2pmemberMax; }

        public static bool NormalMode { get => Instance != null ? Instance.normalMode : false; } 

        public string LobbyMemberToString 
        { 
            get 
            {
                if(CurrentLobby == null)
                {
                    return "";
                }
                return (CurrentLobby?.MemberCount ?? _p2pmemberConnect).ToString() + "/" + (CurrentLobby?.MaxMembers ?? _p2pmemberMax).ToString();
            } 
        }

        private void Awake()
        {            
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        #region Load Scene Event
        private void OnLoadScene(ulong clientId, string scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, UnityEngine.AsyncOperation asyncOperation)
        {
            Debug.Log($"OnLoadScene: {scene} clientID: {clientId} mode: {loadSceneMode}");
            if (NetworkManager.Singleton.IsHost)
            {
                loadNetworkScene = true;
                endLoadCount = 0;
            }
            if (NetworkManager.Singleton.IsClient)
            {
                //LoaderSystem.IsDisplayLoadingText = true;
                //LoaderSystem.FadeIn(0.5f, findSo.LoadingScreen);
            }
        }

        private void OnLoadSceneComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"OnLoadSceneComplete: {sceneName} clientID: {clientId} mode: {loadSceneMode}");
            if (loadNetworkScene && NetworkManager.Singleton.IsHost)
            {
                if (++endLoadCount >= _p2pmemberConnect)
                {
                    loadNetworkScene = false;
                    Debug.Log("All player has Changed Scene");
                    InitGame();
                }
            }
        }

        public void InitGame()
        {
            foreach (var prefab in initNetworkPrefabInGame)
            {
                GameObject network = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                if (network.GetComponent<NetworkObject>() != null)
                {
                    network.GetComponent<NetworkObject>().Spawn(true);
                }
            }
        }

        private void OnUnloadSceneComplete(ulong clientId, string sceneName)
        {
            Debug.Log($"OnUnloadSceneComplete: {sceneName} clientID: {clientId}");
        }

        private void OnUnloadScene(ulong clientId, string sceneName, UnityEngine.AsyncOperation asyncOperation)
        {
            Debug.Log($"OnUnloadScene: {sceneName} clientID: {clientId}");
        }

        #endregion  Load Scene Event
        private void Start()
        {
            _transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            if (ServerSystem.IsGraphicsBuild)
            {
                SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
                SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
                SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
                SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
                SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
                SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
                SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
                SteamNetworking.AllowP2PPacketRelay(true);
                //SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
                //SteamNetworking.OnP2PConnectionFailed += OnP2PConnectionFailed;
            }
            else
            {
                NetworkManager.Singleton.LogLevel = ServerBuildlogLevel;
                SwitchTransport(_unityTransport);
                StartHostServer(ServerSystem.Port, 4);
            }
            if (NetworkManager.Singleton != null)
            {                
                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            }
        }

        public void OnDestroy()
        {
            Disconnect();
            if (ServerSystem.IsGraphicsBuild)
            {
                SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
                SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
                SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
                SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
                SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
                SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
                SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
            }
            Instance = null;
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.ConnectionApprovalCallback -=  ApprovalCheck;
        }

        public void SwitchTransport(NetworkTransport t)
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = t;
        }

        private void OnApplicationQuit() => Disconnect();

        public async void FindServerGame(int maxPlayers)
        {
            if (CurrentLobby != null)
            {
                return;
            }
            SwitchTransport(_unityTransport);
            isFindingGame = true;
            bool findlobby = false;
            if(clientDebugStartDirectGame)
            {
                StartClient(clientDebugDirectIP, port);
                return;
            }
            if(serverDebugStartDirectGame)
            {
                StartHostServer(port, maxPlayers);
                return;
            }
            int lobbyCountPerPlayer = 0;
            SteamId lobbySelect = 0;
            try
            {
                Debug.Log("Finding lobby");
                var lobbylist = await SteamMatchmaking.LobbyList.RequestAsync();
                if (lobbylist != null)
                {
                    Debug.Log("Lobby list received size: " + lobbylist.Length);
                    foreach (var lobby in lobbylist)
                    {
                        if (lobby.MaxMembers == maxPlayers && lobby.MemberCount < lobby.MaxMembers && lobbyCountPerPlayer < lobby.MemberCount)
                        {
                            lobbySelect = lobby.Id;//TODO : check if lobby
                            lobbyCountPerPlayer = lobby.MemberCount;
                            findlobby = true;
                        }
                    }
                }
                else
                {
                    Debug.Log("LobbyList is null");
                }
            }
            catch (System.Exception e)
            {
                isFindingGame = false;
                Debug.LogError(e);
                return;
            }
            if (findlobby)
            {
                CurrentLobby = await SteamMatchmaking.JoinLobbyAsync(lobbySelect);
            }
            else
            {
                CreateLobbyAsync(maxPlayers);
            }
        }

        private async void CreateLobbyAsync(int maxPlayers)
        {
            try
            {
                Lobby? createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
                if (!createLobbyOutput.HasValue)
                {
                    Debug.Log("Lobby created but not correctly instantiated");
                    throw new Exception();
                }
                //CurrentLobby.SetFriendsOnly();
                var lobby = createLobbyOutput.Value;
                lobby.Refresh();
                lobby.SetPublic();
                lobby.SetJoinable(true);
                lobby.SetData("name", "GameServer");
                CurrentLobby = lobby;
                Debug.Log("Lobby created", this);
            }
            catch (Exception exception)
            {
                Debug.LogError("Failed to create multiplayer lobby");
                Debug.LogError(exception.ToString());
                isFindingGame = false;
            }
        }

        public async void FindGame(int maxPlayers,bool normalM = true)
        {
            if (CurrentLobby != null)
            {
                return;
            }
            if(clientDebugStartDirectGame)
            {
                SwitchTransport(_unityTransport);
                StartClient(clientDebugDirectIP, 7777);
                return;
            }
            if(serverDebugStartDirectGame)
            {
                SwitchTransport(_unityTransport);
                StartHostServer(7777, maxPlayers);
                return;
            }
            normalMode = normalM;
            SwitchTransport(_transport);
            isFindingGame = true;
            bool findlobby = false;
            int lobbyCountPerPlayer = 0;
            SteamId lobbySelect = 0;
            try
            {
                Debug.Log("Finding lobby");
                var lobbylist = await SteamMatchmaking.LobbyList.RequestAsync();
                if (lobbylist != null)
                {
                    Debug.Log("Lobby list received size: " + lobbylist.Length);
                    foreach (var lobby in lobbylist)
                    {
                        bool isSameMode = normalMode ? lobby.GetData("name") == "NormalGame" : lobby.GetData("name") == "SmallGame";
                        if (isSameMode && lobby.MaxMembers == maxPlayers && lobby.MemberCount < lobby.MaxMembers && lobbyCountPerPlayer < lobby.MemberCount)
                        {
                            lobbySelect = lobby.Id;//TODO : check if lobby
                            lobbyCountPerPlayer = lobby.MemberCount;
                            findlobby = true;
                        }
                    }
                }
                else
                {
                    Debug.Log("LobbyList is null");
                }
            }
            catch (System.Exception e)
            {
                isFindingGame = false;
                Debug.LogError(e);
                return;
            }
            if (findlobby)
            {
                CurrentLobby = await SteamMatchmaking.JoinLobbyAsync(lobbySelect);
            }
            else
            {
                StartHostLobby(maxPlayers);
            }
        }

        public void StartHostServer(int port, int maxPlayers = 4)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            _p2pmemberMax = maxPlayers;
            _unityTransport.ConnectionData.Port = (ushort)port;
            
            if (NetworkManager.Singleton.StartHost())
            {
                CallbacksScene(true);
                Debug.Log("Start Server Host");
            }
            isConnectedToLobby = true;
            isFindingGame = false;
        }

        public async void StartHostLobby(int maxMembers = 4)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            _p2pmemberMax = maxMembers;
            if (NetworkManager.Singleton.StartHost())
            {
                CallbacksScene(true);
                try
                {
                    var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
                    if (!createLobbyOutput.HasValue)
                    {
                        Debug.Log("Lobby created but not correctly instantiated");
                        throw new Exception();
                    }
                    //CurrentLobby.SetFriendsOnly();
                    var lobby = createLobbyOutput.Value;
                    lobby.SetPublic();
                    lobby.SetJoinable(true);
                    if(normalMode)
                    {
                        lobby.SetData("name", "NormalGame");
                    }
                    else
                    {
                        lobby.SetData("name", "SmallGame");
                    }
                    CurrentLobby = lobby;
                    Debug.Log("Lobby created", this);
                }
                catch (Exception exception)
                {
                    Debug.LogError("Failed to create multiplayer lobby");
                    Debug.LogError(exception.ToString());
                    return;
                }
            }
            isConnectedToLobby = true;
            isFindingGame = false;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest connectionRequest, NetworkManager.ConnectionApprovalResponse connectionResponse)
        {     
            Debug.Log("[Server] Approval Check clientid: " + connectionRequest.ClientNetworkId);
            connectionResponse.Approved = true;//serverDebugStartDirectGame || CurrentLobby?.MemberCount <= CurrentLobby?.MaxMembers;
            connectionResponse.CreatePlayerObject = false;
            connectionResponse.Position = Vector3.zero;
            connectionResponse.Rotation = Quaternion.identity;
        }

        public void StartClient(SteamId id)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            Debug.Log("Starting client target : " + id);
            _transport.targetSteamId = id;

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client has Joined lobby", this);
                CallbacksScene(true);
            }
            isConnectedToLobby = true;
            isFindingGame = false;
        }

        public void StartClient(string ip, int port)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            Debug.Log("Starting client target : " + ip + ":" + port);
            _unityTransport.ConnectionData.Address = ip;
            _unityTransport.ConnectionData.Port = ((ushort)port);

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client has Joined lobby", this);
                CallbacksScene(true);
            }
            isConnectedToLobby = true;
            isFindingGame = false;
        }

        public void Disconnect()
        {
            if (isConnectedToLobby)
            {
                CurrentLobby?.Leave();
                Debug.Log("Disconnecting from lobby", this);
                isConnectedToLobby = false;
                CurrentLobby = null;
            }
            _p2pmemberConnect = 0;
            _p2pmemberMax = 0;
            isFindingGame = false;
            loadNetworkScene = false;
            endLoadCount = 0;
            if (NetworkManager.Singleton == null)
            {
                return;
            }
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                CallbacksScene(false);
            }
            normalMode = false;
            NetworkManager.Singleton.Shutdown();
        }

        private void AcceptP2P(SteamId opponentId)
        {
            try
            {
                // Pour que deux joueurs s'envoient des paquets P2P, ils doivent chacun appeler cette fonction sur l'autre joueur.                
                SteamNetworking.AcceptP2PSessionWithUser(opponentId);
            }
            catch
            {
                Debug.Log("Unable to accept P2P Session with user");
            }
        }


        #region P2P Callbacks
        private void OnP2PSessionRequest(SteamId steamId)
        {
            Debug.Log("P2P Session Request from " + steamId);
        }

        private void OnP2PConnectionFailed(SteamId steamId, P2PSessionError error)
        {
            Debug.Log("P2P Connection Failed with " + steamId + " error: " + error);
        }

        #endregion P2P Callbacks

        public void CallbacksScene(bool active)
        {
            if (active)
            {
                NetworkManager.Singleton.SceneManager.OnLoad += OnLoadScene;
                NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadSceneComplete;
                NetworkManager.Singleton.SceneManager.OnUnload += OnUnloadScene;
                NetworkManager.Singleton.SceneManager.OnUnloadComplete += OnUnloadSceneComplete;
            }
            else
            {
                NetworkManager.Singleton.SceneManager.OnLoad -= OnLoadScene;
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadSceneComplete;
                NetworkManager.Singleton.SceneManager.OnUnload -= OnUnloadScene;
                NetworkManager.Singleton.SceneManager.OnUnloadComplete -= OnUnloadSceneComplete;
            }
        }

        #region Callbacks_Network
        private void OnServerStarted()
        {
            Debug.Log("Server has Started", this);
        }
        private void OnClientConnect(ulong clientId)
        {
            Debug.Log($"Client has Connected clientId={clientId}", this);
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                _p2pmemberConnect++;
                Debug.Log("Member Connected: " + _p2pmemberConnect + " / " + _p2pmemberMax, this);

                if (_p2pmemberConnect == _p2pmemberMax)
                {
                    Debug.Log("Lobby is full Start the Game ...", this);
                    CurrentLobby?.SetPrivate();
                    NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);// change this
                }
            }
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            {
                //WaitingForHost
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client has Disconnected clientId={clientId}", this);
            //SteamNetworking.CloseP2PSessionWithUser(OpponentSteamId);
            if (NetworkManager.Singleton.IsHost)
            {
                _p2pmemberConnect--;
                Disconected?.Invoke(clientId);
            }
            if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Disconnecting from server");
                isConnectedToLobby = false;
                CurrentLobby?.Leave();
            }
        }

        #endregion Callbacks_Network

        #region Callbacks_Lobby

        private void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamID)
        {
            bool isSame = lobby.Owner.Id.Equals(steamID);

            Debug.Log($"Owner: {lobby.Owner}");
            Debug.Log($"Id: {steamID}");
            Debug.Log($"IsSame: {isSame}", this);

            StartClient(steamID);
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Debug.LogError($"Lobby creation failed {result}", this);
                return;
            }

        }

        private void OnLobbyEntered(Lobby lobby)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                return;
            }
            //AcceptP2P(lobby.Owner.Id);
            StartClient(lobby.Owner.Id);
            Debug.Log($"Lobby entered lobbyId={lobby.Id} OwnerID={lobby.Owner.Id} Owner={lobby.Owner.Name}", this);
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            Debug.Log($"Lobby member joined lobbyId={lobby.Id} friendId={friend.Id} name={friend.Name}", this);
            if (friend.Id != SteamClient.SteamId)
            {
                OpponentSteamId = friend.Id;
                //AcceptP2P(friend.Id);
            }
        }

        private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
        {
            Debug.Log($"Lobby member left lobbyId={lobby.Id} friendId={friend.Id} name={friend.Name}", this);
            SteamNetworking.CloseP2PSessionWithUser(friend.Id);
        }

        private void OnLobbyInvite(Friend friend, Lobby lobby)
        {
            Debug.Log($"You got a invite from {friend.Name}");
        }

        private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
        {
            Debug.Log($"Lobby game created lobbyId={lobby.Id} ip={ip} port={port} steamId={steamId}", this);
        }

        #endregion Callbacks_Lobby
    }
}
