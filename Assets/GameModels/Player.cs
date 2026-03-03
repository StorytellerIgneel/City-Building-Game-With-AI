using System;
using System.Collections.Generic;

namespace GameModels   // 👈 use a custom namespace, not just "Models"
{
    [Serializable]
    public class BuildingData
    {
        public string type;
        public float posX;
        public float posY;
        public float posZ;
        public int level;
    }

    [Serializable]
    public class PlayerData
    {
        public string id;
        public string playerName;
        public int money;
        public int xp;
        public List<BuildingData> buildings;
    }

    [Serializable]
    public class PlayerListWrapper
    {
        public List<PlayerData> players;
    }
}
