using GameOfLifeApi.Models;
using LiteDB;

namespace GameOfLifeApi.Repositories;

public class LiteDbBoardRepository : IBoardRepository
{
    private readonly ILiteDatabase _db;
    private readonly ILiteCollection<Board> _boards;

    public LiteDbBoardRepository(ILiteDatabase db)
    {
        _db = db;
        _boards = _db.GetCollection<Board>("boards");
        _boards.EnsureIndex(x => x.CreatedAtUtc);
    }

    public Board? Get(Guid id) => _boards.FindById(id);
    public void Insert(Board board) => _boards.Insert(board);
    public void Update(Board board) => _boards.Update(board);
}
