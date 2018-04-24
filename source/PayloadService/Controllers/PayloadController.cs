using System;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Infrastructure;
using Eu.EDelivery.AS4.PayloadService.Models;
using Eu.EDelivery.AS4.PayloadService.Persistance;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.PayloadService.Controllers
{
    /// <summary>
    /// Controller to handle calls to persistence payload services.
    /// </summary>
    [Route("api/[controller]")]
    public class PayloadController : Controller
    {
        private readonly IPayloadPersister _payloadPersistor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadController" /> class.
        /// </summary>
        /// <param name="payloadPersister">The payload persister.</param>
        public PayloadController(IPayloadPersister payloadPersister)
        {
            _payloadPersistor = payloadPersister;
        }

        /// <summary>
        /// Upload a given Multipart payload to the configured persistence serivce.
        /// </summary>
        /// <remarks>In this concept, only one file-content in the Request content is supported.</remarks>
        /// <returns>Web API payload id reference.</returns>
        /// <response code="200">Returns 'Success' if the payload was uploaded correctly in the configured persistence service..</response>
        /// <response code="400">Returns 'Bad Request' if the request Content Type isn't of the type 'Multi Part'.</response>
        [HttpPost]
        [Route("Upload")]
        [ProducesResponseType(typeof(UploadResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestObjectResult), (int) HttpStatusCode.BadRequest)]
        [Produces("application/xml", "application/json")]
        public async Task<IActionResult> Upload()
        {
            (bool success, MultipartPayloadReader reader) result = MultipartPayloadReader.TryCreate(Request.Body, Request.ContentType);

            if (!result.success)
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            string payloadId = await UploadPayloadsWith(result.reader);
            string downloadUrl = FormatWithDownloadUrl(payloadId);

            return new OkObjectResult(new UploadResult(payloadId, downloadUrl));
        }

        private async Task<string> UploadPayloadsWith(MultipartPayloadReader reader)
        {
            string payloadId = Guid.NewGuid().ToString();

            await reader.StartReading(
                async payload => payloadId = await _payloadPersistor.SavePayload(payload));

            return payloadId;
        }

        private string FormatWithDownloadUrl(string payloadId)
        {
            var location = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path.Value.Replace("Upload", payloadId)}");

            return location.AbsoluteUri;
        }

        /// <summary>
        /// Download a Multipart payload from the configured persistence service.
        /// </summary>
        /// <param name="id">Id that references a multipart payload in the persistence service.</param>
        /// <returns>Multipart payload.</returns>
        /// <response code="200">Returns 'Success' if the payload was downloaded correctly in the configured persistence service..</response>
        /// <response code="404">Returns 'Not Found' if the given Payload Id is not found in the configured persistence service.</response>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(StreamedFileResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(NotFoundResult), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Download([FromRoute] string id)
        {
            Payload payload = await _payloadPersistor.LoadPayload(id);

            if (payload.Equals(Payload.Null))
            {
                return NotFound();
            }

            return new StreamedFileResult(payload.Content, payload.Meta.OriginalFileName, "application/octet-stream");
        }
    }
}