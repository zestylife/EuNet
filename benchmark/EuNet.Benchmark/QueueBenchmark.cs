using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using EuNet.Client;
using EuNet.Core;
using EuNet.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EuNet.Benchmark
{
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.NetCoreApp31, warmupCount: 3, iterationCount: 10)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.NetCoreApp50, warmupCount: 3, iterationCount: 10)]
    [MemoryDiagnoser]
    [RPlotExporter]
    public class QueueBenchmark
    {
        private List<DataClass> _list;

        private ConcurrentQueue<DataClass> _concurrentQueue;
        private ConcurrentCircularQueue<DataClass> _concurrentCircluarQueue;

        private int TestCount = 1000000;

        public QueueBenchmark()
        {
            _list = new List<DataClass>(TestCount);
            for (int i = 0; i < TestCount; ++i)
                _list.Add(new DataClass() { Value = i });

            _concurrentQueue = new ConcurrentQueue<DataClass>();
            _concurrentCircluarQueue = new ConcurrentCircularQueue<DataClass>(TestCount);
        }

        [Benchmark]
        public void ConcurrentQueue()
        {
            for (int i = 0; i < TestCount; ++i)
                _concurrentQueue.Enqueue(_list[i]);

            DataClass result;
            for (int i = 0; i < TestCount; ++i)
                _concurrentQueue.TryDequeue(out result);

            for (int i = 0; i < TestCount; ++i)
            {
                _concurrentQueue.Enqueue(_list[i]);
                _concurrentQueue.TryDequeue(out result);
            }
        }

        [Benchmark]
        public void ConcurrentCircularQueue()
        {
            for (int i = 0; i < TestCount; ++i)
                _concurrentCircluarQueue.Enqueue(_list[i]);

            for (int i = 0; i < TestCount; ++i)
                _concurrentCircluarQueue.Dequeue();

            for (int i = 0; i < TestCount; ++i)
            {
                _concurrentCircluarQueue.Enqueue(_list[i]);
                _concurrentCircluarQueue.Dequeue();
            }
        }
    }
}
