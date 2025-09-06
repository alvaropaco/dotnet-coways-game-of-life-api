using GameOfLifeApi.Models;

namespace GameOfLifeApi.Repositories;

public interface IBoardRepository
{
    Board? Get(Guid id);
    void Insert(Board board);
    void Update(Board board);
}
