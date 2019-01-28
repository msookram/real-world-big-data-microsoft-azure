using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telemetry.RealTime.Worker.Tests.Unit.Stubs.Models;
using System.Collections.Generic;
using Telemetry.RealTime.Worker.Buffers.ModelBuffer;
using System.Threading;
using System.Threading.Tasks;

namespace Telemetry.RealTime.Worker.Tests.Unit.Buffers
{
    [TestClass]
    public class ModelBufferTests
    {
        private List<StubModel> _models;
        private Random _random = new Random();

        [TestMethod]
        public void InsertWithoutSleepDoesNotTriggerSave()
        {
            _models = new List<StubModel>();
            var buffer = new ModelBuffer<StubModel>(x => SaveModels(x), y => string.Format("{0}p{1}i{2}", y.Period, y.PartitionId, y.Index), TimeSpan.FromMilliseconds(200));
            var partitionId = "0";

            SetModel(buffer, 0, partitionId, "2015050116", 1);
            Assert.AreEqual(0, _models.Count);
            
            Thread.Sleep(300);
            AssertSavedModels(1, partitionId, "2015050116", 1);
        }

        [TestMethod]
        public void InsertWithSleepTriggersSave()
        {
            _models = new List<StubModel>();
            var partitionId = "1";
            var count = _random.Next(100, 500);

            var buffer = new ModelBuffer<StubModel>(
                x => SaveModels(x), 
                y => string.Format("{0}p{1}i{2}", y.Period, y.PartitionId, y.Index), 
                TimeSpan.FromMilliseconds(200));            
            
            for (int i=0; i < count; i++)
            {
                SetModel(buffer, i, partitionId, "2015050116", 1);
            }

            Assert.AreEqual(0, _models.Count);

            Thread.Sleep(300);
            AssertSavedModels(count, partitionId, "2015050116", 1);
        }

        [TestMethod]
        public void InsertAndUpdate()
        {
            _models = new List<StubModel>();
            var buffer = new ModelBuffer<StubModel>(x => SaveModels(x), y => string.Format("{0}p{1}i{2}", y.Period, y.PartitionId, y.Index), TimeSpan.FromMilliseconds(200));
            var count = _random.Next(100, 500);

            var period = "2015050116";
            var partitionId = "2";
            var itemCount = 1;

            for (int i = 0; i < count; i++)
            {
                SetModel(buffer, i, partitionId, period, itemCount);
            }           
            for (int i = 0; i < count; i++)
            {
                SetModel(buffer, i, partitionId, period, itemCount);
            }
            Assert.AreEqual(0, _models.Count);

            Thread.Sleep(300);
            AssertSavedModels(count, partitionId, period, 2);
        }

        [TestMethod]
        public void InsertAndUpdateSinglePeriodMultithreaded()
        {
            _models = new List<StubModel>();
            var buffer = new ModelBuffer<StubModel>(x => SaveModels(x), y => string.Format("{0}p{1}i{2}", y.Period, y.PartitionId, y.Index), TimeSpan.FromMilliseconds(50));
            var count = _random.Next(100, 500);

            var period = "2015050116";
            var partitionId = "3";
            var itemCount = 1;

            var t1 = new Task(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    SetModel(buffer, i, partitionId, period, itemCount);
                }
            });

            var t2 = new Task(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    SetModel(buffer, i, partitionId, period, itemCount);
                }
            });

            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);
            Assert.AreEqual(0, _models.Count);

            Thread.Sleep(750);
            AssertSavedModels(count, partitionId, period, 2);
        }

        [TestMethod]
        public void InsertAndUpdateMultiplePeriodsMultithreaded()
        {
            _models = new List<StubModel>();
            var buffer = new ModelBuffer<StubModel>(x => SaveModels(x), y => string.Format("{0}p{1}i{2}", y.Period, y.PartitionId, y.Index), TimeSpan.FromMilliseconds(100));

            var count1 = _random.Next(100, 500);
            var period1 = "2015050116";

            var count2 = _random.Next(100, 500);
            var period2 = "2015050507";

            var count3 = _random.Next(100, 500);
            var period3 = "2015050508";
            var itemCount = 1;
            var partitionId = "4";

            var t1 = new Task(() =>
            {
                for (int i = 0; i < count1; i++)
                {
                    SetModel(buffer, i, partitionId, period1, itemCount);
                }
            });

            var t2 = new Task(() =>
            {
                for (int i = 0; i < count1; i++)
                {
                    SetModel(buffer, i, partitionId, period1, itemCount);
                }
            });

            var t3 = new Task(() =>
            {
                for (int i = 0; i < count2; i++)
                {
                    SetModel(buffer, i, partitionId, period2, itemCount);
                }
            });

            var t4 = new Task(() =>
            {
                for (int i = 0; i < count2; i++)
                {
                    SetModel(buffer, i, partitionId, period2, itemCount);
                }
            });

            var t5 = new Task(() =>
            {
                for (int i = 0; i < count3; i++)
                {
                    SetModel(buffer, i, partitionId, period3, itemCount);
                }
            });

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();
            Task.WaitAll(t1, t2, t3, t4, t5);

            Thread.Sleep(750);
            Assert.AreEqual(count1 + count2 + count3, _models.Count);
            AssertSavedModels(count1, partitionId, period1, 2, x => x.Period == period1);
            AssertSavedModels(count2, partitionId, period2, 2, x => x.Period == period2);
            AssertSavedModels(count3, partitionId, period3, 1, x => x.Period == period3);
        }

        private static void SetModel(ModelBuffer<StubModel> buffer, int index, string partitionId, string period, int itemCount)
        {
            var newModel = new StubModel
                {
                    Index= index,
                    PartitionId = partitionId,
                    Period = period,
                    Count= itemCount
                };
            var existingModel = buffer.Get(newModel);
            if (existingModel != null)
            {
                existingModel.Count += newModel.Count;
            }
            else
            {
                buffer.Add(newModel);
            }            
        }

        private void AssertSavedModels(int count, string partitionId, string period, int itemCount, Func<StubModel, bool> predicate = null)
        {
            IEnumerable<StubModel> models = _models;
            if (predicate != null)
            {
                models = _models.Where(predicate);
            }
            Assert.AreEqual(count, models.Count());
            for (int i = 0; i<count; i++)
            {
                var model = models.Single(x => x.Index == i);
                Assert.AreEqual(partitionId, model.PartitionId);
                Assert.AreEqual(period, model.Period);
                Assert.AreEqual(itemCount, model.Count);
            }
        }

        private void SaveModels(IEnumerable<StubModel> models)
        {
            _models.AddRange(models);
        }
    }
}
