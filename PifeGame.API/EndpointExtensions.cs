using PifeGame.Application;

namespace PifeGame.API
{
    public static class EndpointExtensions
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapGet("/rooms", (GameLobbyService hub) =>
            {
                return hub.ListRooms();
            }).RequireAuthorization();
        }
    }
}
