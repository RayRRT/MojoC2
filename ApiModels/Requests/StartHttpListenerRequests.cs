using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
    public class StartHttpListenerRequests
    {
        public string  Name { get; set; }
        public int BindPort { get; set; }
    }
}
