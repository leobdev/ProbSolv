using ProbSolv.Models;

namespace ProbSolv.Services.Interfaces
{
    public interface IPSProjectService
    {
        public Task AddNewProjectAsync(Project project);

        public Task<bool> AddProjectManagerAsync(string userId, int projectId);

        public Task<bool> AddUserToProjectAsync(string userId, int projectId);

        public Task ArchiveProjectAsync(Project project);

        public Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId);

        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName);

        public Task<List<PSUser>> GetAllProjectMembersExceptPMAsync(int projectId);

        public Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId);

        public Task<List<PSUser>> GetDevelopersOnProjectAsync(int projectId);

        public Task<PSUser> GetProjectManagerAsync(int projectId);

        public Task<List<PSUser>> GetProjectMembersByRoleAsync(int projectId, string role);

        public Task<Project> GetProjectByIdAsync(int projectId, int companyId);

        public Task<List<PSUser>> GetSubmittersOnProjectAsync(int projectId);

        public Task<List<Project>> GetUnassignedProjectsAsync(int companyId);

        public Task<List<PSUser>> GetUsersNotOnProjectAsync(int projectId, int companyId);

        public Task<List<Project>> GetUserProjectsAsync(string userId);

        public Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId);

        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);

        public Task<int> LookupProjectPriorityId(string priorityName);

        public Task RemoveProjectManagerAsync(int projectId);

        public Task RemoveUsersFromProjectByRoleAsync(string role, int projectId);

        public Task RemoveUserFromProjectAsync(string userId, int projectId);

        public Task RestoreProjectAsync(Project project);


        public Task UpdateProjectAsync(Project project);


    }
}
