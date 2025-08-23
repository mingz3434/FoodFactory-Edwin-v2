using UnityEngine;
public class TrajectoryLine : Actor_Game {

   public LineRenderer lineRenderer;
   public static TrajectoryLine CreateTrajectoryLine(TrajectoryLine prefab, Transform parentTransform){
      var trajectory = Instantiate(prefab, parentTransform);
      trajectory.gameObject.name = prefab.gameObject.name;
      trajectory.lineRenderer = trajectory.GetComponent<LineRenderer>();
      return trajectory;
   }
}