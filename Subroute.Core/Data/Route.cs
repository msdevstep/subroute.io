using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Subroute.Core.Models.Routes;

namespace Subroute.Core.Data
{
    public class Route
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Uri { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public byte[] Assembly { get; set; }
        public bool IsOnline { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsDefault { get; set; }
        public int StarredCount { get; set; }
        public int ClonedCount { get; set; }
        public DateTimeOffset? ClonedOn { get; set; }
        public DateTimeOffset? PublishedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
    }
}