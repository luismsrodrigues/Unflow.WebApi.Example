using Unflow.WebApi.Example.Database;
using Unflow.WebApi.Example.Database.Entities;

namespace Unflow.WebApi.Example.Business;

public class UserService: IService
{
    private UnflowDatabaseContext _databaseContext;

    public UserService(UnflowDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<User> CreateUserAsync(CancellationToken ct)
    {
        var user = new User
        {
            Name = "Random Name"
        };

        await _databaseContext.User.AddAsync(user, ct);

        await _databaseContext.SaveChangesAsync(ct);

        return user;
    }
}