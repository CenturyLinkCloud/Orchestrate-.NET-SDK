using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public static class RequestExceptionUtility
    {
        public static async Task<RequestException> Make(HttpResponseMessage response)
        {
            string requestId = response.Headers.GetValues("x-orchestrate-req-id").FirstOrDefault();
            string rawJson = await response.Content.ReadAsStringAsync();

            var error = JsonConvert.DeserializeObject<Error>(rawJson);
            if (error != null && error.Code != null)
            {
                if (error.Code.Equals("items_not_found"))
                    return new NotFoundException(response.StatusCode, rawJson, requestId);

                if (error.Code.Equals("api_bad_request"))
                    return new BadRequestException(response.StatusCode, rawJson, requestId);

                if (error.Code.Equals("patch_conflict"))
                    return new PatchConflictException(response.StatusCode, rawJson, requestId);
            }

            return new RequestException(response.StatusCode, rawJson, requestId);
        }
    }
}
