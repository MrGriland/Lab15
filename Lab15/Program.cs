using System;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.IO;

namespace Lab15
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Задание 1");
            foreach (Process proc in Process.GetProcesses())
            {
                Console.WriteLine($"Запущенный процесс имеет {proc.Id} - ID, " +
                    $"{proc.ProcessName} - Имя, {proc.BasePriority} - Приоритет, " +
                    $"{proc.Responding} - текущее состояние"
                );
            }

            
            Console.WriteLine($"Задание 2");
            AppDomain domain = AppDomain.CurrentDomain;
            Console.WriteLine($"Имя: {domain.FriendlyName}, детали конфигурации: {domain.SetupInformation}");
            Console.WriteLine($"Все сборки:");
            foreach (Assembly el in domain.GetAssemblies())
            {
                Console.WriteLine($"Имя сборки: {el.GetName().Name}");
            }
            //создание и настройка домена ( не поддерживается на ОС Windows)
            //Assembly[] assembly = domain.GetAssemblies();
            //AppDomain newDomain = AppDomain.CreateDomain("NewDomain");//создаем новый домен
            //newDomain.Load(assembly[1].GetName().Name);//получаем имя сборки
            //AppDomain.Unload(newDomain);


            // Задание 3
            Thread thread = new Thread(new ParameterizedThreadStart(PrintSimple));
            thread.Priority = ThreadPriority.Lowest;//Свойство Priority хранит приоритет потока - значение перечисления ThreadPriority
            thread.Start(20);//метод старт запускает поток
            thread.Join();


            // Задание 4
            EvenCl ev = new EvenCl(20);
            Thread.Sleep(100);
            OddCl od = new OddCl(20);

            ev.Thread.Join();// join блокирует выполнение вызвавшего его потока до тех пор, пока не завершится поток, для которого был вызван данный метод 
            od.Thread.Join();

            NewEven ne = new NewEven(20);
            Thread.Sleep(10);//Sleep останавливает поток на определенное количество миллисекунд
            NewOdd no = new NewOdd(20);
            ne.thr.Join();
            od.Thread.Join();


            // Задание 5
            int num = 0;
            // устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(Count);
            // создаем таймер
            Timer timer = new Timer(tm, num, 0, 2000);
            Console.ReadLine();
        }
        public static void PrintSimple(object N)
        {
            Thread t = Thread.CurrentThread;
            t.Name = "prost";
            using (StreamWriter file = new StreamWriter("Simple.txt", false))
            {
                for (var p = 0u; p < (int)N; p++)
                {
                    if (p == 4)
                    {
                        //t.Suspend();
                        Console.WriteLine($"Поток приостановлен на 3 секунды");
                        Thread.Sleep(3000);
                        //t.Resume();
                    }
                    if (IsPrimeNumber(p))
                    {
                        file.WriteLine($"{p} - простое число {t.IsAlive} - жив? {t.Priority} - приоритет {t.Name} - имя {t.ManagedThreadId} - id");
                        Console.WriteLine($"{p} - простое число {t.IsAlive} - жив? {t.Priority} - приоритет {t.Name} - имя {t.ManagedThreadId} - id");
                        Thread.Sleep(300);
                    }
                }
            }
        }

        public static bool IsPrimeNumber(uint n)
        {
            var result = true;

            if (n > 1)
            {
                for (var i = 2u; i < n; i++)
                {
                    if (n % i == 0)
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        class OddCl
        {
            int N;
            public Thread Thread;
            public OddCl(int _N)
            {
                Thread = new Thread(this.OddFirst);
                N = _N;
                Thread.Start();
            }

            void OddFirst()
            {
                Mutexx.mtx.WaitOne();//вход в критическую секцию, 


                // выводились сначала четные, потом нечетные числа 
                using (StreamWriter file = new StreamWriter("Numbers.txt", false))
                {
                    for (int i = 0; i < (int)N; i++)
                    {
                        if (i % 2 != 0)
                        {
                            //Thread.Sleep(100);
                            Console.WriteLine($"{i} - нечет");
                            file.WriteLine(i + "нечет");
                        }
                    }
                }
                Mutexx.mtx.ReleaseMutex();//(выход из крит секции)
            }


        }

        class EvenCl
        {
            int N;
            public Thread Thread;
            public EvenCl(int _N)
            {
                Thread = new Thread(this.EvenLast);
                N = _N;
                Thread.Start();
            }
            void EvenLast()
            {
                Mutexx.mtx.WaitOne();
                using (StreamWriter file = new StreamWriter("Numbers.txt", true))
                {
                    for (int i = 0; i < (int)N; i++)
                    {
                        if (i % 2 == 0)
                        {

                            Console.WriteLine($"{i} - чет");
                            file.WriteLine(i + "чет");
                        }
                    }
                }
                Mutexx.mtx.ReleaseMutex();
            }
        }

        static class Mutexx
        {
            public static Mutex mtx = new Mutex();
        }

        class NewOdd
        {
            int N;
            public Thread thr;

            public NewOdd(int _N)
            {
                thr = new Thread(this.Run);
                N = _N;
                thr.Start();
            }

            // последовательно выводились одно четное, другое нечетное. 

            void Run()
            {
                int a = 1;
                while (a < N)
                {
                    Mutexx.mtx.WaitOne();//вход
                    using (StreamWriter file = new StreamWriter("EvenOdd.txt", true))
                    {
                        if (a % 2 != 0)
                        {
                            Console.WriteLine(a +" - нечет");
                            file.WriteLine(a + " - нечет");
                        }
                    }
                    Mutexx.mtx.ReleaseMutex();//освоб объект
                    Thread.Sleep(200);
                    a++;
                }
            }
        }

        class NewEven
        {
            int N;
            public Thread thr;

            public NewEven(int _N)
            {
                thr = new Thread(this.Run);
                N = _N;
                thr.Start();
            }

            void Run()
            {
                int a = 1;
                while (a < N)
                {
                    Mutexx.mtx.WaitOne();
                    using (StreamWriter file = new StreamWriter("EvenOdd.txt", true))
                    {
                        if (a % 2 == 0)
                        {
                            Console.WriteLine(a + " - чет");
                            file.WriteLine(a + " - чет");
                        }
                    }
                    Mutexx.mtx.ReleaseMutex();
                    Thread.Sleep(200);
                    a++;
                }
            }
        }
        public static void Count(object obj)
        {
            int x = (int)obj;
            for (int i = 1; i < 9; i++, x++)
            {
                Console.WriteLine($"{x * i}");
            }
        }
    }
}
