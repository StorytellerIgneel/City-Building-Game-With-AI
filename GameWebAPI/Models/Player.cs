using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameWebAPI.Models
{
    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("playerName")]
        public string PlayerName { get; set; }
        
        [BsonElement("money")]
        public int Money { get; set; }

        [BsonElement("xp")]
        public int Xp { get; set; }

        [BsonElement("buildings")]
        public List<BuildingData> Buildings { get; set; }
    }

    public class BuildingData
    {
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("posX")]
        public float PosX { get; set; }

        [BsonElement("posY")]
        public float PosY { get; set; }

        [BsonElement("posZ")]
        public float PosZ { get; set; }

        [BsonElement("level")]
        public int Level { get; set; }
    }
}
