using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCachingProject.Data;
using RedisCachingProject.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisCachingProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _cache;

        public UsersController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }

        // 📥 GET: Fetch all users with caching
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            string cacheKey = "users";
            string cachedUsers = await _cache.StringGetAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedUsers))
            {
                var users = JsonSerializer.Deserialize<List<User>>(cachedUsers);
                return Ok(users); // from cache
            }

            var dbUsers = await _context.Users.ToListAsync();
            var serialized = JsonSerializer.Serialize(dbUsers);

            await _cache.StringSetAsync(cacheKey, serialized, TimeSpan.FromMinutes(10));

            return Ok(dbUsers); // from DB
        }

        // ➕ POST: Add user and clear cache
        [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _cache.KeyDeleteAsync("users"); // invalidate cache
            return Ok(user);
        }

        // ✏️ PUT: Update user and clear cache
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;

            await _context.SaveChangesAsync();
            await _cache.KeyDeleteAsync("users");

            return NoContent();
        }

        // ❌ DELETE: Delete user and clear cache
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _cache.KeyDeleteAsync("users");

            return NoContent();
        }
    }
}
