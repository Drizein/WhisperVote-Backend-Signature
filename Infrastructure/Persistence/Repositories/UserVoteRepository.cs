using Domain.Entities;
using Application.Interfaces;

namespace Infrastructure.Persistence.Repositories;

public class UserVoteRepository : _BaseRepository<UserVote>, IUserVoteRepository
{
    public UserVoteRepository(CDbContext context) : base(context)
    {
    }
}