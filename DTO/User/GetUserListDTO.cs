namespace PotatoTrade.DTO.User
{
    public record GetUserListDTO
    {
        public int Id { get; set; }
        public string Avatar { get; set; } 
        public int RoleId  {get; set;} 
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string RoleType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt {get; set;} 
    }
}