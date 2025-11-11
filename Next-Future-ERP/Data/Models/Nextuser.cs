using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Models
{
    public class Nextuser
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Fname { get; set; }
        public  string Mobile { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public int UserRollid { get; set; }
        public int Nsync { get; set; }
        [Timestamp]
        public byte[] Dbtimestamp { get; set; }
        public DateTime LastLogin { get; set; } = DateTime.Now;
    }
}
