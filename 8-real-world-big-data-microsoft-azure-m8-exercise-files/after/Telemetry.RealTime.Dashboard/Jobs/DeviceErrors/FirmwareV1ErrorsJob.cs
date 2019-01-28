using dashing.net.common;
using dashing.net.Jobs.DeviceErrors;
using System;
using System.ComponentModel.Composition;
using System.Threading;

namespace dashing.net.Jobs
{
    [Export(typeof(IJob))]
    public class FirmwareV1ErrorsJob : FirmwareErrorsJobBase, IJob
    {
        protected override int FirmwareVersion
        {
            get { return 1; }
        }

        public Lazy<Timer> Timer { get; private set; }

        public FirmwareV1ErrorsJob()
        {
            Timer = new Lazy<Timer>(() => new Timer(SendFirmwareErrorMessages, null, TimeSpan.Zero, TimeSpan.FromMinutes(30)));
        }        
    }
}