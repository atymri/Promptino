using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promptino.Core.Validators.CustomValidators;

public static class CustomRequestValidator
{
    public static bool BeAValidImagePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
        return validExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    //public static bool BeAValidGuid(string id)
    //{
    //    return Guid.TryParse(id, out var guid) && guid != Guid.Empty;
    //}

    public static bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var validDomains = new[]
        {
            "gmail.com", "google.com", "outlook.com", "outlook.org", "hotmail.com",
            "yahoo.com", "yahoo.co.uk", "icloud.com", "aol.com", "protonmail.com",
            "zoho.com", "mail.com", "gmx.com", "live.com", "msn.com", "rediffmail.com",
            "inbox.com", "fastmail.com", "btinternet.com", "me.com", "mac.com"
        };

        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var domain = parts[1].ToLowerInvariant();

        return validDomains.Contains(domain);
    }
}
