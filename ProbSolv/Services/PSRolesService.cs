using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Models;
using ProbSolv.Models.Enums;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Services
{
    public class PSRolesService : IPSRolesService
    {

        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<PSUser> _userManager;

        public PSRolesService(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<PSUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }


        public async Task<bool> AddUserRoleAsync(PSUser user, string roleName)
        {
            bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;

            return result;
        }

        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            IdentityRole role = _context.Roles.Find(roleId);

            string result = await _roleManager.GetRoleNameAsync(role);

            return result;
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                List<IdentityRole> result = new List<IdentityRole>();

                result = await _context.Roles.ToListAsync();

                return result;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(PSUser user)
        {
            try
            {
                IEnumerable<string> result = await _userManager.GetRolesAsync(user);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> GetUserMainRoleAsync(PSUser user)
        {
            try
            {
                IEnumerable<string> userRoles = await _userManager.GetRolesAsync(user);

                List<Roles> allRoles = userRoles.Select(r => (Roles)Enum.Parse(typeof(Roles), r)).ToList();

                string mainRole = allRoles.OrderBy(r => (int)r).FirstOrDefault().ToString();

                return mainRole;

            }
            catch (Exception)
            {

                throw;
            }
        }

       

        public async Task<List<PSUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {

                List<PSUser> users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();

                List<PSUser> result = users.Where(x => x.CompanyId == companyId).ToList();

                return result;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<PSUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            try
            {
                List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(x => x.Id).ToList();

                List<PSUser> roleUsers = _context.Users.Where(x => !userIds.Contains(x.Id)).ToList();

                List<PSUser> result = roleUsers.Where(x => x.CompanyId == companyId).ToList();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserInRoleAsync(PSUser user, string roleName)
        {
            try
            {
                bool result = await _userManager.IsInRoleAsync(user, roleName);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(PSUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(PSUser user, IEnumerable<string> roles)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
