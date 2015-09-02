using System.Collections.Generic;
using System.Web.Http;

namespace main
{
    /// <summary>
    /// Controller für die <c>/values</c>-Route
    /// </summary>
    public class ValuesController : ApiController
    {
        /// <summary>
        /// <c>GET api/values</c>
        /// </summary>
        /// <returns>The enumeration of values.</returns>
        public IEnumerable<string> Get()
        {
            return new [] { "value1", "value2" };
        }

        /// <summary>
        /// <c>GET api/values/5</c>
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The value with the given id.</returns>
        public string Get(int id)
        {
            return $"value of {id}";
        }

        /// <summary>
        /// <c>POST api/values</c>
        /// </summary>
        /// <param name="value">The value.</param>
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// <c>PUT api/values/5</c>
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// <c>DELETE api/values/5</c>
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(int id)
        {
        }
    }
}
