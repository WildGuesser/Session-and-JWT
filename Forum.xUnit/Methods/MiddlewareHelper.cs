using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Forum.xUnit.Methods
{
    /// <summary>
    /// Just a helper class you created to avoid repeating the setup code in every test class. 
    /// Instead of every test class setting up the factory itself, they all use this one helper.
    /// </summary>
    public class MiddlewareHelper
    {
        //This declares a field that will hold the factory instance
        private WebApplicationFactory<Program> _factory = null!;

        //This method just creates a new client each call, that is prepointed to the test server.
        //Each call resets the client to a fresh state, so no leftover headers or cookies from previous tests.
        //HttpClient is just a class that lets you send HTTP requests(GET, POST, PUT, DELETE etc.)
        /// <summary>
        /// When you call this:
        ///1.WebApplicationFactory runs your Program.cs top to bottom
        ///2.All services get registered
        ///3.All middleware gets registered
        ///4.Test server starts in memory
        ///5.Returns an HttpClient already pointed at that server
        /// </summary>
        /// <returns>An HttpClient already pointed at the test server.</returns>
        public HttpClient CreateClient()
        {
            return _factory.CreateClient();
        }
        //this method is called before any tests run, it initializes the factory that will be used to create clients for testing.
        //the sequence is:
        //InitializeAsync() → factory created but app not started yet
        //CreateClient()    → app boots, Program.cs runs, middleware registered, test server starts
        //GetAsync(...)     → request sent into the pipeline
        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        public async Task DisposeAsync()
        {
            await _factory.DisposeAsync();
        }
    }

    // Execution flow:
    // xUnit starts
    // → IAsyncLifetime.InitializeAsync() called
    // → MiddlewareHelper.InitializeAsync() called
    // → _factory created (app not started yet)
    //
    // Test method executes
    // → CreateClient() called (on first call, factory boots Program.cs)
    // → All services registered
    // → All middleware registered
    // → Test server starts in memory
    // → HttpClient returned
    // → client.GetAsync("/route") called
    // → Request goes through middleware pipeline
    // → Response returned and tested
    //
    // IAsyncLifetime.DisposeAsync() called
    // → _factory.DisposeAsync() cleans up test server
    // → Test completes
}
