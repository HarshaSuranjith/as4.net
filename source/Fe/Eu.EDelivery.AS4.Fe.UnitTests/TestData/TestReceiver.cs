﻿using System.ComponentModel;

namespace Eu.EDelivery.AS4.Fe.Tests.TestData
{
    [Info("Test receiver")]
    public class TestReceiver : ITestReceiver
    {
        public string Name { get; set; }
        [Info("FRIENDLYNAME","REGEX","TYPE")]
        [Description("DESCRIPTION")]
        public string Test { get; set; }
    }
}