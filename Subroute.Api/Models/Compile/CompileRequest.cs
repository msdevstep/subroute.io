using Subroute.Core.Models.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Subroute.Api.Models.Compile
{
    public class CompileRequest
    {
        public string Code { get; set; }
        public Dependency[] Dependencies { get; set; }
    }
}