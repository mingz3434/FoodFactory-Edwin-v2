using System;
using System.Collections;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using _ = GameInstance;
using Matchmaking = Steamworks.SteamMatchmaking;
using TMPro;

public class SteamLobby_UI : MonoBehaviour{


   public Text GetLobbyNameText(){ return this.transform.GetChild(0).GetChild(0).GetComponent<Text>(); }
   public Button GetHostServerButton(){ return this.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Button>(); }
   public TMP_Text GetDebugMessageText(){ return this.transform.GetChild(1).GetComponent<TMP_Text>(); }
   public Button GetInviteButton(){ return this.transform.GetChild(0).GetChild(2).GetComponent<Button>(); }
   public Button GetGoButton(){ return this.transform.GetChild(0).GetChild(3).GetComponent<Button>(); }


   void Awake(){
      _.steamLobby_UI = this;
   }

   void Start(){
      GetHostServerButton().onClick.AddListener(() => { Matchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4); });
      GetInviteButton().onClick.AddListener(() => { _.gameInstance.PopInviteOverlay(); });
      GetGoButton().onClick.AddListener(() => { _.gameInstance.GoGameScene(); });
   }




}