//using Unity.Behavior;
//using UnityEngine;
//using UnityEngine.AI;

//[BlackboardElement]
//public class GetRandomPatrolPoint : Action
//{
//    [BlackboardValue] public Vector3 SpawnPosition;
//    [BlackboardValue] public float RoamRadius;
//    [BlackboardValue] public Vector3 OutputPosition;

//    public override Status OnStart()
//    {
//        for (int i = 0; i < 10; i++)
//        {
//            Vector3 randomDir = Random.insideUnitSphere * RoamRadius;
//            randomDir += SpawnPosition;

//            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, RoamRadius, NavMesh.AllAreas))
//            {
//                OutputPosition = hit.position;
//                return Status.Success;
//            }
//        }

//        OutputPosition = SpawnPosition;
//        return Status.Success;
//    }
//}