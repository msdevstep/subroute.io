using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subroute.Api.Models.Routes;
using Subroute.Core.Compiler;
using Subroute.Core.Data;

namespace Subroute.Api.Models.Publish
{
    public class PublishResponse
    {
        public RouteResponse Route { get; set; }
        public CompilationResult Compilation { get; set; }
    }
}
