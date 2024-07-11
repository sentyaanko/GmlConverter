using System.Xml;

namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// Xml を読み込むためのヘルパークラス
	/// </summary>
	internal class XmlHolder
	{
		/// <summary>
		/// Xml の情報を保持するクラス
		/// </summary>
		internal XmlDocument Document;

		/// <summary>
		/// Xml 内の namespace を管理するクラス
		/// </summary>
		internal XmlNamespaceManager NamespaceManager;

		/// <summary>
		/// デフォルトネームスペースを指定するための文字列
		/// </summary>
		internal string DefaultXmlNamespace;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="document"></param>
		/// <param name="namespaceManager"></param>
		/// <param name="defaultXmlNamespace"></param>
		internal XmlHolder(XmlDocument document, XmlNamespaceManager namespaceManager, string defaultXmlNamespace)
		{
			Document = document;
			NamespaceManager = namespaceManager;
			DefaultXmlNamespace = defaultXmlNamespace;
		}

		/// <summary>
		/// Xml 内のノードのテキストを取得するための関数。
		/// </summary>
		/// <param name="format">ノード名を示す xpath</param>
		/// <returns>指定されたノードに設定されているテキスト</returns>
		internal string? GetSingleNodeInnerText(string format) =>
			Document.SelectSingleNode(string.Format(format, DefaultXmlNamespace), NamespaceManager)?.InnerText;
	}
}
