using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Simple.OData.Client;

namespace Simple.Odata.Client.Host.Controllers
{
    public class TestController : ApiController
    {
        // GET: api/Test
        public async Task<HttpResponseMessage> Get(HttpRequestMessage request)
        {
            V4ModelAdapter.Reference();

            var client = new ODataClient("http://services.odata.org/V4/TripPinServiceRW");
            var peopleTask = await client.For("Photos").FindEntriesAsync();

            return request.CreateResponse(HttpStatusCode.OK, peopleTask);
        }

        // GET: api/Test/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Test
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Test/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Test/5
        public void Delete(int id)
        {
        }
    }
}
