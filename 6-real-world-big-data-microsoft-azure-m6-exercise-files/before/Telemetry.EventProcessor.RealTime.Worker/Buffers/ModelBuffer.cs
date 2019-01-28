using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Telemetry.Core;
using Telemetry.Core.Logging;
using timers = System.Timers;

namespace Telemetry.RealTime.Worker.Buffers.ModelBuffer
{
    public class ModelBuffer<TModel>
    {
        private Action<IEnumerable<TModel>> _doSave;
        private Func<TModel, string> _getKey;
        private ConcurrentDictionary<string, TModel> _models;
        
        private timers.Timer _timer;
        protected Logger _log;

        public ModelBuffer(Action<IEnumerable<TModel>> saveAction, Func<TModel, string> keyAccessor, TimeSpan flushTimespan)
        {
            Initialise(saveAction, keyAccessor, flushTimespan);
        }

        public ModelBuffer(Action<IEnumerable<TModel>> saveAction, Func<TModel, string> keyAccessor)
        {
            var config = Config.Get("ModelBuffers." + typeof(TModel).Name + ".BufferFlushTime");
            if (string.IsNullOrEmpty(config))
            {
                config = Config.Get("ModelBuffers.BufferFlushTime");
            }
            Initialise(saveAction, keyAccessor, TimeSpan.Parse(config));
        }

        private void Initialise(Action<IEnumerable<TModel>> saveAction, Func<TModel, string> keyAccessor, TimeSpan flushTimespan)
        {
            _log = this.GetLogger();
            _doSave = saveAction;
            _getKey = keyAccessor;
            _models = new ConcurrentDictionary<string, TModel>();

            _timer = new timers.Timer(flushTimespan.TotalMilliseconds);
            _timer.Elapsed += FlushBuffers;
        }

        public void Add(TModel model)
        {
            var key = _getKey(model);
            _models.TryAdd(key, model);

            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        public TModel Get(TModel template)
        {
            TModel model;
            var key = _getKey(template);
            if (!_models.TryGetValue(key, out model))
            {
                model = default(TModel);
            }
            return model;
        }

        private void FlushBuffers(object sender, timers.ElapsedEventArgs e)
        {
            FlushBuffers();
        }

        private void FlushBuffers()
        {
            _log.TraceEvent("FlushBuffers",
                        new Facet("status", "Started"));

            var allEntities = new List<TModel>();
            allEntities.AddRange(_models.Select(x => x.Value));
            _models.Clear();

            if (_doSave != null && allEntities.Any())
            {
                _doSave(allEntities);
            }

            _log.TraceEvent("FlushBuffers",
                    new Facet("status", "Completed"),
                    new Facet("count", allEntities.Count()));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                FlushBuffers();
            }
        }
    }
}