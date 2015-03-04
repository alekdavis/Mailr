using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Mail;

using Plossum.CommandLine;

using Mailr;

namespace Mailr.Client
{
/// <summary>
/// Implements <token>PRODUCTNAMEEXE</token>,
/// a command-line tool allowing you to test
/// the <see cref="N:Mailr"/> library.
/// </summary>
/// <remarks>
/// <para>
/// <token>PRODUCTNAMEEXE</token> sends email message generated from
/// a localized email template file after applying transformations
/// defined in a data file (the data file is a text file holding
/// the name=value pairs that will be used for data transformations).
/// </para>
/// <para>
/// <token>PRODUCTNAMEEXE</token> is implemented as a single
/// stand-alone executable with all dependencies,
/// including <token>PRODUCTNAMELIB</token>, merged using
/// <a href="https://www.nuget.org/packages/ilmerge">ILMerge</a>.
/// </para>
/// <para>
/// For additional information and usage examples,
/// see the <see cref="Main"/> method description.
/// </para>
/// </remarks>
internal class Program
{
	#region Main method
	/// <summary>
	/// Main routine invoked by <token>PRODUCTNAMEEXE</token>.
	/// </summary>
	/// <param name="args">
	/// Command-line arguments that define the program options.
	/// The <see cref="Mailr.Client.Options"/> class documentation
	/// describes the supported options and their mapping to
	/// command-line parameters.
	/// </param>
	/// <remarks>
	/// To see the basic usage information,
	/// run the program with out command-line arguments.
	/// </remarks>
	/// <example>
	/// <note>
	/// In the following examples, command-line options are split across
	/// multiple lines for better readability.
	/// When running the program, make sure that all command-line options
	/// are passed on the same line.
	/// </note>
	/// <para>
	/// Display program usage information:
	/// </para>
	/// <code language="none">
	/// <token>FILENAMEEXE</token> /help
	/// </code>
	/// <para>
	/// Send message from a template file without transforming it.
	/// Use the sender's email address for the recipient. 
	/// The email template file comes from the <c>Templates</c> 
	/// sub-folder under the working directory:
	/// </para>
	/// <code language="none">
	/// <token>FILENAMEEXE</token> /s:smtp.contoso.com 
	/// /from:sender@contoso.com 
	/// /file:Templates\Hello_en-us.html
	/// </code>
	/// <para>
	/// Same as above, only send the message to a different recipient:
	/// </para>
	/// <code language="none">
	/// <token>FILENAMEEXE</token> /s:smtp.contoso.com 
	/// /from:sender@contoso.com 
	/// /to:recipient@contoso.com
	/// /file:Templates\Hello_en-us.html
	/// </code>
	/// </example>
	internal static void Main(string[] args)
	{
		// Handles command-line options. 
		Options options;

		// Initialize program settings from command-line options.
		if (!Initialize(args, out options))
			return;

		// This object will hold data used for template transformations.
		Dictionary<string, string> data = null;
		
		try
		{
			// If data file holding name=pair strings is specified,
			// read its contents into the data object.
			if (!String.IsNullOrEmpty(options.DataFilePath))
				data = GetData(options.DataFilePath);
		}
		catch(Exception ex)
		{
			ShowError("Cannot get data transformation values from '{0}'.", 
				options.DataFilePath);
			ShowError(ex);

			return;
		}

		// In this method we only use standard .NET Framework functionality.
		MailMessage msg;
		
		try
		{
			// Read email message template from a file and
			// perform data transformations.
			msg = ProcessTemplate(options, data);
		}
		catch(Exception ex)
		{
			ShowError("Cannot process template.", 
				options.DataFilePath);
			ShowError(ex);

			return;
		}

		try
		{
			// Send email message using standard .NET protocol.
			SendMessage(options, msg);
		}
		catch(Exception ex)
		{
			ShowError("Cannot send message.", 
				options.DataFilePath);
			ShowError(ex);
		}
	}
	#endregion

	#region Initialize method
	/// <summary>
	/// Initializes the program settings using command-line options.
	/// </summary>
	/// <param name="args">
	/// Command-line arguments.
	/// </param>
	/// <param name="options">
	/// Program options.
	/// </param>
	/// <returns>
	/// <c>true</c> if the program needs to continue;
	/// otherwise, <c>false</c>.
	/// </returns>
	private static bool Initialize
	(
		string[]	args,
		out Options options
	)
	{
		// Holds command-line options (local class derived from Plossum CommandLine).
		options	= new Options();

		// Handles command-line parsing (implemented by Plossum CommandLine).
		CommandLineParser parser = new CommandLineParser(options);

		// Parse command line.
		parser.Parse();

		// If no arguments specified or help switch is provided.
		if (options.Help || args.Length == 0)
		{
			// Display help usage info.
			Console.WriteLine(parser.UsageInfo.ToString(Console.WindowWidth, false));

			// Tell caller to exist the program.
			return false;
		}
		// If there are problems with provided switches.
		else if (parser.HasErrors)
		{
			// Display error info.
			Console.WriteLine(parser.UsageInfo.GetErrorsAsString(Console.WindowWidth));
			return false;
		}

		// If only the From address is specified, use it as the To address.
		if (String.IsNullOrEmpty(options.MailToAddress))
			options.MailToAddress = options.MailFromAddress;

		// Check required parameters.
		if (String.IsNullOrEmpty(options.TemplateFilePath) &&
			String.IsNullOrEmpty(options.TemplateName))
		{
			// At the very least, either full path or name of the
			// template file must be specified.
			string errMsgFormat = "   * Missing required option \"{0}\".";
			
			Console.WriteLine("Errors:");

			if (String.IsNullOrEmpty(options.TemplateFilePath))
				Console.WriteLine(String.Format(errMsgFormat, "file"));
			else if (String.IsNullOrEmpty(options.TemplateName))
				Console.WriteLine(String.Format(errMsgFormat, "name"));

			return false;
		}

		// Continue with the program.
		return true;
	}
	#endregion

