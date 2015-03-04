using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plossum.CommandLine;

namespace Mailr.Client
{
	/// <summary>
	/// Defines command-line options supported by <token>PRODUCTNAMEEXE</token>.
	/// </summary>
	/// <remarks>
	/// Command-line handling requires
	/// <see href="https://www.nuget.org/packages/Plossum.CommandLine/">Plossum CommandLine</see>
	/// Nuget package (see
	/// <see href="http://www.codeproject.com/Articles/19869/Powerful-and-simple-command-line-parsing-in-C">documentation</see>).
	/// </remarks>
	[CommandLineManager(EnabledOptionStyles=OptionStyles.Windows)]
	[CommandLineOptionGroup(
		"smtp", 
		Name="SMTP host info", 
		Description="Defines the SMTP host info.", 
		Require=OptionGroupRequirement.None)]
	[CommandLineOptionGroup(
		"mail", 
		Name="Mail settings", 
		Description="Defines the email properties, such as subject, sender's address, recipient address(es), etc.", 
		Require=OptionGroupRequirement.None)]
	[CommandLineOptionGroup(
		"template", 
		Name="Template definition", 
		Description="Specifies the template information. " +
			"The /file switch cannot be used with the combination of any other switch in this group.", 
		Require=OptionGroupRequirement.AtLeastOne)]
	[CommandLineOptionGroup(
		"data", 
		Name="Data substitutions", 
		Description="Defines text substitutions for template transformation.")]
	public class Options
	{
		/// <summary>
		/// Gets or sets a value indicating whether 
		/// <token>PRODUCTNAMEEXE</token> is invoked in the help mode,
		/// so it would print usage information.
		/// </summary>
		/// <value>
		///   <c>true</c> if the program is invoked with the <c>/help</c> command-line switch
		///   or without any argument; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/help</c>, <c>/?</c>, <c>/h</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /h
		/// </code>
		/// </example>
		[CommandLineOption(
			Name="help", 
			Aliases="h,?", 
			Description="Shows this help text.", 
			MinOccurs=0, 
			MaxOccurs=1)]
		public bool Help {  get; set; }

		/// <summary>
		/// Gets or sets the name or IP address of the SMTP server host.
		/// </summary>
		/// <value>
		/// The name or IP address of the SMTP server passed via 
		/// the <c>/server</c> command-line switch and then assigned to the
		/// <see cref="System.Net.Mail.SmtpClient.Host">SmtpClient.Host</see>
		/// property.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/server</c>, <c>/s</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /s:smtp.company.com [...]
		/// </code>
		/// </example>
		/// <seealso cref="SmtpPort"/>
		[CommandLineOption(
			Name="server", 
			Aliases="s", 
			Description="Name or IP address of the SMTP server.", 
			MinOccurs=0, 
			MaxOccurs=1, 
			GroupId="smtp")]
		public string SmtpServer {  get; set; }

		/// <summary>
		/// Gets or sets the port number of the SMTP server host process.
		/// </summary>
		/// <value>
		/// The SMTP server host port passed via 
		/// the <c>/port</c> command-line switch and then assigned to the
		/// <see cref="System.Net.Mail.SmtpClient.Port">SmtpClient.Port</see>
		/// property.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/port</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /s:smtp.company.com /port:25 [...]
		/// </code>
		/// </example>
		/// <seealso cref="SmtpServer"/>
		[CommandLineOption(
			Name="port", 
			Description="SMTP server port.", 
			MinOccurs=0, 
			MaxOccurs=1, 
			GroupId="smtp")]
		public int SmtpPort {  get; set; }

