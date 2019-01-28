using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telemetry.Api.Tests.Acceptance
{
    [DeploymentItem(@"Resources\Requests\device-event.json", @"Resources\Requests")]
    [DeploymentItem(@"Resources\Requests\no-event.json", @"Resources\Requests")]
    [DeploymentItem(@"Resources\Requests\device-event-compressed.json.gz", @"Resources\Requests")]
    [DeploymentItem(@"Resources\Requests\device-events-large.json.gz", @"Resources\Requests")]
    public partial class TelemetryIngestionFeature
    {
    }
}
