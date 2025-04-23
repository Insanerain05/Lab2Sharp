using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab2Sharp
{
    class Program
    {
        private static readonly int dim = 10_000_000;
        private static readonly int threadNum = 12;

        private readonly Thread[] threads = new Thread[threadNum];
        private readonly int[] arr = new int[dim];

        private int minValue = int.MaxValue;
        private int minIndex = -1;

        private int threadCount = 0;
        private readonly object lockerForMin = new object();
        private readonly object lockerForCount = new object();

        static void Main(string[] args)
        {
            Program program = new Program();
            program.InitArray();
            program.FindMinParallel();

            Console.WriteLine($"Мiнiмальне значення: {program.minValue}; Iндекс: {program.minIndex}");
            Console.ReadKey();
        }

        private void InitArray()
        {
            Random rnd = new Random();
            for (int i = 0; i < dim; i++)
            {
                arr[i] = rnd.Next(0, 1000);
            }
            // Вставляємо від’ємне значення випадково
            int randomIndex = new Random().Next(0, dim);
            arr[randomIndex] = -rnd.Next(1, 100);
        }

        private void FindMinParallel()
        {
            int chunkSize = dim / threadNum;
            for (int i = 0; i < threadNum; i++)
            {
                int start = i * chunkSize;
                int end = (i == threadNum - 1) ? dim : start + chunkSize;

                threads[i] = new Thread(MinWorker);
                threads[i].Start(new Bound(start, end));
            }

            lock (lockerForCount)
            {
                while (threadCount < threadNum)
                {
                    Monitor.Wait(lockerForCount);
                }
            }
        }
        private void MinWorker(object obj)
        {
            if (obj is Bound bounds)
            {
                int localMin = int.MaxValue;
                int localIndex = -1;

                for (int i = bounds.StartIndex; i < bounds.FinishIndex; i++)
                {
                    if (arr[i] < localMin)
                    {
                        localMin = arr[i];
                        localIndex = i;
                    }
                }

                lock (lockerForMin)
                {
                    if (localMin < minValue)
                    {
                        minValue = localMin;
                        minIndex = localIndex;
                    }
                }

                lock (lockerForCount)
                {
                    threadCount++;
                    Monitor.Pulse(lockerForCount);
                }
            }
        }
        class Bound
        {
            public int StartIndex { get; }
            public int FinishIndex { get; }

            public Bound(int start, int end)
            {
                StartIndex = start;
                FinishIndex = end;
            }
        }
    }
}
