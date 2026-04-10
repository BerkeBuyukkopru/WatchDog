using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Watchdog.Application.Attributes
{
    // Kendi yazdığımız, her yerde kullanılabilecek evrensel e-posta listesi doğrulama kuralı
    public class CommaSeparatedEmailsAttribute : ValidationAttribute
    {
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Boş bırakılmışsa geçerli say (Çünkü zorunlu [Required] değil)
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            var emails = value.ToString()!.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var email in emails)
            {
                if (!EmailRegex.IsMatch(email.Trim()))
                {
                    // Hatalı mail bulduğunda, DTO'da belirlediğimiz ErrorMessage ile birlikte maili göster
                    return new ValidationResult($"{ErrorMessage}: {email.Trim()}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