		/// <summary>
		/// Gets or sets the user ID for authenticating the mail sender.
		/// </summary>
		/// <value>
		/// The user ID passed via 
		/// the <c>/user</c> command-line switch and then used to initialize the
		/// <see cref="System.Net.Mail.SmtpClient.Credentials">SmtpClient.Credentials</see>
		/// property.
		/// If the user ID is not specified, the 
		/// <see cref="System.Net.CredentialCache.DefaultNetworkCredentials"/>
		/// value will be used.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/user</c>, <c>/u</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /u:myuserid /p:mypassword [...]
		/// </code>
		/// </example>
		/// <seealso cref="SmtpPassword"/>
		[CommandLineOption(
			Name="user", 
			Aliases="u", 
			Description="User name used for SMTP server authentication.", 
			MinOccurs=0, 
			MaxOccurs=1, 
			GroupId="smtp")]
		public string SmtpUser {  get; set; }

		/// <summary>
		/// Gets or sets the password for authenticating the mail sender.
		/// </summary>
		/// <value>
		/// The password passed via 
		/// the <c>/password</c> command-line switch and then used to initialize the
		/// <see cref="System.Net.Mail.SmtpClient.Credentials">SmtpClient.Credentials</see>
		/// property.
		/// If <see cref="SmtpUser"/> is not specified,
		/// this parameter will be ignored.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/user</c>, <c>/u</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /u:myuserid /p:mypassword [...]
		/// </code>
		/// </example>
		/// <seealso cref="SmtpUser"/>
		[CommandLineOption(
			Name="password", 
			Aliases="p,pwd,pass", 
			Description="Password used for SMTP server authentication.", 
			MinOccurs=0, 
			MaxOccurs=1, 
			GroupId="smtp")]
		public string SmtpPassword {  get; set; }

		/// <summary>
		/// Gets or sets the email subject.
		/// </summary>
		/// <value>
		/// The email subject passed via 
		/// the <c>/subject</c> command-line switch and then used to initialize the
		/// <see cref="System.Net.Mail.MailMessage.Subject">MailMessage.Subject</see>
		/// property.
		/// If the email subject is not specified,
		/// the program will attempt to set it using the transformed value of the
		/// <c>title</c> tag of the HTML message body.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/subject</c>, <c>/sub</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /sub:"Hello, world!" [...]
		/// </code>
		/// </example>
		/// <seealso cref="Mailr.MailTemplate"/>
		/// <seealso cref="O:Mailr.MailTemplate.Transform"/>
		[CommandLineOption(
			Name="subject",  
			Aliases="sub", 
			Description="Email message subject.",
			MinOccurs=0,
			MaxOccurs=1, 
			GroupId="mail")]
		public string MailSubject { get; set; }

		/// <summary>
		/// Gets or sets the email address of the message sender.
		/// </summary>
		/// <value>
		/// The email address of the message sender passed via 
		/// the <c>/from</c> command-line switch and then used to initialize the
		/// <see cref="System.Net.Mail.MailMessage.Subject">MailMessage.From</see>
		/// property.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/from</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /from:sender@company.com [...]
		/// </code>
		/// </example>
		[CommandLineOption(
			Name="from",  
			Description="Email address of the message sender.",
			MinOccurs=1,
			MaxOccurs=1, 
			GroupId="mail")]
		public string MailFromAddress { get; set; }

		/// <summary>
		/// Gets or sets the email address(es) of the message recipient(s).
		/// </summary>
		/// <value>
		/// The email address(es) of the message recipient(s) passed via 
		/// the <c>/to</c> command-line switch and then used to initialize the
		/// <see cref="System.Net.Mail.MailMessage.To">MailMessage.To</see>
		/// property.
		/// If this parameter is not specified,
		/// the recipient's email address will be set to the 
		/// <see cref="MailFromAddress"/> parameter value.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/from</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none" title="Send mail to one recipient">
		/// <token>FILENAMEEXE</token> /to:recipient@company.com [...]
		/// </code>
		/// <code language="none" title="Send mail to two recipients">
		/// <token>FILENAMEEXE</token> /to:recipient1@company.com,recipient2@company.com [...]
		/// </code>
		/// </example>
		/// <seealso cref="MailFromAddress"/>
		[CommandLineOption(
			Name="to",  
			Description="Email address(es) of the message recipient.",
			MinOccurs=0,
			MaxOccurs=1, 
			GroupId="mail")]
		public string MailToAddress { get; set; }

