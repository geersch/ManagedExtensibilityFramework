using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.ComponentModel.Composition.Primitives;

namespace ConsoleApplication
{
    class Program
    {
        public static IEnumerable<ILogger> Loggers { get; set; }

        public static void Log(string message)
        {
            foreach (ILogger logger in Loggers)
            {
                logger.Log(message);
            }
        }

        static void Main()
        {
            //Loggers = new List<ILogger> { new ConsoleLogger(), new FileLogger() };
            //Log("Hello, World!");

            Logger logger = new Logger();

            AssemblyCatalog assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            //DirectoryCatalog directoryCatalog = new DirectoryCatalog(@"c:\temp\loggers");

            AggregateCatalog catalog =
                new AggregateCatalog(new ComposablePartCatalog[] { assemblyCatalog }); //{, directoryCatalog });

            //TypeCatalog catalog = new TypeCatalog(typeof(ConsoleLogger), typeof(FileLogger));
            CompositionContainer container = new CompositionContainer(catalog);
            container.ComposeParts(logger);

            logger.Log("Hello, World!");

            Console.ReadLine();
        }

        //static void Main(string[] args)
        //{
        //    ConsoleLogger logger = new ConsoleLogger();
        //    logger.Log("Hello, World!");
        //    Console.ReadLine();
        //}
    }

    public interface ILogger
    {
        void Log(string message);
    }

    [Export(typeof(ILogger))]
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }

    [Export(typeof(ILogger))]
    public class FileLogger : ILogger
    {
        public void Log(string message)
        {
            // Write the message to a file
        }
    }

    [Export(typeof(ILogger))]
    public class EmailLogger : ILogger
    {
        #region ILogger Members

        public void Log(string message)
        {
            // Log the message to e-mail
            // ...
        }

        #endregion
    }

    public class Logger
    {
        public Logger()
        {
            Loggers = new List<ILogger>();
        }

        [ImportMany(typeof(ILogger))]
        public IEnumerable<ILogger> Loggers { get; set; }

        public void Log(string message)
        {
            foreach (ILogger logger in Loggers)
            {
                logger.Log(message);
            }
        }
    }
}
