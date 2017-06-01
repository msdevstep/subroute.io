using NuGet;
using Subroute.Core.Data;
using Subroute.Core.Nuget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Subroute.Api.Controllers
{
    /// <summary>
    /// Provides functionality for searching nuget packages.
    /// </summary>
    public class NugetController : ApiController
    {
        private readonly INugetService _nugetService;

        /// <summary>
        /// Creates an instance of <see cref="NugetController"/>.
        /// </summary>
        /// <param name="nugetService">Provides logic for searching package source for nuget packages.</param>
        public NugetController(INugetService nugetService)
        {
            _nugetService = nugetService;
        }

        /// <summary>
        /// Searches nuget package repository by keyword for relevant results.
        /// </summary>
        /// <param name="keyword">Keywords used to narrow search results.</param>
        /// <param name="skip">Specifies the starting index of the results returned.</param>
        /// <param name="take">Specifies the number of records to return in the results.</param>
        /// <returns>An array of <see cref="IPackage"/> representing a list of package results.</returns>
        [Route("nuget/v1")]
        public async Task<PagedCollection<NugetPackage>> GetSearchPackages(string keyword = null, int? skip = 0, int? take = 20)
        {
            return await _nugetService.SearchPackagesAsync(keyword, skip, take);
        }
    }
}
