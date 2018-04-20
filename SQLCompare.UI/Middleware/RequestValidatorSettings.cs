using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLCompare.UI.Middleware
{
    public class RequestValidatorSettings
    {
        public string AllowedRequestGuid { get; set; }
        public string AllowedRequestAgent { get; set; }
    }
}
