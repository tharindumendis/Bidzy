using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Bidzy.Domain.Enum;

namespace Bidzy.Domain.Enties
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        private static readonly Regex _phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Phone number is required.");

            if (!_phoneRegex.IsMatch(value.ToString()))
                return new ValidationResult("Invalid phone number format.");

            return ValidationResult.Success;
        }
    }

    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required]
        [PhoneNumber]
        public string Phone { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; } // Seller, Bidder, Admin

        public DateTime CreatedAt { get; set; }
    }
}
