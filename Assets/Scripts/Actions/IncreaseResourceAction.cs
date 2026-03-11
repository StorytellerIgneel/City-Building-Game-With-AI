// using Unity.VisualScripting;
// using UnityEngine;

// // Injection of the service should not be done in this scriptable object, as it is an asset that is shared globally.
// // Instead, we will initialize the service in the Building class and pass it to the action when needed.
// // scriptable object should only define what action to do
// [CreateAssetMenu(menuName = "Buildings/Actions/Increase Workforce Action")]
// public class IncreaseWorkforceAction : BuildingAction
// {
//     public int WorkforceIncreaseAmount = 10; // Amount to increase Workforce by

//     // public override void OnPlacedExecute(Building building)
//     // {
//     //     building.WorkforceController.IncreaseWorkforce(WorkforceIncreaseAmount);
//     // }

//     public override void OnRemovedExecute(Building building)
//     {
//         // WorkforceService.RemoveWorkforce(WorkforceIncreaseAmount);
//     }

//     public override void OnTurnStartExecute(Building building)
//     {
//         // Implementation for turn start action, if needed
//     }
// }