		/// <summary>
		/// Gets or sets the email address(es) of the message CC recipient(s).
		/// </summary>
		/// <value>
		/// The email address(es) of the message 
		/// <see href="http://en.wikipedia.org/wiki/Carbon_copy#Email">CC</see>
		/// recipient(s) passed via 
		/// the <c>/cc</c> command-line switch and then used to initialize the
		/// <see cref="System.Net.Mail.MailMessage.CC">MailMessage.CC</see>
		/// property.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/cc</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none" title="Send carbon copy to one recipient">
		/// <token>FILENAMEEXE</token> /bcc:recepient@company.com [...]
		/// </code>
		/// <code language="none" title="Send carbon copy to two recipients">
		/// <token>FILENAMEEXE</token> /bcc:recipient1@company.com,recipient2@company.com [...]
		/// </code>
		/// </example>
		[CommandLineOption(
			Name="cc",  
			Description="Email address(es) of the message CC recipient(s).",
			MinOccurs=0,
			MaxOccurs=1, 
			GroupId="mail")]
		public string MailCcAddress { get; set; }

		/// <summary>
		/// Gets or sets the email address(es) of the message BCC recipient(s).
		/// </summary>
		/// <value>
		/// The email address(es) of the message
		/// <see href="http://en.wikipedia.org/wiki/Carbon_copy#Email">BCC</see>
		/// recipient(s) passed via 
		/// the <c>/bcc</c> command-line switch and then used to initialize the
		/// <see cref="System.Net.Mail.MailMessage.Bcc">MailMessage.Bcc</see>
		/// property.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/bcc</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none" title="Send blind carbon copy to one recipient">
		/// <token>FILENAMEEXE</token> /bcc:recepient@company.com [...]
		/// </code>
		/// <code language="none" title="Send blind carbon copy to two recipients">
		/// <token>FILENAMEEXE</token> /bcc:recipient1@company.com,recipient2@company.com [...]
		/// </code>
		/// </example>
		[CommandLineOption(
			Name="bcc",  
			Description="Email address(es) of the message BCC recipient(s).",
			MinOccurs=0,
			MaxOccurs=1, 
			GroupId="mail")]
		public string MailBccAddress { get; set; }

		/// <summary>
		/// Gets or sets the template file path.
		/// </summary>
		/// <value>
		/// The path to the template file passed via 
		/// the <c>/file</c> command-line switch and used
		/// to generate email message body.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/file</c>, <c>/f</c>.
		/// </para>
		/// <para>
		/// This command-line switch cannot be used in combination with the
		/// following switch(es): 
		/// <c>/dir</c>, <c>/name</c>, <c>/culture</c>, <c>/ext</c>, and <c>/format</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none" title="Use relative path">
		/// <token>FILENAMEEXE</token> /f:..\..\Email\Templates\Hello_en-us.html [...]
		/// </code>
		/// <code language="none" title="Use absolute path">
		/// <token>FILENAMEEXE</token> /f:"c:\AppFolder\Email\Templates\Hello_en-us.html" [...]
		/// </code>
		/// </example>
		/// <seealso cref="TemplateDirPath"/>
		/// <seealso cref="TemplateName"/>
		/// <seealso cref="TemplateCulture"/>
		/// <seealso cref="TemplateExtension"/>
		/// <seealso cref="TemplateNameFormat"/>
		[CommandLineOption(
			Name="file", 
			Aliases="f", 
			Description="Path to the template file.",
			MinOccurs=0, 
			MaxOccurs=1, 
			Prohibits="dir,name,culture,ext,format",
			GroupId="template")]
		public string TemplateFilePath { get; set; }

