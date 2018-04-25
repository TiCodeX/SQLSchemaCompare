using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLCompare.UI.Middlewares
{
    public class RequestValidatorSettings
    {
        public string AllowedRequestGuid { get; set; }
        public string AllowedRequestAgent { get; set; }
    }
}
