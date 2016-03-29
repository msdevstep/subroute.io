using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subroute.Core.TypeConverters;

namespace Subroute.Core.Models.Routes
{
    [TypeConverter(typeof(RouteIdentifierTypeConverter))]
    public class RouteIdentifier
    {
        public RouteIdentifier(string identifier)
        {
            Identifier = identifier;

            int intId;
            if (int.TryParse(identifier, out intId))
            {
                Type = RouteIdentifierType.Id;
                Id = intId;
                return;
            }

            Type = RouteIdentifierType.Uri;
            Uri = identifier;
        }

        public string Identifier { get; }

        public RouteIdentifierType Type { get; }

        public int Id { get; }

        public string Uri { get; }

        public static implicit operator RouteIdentifier(string uri)
        {
            return new RouteIdentifier(uri);
        }

        public static implicit operator RouteIdentifier(int id)
        {
            return new RouteIdentifier(id.ToString());
        }

        public override string ToString()
        {
            return Identifier;
        }
    }
}