		/// <summary>
		/// Gets or sets the directory part of the template file path.
		/// </summary>
		/// <value>
		/// The path to the directory holding the template file passed via 
		/// the <c>/dir</c> command-line switch.
		/// This folder will be used to generate path to the template file
		/// in the specified <see cref="TemplateNameFormat">format</see>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/dir</c>, <c>/d</c>, <c>/directory</c>.
		/// </para>
		/// <para>
		/// This command-line switch cannot be used in combination with the
		/// <c>/file</c> switch. 
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none" title="Use relative path">
		/// <token>FILENAMEEXE</token> /f:..\..\Email\Templates [...]
		/// </code>
		/// <code language="none" title="Use absolute path">
		/// <token>FILENAMEEXE</token> /f:"c:\AppFolder\Email\Templates" [...]
		/// </code>
		/// </example>
		/// <seealso cref="TemplateFilePath"/>
		/// <seealso cref="TemplateName"/>
		/// <seealso cref="TemplateCulture"/>
		/// <seealso cref="TemplateExtension"/>
		/// <seealso cref="TemplateNameFormat"/>
		[CommandLineOption(
			Name="dir", 
			Aliases="d,directory", 
			Description="Path to the template folder.",
			MinOccurs=0, 
			MaxOccurs=1,
			Prohibits="file",
			GroupId="template")]
		public string TemplateDirPath { get; set; }

		/// <summary>
		/// Gets or sets the template name part of the template file path.
		/// </summary>
		/// <value>
		/// The base name of the template file passed via 
		/// the <c>/name</c> command-line switch.
		/// This name will be used to generate path to the template file
		/// in the specified <see cref="TemplateNameFormat">format</see>.
		/// </value>
		/// <remarks>
		/// <para>
		/// This parameter should not include the culture (or language)
		/// as well as the file extension parts.
		/// </para>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/name</c>, <c>/n</c>.
		/// </para>
		/// <para>
		/// This command-line switch cannot be used in combination with the
		/// <c>/file</c> switch. 
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /n:Hello [...]
		/// </code>
		/// </example>
		/// <seealso cref="TemplateFilePath"/>
		/// <seealso cref="TemplateDirPath"/>
		/// <seealso cref="TemplateCulture"/>
		/// <seealso cref="TemplateExtension"/>
		/// <seealso cref="TemplateNameFormat"/>
		[CommandLineOption(
			Name="name", 
			Aliases="n", 
			Description="Name of the template.",
			MinOccurs=0, 
			MaxOccurs=1, 
			Prohibits="file",
			GroupId="template")]
		public string TemplateName { get; set; }

		/// <summary>
		/// Gets or sets the culture or language code part of the
		/// template file path.
		/// </summary>
		/// <value>
		/// The template localization culture or language code passed via 
		/// the <c>/culture</c> command-line switch.
		/// This name will be used to generate path to the template file
		/// in the specified <see cref="TemplateNameFormat">format</see>.
		/// If this argument is not specified, the 
		/// <see cref="Mailr.MailTemplate.DefaultCulture"/>
		/// may be used.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/culture</c>, <c>/c</c>.
		/// </para>
		/// <para>
		/// This command-line switch cannot be used in combination with the
		/// <c>/file</c> switch. 
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /c:es-mx [...]
		/// </code>
		/// </example>
		/// <seealso cref="TemplateFilePath"/>
		/// <seealso cref="TemplateDirPath"/>
		/// <seealso cref="TemplateName"/>
		/// <seealso cref="TemplateExtension"/>
		/// <seealso cref="TemplateNameFormat"/>
		/// <seealso cref="Mailr.MailTemplate.DefaultCulture"/>
		[CommandLineOption(
			Name="culture", 
			Aliases="c", 
			Description="BodyTemplate culture or language code.",
			MinOccurs=0, 
			MaxOccurs=1, 
			Prohibits="file",
			GroupId="template")]
		public string TemplateCulture { get; set; }

