using System.ComponentModel.DataAnnotations;

namespace LibraryWebApplication.Controllers;

public class LoginViewModel
{
    [Required] [DataType(DataType.EmailAddress)]
    public string Email;

    [Required] [DataType(DataType.Password)]
    public string Password;

    public bool RememberMe;
}