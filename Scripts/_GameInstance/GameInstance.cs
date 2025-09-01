using System;
using UnityEngine;
using Steamworks;
using Matchmaking = Steamworks.SteamMatchmaking;
using User = Steamworks.SteamUser;
using Friends = Steamworks.SteamFriends;
using Mirror;
using ServerActive = Mirror.ServerAttribute;

[AddComponentMenu("Game Instance/Game Instance")]
public class GameInstance : MonoBehaviour{

   public static GameInstance gameInstance; //* No need of having network.
   public static GameMode gm; //! Need network
   public static GameState gs; //! Need network
   public static PlayerController localPC; //! Need network, having pc_RPCM, pChar, PS....

   public Camera preGameCamera;
   public static CustomNetworkManager myNetworkManager;
   
   public static SteamLobby_UI steamLobby_UI;

   public CustomNetworkManager myNetworkManager_Inst;
   const string HOST_ADDRESS_KEY = "hostAddress";
   const string NAME_KEY = "name";

   CSteamID lobbyId;

   void Awake(){
      gameInstance = this;
      myNetworkManager = myNetworkManager_Inst;
   }


   void Start(){
      
      DontDestroyOnLoad(this.gameObject);
      DontDestroyOnLoad(steamLobby_UI.transform.parent.gameObject);

      //P: Require SteamManager to be initialized.
      if(!SteamManager.Initialized) { Debug.Log("SteamManager not initialized."); return; }

      //P: Bind events when triggered, returning Callback data... (onReceive_XXX)
      Callback<LobbyCreated_t>.Create(OnLobbyCreated);
      Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
      Callback<LobbyEnter_t>.Create(OnLobbyEnter);

   }

   void OnLobbyCreated(LobbyCreated_t lobbyCreated_Callback){

      AddBothLog("Lobby: 1: Lobby created.");

      var nm = myNetworkManager;

      if (!NetworkServer.active && !NetworkClient.active){
         nm.StartHost();
         // nm.ServerChangeScene("Level");
      }
      else{
         AddBothLog("Mirror: Server or Client already being active.");
      }

      var cb = lobbyCreated_Callback;
      var lobbyId = new CSteamID(cb.m_ulSteamIDLobby);
      var hostAddress = User.GetSteamID().ToString();
      var name = Friends.GetPersonaName();
      var lobbysName = name + "'s Lobby";

      Matchmaking.SetLobbyData(lobbyId, HOST_ADDRESS_KEY, hostAddress);
      Matchmaking.SetLobbyData(lobbyId, NAME_KEY, lobbysName);

      AddBothLog("Lobby: 2: Lobby data of Matchmaking is now set.");
      


   }

   void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t gameLobbyJoinRequested_Callback){

      AddBothLog("Lobby: 3: Client(Me) requested to join the lobby.");

      var cb = gameLobbyJoinRequested_Callback;
      var lobbyId = cb.m_steamIDLobby;

      Matchmaking.JoinLobby(lobbyId);

   }

   void OnLobbyEnter(LobbyEnter_t lobbyEnter_Callback){

      AddBothLog("Lobby: 4: Lobby entered.");

      //P: For everyone
      var nm = myNetworkManager;
      var cb = lobbyEnter_Callback;
      var lobbyId = new CSteamID(cb.m_ulSteamIDLobby);
      this.lobbyId = lobbyId;
      var lobbysName = Matchmaking.GetLobbyData(lobbyId, NAME_KEY);

      Action renderToUI = () => {
         var ui = steamLobby_UI;
         var hostButton = ui.GetHostServerButton();
         var lobbyNameText = ui.GetLobbyNameText();
         hostButton.gameObject.SetActive(false);
         lobbyNameText.gameObject.SetActive(true);
         lobbyNameText.text = lobbysName;
         preGameCamera.gameObject.SetActive(false);
      };
      renderToUI();

      //P: For client
      if(NetworkServer.active) return;
      nm.networkAddress = Matchmaking.GetLobbyData(lobbyId, HOST_ADDRESS_KEY); nm.StartClient();
   }


   public void AddBothLog(string log){ //P: Add log to both editor and screen
      Debug.Log(log);
      steamLobby_UI.GetDebugMessageText().text += log + "\n";
   }

   public void PopInviteOverlay(){
      Friends.ActivateGameOverlayInviteDialog(this.lobbyId);
   }

   [ServerActive]
   public void GoGameScene(){
      var nm = myNetworkManager;
      nm.ServerChangeScene("Level");
      steamLobby_UI.transform.GetChild(0).gameObject.SetActive(false);
   }
   

}






