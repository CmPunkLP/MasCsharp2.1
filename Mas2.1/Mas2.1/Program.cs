using System;
using System.Linq;
using System.Threading;

class Program
{
    private static int[] array;
    private static int currentLength;
    private static int numberOfThreads;
    private static AutoResetEvent resetEvent;
    private static int completedPairs;
    private static object lockObject = new object();

    static void Main(string[] args)
    {
        Random rand = new Random();
        currentLength = 500000; 
        array = Enumerable.Range(1, currentLength).Select(x => rand.Next(1, 10)).ToArray();

        Console.WriteLine("Початковий масив: " + string.Join(", ", array));

        numberOfThreads = Environment.ProcessorCount;
        resetEvent = new AutoResetEvent(false);
        completedPairs = 0;

        while (currentLength > 1)
        {
            completedPairs = 0;
            int pairsToProcess = currentLength / 2;
            int pairsPerThread = pairsToProcess / numberOfThreads;

            for (int i = 0; i < numberOfThreads; i++)
            {
                int startPair = i * pairsPerThread;
                int endPair = (i == numberOfThreads - 1) ? pairsToProcess : startPair + pairsPerThread;
                ThreadPool.QueueUserWorkItem(Worker, (startPair, endPair));
            }

            resetEvent.WaitOne();
            currentLength = pairsToProcess; 
            Console.WriteLine("Проміжний масив: " + string.Join(", ", array.Take(currentLength)));
        }

        Console.WriteLine("Фінальна сума: " + array[0]);
    }

    private static void Worker(object state)
    {
        var (startPair, endPair) = ((int, int))state;
        for (int i = startPair; i < endPair; i++)
        {
            int index1 = i;
            int index2 = currentLength - 1 - i;

            int sum = array[index1] + array[index2];
            array[index1] = sum; 

            lock (lockObject)
            {
                completedPairs++;
                if (completedPairs == (currentLength / 2))
                {
                    resetEvent.Set(); 
                }
            }
        }
    }
}
