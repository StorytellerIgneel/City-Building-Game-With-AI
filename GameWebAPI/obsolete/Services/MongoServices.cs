using GameWebAPI.Models;
using MongoDB.Driver;
using Serilog;

namespace GameWebAPI.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<Player> _players;

        public MongoService()
        {
            var client = new MongoClient("mongodb://localhost:27017"); // adjust if needed
            Log.Information("Connected to MongoDB at {@Client}", client);
            var database = client.GetDatabase("CityGame");
            _players = database.GetCollection<Player>("Players");
        }

        public List<Player> GetAll() =>
            _players.Find(player => true).ToList();

        public Player? GetById(string id) =>
            _players.Find(player => player.Id == id).FirstOrDefault();

        public void Create(Player player) =>
            _players.InsertOne(player);

        public void Update(string id, Player updatedPlayer) =>
            _players.ReplaceOne(player => player.Id == id, updatedPlayer);

        public void Delete(string id) =>
            _players.DeleteOne(player => player.Id == id);
    }
}
