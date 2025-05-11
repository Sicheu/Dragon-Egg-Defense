// using UnityEngine;
// using BehaviorDesigner.Runtime;
// using BehaviorDesigner.Runtime.Tasks;
//
// public class GoToDestinatioin : Action
// {
//     private int DestinationIndex;
//     private Monster monster1;
// 	
//     public override void OnAwake()
//     {
//         base.OnAwake();
//         monster1 = gameObject.GetComponent<Monster>();
//     }
// 	
//     public override void OnStart()
//     {
//         DestinationIndex = 0;
//     }
//
//     // public override TaskStatus OnUpdate()
//     // {
//     //     (int, Vector3) destinationInfo = PhaseManager.Instance.GetDestination(DestinationIndex);
//     //     DestinationIndex = destinationInfo.Item1;
//     //     if (monster1.MoveToDestination(destinationInfo.Item2))
//     //     {
//     //         DestinationIndex++;
//     //     }
// 		  //
//     //     return TaskStatus.Running;
//     // }
// }