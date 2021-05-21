using System;
using System.Threading;
using System.IO;

namespace _7_laba
{
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
        public int wait;
        public int work;
    }
    class Server
    {
        public int requestCount;
        public int processedCount;
        public int rejectedCount;
        public int poolcount;

        public PoolRecord[] pool;
        object threadLock;

        public Server(int count, PoolRecord[] pool)
        {
            requestCount = 0;
            processedCount = 0;
            requestCount = 0;
            this.pool = pool;
            this.poolcount = count;
            threadLock = new object();
            for (int i = 0; i < poolcount; i++)
                pool[i].in_use = false;
        }
        void Answer(object e)
        {
            Console.WriteLine("Выполняется заявка с номером {0}", e);
            int ojidanie = 20;
            for( int i = 0; i < ojidanie;i++)
                Console.WriteLine($"Осталось : {ojidanie - i} \n");
            Thread.Sleep(ojidanie);
            Console.WriteLine("Заявка с номером {0} выполнена", e);
            for (int i = 0; i < poolcount; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                    pool[i].thread = null;
                    break;
                }
            }
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером {0}", e.id);
                requestCount++;
                for (int i = 0; i < poolcount; i++)
                {
                    if (!pool[i].in_use)
                        pool[i].wait++;
                }
                for (int i = 0; i < poolcount; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].work++;
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e.id);
                        processedCount++;
                        return;
                    }
                }
                rejectedCount++;
            }
        }
    }
    class Client
    {
        public event EventHandler<procEventArgs> request;
        Server server;

        int ID = 0;

        public Client(Server server)
        {
            this.server = server;
            this.request += server.proc;
        }
        protected virtual void OnProc(procEventArgs e)
        {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
                handler(this, e);
        }
        public void Work()
        {
            procEventArgs e = new procEventArgs();
            ID++;
            e.id = ID;
            this.OnProc(e);
        }
    }
    class Program
    {
        static long Faktorial(long n)
        {
            if (n == 0)
                return 1;
            else
                return n * Faktorial(n - 1);
        }

        static int ThreadCount = 15;
        static int RequestCount = 75;
        static PoolRecord[] pool = new PoolRecord[ThreadCount];

        static void Main(string[] args)
        {
            Server server = new Server(ThreadCount, pool);
            Client client = new Client(server);
            for (int i = 0; i < RequestCount; i++)
                client.Work();
            Thread.Sleep(1500);
            Console.WriteLine("######################################################################\n");
            Console.WriteLine($"All count request : {server.requestCount}");
            Console.WriteLine($"Completed request: {server.processedCount}");
            Console.WriteLine($"Disaccepted request: {server.rejectedCount}");
            for (int i = 0; i < ThreadCount; ++i)
            {
                Console.WriteLine($"Thread {i + 1} : \n");
                Console.WriteLine($"Completed request : {server.pool[i].work} \n");
                Console.WriteLine($"Waiting time {server.pool[i].wait} \n");
                Console.WriteLine($"-----------------------------------------------\n");
            }
            double p = server.requestCount / server.poolcount;
            double p0 = 0;
            for (int i = 0; i < server.poolcount; i++)
            {
                p0 += Math.Pow(p, i) / Faktorial(i);
                if (i == server.poolcount - 1)
                    p0 = Math.Pow(p0, -1);
            }
            double pn = Math.Pow(p, server.poolcount) * p0 / Faktorial(server.poolcount);
            double Q = 1 - pn;
            double A = server.requestCount * (1 - pn);
            double k = A / server.poolcount;

            Console.WriteLine($"Приведенная интенсивность потока заявок: {p}\n");
            Console.WriteLine($"Вероятность простоя системы: {p0}\n");
            Console.WriteLine($"Вероятность отказа системы: {pn}\n");
            Console.WriteLine($"Относительная пропускная способность: {Q}\n");
            Console.WriteLine($"Абсолютная пропускная способность: {A}\n");
            Console.WriteLine($"Среднее число занятых каналов: {k}\n");
            Console.ReadKey();






        }
    }

}