	#region GetData method
	/// <summary>
	/// Reads <c>name=value</c> pairs from a text file into a dictionary object..
	/// </summary>
	/// <param name="dataFilePath">
	/// The data file path.
	/// </param>
	/// <returns>
	/// Dictionary object holding string substitutions that will be used
	/// for template transformation.
	/// </returns>
	private static Dictionary<string,string> GetData
	(
		string dataFilePath
	)
	{
		// Read all text from the file into a string array.
		string[] lines = File.ReadAllLines(dataFilePath);

		// Convert string array into a string to string dictionary.
		var data = lines.Select(l => l.Split('=')).ToDictionary(a => a[0], a => a[1]);

		return data;
	}
	#endregion

	#region ProcessTemplate method
	/// <summary>
	/// Loads email message template from a file and performs transformation.
	/// </summary>
	/// <param name="options">
	/// Program options.
	/// </param>
	/// <param name="data">
	/// Dictionary object holding substitution strings used by the transformation.
	/// </param>
	/// <returns>
	/// Mail message generated from the template file.
	/// </returns>
	private static MailMessage ProcessTemplate
	(
		Options options,
		Dictionary<string, string> data
	)
	{
		// The Mailr class used for template transformations.
		MailTemplate msg = new MailTemplate();

		// If the template file name format was not specified,
		// use the default format.
		if (!String.IsNullOrEmpty(options.TemplateNameFormat))
			msg.FileNameFormat = options.TemplateNameFormat;

		// If the mail subject was specified, set it explicitly.
		// Doing so would ignore the contents of the <title> tag.
		if (!String.IsNullOrEmpty(options.MailSubject))
			msg.Subject = options.MailSubject;

		// Either load template form the specified file,
		// or use the template parts to generate the file name 
		// on the fly.
		try
		{
			
			if (String.IsNullOrEmpty(options.TemplateFilePath))
				msg.Load
				(
					options.TemplateDirPath,	// required
					options.TemplateName,		// required
					options.TemplateCulture,	// optional
					options.TemplateExtension	// optional
				);
			else
				msg.Load(options.TemplateFilePath);
		}
		catch(Exception ex)
		{
			String errMsg;

			if (String.IsNullOrEmpty(options.TemplateFilePath))
				errMsg = String.Format("Cannot load template from directory '{0}'.", 
					options.TemplateDirPath);
			else
				errMsg = String.Format("Cannot load template from file '{0}'.", 
					options.TemplateFilePath);

			throw new Exception(errMsg, ex);
		}

		// If we have data, perform substitutions in the template.
		try
		{
			if (data == null)
				msg.Transform();
			else
				msg.Transform(data);
		}
		catch(Exception ex)
		{
			throw new Exception("Cannot transform template.", ex);
		}

		return msg as MailMessage;
	}
	#endregion

	#region SendMessage method
	/// <summary>
	/// Sends the generated email message.
	/// </summary>
	/// <param name="options">
	/// Program options.
	/// </param>
	/// <param name="msg">
	/// Email message.
	/// </param>
	private static void SendMessage
	(
		Options		options,
		MailMessage	msg
	)
	{
		SmtpClient smtp = new SmtpClient();

		// Define SMTP host and port.
		if (!String.IsNullOrEmpty(options.SmtpServer))
		{
			smtp.Host = options.SmtpServer;

			if (options.SmtpPort > 0)
				smtp.Port = options.SmtpPort;
		}

		// Use appropriate credentials.
		if (String.IsNullOrEmpty(options.SmtpUser))
			smtp.Credentials = CredentialCache.DefaultNetworkCredentials;
		else
			smtp.Credentials = new NetworkCredential
			(
				options.SmtpUser, options.SmtpPassword
			);

		// Set the sender's and the recipient's addresses.
		msg.From = new MailAddress(options.MailFromAddress);
		msg.To.Add(options.MailToAddress);

		// Include CC list (if needed).
		if (!String.IsNullOrEmpty(options.MailCcAddress))
			msg.CC.Add(options.MailCcAddress);

		// Include BCC list (if needed).
		if (!String.IsNullOrEmpty(options.MailBccAddress))
			msg.Bcc.Add(options.MailBccAddress);

		// Standard .NET call.
		smtp.Send(msg);
	}
	#endregion


	/// <summary>
	/// Builds and writes error message to console.
	/// </summary>
	/// <param name="errMsg">
	/// The error message (or format string).
	/// </param>
	/// <param name="args">
	/// Optional arguments.
	/// </param>
	private static void ShowError
	(
		string errMsg,
		params object[] args
	)
	{
		if (String.IsNullOrEmpty(errMsg))
			return;

		if (args != null)
			errMsg = String.Format(errMsg, args);

		Console.WriteLine(errMsg);
	}

	/// <summary>
	/// Writes error messages from immediate and inner exceptions to console.
	/// </summary>
	/// <param name="ex">
	/// Exception object.
	/// </param>
	private static void ShowError
	(
		Exception ex
	)
	{
		Console.WriteLine(ex.Message);

		while (ex != null)
		{
			Console.WriteLine(ex.Message);
			ex = ex.InnerException;
		}
	}
}
}
