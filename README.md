# What is Mailr.NET?
Mailr.NET offers a very simple .NET API and a command-line tool for building, sending, and testing email messages generated from localized template files or text strings.

## Why use Mailr.NET?
While you can find many alternative solutions that will allow you to generate email messages from localized templates, you may want to consider Mailr.NET for a number of reasons:

- The Mailr.NET API is simple and familiar. It is based on and supports all features of the .NET Framework's [MailMessage](https://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage(v=vs.110).aspx) class. The API just adds a couple of methods for loading and transforming templates into the mail message body.

- The templating syntax utilized by Mailr.NET relies on the popular and capable [Razor Engine](https://github.com/Antaris/RazorEngine/wiki/1.-About-Razor-and-its-syntax).

- Mailr.NET also offers a client tool that you can use to test your email template. This command-line tool will transform an email template using the data provided in a text file and send the resulting message to the specified recipient(s). You can use the client tool for testing email templates even if you do not use the Mailr.NET class library.

- The Mailr.NET components, projects, and workflows are thoroughly documented.

## What's inside of this solution?
The solution includes three projects:

- MailrLib: implements the Mailr.NET class library (API)
- MailrExe: implements a command-line tool for testing email templates
- MailrDoc: implements a [Sandcastle](https://github.com/EWSoftware/SHFB) documentation project that builds the user's guide

## How to build this solution?
To build this solution you need:

- Visual Studio 2013 (any edition)
- [Sandcastle Help File Builder](http://ewsoftware.github.io/SHFB/html/8c0c97d0-c968-4c15-9fe9-e8f3a443c50a.htm)

When installing Sandcastle Help File Builder, select the appropriate Visual Studio integration options. The documentation project in the solution is built using Sandcastle v2015.1.12.0; if you run into any problem when attempting to build the project using a later version of Sandcastle, please [submit an issue](../../issues).

## Can I just download the binaries?
You can download the command-line tool, the help file&#42;, and the library assembly from:

- [Mailr.NET Latest Release Downloads](../../releases)

Be aware that the class library (Mailr.dll) has run-time dependencies on external DLLs described [below](#what-are-the-run-time-dependencies).

## What are the run-time dependencies?
The Mailr.NET class library depends on the components installed by the following NuGet packages (if you use the [Mailr.NET Class Library](http://www.nuget.org/packages/mailr/) package, the following dependencies will be added to the project automatically):

- [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack/)
- [Microsoft ASP.NET Razor](https://www.nuget.org/packages/Microsoft.AspNet.Razor)
- [RazorEngine](https://www.nuget.org/packages/RazorEngine/)

Additional dependencies for building the Mailr.NET projects are described in Mailr.NET User's Guide.&#42;

## Which version of the .NET Framework is required?
Mailr.NET requires .NET Framework 4.5 or later.

## How to use the Mailr.NET API?
To use the Mailr.NET class library (`Mailr.dll`) in your application, add the following package to the project:

- [Mailr.NET Class Library NuGet Package](http://www.nuget.org/packages/mailr/)

Here is the abbreviated C# code to load a localized template from a file, transform it, and send the generated email message to the specified recipient (see [template examples](../../tree/master/MailrExe/Email/Templates)):

```c#
// Reference Mailr namespace.
using Mailr;
...

// MailTemplate is derived from System.Net.Mail.MailMessage.
// It's the only additional class that your app will need.
// Use the same constructor to create a mail template object
// as you would if you were to create a MailMessage object.
MailTemplate msg = new MailTemplate
(
    "mary@contoso.com", // sender
    "jose@contoso.com"  // recipient
);
  
// Option 1: 
// Load the template from a file with specific path.
msg.Load("..\\Templates\\Welcome_es-mx.xhtml");

// Option 2:
// Generate template file path on the fly using
// folder, file naming format, and file name parts
// (name of the template, culture or language, extension).
// If a file for the specified culture does not exist,
// Mailr.NET will try loading a fallback version,
// e.g. try cultures in the following order:
// - Start with the specified culture 'es-mx'.
// - If 'Welcome_es-mx.xhtml' is not found, try 'es'.
// - If 'Welcome_es.xhtml' is not found, try default culture 'en-us'.
// - If 'Welcome_en-us.xhtml' is not found, try without culture.
// - 'Welcome.xhtml' would be the final file to be tried. 
msg.Load("..\\Templates", "Welcome", "es-mx", ".xhtml");

// Transform template passing data substitutions.
// For data substitutions, you can use anonymous
// objects (shown here), strongly typed object,
// string dictionary, and other data types.
msg.Transform(new { Name = "Jose", });

// Now use standard .NET Framework's classes to send message.
SmtpClient smtp = new SmtpClient(server);
smtp.Send(msg);
```
For more detailed examples, see Mailr.NET User's Guide.&#42;

## How to use the Mailr.NET command-line tool to test email templates?
The Mailr.NET command-line tool (`MailrCmd.exe`) is a stand-alone console application (all dependencies are embedded in the primary assembly). Just run it from a command-line prompt with the `'/?'` switch to see usage information. Here are some examples:

```
MailrCmd /s:smtp.contoso.com /from:sender@contoso.com /to:recipient@contoso.com /file:Hello-es.htm /data:Hello-es.txt
```

```
MailrCmd /s:smtp.contoso.com /from:sender@contoso.com /to:recipient@contoso.com /dir:. /name:Hello /culture:es /ext:.htm /data:Hello.txt
```

For more information check Mailr.NET User's Guide.&#42;

## Limitations

The Mailr.NET command-line tool does not support email attachments.

## See also
[XslMail](https://github.com/alekdavis/XslMail)

---

&#42; Download the latest Mailr.NET User's Guide (CHM file) from the [Downloads page](../../releases). You may need to [unblock the CHM file](http://www.jeff.wilcox.name/2008/11/unblock-chms/) after downloading it (if you don't unblock the file, you may not be able to see the contents).
