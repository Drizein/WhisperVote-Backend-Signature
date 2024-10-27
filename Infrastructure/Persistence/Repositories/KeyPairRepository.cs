using Domain.Entities;
using Application.Interfaces;

namespace Infrastructure.Persistence.Repositories;

public class KeyPairRepository : _BaseRepository<KeyPair>, IKeyPairRepository
{
    public KeyPairRepository(CDbContext context) : base(context)
    {
    }
}