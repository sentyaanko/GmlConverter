using System.Xml.Schema;
using System.Xml;
using System.Xml.XPath;

namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// XML ファイルを読み込むためのヘルパークラスです。
	/// </summary>
	internal static class XmlHelpers
	{
		/// <summary>
		/// xsd ファイルによるバリデーションチェックを行うかを示す設定です。
		/// </summary>
		private static bool s_isCheckValidate = false;

		/// <summary>
		/// 読み込んだ xsd ファイル
		/// </summary>
		private static XmlSchemaSet? s_schemaSet = null;

		/// <summary>
		/// デフォルトネームスペース
		/// </summary>
		private static string s_defaultNamespace = "ns";

		/// <summary>
		/// xsd ファイルをロードし、 s_schemaSet に設定します。
		/// </summary>
		/// <param name="xsdPath">xsd ファイルのパス</param>
		/// <returns></returns>
		internal static XmlSchemaSet LoadSchema(string xsdPath)
		{
			XmlUrlResolver resolver = new();
			resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

			XmlSchemaSet result = new();
			result.XmlResolver = resolver;

			result.Add(null, xsdPath);
			result.Compile();
			return result;
		}

		/// <summary>
		/// xml ファイルを読み込みます。
		/// </summary>
		/// <param name="xsdPath">xsd ファイルのパス</param>
		/// <param name="xmlPath">xml ファイルのパス</param>
		/// <returns></returns>
		internal static XmlHolder? LoadXml(string xsdPath, string xmlPath)
		{
			XmlDocument doc = new();
			doc.Load(xmlPath);

			//チェックする場合はする。
			if (s_isCheckValidate)
			{
				if (s_schemaSet == null)
				{
					s_schemaSet = LoadSchema(xsdPath);
				}
				doc.Schemas = s_schemaSet;
				ValidationEventHandler eventHandler = new(ValidationEventHandler);
				doc.Validate(eventHandler);
			}
			//Dataset 要素があるか
			if (doc.DocumentElement == null)
			{
				return null;
			}

			//xml 内の xmlns から XmlNamespaceManager の作成
			XPathNavigator? docNavi = doc.DocumentElement.CreateNavigator();
			if (docNavi == null)
			{
				return null;
			}
			var defaultxmlns = s_defaultNamespace;
			XmlNamespaceManager nsmgr = new(doc.NameTable);
			var scopes = docNavi.GetNamespacesInScope(XmlNamespaceScope.All);
			foreach (var scope in scopes)
			{
				switch (scope.Key)
				{
				case "xml":
					break;
				case "":
					//nsmgr.AddNamespace(defaultxmlns, doc.DocumentElement.NamespaceURI);
					nsmgr.AddNamespace(defaultxmlns, scope.Value);
					break;
				default:
					nsmgr.AddNamespace(scope.Key, scope.Value);
					break;
				}
			}
			return new(doc, nsmgr, defaultxmlns);
		}

		/// <summary>
		/// XmlDocument.Validate() に渡すバリデーションのコールバックです。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void ValidationEventHandler(object? sender, ValidationEventArgs e)
		{
			switch (e.Severity)
			{
			case XmlSeverityType.Error:
				//errorInfoList.Add(e.Message);

				Console.WriteLine($"Error:{e.Message}");
				break;
			case XmlSeverityType.Warning:
				Console.WriteLine($"Warning:{e.Message}");
				break;
			}
		}
	}
}
