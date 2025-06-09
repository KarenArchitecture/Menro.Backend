using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.DTO.Auth
{
    public class VerifyOtpDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
