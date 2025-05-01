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
        private static readonly int ArraySize = 100_000_000;
        private static readonly int ThreadCount = 6;

        private readonly int[] numbers = new int[ArraySize];
        private readonly Thread[] workers = new Thread[ThreadCount];

        private int minValue = int.MaxValue;
        private int minIndex = -1;
        private int finishedThreadCount = 0;

        private readonly object syncLock = new object();

        static void Main(string[] args)
        {
            Program program = new Program();
            program.FillArray();
            program.FindMinParallel();

            Console.WriteLine($"Мiнiмальне значення: {program.minValue}");
            Console.WriteLine($"Iндекс мiнiмального значення: {program.minIndex}");
            Console.ReadKey();
        }

        private void FillArray()
        {
            for (int i = 0; i < ArraySize; i++)
            {
                numbers[i] = i;
            }

            Random random = new Random();
            int randomPosition = random.Next(0, ArraySize);
            numbers[randomPosition] = -9999;

            Console.WriteLine($"{randomPosition} --- value: {numbers[randomPosition]}");
        }

        private void FindMinParallel()
        {
            int chunkSize = ArraySize / ThreadCount;

            for (int i = 0; i < ThreadCount; i++)
            {
                int start = i * chunkSize;
                int end = (i == ThreadCount - 1) ? ArraySize : start + chunkSize;

                workers[i] = new Thread(() => FindLocalMin(start, end));
                workers[i].Start();
            }

            lock (syncLock)
            {
                while (finishedThreadCount < ThreadCount)
                {
                    Monitor.Wait(syncLock);
                }
            }
        }

        private void FindLocalMin(int start, int end)
        {
            int localMin = int.MaxValue;
            int localMinPos = -1;

            for (int i = start; i < end; i++)
            {
                if (numbers[i] < localMin)
                {
                    localMin = numbers[i];
                    localMinPos = i;
                }
            }

            lock (syncLock)
            {
                if (localMin < minValue)
                {
                    minValue = localMin;
                    minIndex = localMinPos;
                }

                finishedThreadCount++;
                Monitor.Pulse(syncLock);
            }
        }
    }
}
