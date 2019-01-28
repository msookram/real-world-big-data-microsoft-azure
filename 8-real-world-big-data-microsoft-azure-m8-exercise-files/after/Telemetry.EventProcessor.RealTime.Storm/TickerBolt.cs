using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.SCP;
using Microsoft.SCP.Rpc.Generated;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    public class TickerBolt : ISCPBolt
    {
        private Context ctx;

        public TickerBolt(Context ctx)
        {
            this.ctx = ctx;

            var inputSchema = new Dictionary<string, List<Type>>();

            //Add the Tick tuple Stream in input streams - A tick tuple has only one field of type long
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));
        }

        public static TickerBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new TickerBolt(ctx);
        }

        public void Execute(SCPTuple tuple)
        {
            Context.Logger.Info(" --TickerBolt--> tick: {0}", tuple.GetLong(0));
        }
    }
}