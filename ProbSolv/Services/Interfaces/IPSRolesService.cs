using Microsoft.AspNetCore.Identity;
using ProbSolv.Models;

namespace ProbSolv.Services.Interfaces
{
    public interface IPSRolesService
    {
        public Task<bool> IsUserInRoleAsync(PSUser user, string roleName);
        public Task<List<IdentityRole>> GetRolesAsync();

        public Task<IEnumerable<string>> GetUserRolesAsync(PSUser user);

        public Task<bool> AddUserRoleAsync(PSUser user, string roleName);

        public Task<bool> RemoveUserFromRoleAsync(PSUser user, string roleName);

        public Task<bool> RemoveUserFromRolesAsync(PSUser user, IEnumerable<string> roles);

        public Task<List<PSUser>> GetUsersInRoleAsync(string roleName, int companyId);

        public Task<List<PSUser>> GetUsersNotInRoleAsync(string roleName, int companyId);

        public Task<string> GetRoleNameByIdAsync(string roleId);


    }
}
