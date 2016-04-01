using System;
using System.ComponentModel.DataAnnotations;

namespace Subroute.Core.Data
{
    public class RouteSetting
    {
        [Key]
        public string Name { get; set; }
        public int RouteId { get; set; }
        public string Value { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }

        public virtual Route Route { get; set; }
    }
}