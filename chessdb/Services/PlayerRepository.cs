
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using chessdb.Models; 

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player?> GetByIdAsync(int id);
    Task AddAsync(Player p);
    Task UpdateAsync(Player p);
    Task DeleteAsync(int id);
}

public class PlayerRepository : IPlayerRepository
{
    private readonly ChessFedDbContext _db;
    public PlayerRepository(ChessFedDbContext db) { _db = db; }
    public async Task<IEnumerable<Player>> GetAllAsync() => await _db.Players.ToListAsync();
    public async Task<Player?> GetByIdAsync(int id) => await _db.Players.FindAsync(id);
    public async Task AddAsync(Player p) { _db.Players.Add(p); await _db.SaveChangesAsync(); }
    public async Task UpdateAsync(Player p) { _db.Players.Update(p); await _db.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var p = await GetByIdAsync(id); if(p!=null){_db.Players.Remove(p);await _db.SaveChangesAsync();} }
}