		/// <summary>
		/// Gets or sets the file extension part of the template file path.
		/// </summary>
		/// <value>
		/// The file extension passed via 
		/// the <c>/culture</c> command-line switch.
		/// This extension will be used to generate path to the template file
		/// in the specified <see cref="TemplateNameFormat">format</see>.
		/// If this argument is not specified, the default
		/// <see cref="Mailr.MailTemplate.FileExtension"/>
		/// may be used.
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/extension</c>, <c>/e</c>, <c>/ext</c>.
		/// </para>
		/// <para>
		/// This command-line switch cannot be used in combination with the
		/// <c>/file</c> switch. 
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /e:.xhtml [...]
		/// </code>
		/// </example>
		/// <seealso cref="TemplateFilePath"/>
		/// <seealso cref="TemplateDirPath"/>
		/// <seealso cref="TemplateName"/>
		/// <seealso cref="TemplateCulture"/>
		/// <seealso cref="TemplateNameFormat"/>
		/// <seealso cref="Mailr.MailTemplate.FileExtension"/>
		[CommandLineOption(
			Name="ext", 
			Aliases="e,extension", 
			Description="BodyTemplate file extension.",
			MinOccurs=0, 
			MaxOccurs=1, 
			Prohibits="file",
			GroupId="template")]
		public string TemplateExtension { get; set; }

		/// <summary>
		/// Gets or sets the format string used to generate a localized template name.
		/// </summary>
		/// <value>
		/// The format string passed via 
		/// the <c>/format</c> command-line switch.
		/// This format will be used to generate the file name part in the 
		/// template file path.
		/// The format string must have three placeholders with indexes 0, 1, and 2
		/// corresponding to the template name, culture/language code, 
		/// and file extension respectively, e.g. <c>{0}_{1}{2}</c> which may result
		/// in the file name like <c>Hello_en-us.html</c>.
		/// If this argument is not specified, the default
		/// will be used (see additional details in the 
		/// <see cref="Mailr.MailTemplate.FileNameFormat">documentation</see>).
		/// </value>
		/// <remarks>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/format</c>.
		/// </para>
		/// <para>
		/// This command-line switch cannot be used in combination with the
		/// <c>/file</c> switch. 
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none">
		/// <token>FILENAMEEXE</token> /c:{0}_{1}{2} [...]
		/// </code>
		/// </example>
		/// <seealso cref="TemplateFilePath"/>
		/// <seealso cref="TemplateDirPath"/>
		/// <seealso cref="TemplateName"/>
		/// <seealso cref="TemplateCulture"/>
		/// <seealso cref="TemplateExtension"/>
		/// <seealso cref="Mailr.MailTemplate.FileNameFormat"/>
		[CommandLineOption(
			Name="format", 
			Description="Format of the template file name.",
			MinOccurs=0, 
			MaxOccurs=1, 
			Prohibits="file",
			GroupId="template")]
		public string TemplateNameFormat { get; set; }

		/// <summary>
		/// Gets or sets the data file path.
		/// </summary>
		/// <value>
		/// The path to the data file passed via 
		/// the <c>/data</c> command-line switch and used
		/// to set up the substitution stings for template transformation.
		/// </value>
		/// <remarks>
		/// <para>
		/// The data file must be a text file holding <c>name=value</c> pairs
		/// matching the placeholder names in the email template.
		/// Each data pair must be on a separate line:
		/// <code language="none">
		/// Name=John Smith
		/// UnsubscribeID=467DAA31B9F941E99320C44644FFD104
		/// </code>
		/// </para>
		/// <para>
		/// Command-line switch (and aliases): 
		/// <c>/data</c>.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="none" title="Use relative path">
		/// <token>FILENAMEEXE</token> /d:..\..\Email\Templates\Hello_en-us.txt [...]
		/// </code>
		/// <code language="none" title="Use absolute path">
		/// <token>FILENAMEEXE</token> /d:"c:\AppFolder\Email\Templates\Hello_en-us.txt" [...]
		/// </code>
		/// </example>
		[CommandLineOption(
			Name="data", 
			Description="Path to the file holding name=value pairs used for data transformation. " +
				"If this parameter is not specified, the transformations will not be performed.",
			MinOccurs=0, 
			MaxOccurs=1, 
			GroupId="data")]
		public string DataFilePath { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Options"/> class.
		/// </summary>
		internal Options()
		{
			SmtpPort = 0;
		}
	}
}
