// using GameModels;
// using System.Data.Common;
// using System.IO;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;
// using UnityEngine.Rendering;
// using TMPro;

// public class SaveManager : MonoBehaviour
// {
//     public static SaveManager instance; //singleton instance
//     public string filepath;
//     public GameObject buildingPrefab;
//     public TMP_InputField playerNameInput;

//     private void Awake()
//     {
//         if (instance == null)
//         {
//             instance = this; //assign the instance to this script
//             DontDestroyOnLoad(gameObject); //do not destroy this object when loading a new scene
//             filepath = Application.persistentDataPath + "/saveData.json";

//             ///the persistentDataPath is a platform-independent path to store data
//             /// in windows it is C:/Users/YourName/AppData/LocalLow/CompanyName/GameName/
//             /// macos: ~/Library/Application Support/CompanyName/GameName/
//             /// android: /storage/emulated/0/Android/data/CompanyName.GameName/files/
//             /// //can open the file to see the data
//         }
//         else
//         {
//             Destroy(gameObject); //if instance already exists, destroy this object
//         }
//     }

//     public SaveData LoadGame()
//     {
//         if (File.Exists(filepath))
//         {
//             string json = File.ReadAllText(filepath); //read json
//             SaveData data = JsonUtility.FromJson<SaveData>(json); //convert json to SaveData object
//             return data;
//         }
//         else
//         {
//             Debug.LogWarning("Save file not found, returning new data");
//             return new SaveData(); //if file does not exist, return a new SaveData object
//         }
//     }

//     public void SaveGame()
//     {
//         StartCoroutine(SaveSceneToJSON());
//     }

//     private IEnumerator SaveSceneToJSON()
//     {
//         //Get all buildings in the scene
//         Building[] buildingsInScene = FindObjectsOfType<Building>();

//         List<BuildingData> buildingList = new List<BuildingData>();

//         foreach (var building in buildingsInScene)
//         {
//             Vector3 pos = building.transform.position;

//             buildingList.Add(new BuildingData
//             {
//                 type = building.type,
//                 posX = pos.x,
//                 posY = pos.y,
//                 posZ = pos.z,
//                 level = building.level
//             });
//         }

//         PlayerData save = new PlayerData
//         {
//             playerName = playerNameInput.text,
//             money = 1000,
//             xp = 500,
//             buildings = buildingList
//         };

//         // Player saved = new PlayerData();
//         string json = JsonUtility.ToJson(save);
//         Debug.Log(json);

//         // POST to API
//         using (UnityWebRequest req = new UnityWebRequest("http://localhost:5164/api/players", "POST"))
//         {
//             byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
//             req.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             req.downloadHandler = new DownloadHandlerBuffer();
//             req.SetRequestHeader("Content-Type", "application/json");

//             yield return req.SendWebRequest();

//             if (req.result == UnityWebRequest.Result.Success)
//             {
//                 Debug.Log("✅ Player saved: " + req.downloadHandler.text);
//             }
//             else
//             {
//                 Debug.LogError("❌ Save failed: " + req.error);
//             }
//         }
//     }

//     public void GetBuildings()
//     {
//         StartCoroutine(GetBuildingsCoroutine());
//     }

//     private IEnumerator GetBuildingsCoroutine()
//     {
//         UnityWebRequest request = UnityWebRequest.Get("http://localhost:5164/api/players");
//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.Success)
//         {
//             string json = request.downloadHandler.text;
//             Debug.Log("✅ Response: " + json);

//             // Because Unity's JsonUtility doesn’t support arrays directly, wrap it
//             string wrappedJson = "{ \"players\": " + json + " }";

//             PlayerListWrapper wrapper = JsonUtility.FromJson<PlayerListWrapper>(wrappedJson);

//             foreach (var player in wrapper.players)
//             {
//                 Debug.Log($"👤 Player: {player.playerName}, 💰 Money: {player.money}, XP: {player.xp}");
//                 foreach (var b in player.buildings)
//                 {
//                     Debug.Log($"🏠 {b.type} at ({b.posX}, {b.posY}, {b.posZ}) level {b.level}");
//                     Instantiate(buildingPrefab, new Vector3(b.posX, b.posY, b.posZ), Quaternion.identity);
//                 }
                    
//             }
//         }
//         else
//         {
//             Debug.LogError("❌ Error: " + request.error);
//     }
//     }

// }