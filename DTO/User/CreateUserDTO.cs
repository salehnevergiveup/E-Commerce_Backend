// Models/CreateUserDTO.cs

using PotatoTrade.DTO.MediaDTO;

public class CreateUserDTO 
{
    public int RoleId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    
    public string Gender { get; set; }
    
    public int Age { get; set; }
    
    public string Email { get; set; }
    
    public string Status { get; set; }
    public string PhoneNumber { get; set; }
    
    public string BillingAddress { get; set; }
    
    public List<HandleMedia> Medias { get; set; } // New property to handle media
}
