﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    [Route("api/[controller]")]
    public class RuntimeController
    {
        private readonly IRuntimeLoader runtimeLoader;

        public RuntimeController(IRuntimeLoader runtimeLoader)
        {
            this.runtimeLoader = runtimeLoader;
        }

        [HttpGet]
        [Route("getreceivers")]
        public IEnumerable<ItemType> GetReceivers()
        {
            return runtimeLoader.Receivers;
        }

        [HttpGet]
        [Route("getsteps")]
        public IEnumerable<ItemType> GetSteps()
        {
            return runtimeLoader.Steps;
        }

        [HttpGet]
        [Route("gettransformers")]
        public IEnumerable<ItemType> GetTransformers()
        {
            return runtimeLoader.Transformers;
        }

        [HttpGet]
        [Route("getcertificaterepositories")]
        public IEnumerable<ItemType> GetCertificateRepositories()
        {
            return runtimeLoader.CertificateRepositories;
        }

        [HttpGet]
        [Route("getdeliversenders")]
        public IEnumerable<ItemType> GetDeliverSenders()
        {
            return runtimeLoader.DeliverSenders;
        }

        [HttpGet]
        [Route("getall")]
        public IActionResult GetAll()
        {
            return new ObjectResult(new
            {
                Receivers = GetReceivers(),
                Steps = GetSteps(),
                Transformers = GetTransformers(),
                CertificateRepositories = GetCertificateRepositories(),
                DeliverSenders = GetDeliverSenders(),
                RuntimeMetaData = JObject.Parse(JsonConvert.SerializeObject(runtimeLoader.ReceivingPmode, Formatting.Indented, new FlattenRuntimeToJsonConverter()))
            });
        }

        [HttpGet]
        [Route("getruntimemetadata")]
        public IActionResult GetRuntimeMetaData()
        {
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(runtimeLoader.ReceivingPmode, Formatting.Indented, new FlattenRuntimeToJsonConverter()),
                ContentType = "application/json"
            };
        }
    }
}