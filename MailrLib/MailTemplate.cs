using System;
using System.IO;
using System.Net.Mail;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;

using RazorEngine;
using HtmlAgilityPack;

namespace Mailr
{
	/// <summary>
	/// Extends the .NET Framework's 
	/// <see cref="System.Net.Mail.MailMessage"/> 
	/// class to simplify email message generation from localized
	/// stand-alone template files or text strings
	/// using <a href="https://www.nuget.org/packages/RazorEngine/">RazorEngine</a>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// To generate an email message, first instantiate the object just
	/// as you would with the 
	/// <see cref="System.Net.Mail.MailMessage"/> 
	/// class:
	/// </para>
	/// <para>
	/// <code>
	/// MailTemplate msg = new MailTemplate(
	///     "mary@contoso.com",
	///     "jose@contoso.com");
	/// </code>
	/// </para>
	/// <para>
	/// Then load the template from the specific file path:
	/// </para>
	/// <para>
	/// <code>
	/// msg.Load("..\\Templates\\Welcome_es-mx.xhtml");
	/// </code>
	/// </para>
	/// <para>
	/// Instead of using the template file path,
	/// you can specify the path to the template directory, 
	/// the template name, and the language or the culture code
	/// of the specific translation:
	/// </para>
	/// <para>
	/// <code>
	/// msg.Load("..\\Templates", "Welcome", "es-mx", ".xhtml");
	/// </code>
	/// </para>
	/// <para>
	/// In this case, MailmessageTemplate will generate the path 
	/// using the default or explicitly defined format
	/// (see <see cref="FileNameFormat"/>),
	/// verify that the template file exists, and if not, 
	/// it will us the path of the fallback or default culture
	/// (see <see cref="DefaultCulture"/>).
	/// This option allows you to incrementally add translations
	/// without making changes to code or application configuration.
	/// </para>
	/// <para>
	/// Once the template is loaded, 
	/// the caller must apply data transformations
	/// by passing the data structure holding data substitutions.
	/// MailTemplate uses 
	/// <see href="http://antaris.github.io/RazorEngine/">Razor Engine</see>
	/// for data transformations.
	/// All occurrences of text that start with
	/// <c>@Model.</c> and immediately followed by a placeholder 
	/// will be substituted with the value of object property with 
	/// the same name as the placeholder.
	/// For example, the template text 
	/// <c>"¡Hola @Model.Name!"</c>
	/// will be changed to 
	/// <c>"¡Hola Jose!"</c>
	/// if the transformation uses a data object with the property named
	/// <c>Name</c> holding the string value <c>Jose</c>.
	/// Razor Engine supports both strict as well as anonymous types, 
	/// as illustrated in this example:
	/// </para>
	/// <para>
	/// <code>
	/// msg.Transform(new { Name = "Jose", });
	/// </code>
	/// </para>
	/// <para>
	/// If a template does not contain data placeholders,
	/// you can skip the data transformation step.
	/// </para>
	/// <para>
	/// In some cases, the transformation step may implicitly set the email 
	/// message subject using the transformed inner text value of the 
	/// email template's <c>title</c> tag. For additional information,
	/// see the remarks section in the description of the 
	/// <see cref="Transform(NameValueCollection)"/> method.
	/// </para>
	/// <para>
	/// You can now set all other required message fields and send the
	/// message via the <see cref="System.Net.Mail.SmtpClient"/> object.
	/// </para>
	/// <para>
	/// <code>
	/// SmtpClient smtp = new SmtpClient(server);
	/// 
	/// smtp.Send(msg);
	/// </code>
	/// </para>
	/// <para>
	/// For other message sending options, see the 
	/// <see cref="System.Net.Mail.MailMessage"/> 
	/// class documentation.
	/// </para>
	/// </remarks>
	public class MailTemplate: MailMessage
	{
		#region Private properties

		private string	_templateFileNameFormat		= "{0}_{1}{2}";
		private string	_defaultCulture				= "en-us";
		private string	_defaultTemplateExtension	= ".html";
		private string	_templateFilePath			= null;
		private string	_bodyTemplate				= null;
		private bool	_escaped					= false;

