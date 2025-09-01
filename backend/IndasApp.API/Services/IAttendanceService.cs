namespace IndasApp.API.Services
{
    public interface IAttendanceService
    {
        // Yeh contract kehta hai ki hamare paas ek method hoga jo
        // login ke time par attendance mark karega.
        Task MarkLoginAttendanceAsync(int userId, double latitude, double longitude);
         Task EndDutyAsync(int userId, DateTime date);

    }
}