using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Auth.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
    }
}
