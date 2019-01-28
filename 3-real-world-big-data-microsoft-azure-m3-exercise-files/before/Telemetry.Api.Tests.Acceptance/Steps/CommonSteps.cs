using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Telemetry.Api.Tests.Acceptance.Steps
{
    [Binding]
    public class CommonSteps : StepBase
    {
        [Given(@"the event content is from '(.*)'")]
        public void GivenTheEventContentIsFrom(string fileName)
        {
            IsRequestCompressed = fileName.EndsWith(".gz");
            RequestContentFilename = fileName;
        }

        [Then(@"the device receives an API response")]
        public void ThenTheDeviceReceivesAnAPIResponse()
        {
            Assert.IsNotNull(ApiResponse);
        }

        [Then(@"the response status code is '(.*)'")]
        public void ThenTheResponseStatusCodeIs(int expectedStatusCode)
        {
            Assert.AreEqual(expectedStatusCode, (int)ApiResponse.StatusCode);
        }
    }
}
