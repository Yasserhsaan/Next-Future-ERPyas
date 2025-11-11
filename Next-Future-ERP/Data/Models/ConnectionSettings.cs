using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Models
{
  public  class ConnectionSettings
    {
        public  string? Type { get; set; }     // "Local" or "Server"
        public  string? Server { get; set; }
        public  string? Database { get; set; }
        public  string? Username { get; set; } // Optional
        public  string? Password { get; set; } // Optional
        public  int? Port { get; set; }        // Optional SQL port (e.g., 1433)
    }
}
