using Subroute.Core.Models.Compiler;
using Subroute.Core.Nuget;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Subroute.Core.Data
{
    public class RoutePackage
    {
        [Key, Column(Order = 0)]
        public string Id { get; set; }
        [Key, Column(Order = 1)]
        public int RouteId { get; set; }
        public string Version { get; set; }
        public DependencyType Type { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public virtual Route Route { get; set; }
    }
}