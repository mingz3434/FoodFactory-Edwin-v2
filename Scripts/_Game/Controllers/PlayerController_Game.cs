using UnityEngine;
using _ = GameInstance;

[AddComponentMenu("Controllers/Player Controller Game")]
public class PlayerController_Game : PlayerController{

   public bool bBulletLock;
   public GameState_Game gs_Game;
   public HUD_Game hud;
   void Awake(){ _.pc = this;}

   void Start(){
      gs_Game = _.gs as GameState_Game;
      if(gs_Game == null) return;
      // hud = HUD_Game.CreateHUD(gs_Game.prefabs.hud_Prefab, gs_Game.canvasTransform);
      gs_Game.StartGame();
   }


   void Update(){
      if(!_.pChar_Game) return;

      if(Input.GetKey(KeyCode.LeftControl)){
         if(!bBulletLock){
            bBulletLock = true;
            // Debug.Log("Left Control");
            var playerFront = _.pChar_Game.transform.position + _.pChar_Game.transform.up* -20f + _.pChar_Game.transform.right * 100f;
            // var bullet = Bullet.Create(_.pChar_Game.bullet_Prefab, gs_Game.mapTransform, playerFront);
            // Timer.CreateTimer(this.gameObject, 0.23f, () => bBulletLock = false);
            // Timer.CreateTimer(bullet.gameObject, 2f, () => Destroy(bullet.gameObject) );
         }
      }
   }


   void FixedUpdate(){
      if(!_.pChar_Game) return;

      var h = Input.GetAxisRaw("Horizontal");
      var v = Input.GetAxisRaw("Vertical");
      var vector = new Vector2(h, v);
      if (vector.sqrMagnitude < 0.01f){
         _.pChar_Game.GetRigidbody2D().linearVelocity = Vector2.zero;
         return;
      }

      _.pChar_Game.GetRigidbody2D().linearVelocity = vector.normalized * _.pChar_Game.GetSpeed() * 1.5f;

   }


}
