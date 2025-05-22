using System.ComponentModel.DataAnnotations;

namespace ProjectInfrastructure.Models;

public class RegisterViewModel
{
    [Required] [DataType(DataType.EmailAddress)]
    public string Email;
    
    [Required]
    [Display(Name = "Passworld")]
    [DataType(DataType.Password)]
    public string Passworld { get; set; } = null!; 
        
    [Required]
    [Display(Name = "Confirm Passworld")]
    [DataType(DataType.Password)]
    public string ConfirmationPassworld { get; set; } = null!; 
}