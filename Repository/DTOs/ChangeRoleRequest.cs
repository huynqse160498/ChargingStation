using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs
{
   
        public class ChangeRoleRequest
        {
            public string Email { get; set; } = string.Empty;
            public string NewRole { get; set; } = string.Empty;
        }
    

}
