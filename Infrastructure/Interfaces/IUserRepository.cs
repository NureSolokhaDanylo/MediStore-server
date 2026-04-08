namespace Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<(IEnumerable<UserListItem> Items, int TotalCount)> GetUsersAsync(int skip, int take, string? q = null, string? role = null);
    }
}
