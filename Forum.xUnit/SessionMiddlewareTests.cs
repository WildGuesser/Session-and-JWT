using Forum.xUnit.Methods;
using System.Drawing;
using System.Net;

namespace Forum.xUnit
{
    public class SessionMiddlewareTests : IClassFixture<MiddlewareHelper> ,IAsyncLifetime
    {

        private readonly MiddlewareHelper _middlewareHelper;

        public SessionMiddlewareTests(MiddlewareHelper middlewareHelper)
        {
            _middlewareHelper = middlewareHelper;
        }

        /// <summary>
        /// runs before any test in the class executes. 
        /// This is where you boot up your test server, database connections, etc. so everything is ready before tests start.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            await _middlewareHelper.InitializeAsync(); // boots the app before tests
        }

        /// <summary>
        ///  runs after all tests in the class finish. 
        ///  This is where you clean up — close connections, stop containers, dispose the factory. 
        ///  In your case you're returning
        /// </summary>
        /// <returns></returns>
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Request_WithValidCookie_Returns200()
        {
            HttpClient client = _middlewareHelper.CreateClient(); // fresh client, no leftover headers

            //This adds a header called session_id with that GUID value to every request the client makes from this point on. DefaultRequestHeaders means it's permanently attached to the client, not just this one request.
            client.DefaultRequestHeaders.Add("Cookie", "session_id=2cb4da62-f942-4679-9a71-6b3c406954fc");

            //Sends a GET request to /any-route on your test server.
            var response = await client.GetAsync("/api/Auth/5");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }

        [Fact]
        public async Task Request_WithInvalidCookie_Returns401()
        {
            HttpClient client = _middlewareHelper.CreateClient(); // fresh client, no leftover headers

            //This adds a header called session_id with that GUID value to every request the client makes from this point on. DefaultRequestHeaders means it's permanently attached to the client, not just this one request.
            client.DefaultRequestHeaders.Add("Cookie", "session_id=2cb4da62-f942-4679-9a71-6b3c406954ff");

            //Sends a GET request to /any-route on your test server.
            var response = await client.GetAsync("/api/Auth/5");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        }


        [Fact]
        public async Task Request_WithEmptyCookie_Returns401()
        {
            HttpClient client = _middlewareHelper.CreateClient(); // fresh client, no leftover headers

            //This adds a header called session_id with that GUID value to every request the client makes from this point on. DefaultRequestHeaders means it's permanently attached to the client, not just this one request.
            client.DefaultRequestHeaders.Add("Cookie", "session_id=");

            //Sends a GET request to /any-route on your test server.
            var response = await client.GetAsync("/api/Auth/5");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        }
    }
}
