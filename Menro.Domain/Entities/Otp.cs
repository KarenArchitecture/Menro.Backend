using Menro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class Otp
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        //public OtpType? OtpType { get; set; }
        public bool IsUsed { get; set; }
    }
}
