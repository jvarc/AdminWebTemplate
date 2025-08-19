

namespace AdminWebTemplate.Application.Interfaces
{
    public interface IRolePermissionProvider
    {
        Task<IReadOnlyCollection<string>> GetPermissionsForRoles(IEnumerable<string> roles, CancellationToken ct = default);
    }
}
