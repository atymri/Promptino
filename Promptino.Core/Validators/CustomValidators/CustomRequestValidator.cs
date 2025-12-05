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
}
