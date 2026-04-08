using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(IEnumerable<UserListItem> Items, int TotalCount)> GetUsersAsync(int skip, int take, string? q = null, string? role = null)
        {
            var query = _dbContext.Users.AsNoTracking().AsQueryable();

            var search = q?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.UserName!.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            var roleFilter = role?.Trim();
            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var roleUserIds = from userRole in _dbContext.UserRoles
                                  join identityRole in _dbContext.Roles on userRole.RoleId equals identityRole.Id
                                  where identityRole.Name == roleFilter
                                  select userRole.UserId;

                query = query.Where(u => roleUserIds.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var rolePairs = await (
                from userRole in _dbContext.UserRoles
                join identityRole in _dbContext.Roles on userRole.RoleId equals identityRole.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, RoleName = identityRole.Name! }
            ).ToListAsync();

            var rolesByUser = rolePairs
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)g.Select(x => x.RoleName).ToArray());

            var items = users.Select(u =>
            {
                rolesByUser.TryGetValue(u.Id, out var roles);
                return new UserListItem
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email,
                    Roles = roles ?? []
                };
            });

            return (items, totalCount);
        }
    }
}
