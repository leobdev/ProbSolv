using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Models;
using ProbSolv.Services.Interfaces;

namespace ProbSolv.Services
{
    public class PSMemberService : IPSMemberService
    {
        private readonly ApplicationDbContext _context;

        public PSMemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PSUser> GetMemberByIdAsync(int companyId, string userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(user => user.CompanyId == companyId)
                    .FirstOrDefaultAsync(user => user.Id == userId);

                return user ?? throw new Exception("User not found.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveMemberAsync(PSUser member)
        {
            try
            {
                _context.Users.Remove(member);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
    }
}
