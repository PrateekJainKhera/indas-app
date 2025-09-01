using Microsoft.Data.SqlClient;

namespace IndasApp.API.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<int>> GetAllActiveUserIdsAsync()
        {
            var userIds = new List<int>();
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            
            var query = "SELECT Id FROM Users WHERE IsActive = 1;";
            
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                userIds.Add(reader.GetInt32(0));
            }
            
            return userIds;
        }
    }
}