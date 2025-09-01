namespace IndasApp.API.Services
{
    public interface IUserService
    {
        Task<List<int>> GetAllActiveUserIdsAsync();
    }
}