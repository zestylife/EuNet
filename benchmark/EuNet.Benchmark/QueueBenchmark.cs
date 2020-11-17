using BenchmarkDotNet.Attributes;
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
    [SimpleJob(launchCount: 1, warmupCount: 5, targetCount: 10)]
    [MemoryDiagnoser]
    [RPlotExporter]
    public class QueueBenchmark
    {
        private List<DataClass> _list;

        private ConcurrentQueue<DataClass> _concurrentQueue;
        private ConcurrentCircularQueue<DataClass> _concurrentCircluarQueue;

        private int TestCount = 10000000;

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
        }

        [Benchmark]
        public void ConcurrentCircularQueue()
        {
            for (int i = 0; i < TestCount; ++i)
                _concurrentCircluarQueue.Enqueue(_list[i]);

            for (int i = 0; i < TestCount; ++i)
                _concurrentCircluarQueue.Dequeue();
        }
    }
}
