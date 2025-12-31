using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RobloxWebserver.Controllers
{
    /// <summary>
    /// Placeholder controller for future place (per-place) management endpoints.
    ///
    /// Planned responsibilities (to be implemented later):
    /// - Configure Start Place: GET/POST endpoints similar to Roblox's /places/{id}/update
    ///   that allow editing name, description, and basic settings for a place inside a universe.
    /// - List Places in a Universe: endpoint used by the place selector modal
    ///   (/universes/get-places-by-context today is stubbed by static HTML). The list of
    ///   place ids comes from the universes.place_ids array.
    /// - Add New Place to Universe: create an additional place asset and append its id to
    ///   the universes.place_ids array for that universe.
    /// - Toggle place public/private and shutdown servers knobs that the gear menu exposes.
    ///
    /// This controller intentionally has no live endpoints yet so routing will not change
    /// until the database schema and front-end flows are fully wired up.
    /// </summary>
    [ApiController]
    [Route("places")]
    [Authorize]
    public sealed class PlacesController : Controller
    {
    }
}
