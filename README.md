# .NET 4.0: Managed Extensibiliy Framework (MEF)

## Introduction

Yesterday, on a sunny sunday morning I was catching up on some blog reading. One post in particular caught my eye, namely [A Whirlwind Tour Through The Managed Extensibility Framework](http://community.bartdesmet.net/blogs/bart/archive/2009/08/07/a-whirlwind-tour-through-the-managed-extensibility-framework.aspx) by [Bart De Smet](http://community.bartdesmet.net/blogs/bart/default.aspx).

The Managed Extensibility Framework, or MEF, is a new library that will eventually be part of the .NET 4.0 Framework. It enables you to easily create extensible applications that support a plugin model. A lot of modern, popular applications such as FireFox, Winamp, Visual Studio...etc. already provide a plugin / add-in model that allows you to add your own plugins if you want to extend the functionality of the application.

Making your application extensible in the past was certainly possible but you had to create the infrastructure from scratch. Well, no more! MEF allows you to expose certain parts of your application in which you can import built-in or external extensions.

Let's discover the wonderful world of MEF by building a demo application...

## MEF Preview

Although the .NET 4.0 Framework has not been released yet, you can download a preview of [MEF on Codeplex](http://mef.codeplex.com/). At the time of writing the latest release is preview 6. Go ahead and download it.

After extracting the contents of the MEF_Preview_6.zip file you'll get an entire solution. You only to need to add a reference to the assembly called System.ComponentModel.Composition.dll found in the bin folder to the projects which you want to MEF-enable.

For more information you can consult the [MEF Programming Guide](http://mef.codeplex.com/Wiki/View.aspx?title=Guide) on CodePlex.

## Hello, World!

Before entering the world of MEF let's first setup an example application which we can rewrite later on. Start up Visual Studio and create a new blank solution called "Mef" and add a new ConsoleApplication project aptly named ConsoleApplication.

I prefer Console applications for short demos as I don't have to bother with any GUI design. As for the code of this ingenious application please take a look at Listing 1. Prepare to be amazed at the complexity of the code...

**Listing 1** - Hello, World! Console Application.

```csharp
class Program
{
    static void Main()
    {
        ConsoleLogger logger = new ConsoleLogger();
        logger.Log("Hello, World!");
        Console.ReadLine();
    }
}

public interface ILogger
{
    void Log(string message);
}

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}
```

As you can see one interface, ILogger, is declared which contains a method Log(string message) which takes one string parameter called message. The ConsoleLogger class provides an implementation for this interface and writes the message parameter out to the console window.

Finally the Main(...) method of the console application instantiates a ConsoleLogger object and calls it's Log(...) method in order to print a message to the console window. It can't get much simpler than this...

**Remark**: Listing 1 shows all the code as if it were located in the same source file. If you prefer you can place it in seperate code files. I have done so for the source code accompanying this article.

## Extensibility

Suppose I don't want to limit my "logging application" to only be able to log messages to the Console window. I'm not going to look at the console window 24/7, so it might come in handy if the application was able to log it's messages to more than one target. Instead of printing messages to the console window it could e-mail them, write them to an event log, to a file...etc.

Let's create another implementation for our ILogger and rewrite the application to support multiple loggers.

**Listing 2** - Supporting Multiple Loggers

```csharp
class Program
{
    public static IEnumerable<ILogger> Loggers { get; set; }

    public static void Log(string message)
    {
        foreach(ILogger logger in Loggers)
        {
            logger.Log(message);
        }
    }

    static void Main()
    {
        Loggers = new List<ILogger> {new ConsoleLogger(), new FileLogger()};
        Log("Hello, World!");
        Console.ReadLine();
    }
}

public class FileLogger : ILogger
{
    public void Log(string message)
    {
        // Write the message to a file
    }
}
```

Voila, now our application supports multiple loggers. In the future you can add additional loggers by adding them the Loggers collection in the Main(...) method. But that means changing the source code of the application and recompiling it. Wouldn't it be neat if the application was smart enough to figure out which Logger implementations it would need to use? Enter MEF...

## Importing ILogger Implementations

In it's most simplest form MEF is as simple as 1-2-3. First you need to define the needed parts (Imports & Exports) and then you hand them over to MEF so that the framework can compose the parts.

To MEF-enable our logging application only three simple steps need to be followed. First we need to create, what I like to call, an extensibility point for our application.

From our application's point of view it needs to collect all the ILogger implementations it can find and add them to a collection. Let's rewrite our logging logic once again.

Add new class Logger to the console project and paste in the following code.

**Listing 3** - Logger class

```csharp
public class Logger
{
    public Logger()
    {
        Loggers = new List<ILogger>();
    }

    public IEnumerable<ILogger> Loggers { get; set; }

    public void Log(string message)
    {
        foreach (ILogger logger in Loggers)
        {
            logger.Log(message);
        }
    }
}
```

The static Main(...) method could then simply log it's message this way:

**Listing 4** - Instantiating A Logger

```csharp
...
Logger logger = new Logger();
logger.Log("Hello, World!");
...
```

But first the different ILogger implementations need to be added to the Loggers property of the Logger object. This collection property of type IEnumerable<ILogger> is going to act as our "extensibility point".

**Listing 5** - ImportMany Attribute

```csharp
[ImportMany(typeof(ILogger))]
public IEnumerable<ILogger> Loggers { get; set; }
```

By decorating the Loggers property with the ImportMany attribute we are basically notifying the MEF framework that this property accepts ILogger implementations or exports.

**Remark**: MEF also offers an Import attribute if you only want to import one instance instead of an entire collection. You can read more about declaring imports in the [MEF Programming Guide](http://mef.codeplex.com/Wiki/View.aspx?title=Declaring%20Imports&referringTitle=Home).

## Export ILogger Implementations

As it stands now the logging logic is being handled by the Logger class and all the console application does is create a Logger object and call it's Log(...) method. If you were to run the application now nothing would be logged.

We still need to tell MEF were it can find the ILogger implementations. We need to mark the ILogger implementations or export them in MEF parlance.

**Listing 6** - Export Attribute

```csharp
[Export(typeof(ILogger))]
public class ConsoleLogger : ILogger
{
    // ...
}

[Export(typeof(ILogger))]
public class FileLogger : ILogger
{
    // ...
}
```

By decorating the ConsoleLogger and FileLogger class types with the Export attribute you make them available to the MEF framework. By specifying the type of the ILogger interface the framework will be able to bind these types to our "extensibility point" we created earlier.

## Catalogs

The imports and exports are also called composable parts. Now that all the parts are in place to form our compisition the last thing we need to do is to tell MEF how it should bind the exports to the imports. This is done using catalogs.

Using a catalog you can list all of the ILogger implementations you want to provide to the Logger class. Add the following code to the Main(...) method of the console application:

**Listing 7** - TypeCatalog

```csharp
TypeCatalog catalog = new TypeCatalog(typeof(ConsoleLogger), typeof(FileLogger));
CompositionContainer container = new CompositionContainer(catalog);
container.ComposeParts(logger);
```

The above example creates a TypeCatalog and adds the ConsoleLogger and FileLogger types to it. Next a CompositionContainer is created passing in the catalog. The container then composes all of the parts using our Logger instance. The magic that happens in the container makes sure that our loggers get instantiated and added to the IEnumerable<ILogger> collection of the Logger object.

Starting the application now will log the "Hello, World!" message to the Console window and to file. However when you want to create a new logger type you still need to adjust the code and add the new type to the TypeCatalog. That's not what we want. Luckily MEF provides you with different types of catalogs. Consider the following examples:

Using the AssemblyCatalog type you can discover all of the exports in a given assembly. In this example the currently executing assembly is used but any other will do.

**Listing 8** - AssemblyCatalog

```csharp
AssemblyCatalog catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
```

**Listing 9** - DirectoryCatalog

```csharp
DirectoryCatalog catalog = new DirectoryCatalog(@"c:\temp\loggers");
```

Using a DirectoryCatalog you can scan all of the assemblies in a specified directory. This catalog type will certainly come in handy if you plan on building your own plugin system. Third party providers can then create their own plugins and place them in a certain directory so that the application can detect them.

To enable this for our logging application please follow these steps:

1. Move the ILogger interface to a new class library project called Interface. Add a reference to the Interface library from the console application project.
2. Add a new class library project called ThirdPartyLogger. Add references to the Interface library and the System.ComponentModel.Composition assembly.
3. Add a new logger type to the ThirdPartyLogger library. Make sure it implements the ILogger interface and is marked with the Export attribute.
4. Compile the ThirdPartyLogger project and place the resulting assembly in the directory specified by the DirectoryCatalog.

An example third party logger could be a logger that sends every message out via e-mail (not that I'm advocating e-mail as a logging tool):

**Listing 10** - Third Party Logger

```csharp
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
```

And luckily you can use multiple catalogs specifying difference sources by setting up an aggregate catalog.

**Listing 11** - AggregateCatalog

```csharp
AssemblyCatalog assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
DirectoryCatalog directoryCatalog = new DirectoryCatalog(@"c:\temp\loggers");

AggregateCatalog catalog =
    new AggregateCatalog(new ComposablePartCatalog[] { assemblyCatalog, directoryCatalog });
```

The code abode creates instances the AssemblyCatalog and DirectoryCatalog type and aggregates them in a AggregateCatalog instance. This instance can then be used by a CompositionContainer.

## Summary

I hope this article sparked your interest in the Managed Extensibility Framework or MEF, which will be included in the upcoming .NET Framework 4.0. By simply following three steps, namely: Import, Export and Compose you can easily create an extensible application and offer alot of additional functionality to your users even if the application has already been deployed.

By building in the necessary extensibility points you can prepare for the unknown and allow your application to be extended without recompiling it.
