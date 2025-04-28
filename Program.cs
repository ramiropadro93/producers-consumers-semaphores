class Program
{
    private static readonly Queue<string> Buffer = new();
    private static readonly SemaphoreSlim emptySemaphore = new(10);
    private static readonly SemaphoreSlim fullSemaphore = new(0);
    private static readonly object bufferLock = new();

    static async Task Main(string[] args)
    {
        var producerCount = int.Parse(Environment.GetEnvironmentVariable("PRODUCER_COUNT") ?? "2");
        var consumerCount = int.Parse(Environment.GetEnvironmentVariable("CONSUMER_COUNT") ?? "2");

        var tasks = new Task[producerCount + consumerCount];

        for (int i = 0; i < producerCount; i++)
        {
            var producerNumber = i + 1;
            tasks[i] = Task.Run(() => Producer(producerNumber));
        }

        for (int i = 0; i < consumerCount; i++)
        {
            var consumerNumber = i + 1;
            tasks[producerCount + i] = Task.Run(() => Consumer(consumerNumber));
        }

        await Task.WhenAll(tasks);
    }

    static async Task Producer(int producerNumber)
    {
        var cantidadMensajes = int.Parse(Environment.GetEnvironmentVariable("MESSAGE_COUNT") ?? "10");
        for (int i = 0; i < cantidadMensajes; i++)
        {
            await emptySemaphore.WaitAsync();

            string message = $"[M] Mensaje {i + 1} de Productor - {producerNumber}";

            lock (bufferLock)
            {
                Buffer.Enqueue(message);
                Console.WriteLine($"[x] Productor {producerNumber} - Envía {message}");
            }

            fullSemaphore.Release();
            Thread.Sleep(1000);
        }
    }

    static async Task Consumer(int consumerNumber)
    {
        while (true)
        {
            await fullSemaphore.WaitAsync();

            string message;
            lock (bufferLock)
            {
                message = Buffer.Dequeue();
            }

            Console.WriteLine($"[x] Consumidor [{consumerNumber}] - Recibe {message}");
            emptySemaphore.Release();

            Thread.Sleep(100);
        }
    }
}
