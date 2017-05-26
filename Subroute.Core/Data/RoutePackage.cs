using System;
using System.ComponentModel.DataAnnotations;

namespace Subroute.Core.Data
{
    public class RoutePackage
    {
        [Key]
        public int Id { get; set; }
        public int RouteId { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Hash { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public virtual Route Route { get; set; }
    }
}