using System.ComponentModel.DataAnnotations;

namespace Project2.Models
{
    public class User : Entity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "No Email specified")]
        [RegularExpression(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", ErrorMessage = "Incorrect email")]
        [EmailAddress(ErrorMessage = "Incorrect email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password is not specified")]
        [DataType(DataType.Password)]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "The password is too short!")]
        public string Password { get; set; }

        public int? RoleId { get; set; }

        public Role Role { get; set; }
    }
}
