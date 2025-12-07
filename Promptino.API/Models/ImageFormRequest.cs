using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promptino.API.Models;

public class ImageFormRequest
{
    public string Title { get; set; }
    public IFormFile File { get; set; }
    public string GeneratedWith { get; set; }
}
