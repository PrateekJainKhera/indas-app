using Microsoft.Data.SqlClient;

namespace IndasApp.API.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly IConfiguration _configuration;

        public SecurityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> IsUserInLeadsTeamAsync(int teamLeadId, int targetUserId)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);

            // This query finds the team of the logged-in lead,
            // and then checks if the target user is in that same team.
            var query = @"
                SELECT COUNT(1) 
                FROM Users 
                WHERE Id = @TargetUserId 
                  AND TeamId = (SELECT TeamId FROM Users WHERE Id = @TeamLeadId);
            ";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TeamLeadId", teamLeadId);
            command.Parameters.AddWithValue("@TargetUserId", targetUserId);

            await connection.OpenAsync();
            var result = (int)await command.ExecuteScalarAsync();

            return result > 0;
        }
    }
}