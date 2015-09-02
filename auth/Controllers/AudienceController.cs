using System;
using System.Diagnostics;
using System.Web.Http;
using auth.Models;
using JetBrains.Annotations;

namespace auth.Controllers
{
    /// <summary>
    /// Controller für das Registrieren von neuen Audiences.
    /// </summary>
    [RoutePrefix("api/audience")]
    public class AudienceController : ApiController
    {
        /// <summary>
        /// Erzeugt eine neue <see cref="Entities.Audience"/> anhand des <see cref="AudienceModel"/>.
        /// </summary>
        /// <param name="audienceModel">The audience model.</param>
        /// <returns>Der Rückgabecode.</returns>
        [Route("")]
        [NotNull]
        public IHttpActionResult Post([CanBeNull] AudienceModel audienceModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Debug.Assert(audienceModel != null, "audienceModel != null");

            try
            {
                var newAudience = AudiencesStore.AddAudience(audienceModel.Name);
                return Ok(newAudience);
            }
            catch (OverflowException e)
            {
                return InternalServerError(e);
            }
        }
    }
}
