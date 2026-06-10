using System.ComponentModel.DataAnnotations;

namespace WUIAM.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public string Email { get; set; } = default!;
        [Required]
        public string OldPassword { get; set; } = default!;
        [Required]
        public string NewPassword { get; set; } = default!;
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }

    public class ResetPasswordDTo
    {
        [Required]
        public string Email { get; set; } = default!;
        [Required]
        public string NewPassword { get; set; } = default!;

        [Required]
        public int ResetToken { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmedPassword { get; set; }
    }

    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = default!;
        [Required]
        public string Password { get; set; } = default!;
    }

    public class MicrosoftLoginDto
    {
        [Required]
        public string IdToken { get; set; } = default!;
    }

    public class ForgotPasswordDto
    {
        [Required]
        public string Email { get; set; } = default!;
    }
    public class VerifyLoginTokenDto
    {
        [Required]
        public string Email { get; set; } = default!;
        [Required]
        public string Token { get; set; } = default!;
    }
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = default!;
    }
}
