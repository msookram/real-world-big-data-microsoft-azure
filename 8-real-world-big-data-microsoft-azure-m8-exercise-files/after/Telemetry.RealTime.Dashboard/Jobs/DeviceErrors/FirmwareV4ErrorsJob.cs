using dashing.net.common;
using dashing.net.Jobs.DeviceErrors;
using System;
using System.ComponentModel.Composition;
using System.Threading;

namespace dashing.net.Jobs
{
    [Export(typeof(IJob))]
    public class FirmwareV4ErrorsJob : FirmwareErrorsJobBase, IJob
    {
        protected override int FirmwareVersion
        {
            get { return 4; }
        }

        public Lazy<Timer> Timer { get; private set; }

        public FirmwareV4ErrorsJob()
        {
            Timer = new Lazy<Timer>(() => new Timer(SendFirmwareErrorMessages, null, TimeSpan.Zero, TimeSpan.FromSeconds(30)));
        }
    }
}