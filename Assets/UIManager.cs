// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using Unity.Entities;
// using UnityEditor.Search;

// public class UIManager : MonoBehaviour
// {
//     public TextMeshProUGUI goldDisplay;           // Reference to your gold script

//     public void Update()
//     {
//         var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//         EntityQuery goldQuery = entityManager.CreateEntityQuery(typeof(PlayerGold));

//         if (goldQuery.TryGetSingleton<PlayerGold>(out PlayerGold playerGold))
//         {
//             goldDisplay.text = "Gold: " + playerGold.Value;
//         }
//     } 
// }
