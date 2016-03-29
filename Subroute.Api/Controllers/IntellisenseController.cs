using System.Threading.Tasks;
using System.Web.Http;
using Subroute.Core.Compiler;
using Subroute.Core.Models.Intellisense;

namespace Subroute.Api.Controllers
{
    /// <summary>
    /// Provides intellisense support for saved code.
    /// </summary>
    public class IntellisenseController : ApiController
    {
        private readonly ICompilationService _compilationService;

        public IntellisenseController(ICompilationService compilationService)
        {
            _compilationService = compilationService;
        }

        /// <summary>
        /// Gets a list of current members available at the specified cursor.
        /// </summary>
        /// <param name="request">Cursor position and options of intellisense request.</param>
        /// <returns>Returns an array of available members and their types.</returns>
        [Route("intellisense/v1"), AllowAnonymous]
        public async Task<CompletionResult[]> PostMembers([FromUri]CompletionRequest request)
        {
            var code = await Request.Content.ReadAsStringAsync();

            return await _compilationService.GetCompletionsAsync(request, code);
        }
    }
}