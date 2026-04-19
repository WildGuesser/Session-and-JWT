using Npgsql;
using System.Data;

namespace Forum.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly NpgsqlDataSource _dataSource;

        public SessionMiddleware (RequestDelegate next, NpgsqlDataSource dataSource)
        {
            _next = next;
            _dataSource = dataSource;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string sessionId = context.Request.Cookies["session_id"];

            Console.WriteLine($"sessionId value: '{sessionId}'");
            Console.WriteLine($"sessionId is null: {sessionId == null}");
            Console.WriteLine($"sessionId IsNullOrWhiteSpace: {string.IsNullOrWhiteSpace(sessionId)}");


            if (string.IsNullOrWhiteSpace(sessionId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Open an explicit connection so the command has a backing Connection
            // property. NpgsqlDataSource.CreateCommand returns a command that
            // doesn't expose Connection until it's associated with an opened
            // connection, which can throw NotSupportedException when inspected
            // in the debugger. Creating the command from an opened connection
            // avoids that.
            await using var conn = await _dataSource.OpenConnectionAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "Select * FROM sessions where sessionid = $1";
            cmd.Parameters.AddWithValue(Guid.Parse(sessionId));

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                context.Response.StatusCode = 401;
                return;
            }

            context.Items["sessionId"] = sessionId;

            await _next(context);
        }

    }
}
