using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Subroute.Core.Data
{
    public class RouteSetting
    {
        [Key, Column(Order = 0)]
        public string Name { get; set; }
        [Key, Column(Order = 1)]
        public int RouteId { get; set; }
        public string Value { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }

        public virtual Route Route { get; set; }
    }
}