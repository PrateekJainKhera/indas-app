namespace IndasApp.API.Services
{
    public interface ISecurityService
    {
        Task<bool> IsUserInLeadsTeamAsync(int teamLeadId, int userId);
    }
}