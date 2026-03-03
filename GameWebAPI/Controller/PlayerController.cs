using Microsoft.AspNetCore.Mvc;
using GameWebAPI.Models;
using GameWebAPI.Services;
using System.Runtime.ExceptionServices;

namespace GameWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly ILogger<PlayersController> _logger;
        private readonly MongoService _mongoService = new MongoService();

        public PlayersController(ILogger<PlayersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Player>> GetAll() {
            var players = _mongoService.GetAll();
            Console.WriteLine($"✅ Found {players.Count} players in MongoDB");
            return players;
        }

        [HttpGet("{id}")]
        public ActionResult<Player> GetById(string id)
        {
            var player = _mongoService.GetById(id);
            if (player == null)
                return NotFound();

            return player;
        }

        [HttpPost]
        public ActionResult Create(Player player)
        {
            _mongoService.Create(player);
            return Ok(new { message = "Player saved successfully!" });
        }

        [HttpPut("{id}")]
        public ActionResult Update(string id, Player updatedPlayer)
        {
            var existing = _mongoService.GetById(id);
            if (existing == null)
                return NotFound();

            _mongoService.Update(id, updatedPlayer);
            return Ok(new { message = "Player updated successfully!" });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            var existing = _mongoService.GetById(id);
            if (existing == null)
                return NotFound();

            _mongoService.Delete(id);
            return Ok(new { message = "Player deleted successfully!" });
        }

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            _logger.LogInformation("Start");
            return "API is working!";
        }
    }
}
