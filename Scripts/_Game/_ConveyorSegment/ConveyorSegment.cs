using UnityEngine;

public class ConveyorSegment : Actor_Game{

   public static ConveyorSegment CreateConveyorSegment(ConveyorSegment prefab, Transform parentTransform, int conveyorID){
      var cs = Instantiate(prefab, parentTransform);
      cs.gameObject.name = prefab.gameObject.name + " " + conveyorID;
      cs.gameObject.layer = LayerMask.NameToLayer("Conveyor"); //since the rb-collider interaction is for collision, we need ignore Conveyor.
      return cs;
   }

}