		private	static MemoryCache _cache		= MemoryCache.Default;

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets a value indicating whether to automatically
		/// escape the special characters that would cause
		/// template transformation errors when a template is loaded
		/// from a file.
		/// </summary>
		/// <value>
		/// <para>
		/// <c>true</c> if special characters should be escaped automatically; 
		/// otherwise, <c>false</c>.
		/// </para>
		/// <para>
		/// By default, this property is set to <c>true</c>.
		/// </para>
		/// </value>
		/// <seealso cref="Escape()"/>
		/// <seealso cref="Escape(string, bool)"/>
		public bool AutoEscape { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to cache 
		/// non-transformed email templates.
		/// </summary>
		/// <value>
		/// <c>true</c> if caching is enabled; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// When a caller invokes a
		/// <see cref="O:Mailr.MailTemplate.Load">LoadTemplate</see>
		/// method, it may need to access the file system multiple time.
		/// For example, the template for the specified culture may not exist,
		/// so the code will attempt to read the template from an alternative file
		/// using a fallback algorithm (see TBD).
		/// Caching allows the application to avoid multiple file read attempts on 
		/// subsequent calls.
		/// </para>
		/// <para>
		/// With caching enabled, the original template will be saved in memory,
		/// so each subsequent request for the same template and culture combination
		/// would not require file system access.
		/// </para>
		/// <para>
		/// By default, caching is enabled.
		/// </para>
		/// </remarks>
		public bool UseCache { get; set; }

		/// <summary>
		/// Gets or sets the email body template.
		/// </summary>
		/// <value>
		/// The template used to generate the email message body.
		/// </value>
		/// <remarks>
		/// <para>
		/// In a typical case, this property should be loaded from 
		/// a template file using the
		/// <see cref="Load(string)"/>
		/// method.
		/// </para>
		/// <para>
		/// Special characters in the string value assigned to this
		/// property (via the <c>set</c> method) will not be
		/// escaped automatically.
		/// </para>
		/// </remarks>
		/// <seealso cref="Escape()"/>
		/// <seealso cref="Escape(string,bool)"/>
		public string BodyTemplate 
		{
			get
			{
				return _bodyTemplate; 
			}

			set
			{
				Body			= null;
				_bodyTemplate	= value; 
				_escaped		= false;
			}
		}

		/// <summary>
		/// Gets or sets the cache item policy.
		/// </summary>
		/// <value>
		/// The cache item policy.
		/// </value>
		/// <remarks>
		/// For a typical application, the default cache item policy should suffice.
		/// </remarks>
		public CacheItemPolicy CacheItemPolicy { get; set; }

		/// <summary>
		/// Gets or sets the format used to generate the template file name
		/// from the given template name, culture code, and file extension.
		/// </summary>
		/// <value>
		/// The template name format.
		/// </value>
		/// <exception cref="System.ArgumentException">
		/// Indicates whether the value to be set is empty or in a wrong format
		/// (see remarks).
		/// </exception>
		/// <remarks>
		/// <para>
		/// The value must have three placeholders:
		/// {0} - template base file name (e.g. <c>Welcome</c>),  
		/// {1} - culture or language string (e.g. <c>es-mx</c>), and 
		/// {2} - template file extension (e.g. <c>.xhtml</c>).
		/// </para>
		/// <para>
		/// The default template file name format is <c>{0}_{1}{2}</c>.
		/// Given the values of template name, culture code, and file extension
		/// set to <c>Welcome</c>, <c>es-mx</c>, and <c>.xhtml</c>,
		/// the file name generated for this template will be <c>Welcome_es-mx.xhtml</c>.
		/// If you set the template value to <c>{0}.{1}{2}</c>, 
		/// for the same name, culture, and extension, the generated template file name
		/// will be <c>Welcome.es-mx.xhtml</c>.
		/// </para>
		/// </remarks>
		/// <seealso cref="DefaultCulture"/>
		/// <seealso cref="FileExtension"/>
		public string FileNameFormat
		{ 
			set
			{
				if (String.IsNullOrEmpty(value))
					throw new ArgumentException("Value cannot be null or empty.");

				if (!value.Contains("{0}")  || !value.Contains("{1}") || !value.Contains("{2}"))
					throw new ArgumentException(
						"Value must contain placeholders for three indexed elements: " +
						"'{0}' - template base file name, " +
						"'{1}' - culture string, and " + 
						"'{2}' - template file extension.");

				_templateFileNameFormat = value;
			}

			get
			{
				return _templateFileNameFormat;
			}
		}

		/// <summary>
		/// Gets or sets the default culture that would be used as a fallback
		/// in case the requested template localization is not provided.
		/// </summary>
		/// <value>
		/// The default culture.
		/// </value>
		/// <remarks>
		/// <para>
		/// When not explicitly set, the default culture is <c>en-us</c>.
		/// </para>
		/// <para>
		/// The culture string may contain a language code 
		/// or a language and country (locale) code.
		/// Either the underscore (_) or the dash (-) character may be used
		/// between the language and country codes, e.g.
		/// <c>es-mx</c> and <c>es_mx</c>.
		/// </para>
		/// </remarks>
		public string DefaultCulture
		{ 
			set
			{
				if (value == null)
					_defaultCulture = "";
				else
					_defaultCulture = value;
			}

			get
			{
				return _defaultCulture;
			}
		}

		/// <summary>
		/// Gets or sets the default template file extension.
		/// </summary>
		/// <value>
		/// The default template file extension.
		/// </value>
		/// <remarks>
		/// When not explicitly set, the default extension is <c>.html</c>.
		/// </remarks>
		public string FileExtension
		{ 
			set
			{
				if (value == null)
					_defaultTemplateExtension = "";
				else
					_defaultTemplateExtension = value;
			}

			get
			{
				return _defaultTemplateExtension;
			}
		}

		/// <summary>
		/// Gets the path of the localized template file.
		/// </summary>
		/// <value>
		/// The path of the localized template file.
		/// </value>
		/// <remarks>
		/// This property is set when the template is read by
		/// one of the <see cref="O:Mailr.MailTemplate.Load"/>
		/// methods.
		/// </remarks>
		public string FilePath
		{
			get
			{
				return _templateFilePath;
			}
		}

		/// <summary>
		/// Gets or sets the flag indicating whether the email subject
		/// retrieved from the HTML email template
		/// should be normalized (i.e. have all new lines, tabs, and
		/// multiple space characters converted to single space characters).
		/// </summary>
		/// <value>
		/// The value containing email subject normalization flag.
		/// </value>
		/// <remarks>
		/// <para>
		/// When not explicitly set, the default flag is set to <c>true</c>.
		/// </para>
		/// <para>
		/// This setting only affects subjects retrieved from the title tag
		/// of the HTML template file. If the email subject is assigned
		/// explicitly, normalization will not be performed regardless of
		/// this setting.
		/// </para>
		/// </remarks>
		public bool NormalizeSubject { get; set; }
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes an empty instance of the class.
		/// </summary>
		/// <param name="isBodyHtml">
		/// Indicates whether  the email message body is formatted as HTML.
		/// The default value is <c>true</c>.
		/// </param>
		public MailTemplate
		(
			bool isBodyHtml = true
		) 
		{ 
			Initialize(isBodyHtml); 
		}

		#pragma warning disable 1573 
		/// <inheritdoc cref="MailTemplate(bool)" select="param"/> 
		/// <summary>
		/// Initializes a new instance of the class
		/// for the specified message sender and recipient.
		/// </summary>
		/// <param name="from">
		/// Email address of the message sender.
		/// </param>
		/// <param name="to">
		/// Email address(es) of the message recipient(s).
		/// </param>
		public MailTemplate
		(
			MailAddress from, 
			MailAddress to,
			bool		isBodyHtml = true
		) : 
			base(from, to) 
		{
			Initialize(isBodyHtml);
		}
		#pragma warning restore 1573

		#pragma warning disable 1573 
		/// <inheritdoc cref="MailTemplate(MailAddress,MailAddress,bool)" select="summary|param"/> 
		/// <param name="from">
		/// Email address of the message sender.
		/// </param>
		/// <param name="to">
		/// Email address(es) of the message recipient(s).
		/// </param>

		public MailTemplate
		(
			string	from, 
			string	to,
			bool	isBodyHtml = true
		) : 
			base(from, to) 
		{
			Initialize(isBodyHtml);
		}
		#pragma warning restore 1573

		#pragma warning disable 1573 
		/// <inheritdoc cref="MailTemplate(string,string,bool)" select="summary|param"/> 
		/// <param name="subject">
		/// Subject of the email message.
		/// </param>
		/// <param name="body">
		/// Body of the email message .
		/// </param>
		public MailTemplate
		(
			string	from, 
			string	to, 
			string	subject, 
			string	body,
			bool	isBodyHtml = true
		) : 
			base(from, to, subject, body)
		{
			Initialize(isBodyHtml);
		}
		#pragma warning restore 1573
		#endregion

		#region Public methods
		#region Load overloads
		/// <overloads>
		/// Loads a localized message  from a template file.
		/// </overloads>
		/// <summary>
		/// Loads a localized email message template from a 
		/// file which path is built from the template directory, name, 
		/// culture code and file extension.
		/// </summary>
		/// <param name="templateFolderPath">
		/// Path to the folder holding the file template.
		/// It can be either a relative or absolute path.
		/// </param>
		/// <param name="templateName">
		/// The part of the file name that identifies the name of the template.
		/// </param>
		/// <param name="templateFileExtension">
		/// The part of the file name that identifies the template extension.
		/// If this parameter is not specified or is set to <c>null</c>,
		/// the <see cref="FileExtension"/> property will be used.
		/// </param>
		/// <param name="cultureCode">
		/// The part of the file name that identifies the culture or language.
		/// If this parameter is not specified or is set to <c>null</c>,
		/// the <see cref="DefaultCulture"/> property will be used.
		/// </param>
		/// <example>
		/// <code>
		/// MailTemplate msg = new MailTemplate();
		/// msg.Load(".\\Templates", "Welcome", ".xhtml", "es-mx");
		/// </code>
		/// </example>
		/// <remarks>
		/// <para>
		/// This method does not guarantee that the template will be loaded for
		/// the requested culture code. If the file is not found in the
		/// generated path, an alternative template may be loaded instead.
		/// The fallback logic works like this:
		/// </para>
		/// <list type="number">
		/// <item>
		/// <description>
		/// Generate the file name using the <see cref="FileNameFormat"/> string
		/// and the values of the template name, culture code, and extension.
		/// Let's assume that <see cref="FileNameFormat"/>
		/// is defined as <c>{0}_{1}{2}</c>
		/// and the values of <paramref name="templateName"/>, 
		/// <paramref name="cultureCode"/>, and <paramref name="templateFileExtension"/> 
		/// are set to <c>Welcome</c>, <c>es_mx</c>, and <c>.xhtml</c> respectively.
		/// The generated file name will be <c>Welcome_es-mx.xhtml</c>.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Attempt to load the generated <c>Welcome_es-mx.xhtml</c> file
		/// from the folder identified by the <paramref name="templateFolderPath"/>
		/// parameter.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// If the file fails to load,
		/// convert the <c>es-mx</c> culture code to the language code
		/// without the country (<c>-mx</c>) suffix, i.e. <c>es</c>.
		/// <note type="note">
		/// Conversion from a culture to a language code always assumes that
		/// the language separator character is either dash (-) or underscore (_).
		/// So whether the culture code is set to <c>es_mx</c>
		/// or <c>es-mx</c>,
		/// either version would be converted to the same language code: 
		/// <c>es</c>.
		/// </note>
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Attempt to load the <c>Welcome_es.xhtml</c>.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// If the file fails to load,
		/// used the default culture code (<c>en-us</c>) or the code previously
		/// set via the <see cref="DefaultCulture"/> property.
		/// Assuming that the default culture code is <c>en-us</c>,
		/// the next fallback file name would be <c>Welcome_en-us.xhtml</c>.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Attempt to load the <c>Welcome_en-us.xhtml</c> file.
		/// </description>
		/// </item>		
		/// <item>
		/// <description>
		/// If the file fails to load,
		/// use the culture-independent file name:
		/// <c>Welcome.xhtml</c>.
		/// </description>
		/// </item>
		/// </list>
		/// <para>
		/// To summarize, the file names will be tried in the following order:
		/// </para>
		/// <list type="number">
		/// <item><description><c>Welcome_es-mx.xhtml</c></description></item>
		/// <item><description><c>Welcome_es.xhtml</c></description></item>
		/// <item><description><c>Welcome_en-us_mx.xhtml</c></description></item>
		/// <item><description><c>Welcome.xhtml</c></description></item>
		/// </list>
		/// <para>
		/// The first find found will be used as the source of the email template.
		/// </para>
		/// <para>
		/// If <see cref="AutoEscape"/> is set to <c>true</c>,
		/// after loading the template, this method will
		/// implicitly call the
		/// <see cref="Escape()"/>
		/// method.
		/// </para>
		/// </remarks>
		/// <seealso cref="Load(string)"/>
		/// <seealso cref="AutoEscape"/>
		/// <seealso cref="O:Mailr.MailTemplate.Transform"/>
		public void Load
		(
			string templateFolderPath,
			string templateName,
			string cultureCode				= null,
			string templateFileExtension	= null
		)
		{
			// Generate path of for the existing file using 
			// the specified parameters.
			string path = GetTemplatePath
			(
				templateFolderPath, 
				templateName, 
				templateFileExtension, 
				cultureCode
			);

			Load(path);
		}

		/// <summary>
		/// Loads a localized email message template from the 
		/// file identified by the specified file path.
		/// </summary>
		/// <param name="templateFilePath">
		/// The file path of the localized email template.
		/// This value can hold a relative or absolute path.
		/// </param>
		/// <remarks>
		/// <para>
		/// If the specified file does not exist
		/// this method will not attempt to load a template from
		/// an alternate path using the fallback algorithm
		/// described in the 
		/// <see cref="Load(String, String, String,String)"/>
		/// method documentation.
		/// </para>
		/// <para>
		/// If <see cref="AutoEscape"/> is set to <c>true</c>,
		/// after loading the template, this method will
		/// implicitly call the
		/// <see cref="Escape()"/>
		/// method.
		/// </para>
		/// </remarks>
		/// <seealso cref="Load(string,string,string,string)"/>
		/// <seealso cref="AutoEscape"/>
		/// <seealso cref="O:Mailr.MailTemplate.Transform"/>
		public void Load
		(
			string templateFilePath
		)
		{
			Body			= null;
			_bodyTemplate	= null;
			_escaped		= false;

			string cachedItemTemplateName = GetCachedTemplateName(templateFilePath);

			if (UseCache)
			{
				_bodyTemplate = _cache.Get(cachedItemTemplateName) as string;
			}

			if (_bodyTemplate == null)
			{
				try
				{
					using (StreamReader reader = new StreamReader(templateFilePath))
					{
						_bodyTemplate = reader.ReadToEnd();
					}
				}
				catch (Exception ex)
				{
					throw new IOException(
						String.Format("Error reading file '{0}'.", templateFilePath), ex);
				}

				if (UseCache)
				{
					if (!_cache.Contains(cachedItemTemplateName))
					{
						_cache.Set(cachedItemTemplateName, _bodyTemplate, CacheItemPolicy);
					}
				}
			}

			// Save template file path.
			_templateFilePath = templateFilePath;
		}
		#endregion

		#region Transform overloads
		/// <overloads>
		/// Replaces placeholder strings in the email message template
		/// with the specified substitution values.
		/// </overloads>
		/// <summary>
		/// Replaces placeholder strings in the email message template
		/// using the key-value pairs.
		/// </summary>
		/// <param name="keyValuePairs">
		/// The collection of the key-value pairs in which keys
		/// are named after the placeholders in the template.
		/// </param>
		/// <remarks>
		/// <para>
		/// If the message is formatted as HTML 
		/// (i.e. if the <see cref="System.Net.Mail.MailMessage.IsBodyHtml"/> property
		/// is set to <c>true</c>) and the 
		/// <see cref="System.Net.Mail.MailMessage.Subject"/> property
		/// is not set,
		/// after applying the transformations this method will also
		/// set the <see cref="System.Net.Mail.MailMessage.Subject"/> property
		/// of the object to the transformed inner text value of the 
		/// <c>title</c> tag in the template (if the <c>title</c> element is found).
		/// You can override this behavior by explicitly setting or 
		/// overwriting the message subject:
		/// </para>
		/// <code language="C#">
		/// MailTemplate msg = new MailTemplate(...);
		/// msg.Subject = "¡Hola!"
		/// </code>
		/// </remarks>
		/// <example>
		/// <para>
		/// The following example illustrates how to pass different data types
		/// to the template transformation methods.
		/// </para>
		/// <para>
		/// Given the HTML message template that contains following lines:
		/// </para>
		/// <code language="html" title="Snippet of the HTML message template (es-mx)">
		/// <![CDATA[
		/// <html ...>
		/// ...
		/// <title>La bienvenida al @Model.SenderCompany</title>
		/// ...
		/// <p>¡Hola @Model.RecipientName!</p>
		/// ...
		/// </html>
		/// ]]>
		/// </code>
		/// <para>or</para>
		/// <code language="html" title="Snippet of the HTML message template (en-us)">
		/// <![CDATA[
		/// <html ...>
		/// ...
		/// <title>Welcome from @Model.SenderCompany</title>
		/// ...
		/// <p>Hello @Model.RecipientName!</p>
		/// ...
		/// </html>
		/// ]]>
		/// </code>
		/// <para>
		/// The code will replace the <c>@Model.Name</c> and <c>@Model.Sender</c>
		/// placeholders with the values of the similarly named object properties.
		/// If the email message subject is not set before the transformation,
		/// it will also implicitly set it using the inner text of the
		/// <c>title</c> element.
		/// Once the transformation step is performed, you can pass the
		/// <see cref="Mailr.MailTemplate"/> object to the 
		/// <see cref="System.Net.Mail.SmtpClient.Send(System.Net.Mail.MailMessage)">SmtpClient's 
		/// Send</see> method.
		/// </para>
		/// <code language="C#" title="Initialize template">
		/// // Specify sender's and recipient's email addresses.
		/// MailTemplate msg = new MailTemplate("mary@contoso.com", "jose@fabricam.com");
		/// 
		/// // Load the  Spanish (Mexico) version of the template.
		/// // The following file paths will be tried until either the first one is found
		/// // or all attempts are exhausted:
		/// //
		/// // - ..\Templates\Welcome_es-mx.htm (originally requested)
		/// // - ..\Templates\Welcome_es.htm    (fallback to language only)
		/// // - ..\Templates\Welcome_en-us.htm (default culture, assuming that it is 'en-us')
		/// // - ..\Templates\Welcome.htm       (culture-neutral)
		/// //
		/// msg.Load("..\\Templates", "Welcome", ".htm", "es-mx");
		/// </code>
		/// <para>
		/// Now we can perform the transformations. Any of the following options would work.
		/// </para>
		/// <code language="C#" title="Use anonymous object">
		/// msg.Transform(new { SenderCompany = "Contoso", RecipientName = "Jose", });
		/// </code>
		/// <code language="C#" title="Use strongly typed object">
		/// // Assume that WelcomeData is a class with the properties named
		/// // SenderCompany and RecipientName.
		/// WelcomeData data = new WelcomeData();
		/// 
		/// data.SenderCompany = "Contoso";
		/// data.RecipientName = "Jose";
		/// 
		/// msg.Transform(data);
		/// </code>
		/// <code language="C#" title="Use NameValueCollection">
		/// NameValueCollection data = new NameValueCollection();
		/// 
		/// data.Add("SenderCompany", "Contoso");
		/// data.Add("RecipientName", "Jose");
		/// 
		/// msg.Transform(data);
		/// </code>
		/// <code language="C#" title="Use a Dictionary object">
		/// <![CDATA[
		/// Dictionary<string,string> data = new Dictionary<string,string>();
		/// // Alternatively, you can use a string-to-object dictionary:
		/// // Dictionary<string,object> data = new Dictionary<string,object>();
		/// 
		/// data["SenderCompany"] = "Contoso";
		/// data["RecipientName"] = "Jose";
		/// 
		/// msg.Transform(data);
		/// ]]>
		/// </code>
		/// </example>
		public void Transform
		(
			NameValueCollection keyValuePairs
		)
		{
			ExpandoObject expandoObject = new ExpandoObject();

			var expandoCollection = 
				(ICollection<KeyValuePair<string, object>>)expandoObject;

			foreach (string key in keyValuePairs.Keys)
				expandoCollection.Add(
					new KeyValuePair<string, object>(key, keyValuePairs[key]));

			Transform(expandoObject);
		}

		#pragma warning disable 1573 
		/// <inheritdoc cref="Transform(NameValueCollection)" select="summary|param|remarks|example"/> 
		#pragma warning restore 1573
		public void Transform
		(
			Dictionary<string, string> keyValuePairs
		)
		{
			ExpandoObject expandoObject = new ExpandoObject();

			var expandoCollection = 
				(ICollection<KeyValuePair<string, object>>)expandoObject;

			foreach (string key in keyValuePairs.Keys)
				expandoCollection.Add(new KeyValuePair<string, object>
					(key, keyValuePairs[key]));

			Transform(expandoObject);
		}

		#pragma warning disable 1573 
		/// <inheritdoc cref="Transform(NameValueCollection)" select="summary|param|remarks|example"/> 
		#pragma warning restore 1573
		public void Transform
		(
			Dictionary<string, object> keyValuePairs
		)
		{
			ExpandoObject expandoObject = new ExpandoObject();

			var expandoCollection = (ICollection<KeyValuePair<string, object>>)expandoObject;

			foreach (var keyValuePair in keyValuePairs)
				expandoCollection.Add(keyValuePair);

			Transform(expandoObject);
		}

		/// <inheritdoc cref="Transform(NameValueCollection)" select="remarks|example"/> 
		/// <summary>
		/// Replaces placeholder strings in the email message template
		/// with the values mapped to the properties of the corresponding
		/// object type properties.
		/// </summary>
		/// <param name="model">
		/// The object which properties are named after the placeholders
		/// in the email message template.
		/// The object can be strongly typed or anonymous.
		/// </param>
		public void Transform
		(
			object model
		)
		{
			// Escape @ signs for selected keywords, e.g.
			// change '@media' to '@@media'.
			if (AutoEscape)
				Escape();

			Body = Razor.Parse(BodyTemplate, model);

			SetSubject();
		}

		#pragma warning disable 1573 
		/// <inheritdoc cref="Transform(object)" select="summary|remarks|example"/>
		/// <typeparam name="T">
		/// Data type of the object holding template substitutions.
		/// </typeparam>
		/// <param name="model">
		/// The object which properties are named after the placeholders
		/// in the email message template.
		/// The object can be strongly typed or anonymous.
		/// </param>
		#pragma warning restore 1573
		//
		public void Transform<T>
		(
			T model
		)
		{
			// Escape @ signs for selected keywords, e.g.
			// change '@media' to '@@media'.
			if (AutoEscape)
				Escape();

			Body = Razor.Parse<T>(BodyTemplate, model);

			SetSubject();
		}

		/// <summary>
		/// Prepares email message for delivery when no data transformations
		/// need to be performed.
		/// </summary>
		/// <remarks>
		/// This method set the empty <see cref="System.Net.Mail.MailMessage.Body"/>
		/// property to a non-empty value of the <see cref="BodyTemplate"/> property
		/// and process the message subject as described in the 
		/// <see cref="Transform(NameValueCollection)"/> method description.
		/// </remarks>
		public void Transform()
		{
			if (String.IsNullOrEmpty(Body) && 
				!String.IsNullOrEmpty(BodyTemplate))
				Body = BodyTemplate;

			SetSubject();
		}
		#endregion

		#region Escape methods
		/// <overloads>
		/// Escapes the <c>@</c> sign in specific words 
		/// to prevent template transformation errors
		/// (e.g. converts <c>@media</c> to <c>@@media</c>).
		/// </overloads>
		/// <summary>
		/// Escapes the <c>@</c> signs in the words identified in the 
		/// custom HTML <c>meta</c> tag.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is intended for HTML templates only.
		/// It will use the settings identified in the custom
		/// <c>meta</c> tag with the following properties:
		/// </para>
		/// <list type="bullet">
		///	<item>
		///	<description>
		///	The <c>meta</c> element must have the <c>name</c>
		///	attribute with the value <c>mailr-escape</c>.
		///	</description>
		/// </item>
		///	<item>
		///	<description>
		///	The <c>content</c> attribute of the <c>meta</c> element 
		///	must contain a comma-separated list of the words
		///	that must be escaped.
		///	</description>
		///	</item>
		///	<item>
		///	<description>
		/// Words in the comma-separated list defined in the
		///	<c>content</c> attribute of the <c>meta</c> element 
		///	must not include the <c>@</c> signs.
		///	</description>
		///	</item>
		///	<item>
		///	<description>
		/// By default, word match will be case-insensitive.
		/// To use case-sensitive search, append 
		/// <c>;ignoreCase=false</c>
		/// to the value of the <c>content</c> attribute.
		/// Notice the semicolon (;) character used as a
		/// separator between the word list an the case sensitivity
		/// setting.
		///	</description>
		/// </item>
		/// </list>
		/// <para>
		/// When called repeatedly for the same template instance,
		/// this method will do nothing (after the first call).
		/// </para>
		/// <para>
		/// This method is called implicitly by the 
		/// <see cref="O:Mailr.MailTemplate.Transform"/>
		/// methods if the value of the 
		/// <see cref="AutoEscape"/> property is set to
		/// <c>true</c>.
		/// </para>
		/// <para>
		/// When called explicitly, this method must be invoked
		/// after the template is loaded, but before the transformation
		/// is performed.
		/// </para>
		/// </remarks>
		/// <example>
		/// <para>
		/// The following examples illustrate how to specify
		/// the words in the template body that start with the 
		/// <c>@</c> sign and must be escaped to avoid
		/// transformation errors:
		/// </para>
		/// <para>
		/// <code language="html" title="Escape '@media'; case-insensitive search">
		/// <![CDATA[
		/// <meta name='mailr-escape' content='media'>
		/// ]]>
		/// </code>
		/// <code language="html" title="Escape '@media', '@namespace'; case-insensitive search">
		/// <![CDATA[
		/// <meta name='mailr-escape' content='media,namespace'>
		/// ]]>
		/// </code>
		/// <code language="html" title="Escape '@media'; case-sensitive search">
		/// <![CDATA[
		/// <meta name='mailr-escape' content='media;ignoreCase=false'>
		/// ]]>
		/// </code>
		/// <code language="html" title="Escape '@media', '@font-face'; case-sensitive search">
		/// <![CDATA[
		/// <meta name='mailr-escape' content='media,font-face;ignoreCase=false'>
		/// ]]>
		/// </code>
		/// </para>
		/// </example>
		/// <seealso cref="AutoEscape"/>
		/// <seealso cref="Escape(string,bool)"/>
		/// <seealso cref="Load(string)"/>
		/// <seealso cref="Load(string,string,string,string)"/>
		public void Escape()
		{
			if (String.IsNullOrEmpty(BodyTemplate) || 
				_escaped || 
				!IsBodyHtml)
				return;

			bool	ignoreCase	= true;
			string	keywords	= null;

			HtmlDocument doc = new HtmlDocument();
			try
			{
				doc.LoadHtml(BodyTemplate);
			}
			catch (Exception ex)
			{
				throw new IOException(
					String.Format("Error loading HTML from: {0}", 
					BodyTemplate), 
					ex);
			}

			_escaped = true;

			try
			{
				HtmlNode node = 
					doc.DocumentNode.SelectSingleNode("//meta[@name='mailr-escape']");

				if (node == null)
					return;

				HtmlAttribute contentAttrib = node.Attributes["content"];

				if (contentAttrib == null)
					return;

				string contentValue = contentAttrib.Value;

				if (contentValue == null)
					return;
				
				string[] values = contentValue.Split(';');

				if (values == null || values.Length < 1)
					return;

				keywords = values[0];

				if (values.Length > 1 && (!String.IsNullOrEmpty(values[1])))
				{
					if (values[1].StartsWith("ignoreCase=false", 
							StringComparison.InvariantCultureIgnoreCase))
						ignoreCase = false;
				}

				// Turn of indicator, since the overloaded method will also
				// check and set it.
				_escaped = false;
				Escape(keywords, ignoreCase);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Escapes the <c>@</c> signs in the words identified in the 
		/// custom HTML <c>meta</c> tag.
		/// </summary>
		/// <param name="keywords">
		/// The list of comma-separated words that must be escaped,
		/// such as <c>"media,font-face"</c>.
		/// Notice that the words must not include the <c>@</c> sign.
		/// </param>
		/// <param name="ignoreCase">
		/// Indicates whether word search must be case-sensitive.
		/// By default, the search is case-insensitive.
		/// </param>
		/// <remarks>
		/// <para>
		/// This method must be invoked after the template is loaded, 
		/// but before the transformation is performed.
		/// </para>
		/// </remarks>
		/// <seealso cref="AutoEscape"/>
		/// <seealso cref="Escape()"/>
		/// <seealso cref="Load(string)"/>
		/// <seealso cref="Load(string,string,string,string)"/>
		public void Escape
		(
			string	keywords,
			bool	ignoreCase = true
		)
		{
			if (String.IsNullOrEmpty(BodyTemplate) || 
				_escaped || 
				String.IsNullOrEmpty(keywords))
				return;

			string	bodyTemplate= BodyTemplate;
			string	pattern		= null;

			string[] words = keywords.Split(',');

			foreach (string word in words)
			{
				if (String.IsNullOrEmpty(word))
					continue;

				// E.g. @(media).
				pattern = String.Format("@({0})", word);

				bodyTemplate = Regex.Replace
				(
					bodyTemplate, 
					pattern, 
					"@@$1", // @media -> @@media
					ignoreCase ? 
						RegexOptions.IgnoreCase : RegexOptions.None
				);
			}

			BodyTemplate = bodyTemplate;
			_escaped = true;
		}
		#endregion

		#endregion

		#region Private methods
		/// <summary>
		/// Sets the subject for the HTML-formatted email message.
		/// </summary>
		private void SetSubject()
		{
			if (String.IsNullOrEmpty(Body) || 
				(!IsBodyHtml) || 
				(!String.IsNullOrEmpty(Subject)))
				return;

			HtmlDocument doc = new HtmlDocument();
			try
			{
				doc.LoadHtml(Body);
			}
			catch (Exception ex)
			{
				throw new IOException(
					String.Format("Error loading HTML from: {0}", 
					Body), 
					ex);
			}

			try
			{
				HtmlNode node = doc.DocumentNode.SelectSingleNode("//title");

				if (node != null)
				{
					string title = node.InnerText;

					// Convert new lines, tabs, and multiple white spaces to
					// single space characters. New lines are particularly
					// nasty: a subject text containing a new line will not be
					// displayed by most email clients.
					if (!String.IsNullOrEmpty(title) && NormalizeSubject)
						title = Regex.Replace(title, @"\s+", " ");

					Subject = title;
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Gets the template path.
		/// </summary>
		/// <param name="templateFolderPath">The template folder path.</param>
		/// <param name="templateName">Name of the template.</param>
		/// <param name="templateExtension">The template extension.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">
		/// Missing parameter: templateFolderPath.
		/// or
		/// Missing parameter: templateName.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">Could not find the specified template file. Probed locations:  + paths</exception>
		private string GetTemplatePath
		(
			string templateFolderPath,
			string templateName,
			string templateExtension	= null,
			string culture				= null
		)
		{
			if (String.IsNullOrEmpty(templateFolderPath))
				throw new ArgumentException("Missing parameter: templateFolderPath.");

			if (String.IsNullOrEmpty(templateName))
				throw new ArgumentException("Missing parameter: templateName.");

			string path				= null;
			string cachedItemName	= GetCachedTemplatePathName(
										templateFolderPath, 
										templateName, 
										templateExtension, 
										culture);

			if (UseCache)
			{
				path = _cache.Get(cachedItemName) as string;

				if (!String.IsNullOrEmpty(path))
					return path;
			}

			if (String.IsNullOrEmpty(templateExtension))
				templateExtension = _defaultTemplateExtension;

			if (String.IsNullOrEmpty(culture))
				culture = _defaultCulture;

			string paths	= "";
			string langCode	= null;
			
			if (culture.IndexOf("-") > 0)
			{
				langCode	= culture.Substring(0, culture.IndexOf("-"));
			}
			else if (culture.IndexOf("_") > 0)
			{
				langCode	= culture.Substring(0, culture.IndexOf("_"));
			}
			else
			{
				langCode	= culture;
				culture		= null;
			}

			// Check full culture (language + country) version.
			if (!String.IsNullOrEmpty(culture))
			{
				path = Path.Combine(
					templateFolderPath, 
					String.Format(_templateFileNameFormat, templateName, culture, templateExtension));

				if (!File.Exists(path))
				{
					paths	= String.Format("[{0}]", path);
					path	= null;
				}
			}

			// Check language-only version if it exists and it's different from the specified culture.
			if (String.IsNullOrEmpty(path) &&  !String.IsNullOrEmpty(langCode))
			{
				path = Path.Combine(
					templateFolderPath, 
					String.Format(_templateFileNameFormat, templateName, langCode, templateExtension));

				if (!File.Exists(path))
				{
					paths	+= String.Format("[{0}]", path);
					path	 = null;
				}
			}

			// Check default culture version.
			if (String.IsNullOrEmpty(path) && 
				!String.Equals(culture, _defaultCulture, StringComparison.InvariantCultureIgnoreCase) && 
				!String.Equals(langCode, _defaultCulture, StringComparison.InvariantCultureIgnoreCase))
			{
				path = Path.Combine(
					templateFolderPath, 
					String.Format(_templateFileNameFormat, templateName, _defaultCulture, templateExtension));

				if (!File.Exists(path))
				{
					paths	+= String.Format("[{0}]", path);
					path	 = null;
				}
			}

			// Check the culture-independent version.
			if (String.IsNullOrEmpty(path))
			{
				path = Path.Combine(
					templateFolderPath,
					String.Format("{0}{1}", templateName, templateExtension));

				if (!String.IsNullOrEmpty(path))
				{
					if (!File.Exists(path))
					{
						paths	+= String.Format("[{0}]", path);
						path	 = null;
					}
				}
			}

			if (String.IsNullOrEmpty(path))
				throw new FileNotFoundException(
					"Could not find the specified template file. Probed locations: " + paths);

			if (UseCache)
			{
				if (!_cache.Contains(cachedItemName))
					_cache.Set(cachedItemName, path, CacheItemPolicy);
			}

			return path;
		}

		/// <summary>
		/// Gets the cached name of template.
		/// </summary>
		/// <param name="templatePath">
		/// The template file path.
		/// </param>
		/// <returns>
		/// The cached name of template.
		/// </returns>
		private string GetCachedTemplateName
		(
			string templatePath
		)
		{
			return String.Format(
				"{0}:BodyTemplate:{1}",
				this.GetType().ToString(),
				templatePath);
		}

		/// <summary>
		/// Gets the cached template file path.
		/// </summary>
		/// <param name="templateFolderPath">
		/// The path to the template folder.
		/// </param>
		/// <param name="templateName">
		/// The name of the template.
		/// </param>
		/// <param name="templateExtension">
		/// The template file extension.
		/// </param>
		/// <param name="culture">
		/// The template culture.
		/// </param>
		/// <returns>
		/// The cached template file path.
		/// </returns>
		private string GetCachedTemplatePathName
		(
			string templateFolderPath,
			string templateName,
			string templateExtension	= null,
			string culture				= null
		)
		{
			return String.Format(
				"{0}:TemplatePath:{1}:{2}:{3}:{4}",
				this.GetType().ToString(),
				templateFolderPath,
				templateName,
				templateExtension ?? "",
				culture ?? "");
		}

		#pragma warning disable 1573 
		/// <inheritdoc cref="MailTemplate(bool)" select="param"/> 
		/// <summary>
		/// Initializes the properties of a new class instance.
		/// </summary>
		private void Initialize
		(
			bool isBodyHtml
		)
		{
			_escaped		= false;
			AutoEscape		= true;
			BodyTemplate	= null;
			NormalizeSubject= true;
			UseCache		= true;
			IsBodyHtml		= isBodyHtml;
			CacheItemPolicy = new CacheItemPolicy();
		}
		#pragma warning restore 1573
		#endregion
	}
}
