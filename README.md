# What is Mailr.NET?
Mailr.NET offers a very simple .NET API and a command-line tool for building, sending, and testing email messages generated from localized template files or text strings.

## Why use Mailr.NET?
While you may find many alternative solutions that will allow you to generate emial messages from localized templates, you may want to consider Mailr.NET for a number of reasons:

- The Mailr.NET API it is simple and familiar. It is based on and supports all features of the .NET Framework's [MailMessage](https://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage(v=vs.110).aspx) class. The API just adds a couple of methods for loading and transforming templates into the mail message body.

- The templating syntax utilized by Mailr.NET relies on the popular and capable [Razor Engine](https://github.com/Antaris/RazorEngine/wiki/1.-About-Razor-and-its-syntax).

- Mailr.NET also offers a client tool that you can use to test your email template. This command-line tool will transform an email template using the data provided in a text file and send the resulting message to the specified recipient(s). You can use the client tool for testing email templates even if you do not use the Mailr.NET class library.

- The Mailr.NET components, projects, and workflows are thoroughly documented.

## What's inside of this solution?
The solution includes three projects:

- MailrLib: implements the Mailr.NET class library (API)
- MailrExe: implements a command-line tool for testing email templates
- MailrDoc: implements a [Sandcastle](https://github.com/EWSoftware/SHFB) documentation project that builds the user's guide

## Can I just download the binaries?
Of course, you can. Download individual components:

- [Mailr.NET Class Library](../master/MailrLib/bin/Release/Mailr.dll?raw=true)
- [Mailr.NET Client Tool](../master/MailrExe/bin/Release/MailrCmd.exe?raw=true)
- [Mailr.NET User's Guide](../master/MailrDoc/Help/Mailr.chm?raw=true) (don't forget to [unblock the CHM file](http://www.jeff.wilcox.name/2008/11/unblock-chms/) after downloading)

## How to use the Mailr.NET API?
To use Mailr.NET class library (`Mailr.dll`) in your application, add the following package to the project:

- [Mailr.NET Class Library NuGet Package](http://www.nuget.org/packages/mailr/)

Here is the abbreviated C# code to load a localized template from a file, transform it, and send the generated email message to he specified recipient:

```c#
// Reference Mailr namespace.
using Mailr;
...

// MailTemplate is derived from System.Net.Mail.MailMessage.
// It's the only class additional class that your app will need.
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
For more detailed examples, see [Mailr.NET User's Guide](../master/MailrDoc/Help/Mailr.chm?raw=true).

## How to use the Mailr.NET client tool to test email templates?
The Mailr.NET client tool (`MailrCmd.exe`) is a stand-alone command-line program (all dependencies are embedded in the primary assembly). Just run it from a command-line prompt with the `'/?'` switch to see usage information (for more information check the [Mailr.NET User's Guide](../master/MailrDoc/Help/Mailr.chm?raw=true)). 

## Which version of the .NET Framework is required?
Mailr.NET requires .NET Framework 4.5 or later.

## What are the run-time dependencies?
The Mailr.NET class library depends on the components installed by the following NuGet packages (if you use the [Mailr.NET Class Library](http://www.nuget.org/packages/mailr/) package, the following dependencies will be added to the project automatically):

- [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack/)
- [Microsoft ASP.NET Razor](https://www.nuget.org/packages/Microsoft.AspNet.Razor)
- [RazorEngine](https://www.nuget.org/packages/RazorEngine/)

Additional dependencies for building the Mailr.NET projects are described in the [Mailr.NET User's Guide](../master/MailrDoc/Help/Mailr.chm?raw=true).
