using UnityEngine;

public class ConveyorBeltSegment : Actor_Game{

   public static ConveyorBeltSegment CreateConveyorBeltSegment(ConveyorBeltSegment prefab, Transform parentTransform, Vector3 position, Quaternion rotation, int conveyorID){
      var cs = Instantiate(prefab, position, rotation, parentTransform);
      cs.gameObject.name = prefab.gameObject.name + "_" + conveyorID;
      return cs;
   }

}