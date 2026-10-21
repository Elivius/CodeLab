using System;

namespace CodeLab.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
