using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChatApplication.Models;
using BCrypt.Net;

namespace ChatApplication.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SignUpModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? Username { get; set; }

        public string? ErrorMessage { get; set; }

        public string? PasswordErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || !Email.Contains("@"))
            {
                ErrorMessage = "Invalid email format.";
                return Page();
            }

            // if (string.IsNullOrEmpty(Password) || Password.Length < 8 || 
            //     !Password.Any(char.IsUpper) || !Password.Any(char.IsLower) || !Password.Any(char.IsDigit))
            // {
            //     PasswordErrorMessage = "Password must be at least 8 characters long, contain an upper case letter, a lower case letter, and a digit.";
            //     return Page();
            // }

            var existingUser = _context.Users?.FirstOrDefault(u => u.Email == Email);
            if (existingUser != null)
            {
                ErrorMessage = "An account with this email already exists.";
                return Page();
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

            var newUser = new User
            {
                Email = Email,
                PasswordHash = hashedPassword,
                Username = Username,
                CreatedAt = DateTime.UtcNow  
            };

            _context.Users?.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToPage("/SignIn");
        }
    }
}
