namespace IndasApp.API.Models.Entities
{
    public class User
    {
        // ... existing properties (Id, FullName, etc.) ...
        public int RoleId { get; set; }
        
        // --- ADD THIS NEW PROPERTY ---
        public int? TeamId { get; set; } // Nullable int

        public bool IsActive { get; set; }
        // ... rest of the properties ...
    }
}