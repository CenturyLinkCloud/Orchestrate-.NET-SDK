using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public static class RequestExceptionUtility
    {
        public static async Task<RequestException> Make(HttpResponseMessage response)
        {
            string requestId = response.Headers.GetValues("x-orchestrate-req-id").FirstOrDefault();
            string rawJson = await response.Content.ReadAsStringAsync();

            var error = JsonConvert.DeserializeObject<Error>(rawJson);
            if (error != null)
            {
                if (error.code.Equals("items_not_found"))
                    return new NotFoundException(response.StatusCode, rawJson, requestId);

                if (error.code.Equals("api_bad_request"))
                    return new BadRequestException(response.StatusCode, rawJson, requestId);

                if (error.code.Equals("patch_conflict"))
                    return new PatchConflictException(response.StatusCode, rawJson, requestId);
            }

            return new RequestException(response.StatusCode, rawJson, requestId);
        }
    }
}
