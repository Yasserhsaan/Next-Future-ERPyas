using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.Models
{
    public class DatabaseConnectionModel
    {


        public string Type { get; set; }
        public string ServerName { get; set; }
        public string DataBaseName { get; set; }
        public bool UseWindowsAuth { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
