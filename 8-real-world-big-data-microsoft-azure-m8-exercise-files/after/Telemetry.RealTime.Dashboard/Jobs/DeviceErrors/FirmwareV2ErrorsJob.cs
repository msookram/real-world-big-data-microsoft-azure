using dashing.net.common;
using dashing.net.Jobs.DeviceErrors;
using System;
using System.ComponentModel.Composition;
using System.Threading;

namespace dashing.net.Jobs
{
    [Export(typeof(IJob))]
    public class FirmwareV2ErrorsJob : FirmwareErrorsJobBase, IJob
    {
        protected override int FirmwareVersion
        {
            get { return 2; }
        }

        public Lazy<Timer> Timer { get; private set; }

        public FirmwareV2ErrorsJob()
        {
            Timer = new Lazy<Timer>(() => new Timer(SendFirmwareErrorMessages, null, TimeSpan.Zero, TimeSpan.FromSeconds(30)));
        }
    }
}