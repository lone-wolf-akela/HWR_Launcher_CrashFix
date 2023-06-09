using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Xml;

namespace HTMLConverter
{
	public static class HtmlToXamlConverter
	{
		public const string Xaml_FlowDocument = "FlowDocument";

		public const string Xaml_Run = "Run";

		public const string Xaml_Span = "Span";

		public const string Xaml_Hyperlink = "Hyperlink";

		public const string Xaml_Hyperlink_NavigateUri = "NavigateUri";

		public const string Xaml_Hyperlink_TargetName = "TargetName";

		public const string Xaml_Section = "Section";

		public const string Xaml_List = "List";

		public const string Xaml_List_MarkerStyle = "MarkerStyle";

		public const string Xaml_List_MarkerStyle_None = "None";

		public const string Xaml_List_MarkerStyle_Decimal = "Decimal";

		public const string Xaml_List_MarkerStyle_Disc = "Disc";

		public const string Xaml_List_MarkerStyle_Circle = "Circle";

		public const string Xaml_List_MarkerStyle_Square = "Square";

		public const string Xaml_List_MarkerStyle_Box = "Box";

		public const string Xaml_List_MarkerStyle_LowerLatin = "LowerLatin";

		public const string Xaml_List_MarkerStyle_UpperLatin = "UpperLatin";

		public const string Xaml_List_MarkerStyle_LowerRoman = "LowerRoman";

		public const string Xaml_List_MarkerStyle_UpperRoman = "UpperRoman";

		public const string Xaml_ListItem = "ListItem";

		public const string Xaml_LineBreak = "LineBreak";

		public const string Xaml_Paragraph = "Paragraph";

		public const string Xaml_Margin = "Margin";

		public const string Xaml_Padding = "Padding";

		public const string Xaml_BorderBrush = "BorderBrush";

		public const string Xaml_BorderThickness = "BorderThickness";

		public const string Xaml_Table = "Table";

		public const string Xaml_TableColumn = "TableColumn";

		public const string Xaml_TableRowGroup = "TableRowGroup";

		public const string Xaml_TableRow = "TableRow";

		public const string Xaml_TableCell = "TableCell";

		public const string Xaml_TableCell_BorderThickness = "BorderThickness";

		public const string Xaml_TableCell_BorderBrush = "BorderBrush";

		public const string Xaml_TableCell_ColumnSpan = "ColumnSpan";

		public const string Xaml_TableCell_RowSpan = "RowSpan";

		public const string Xaml_Width = "Width";

		public const string Xaml_Brushes_Black = "Black";

		public const string Xaml_FontFamily = "FontFamily";

		public const string Xaml_FontSize = "FontSize";

		public const string Xaml_FontSize_XXLarge = "22pt";

		public const string Xaml_FontSize_XLarge = "20pt";

		public const string Xaml_FontSize_Large = "18pt";

		public const string Xaml_FontSize_Medium = "16pt";

		public const string Xaml_FontSize_Small = "12pt";

		public const string Xaml_FontSize_XSmall = "10pt";

		public const string Xaml_FontSize_XXSmall = "8pt";

		public const string Xaml_FontWeight = "FontWeight";

		public const string Xaml_FontWeight_Bold = "Bold";

		public const string Xaml_FontStyle = "FontStyle";

		public const string Xaml_Foreground = "Foreground";

		public const string Xaml_Background = "Background";

		public const string Xaml_TextDecorations = "TextDecorations";

		public const string Xaml_TextDecorations_Underline = "Underline";

		public const string Xaml_TextIndent = "TextIndent";

		public const string Xaml_TextAlignment = "TextAlignment";

		private static XmlElement InlineFragmentParentElement;

		private static string _xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

		public static string ConvertHtmlToXaml(string htmlString, bool asFlowDocument)
		{
			XmlElement htmlElement = HtmlParser.ParseHtml(htmlString);
			string rootElementName = (asFlowDocument ? "FlowDocument" : "Section");
			XmlDocument xamlTree = new XmlDocument();
			XmlElement xamlFlowDocumentElement = xamlTree.CreateElement(null, rootElementName, _xamlNamespace);
			CssStylesheet stylesheet = new CssStylesheet(htmlElement);
			List<XmlElement> sourceContext = new List<XmlElement>(10);
			InlineFragmentParentElement = null;
			AddBlock(xamlFlowDocumentElement, htmlElement, new Hashtable(), stylesheet, sourceContext);
			if (!asFlowDocument)
			{
				xamlFlowDocumentElement = ExtractInlineFragment(xamlFlowDocumentElement);
			}
			xamlFlowDocumentElement.SetAttribute("xml:space", "preserve");
			return xamlFlowDocumentElement.OuterXml;
		}

		public static string GetAttribute(XmlElement element, string attributeName)
		{
			attributeName = attributeName.ToLower();
			for (int i = 0; i < element.Attributes.Count; i = checked(i + 1))
			{
				if (element.Attributes[i].Name.ToLower() == attributeName)
				{
					return element.Attributes[i].Value;
				}
			}
			return null;
		}

		internal static string UnQuote(string value)
		{
			if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
			{
				value = value.Substring(1, checked(value.Length - 2)).Trim();
			}
			return value;
		}

		private static XmlNode AddBlock(XmlElement xamlParentElement, XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			if (htmlNode is XmlComment)
			{
				DefineInlineFragmentParent((XmlComment)htmlNode, null);
			}
			else if (htmlNode is XmlText)
			{
				htmlNode = AddImplicitParagraph(xamlParentElement, htmlNode, inheritedProperties, stylesheet, sourceContext);
			}
			else if (htmlNode is XmlElement)
			{
				XmlElement htmlElement = (XmlElement)htmlNode;
				string htmlElementName = htmlElement.LocalName;
				string htmlElementNamespace = htmlElement.NamespaceURI;
				if (htmlElementNamespace != "http://www.w3.org/1999/xhtml")
				{
					return htmlElement;
				}
				sourceContext.Add(htmlElement);
				switch (htmlElementName.ToLower())
				{
				case "html":
				case "body":
				case "div":
				case "form":
				case "pre":
				case "blockquote":
				case "caption":
				case "center":
				case "cite":
					AddSection(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "p":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "nsrtitle":
				case "textarea":
				case "dd":
				case "dl":
				case "dt":
				case "tt":
					AddParagraph(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "ol":
				case "ul":
				case "dir":
				case "menu":
					AddList(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "li":
					htmlNode = AddOrphanListItems(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "img":
					AddImage(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "table":
					AddTable(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				default:
					htmlNode = AddImplicitParagraph(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "style":
				case "meta":
				case "head":
				case "title":
				case "script":
					break;
				}
				sourceContext.RemoveAt(checked(sourceContext.Count - 1));
			}
			return htmlNode;
		}

		private static void AddBreak(XmlElement xamlParentElement, string htmlElementName)
		{
			XmlElement xamlLineBreak = xamlParentElement.OwnerDocument.CreateElement(null, "LineBreak", _xamlNamespace);
			xamlParentElement.AppendChild(xamlLineBreak);
			if (htmlElementName == "hr")
			{
				XmlText xamlHorizontalLine = xamlParentElement.OwnerDocument.CreateTextNode("----------------------");
				xamlParentElement.AppendChild(xamlHorizontalLine);
				xamlLineBreak = xamlParentElement.OwnerDocument.CreateElement(null, "LineBreak", _xamlNamespace);
				xamlParentElement.AppendChild(xamlLineBreak);
			}
		}

		private static void AddSection(XmlElement xamlParentElement, XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			bool htmlElementContainsBlocks = false;
			for (XmlNode htmlChildNode2 = htmlElement.FirstChild; htmlChildNode2 != null; htmlChildNode2 = htmlChildNode2.NextSibling)
			{
				if (htmlChildNode2 is XmlElement)
				{
					string htmlChildName = ((XmlElement)htmlChildNode2).LocalName.ToLower();
					if (HtmlSchema.IsBlockElement(htmlChildName))
					{
						htmlElementContainsBlocks = true;
						break;
					}
				}
			}
			if (!htmlElementContainsBlocks)
			{
				AddParagraph(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
				return;
			}
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement xamlElement = xamlParentElement.OwnerDocument.CreateElement(null, "Section", _xamlNamespace);
			ApplyLocalProperties(xamlElement, localProperties, true);
			if (!xamlElement.HasAttributes)
			{
				xamlElement = xamlParentElement;
			}
			XmlNode htmlChildNode;
			for (htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = ((htmlChildNode != null) ? htmlChildNode.NextSibling : null))
			{
				htmlChildNode = AddBlock(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);
			}
			if (xamlElement != xamlParentElement)
			{
				xamlParentElement.AppendChild(xamlElement);
			}
		}

		private static void AddParagraph(XmlElement xamlParentElement, XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement xamlElement = xamlParentElement.OwnerDocument.CreateElement(null, "Paragraph", _xamlNamespace);
			ApplyLocalProperties(xamlElement, localProperties, true);
			for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
			{
				AddInline(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);
			}
			xamlParentElement.AppendChild(xamlElement);
		}

		private static XmlNode AddImplicitParagraph(XmlElement xamlParentElement, XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			XmlElement xamlParagraph = xamlParentElement.OwnerDocument.CreateElement(null, "Paragraph", _xamlNamespace);
			XmlNode lastNodeProcessed = null;
			while (htmlNode != null)
			{
				if (htmlNode is XmlComment)
				{
					DefineInlineFragmentParent((XmlComment)htmlNode, null);
				}
				else if (htmlNode is XmlText)
				{
					if (htmlNode.Value.Trim().Length > 0)
					{
						AddTextRun(xamlParagraph, htmlNode.Value);
					}
				}
				else if (htmlNode is XmlElement)
				{
					string htmlChildName = ((XmlElement)htmlNode).LocalName.ToLower();
					if (HtmlSchema.IsBlockElement(htmlChildName))
					{
						break;
					}
					AddInline(xamlParagraph, (XmlElement)htmlNode, inheritedProperties, stylesheet, sourceContext);
				}
				lastNodeProcessed = htmlNode;
				htmlNode = htmlNode.NextSibling;
			}
			if (xamlParagraph.FirstChild != null)
			{
				xamlParentElement.AppendChild(xamlParagraph);
			}
			return lastNodeProcessed;
		}

		private static void AddInline(XmlElement xamlParentElement, XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			if (htmlNode is XmlComment)
			{
				DefineInlineFragmentParent((XmlComment)htmlNode, xamlParentElement);
			}
			else if (htmlNode is XmlText)
			{
				AddTextRun(xamlParentElement, htmlNode.Value);
			}
			else
			{
				if (!(htmlNode is XmlElement))
				{
					return;
				}
				XmlElement htmlElement = (XmlElement)htmlNode;
				if (htmlElement.NamespaceURI != "http://www.w3.org/1999/xhtml")
				{
					return;
				}
				string htmlElementName = htmlElement.LocalName.ToLower();
				sourceContext.Add(htmlElement);
				switch (htmlElementName)
				{
				case "a":
					AddHyperlink(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "img":
					AddImage(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					break;
				case "br":
				case "hr":
					AddBreak(xamlParentElement, htmlElementName);
					break;
				default:
					if (HtmlSchema.IsInlineElement(htmlElementName) || HtmlSchema.IsBlockElement(htmlElementName))
					{
						AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
					}
					break;
				}
				sourceContext.RemoveAt(checked(sourceContext.Count - 1));
			}
		}

		private static void AddSpanOrRun(XmlElement xamlParentElement, XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			bool elementHasChildren = false;
			for (XmlNode htmlNode = htmlElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
			{
				if (!(htmlNode is XmlElement))
				{
					continue;
				}
				string htmlChildName = ((XmlElement)htmlNode).LocalName.ToLower();
				if (!HtmlSchema.IsInlineElement(htmlChildName) && !HtmlSchema.IsBlockElement(htmlChildName))
				{
					switch (htmlChildName)
					{
					case "img":
					case "br":
					case "hr":
						break;
					default:
						continue;
					}
				}
				elementHasChildren = true;
				break;
			}
			string xamlElementName = (elementHasChildren ? "Span" : "Run");
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement xamlElement = xamlParentElement.OwnerDocument.CreateElement(null, xamlElementName, _xamlNamespace);
			ApplyLocalProperties(xamlElement, localProperties, false);
			for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
			{
				AddInline(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);
			}
			xamlParentElement.AppendChild(xamlElement);
		}

		private static void AddTextRun(XmlElement xamlElement, string textData)
		{
			checked
			{
				for (int i = 0; i < textData.Length; i++)
				{
					if (char.IsControl(textData[i]))
					{
						textData = textData.Remove(i--, 1);
					}
				}
				textData = textData.Replace('\u00a0', ' ');
				if (textData.Length > 0)
				{
					xamlElement.AppendChild(xamlElement.OwnerDocument.CreateTextNode(textData));
				}
			}
		}

		private static void AddHyperlink(XmlElement xamlParentElement, XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			string href = GetAttribute(htmlElement, "href");
			if (href == null)
			{
				AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
				return;
			}
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement xamlElement = xamlParentElement.OwnerDocument.CreateElement(null, "Hyperlink", _xamlNamespace);
			ApplyLocalProperties(xamlElement, localProperties, false);
			string[] hrefParts = href.Split('#');
			if (hrefParts.Length > 0 && hrefParts[0].Trim().Length > 0)
			{
				xamlElement.SetAttribute("NavigateUri", hrefParts[0].Trim());
			}
			if (hrefParts.Length == 2 && hrefParts[1].Trim().Length > 0)
			{
				xamlElement.SetAttribute("TargetName", hrefParts[1].Trim());
			}
			for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
			{
				AddInline(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);
			}
			xamlParentElement.AppendChild(xamlElement);
		}

		private static void DefineInlineFragmentParent(XmlComment htmlComment, XmlElement xamlParentElement)
		{
			if (htmlComment.Value == "StartFragment")
			{
				InlineFragmentParentElement = xamlParentElement;
			}
			else if (htmlComment.Value == "EndFragment" && InlineFragmentParentElement == null && xamlParentElement != null)
			{
				InlineFragmentParentElement = xamlParentElement;
			}
		}

		private static XmlElement ExtractInlineFragment(XmlElement xamlFlowDocumentElement)
		{
			if (InlineFragmentParentElement != null)
			{
				if (InlineFragmentParentElement.LocalName == "Span")
				{
					xamlFlowDocumentElement = InlineFragmentParentElement;
				}
				else
				{
					xamlFlowDocumentElement = xamlFlowDocumentElement.OwnerDocument.CreateElement(null, "Span", _xamlNamespace);
					while (InlineFragmentParentElement.FirstChild != null)
					{
						XmlNode copyNode = InlineFragmentParentElement.FirstChild;
						InlineFragmentParentElement.RemoveChild(copyNode);
						xamlFlowDocumentElement.AppendChild(copyNode);
					}
				}
			}
			return xamlFlowDocumentElement;
		}

		private static void AddImage(XmlElement xamlParentElement, XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
		}

		private static void AddList(XmlElement xamlParentElement, XmlElement htmlListElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			string htmlListElementName = htmlListElement.LocalName.ToLower();
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlListElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement xamlListElement = xamlParentElement.OwnerDocument.CreateElement(null, "List", _xamlNamespace);
			if (htmlListElementName == "ol")
			{
				xamlListElement.SetAttribute("MarkerStyle", "Decimal");
			}
			else
			{
				xamlListElement.SetAttribute("MarkerStyle", "Disc");
			}
			ApplyLocalProperties(xamlListElement, localProperties, true);
			for (XmlNode htmlChildNode = htmlListElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
			{
				if (htmlChildNode is XmlElement && htmlChildNode.LocalName.ToLower() == "li")
				{
					sourceContext.Add((XmlElement)htmlChildNode);
					AddListItem(xamlListElement, (XmlElement)htmlChildNode, currentProperties, stylesheet, sourceContext);
					sourceContext.RemoveAt(checked(sourceContext.Count - 1));
				}
			}
			if (xamlListElement.HasChildNodes)
			{
				xamlParentElement.AppendChild(xamlListElement);
			}
		}

		private static XmlElement AddOrphanListItems(XmlElement xamlParentElement, XmlElement htmlLIElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			XmlElement lastProcessedListItemElement = null;
			XmlNode xamlListItemElementPreviousSibling = xamlParentElement.LastChild;
			XmlElement xamlListElement;
			if (xamlListItemElementPreviousSibling != null && xamlListItemElementPreviousSibling.LocalName == "List")
			{
				xamlListElement = (XmlElement)xamlListItemElementPreviousSibling;
			}
			else
			{
				xamlListElement = xamlParentElement.OwnerDocument.CreateElement(null, "List", _xamlNamespace);
				xamlParentElement.AppendChild(xamlListElement);
			}
			XmlNode htmlChildNode = htmlLIElement;
			string htmlChildNodeName = ((htmlChildNode == null) ? null : htmlChildNode.LocalName.ToLower());
			while (htmlChildNode != null && htmlChildNodeName == "li")
			{
				AddListItem(xamlListElement, (XmlElement)htmlChildNode, inheritedProperties, stylesheet, sourceContext);
				lastProcessedListItemElement = (XmlElement)htmlChildNode;
				htmlChildNode = htmlChildNode.NextSibling;
				htmlChildNodeName = ((htmlChildNode == null) ? null : htmlChildNode.LocalName.ToLower());
			}
			return lastProcessedListItemElement;
		}

		private static void AddListItem(XmlElement xamlListElement, XmlElement htmlLIElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlLIElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement xamlListItemElement = xamlListElement.OwnerDocument.CreateElement(null, "ListItem", _xamlNamespace);
			XmlNode htmlChildNode;
			for (htmlChildNode = htmlLIElement.FirstChild; htmlChildNode != null; htmlChildNode = ((htmlChildNode != null) ? htmlChildNode.NextSibling : null))
			{
				htmlChildNode = AddBlock(xamlListItemElement, htmlChildNode, currentProperties, stylesheet, sourceContext);
			}
			xamlListElement.AppendChild(xamlListItemElement);
		}

		private static void AddTable(XmlElement xamlParentElement, XmlElement htmlTableElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlTableElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement singleCell = GetCellFromSingleCellTable(htmlTableElement);
			checked
			{
				if (singleCell != null)
				{
					sourceContext.Add(singleCell);
					XmlNode htmlChildNode2;
					for (htmlChildNode2 = singleCell.FirstChild; htmlChildNode2 != null; htmlChildNode2 = ((htmlChildNode2 != null) ? htmlChildNode2.NextSibling : null))
					{
						htmlChildNode2 = AddBlock(xamlParentElement, htmlChildNode2, currentProperties, stylesheet, sourceContext);
					}
					sourceContext.RemoveAt(sourceContext.Count - 1);
					return;
				}
				XmlElement xamlTableElement = xamlParentElement.OwnerDocument.CreateElement(null, "Table", _xamlNamespace);
				ArrayList columnStarts = AnalyzeTableStructure(htmlTableElement, stylesheet);
				AddColumnInformation(htmlTableElement, xamlTableElement, columnStarts, currentProperties, stylesheet, sourceContext);
				XmlNode htmlChildNode = htmlTableElement.FirstChild;
				while (htmlChildNode != null)
				{
					switch (htmlChildNode.LocalName.ToLower())
					{
					case "tbody":
					case "thead":
					case "tfoot":
					{
						XmlElement xamlTableBodyElement = xamlTableElement.OwnerDocument.CreateElement(null, "TableRowGroup", _xamlNamespace);
						xamlTableElement.AppendChild(xamlTableBodyElement);
						sourceContext.Add((XmlElement)htmlChildNode);
						Hashtable tbodyElementLocalProperties;
						Hashtable tbodyElementCurrentProperties = GetElementProperties((XmlElement)htmlChildNode, currentProperties, out tbodyElementLocalProperties, stylesheet, sourceContext);
						AddTableRowsToTableBody(xamlTableBodyElement, htmlChildNode.FirstChild, tbodyElementCurrentProperties, columnStarts, stylesheet, sourceContext);
						if (xamlTableBodyElement.HasChildNodes)
						{
							xamlTableElement.AppendChild(xamlTableBodyElement);
						}
						sourceContext.RemoveAt(sourceContext.Count - 1);
						htmlChildNode = htmlChildNode.NextSibling;
						break;
					}
					case "tr":
					{
						XmlElement xamlTableBodyElement2 = xamlTableElement.OwnerDocument.CreateElement(null, "TableRowGroup", _xamlNamespace);
						htmlChildNode = AddTableRowsToTableBody(xamlTableBodyElement2, htmlChildNode, currentProperties, columnStarts, stylesheet, sourceContext);
						if (xamlTableBodyElement2.HasChildNodes)
						{
							xamlTableElement.AppendChild(xamlTableBodyElement2);
						}
						break;
					}
					default:
						htmlChildNode = htmlChildNode.NextSibling;
						break;
					}
				}
				if (xamlTableElement.HasChildNodes)
				{
					xamlParentElement.AppendChild(xamlTableElement);
				}
			}
		}

		private static XmlElement GetCellFromSingleCellTable(XmlElement htmlTableElement)
		{
			XmlElement singleCell = null;
			for (XmlNode tableChild = htmlTableElement.FirstChild; tableChild != null; tableChild = tableChild.NextSibling)
			{
				switch (tableChild.LocalName.ToLower())
				{
				case "tbody":
				case "thead":
				case "tfoot":
				{
					if (singleCell != null)
					{
						return null;
					}
					for (XmlNode tbodyChild = tableChild.FirstChild; tbodyChild != null; tbodyChild = tbodyChild.NextSibling)
					{
						if (tbodyChild.LocalName.ToLower() == "tr")
						{
							if (singleCell != null)
							{
								return null;
							}
							for (XmlNode trChild = tbodyChild.FirstChild; trChild != null; trChild = trChild.NextSibling)
							{
								string cellName = trChild.LocalName.ToLower();
								if (cellName == "td" || cellName == "th")
								{
									if (singleCell != null)
									{
										return null;
									}
									singleCell = (XmlElement)trChild;
								}
							}
						}
					}
					break;
				}
				default:
				{
					if (!(tableChild.LocalName.ToLower() == "tr"))
					{
						break;
					}
					if (singleCell != null)
					{
						return null;
					}
					for (XmlNode trChild2 = tableChild.FirstChild; trChild2 != null; trChild2 = trChild2.NextSibling)
					{
						string cellName2 = trChild2.LocalName.ToLower();
						if (cellName2 == "td" || cellName2 == "th")
						{
							if (singleCell != null)
							{
								return null;
							}
							singleCell = (XmlElement)trChild2;
						}
					}
					break;
				}
				}
			}
			return singleCell;
		}

		private static void AddColumnInformation(XmlElement htmlTableElement, XmlElement xamlTableElement, ArrayList columnStartsAllRows, Hashtable currentProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			checked
			{
				if (columnStartsAllRows != null)
				{
					for (int columnIndex = 0; columnIndex < columnStartsAllRows.Count - 1; columnIndex++)
					{
						XmlElement xamlColumnElement = xamlTableElement.OwnerDocument.CreateElement(null, "TableColumn", _xamlNamespace);
						xamlColumnElement.SetAttribute("Width", ((double)columnStartsAllRows[columnIndex + 1] - (double)columnStartsAllRows[columnIndex]).ToString());
						xamlTableElement.AppendChild(xamlColumnElement);
					}
					return;
				}
				for (XmlNode htmlChildNode = htmlTableElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
				{
					if (htmlChildNode.LocalName.ToLower() == "colgroup")
					{
						AddTableColumnGroup(xamlTableElement, (XmlElement)htmlChildNode, currentProperties, stylesheet, sourceContext);
					}
					else if (htmlChildNode.LocalName.ToLower() == "col")
					{
						AddTableColumn(xamlTableElement, (XmlElement)htmlChildNode, currentProperties, stylesheet, sourceContext);
					}
					else if (htmlChildNode is XmlElement)
					{
						break;
					}
				}
			}
		}

		private static void AddTableColumnGroup(XmlElement xamlTableElement, XmlElement htmlColgroupElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			Hashtable localProperties;
			Hashtable currentProperties = GetElementProperties(htmlColgroupElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			for (XmlNode htmlNode = htmlColgroupElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
			{
				if (htmlNode is XmlElement && htmlNode.LocalName.ToLower() == "col")
				{
					AddTableColumn(xamlTableElement, (XmlElement)htmlNode, currentProperties, stylesheet, sourceContext);
				}
			}
		}

		private static void AddTableColumn(XmlElement xamlTableElement, XmlElement htmlColElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			Hashtable localProperties;
			GetElementProperties(htmlColElement, inheritedProperties, out localProperties, stylesheet, sourceContext);
			XmlElement xamlTableColumnElement = xamlTableElement.OwnerDocument.CreateElement(null, "TableColumn", _xamlNamespace);
			xamlTableElement.AppendChild(xamlTableColumnElement);
		}

		private static XmlNode AddTableRowsToTableBody(XmlElement xamlTableBodyElement, XmlNode htmlTRStartNode, Hashtable currentProperties, ArrayList columnStarts, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			XmlNode htmlChildNode = htmlTRStartNode;
			ArrayList activeRowSpans = null;
			if (columnStarts != null)
			{
				activeRowSpans = new ArrayList();
				InitializeActiveRowSpans(activeRowSpans, columnStarts.Count);
			}
			while (htmlChildNode != null && htmlChildNode.LocalName.ToLower() != "tbody")
			{
				if (htmlChildNode.LocalName.ToLower() == "tr")
				{
					XmlElement xamlTableRowElement2 = xamlTableBodyElement.OwnerDocument.CreateElement(null, "TableRow", _xamlNamespace);
					sourceContext.Add((XmlElement)htmlChildNode);
					Hashtable trElementLocalProperties;
					Hashtable trElementCurrentProperties = GetElementProperties((XmlElement)htmlChildNode, currentProperties, out trElementLocalProperties, stylesheet, sourceContext);
					AddTableCellsToTableRow(xamlTableRowElement2, htmlChildNode.FirstChild, trElementCurrentProperties, columnStarts, activeRowSpans, stylesheet, sourceContext);
					if (xamlTableRowElement2.HasChildNodes)
					{
						xamlTableBodyElement.AppendChild(xamlTableRowElement2);
					}
					sourceContext.RemoveAt(checked(sourceContext.Count - 1));
					htmlChildNode = htmlChildNode.NextSibling;
				}
				else if (htmlChildNode.LocalName.ToLower() == "td")
				{
					XmlElement xamlTableRowElement = xamlTableBodyElement.OwnerDocument.CreateElement(null, "TableRow", _xamlNamespace);
					htmlChildNode = AddTableCellsToTableRow(xamlTableRowElement, htmlChildNode, currentProperties, columnStarts, activeRowSpans, stylesheet, sourceContext);
					if (xamlTableRowElement.HasChildNodes)
					{
						xamlTableBodyElement.AppendChild(xamlTableRowElement);
					}
				}
				else
				{
					htmlChildNode = htmlChildNode.NextSibling;
				}
			}
			return htmlChildNode;
		}

		private static XmlNode AddTableCellsToTableRow(XmlElement xamlTableRowElement, XmlNode htmlTDStartNode, Hashtable currentProperties, ArrayList columnStarts, ArrayList activeRowSpans, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			XmlNode htmlChildNode = htmlTDStartNode;
			double columnWidth = 0.0;
			int columnIndex = 0;
			int columnSpan = 0;
			checked
			{
				while (htmlChildNode != null && htmlChildNode.LocalName.ToLower() != "tr" && htmlChildNode.LocalName.ToLower() != "tbody" && htmlChildNode.LocalName.ToLower() != "thead" && htmlChildNode.LocalName.ToLower() != "tfoot")
				{
					if (htmlChildNode.LocalName.ToLower() == "td" || htmlChildNode.LocalName.ToLower() == "th")
					{
						XmlElement xamlTableCellElement = xamlTableRowElement.OwnerDocument.CreateElement(null, "TableCell", _xamlNamespace);
						sourceContext.Add((XmlElement)htmlChildNode);
						Hashtable tdElementLocalProperties;
						Hashtable tdElementCurrentProperties = GetElementProperties((XmlElement)htmlChildNode, currentProperties, out tdElementLocalProperties, stylesheet, sourceContext);
						ApplyPropertiesToTableCellElement((XmlElement)htmlChildNode, xamlTableCellElement);
						if (columnStarts != null)
						{
							for (; columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0; columnIndex++)
							{
								activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
							}
							double num = (double)columnStarts[columnIndex];
							columnWidth = GetColumnWidth((XmlElement)htmlChildNode);
							columnSpan = CalculateColumnSpan(columnIndex, columnWidth, columnStarts);
							int rowSpan = GetRowSpan((XmlElement)htmlChildNode);
							xamlTableCellElement.SetAttribute("ColumnSpan", columnSpan.ToString());
							for (int spannedColumnIndex = columnIndex; spannedColumnIndex < columnIndex + columnSpan; spannedColumnIndex++)
							{
								activeRowSpans[spannedColumnIndex] = rowSpan - 1;
							}
							columnIndex += columnSpan;
						}
						AddDataToTableCell(xamlTableCellElement, htmlChildNode.FirstChild, tdElementCurrentProperties, stylesheet, sourceContext);
						if (xamlTableCellElement.HasChildNodes)
						{
							xamlTableRowElement.AppendChild(xamlTableCellElement);
						}
						sourceContext.RemoveAt(sourceContext.Count - 1);
						htmlChildNode = htmlChildNode.NextSibling;
					}
					else
					{
						htmlChildNode = htmlChildNode.NextSibling;
					}
				}
				return htmlChildNode;
			}
		}

		private static void AddDataToTableCell(XmlElement xamlTableCellElement, XmlNode htmlDataStartNode, Hashtable currentProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			XmlNode htmlChildNode;
			for (htmlChildNode = htmlDataStartNode; htmlChildNode != null; htmlChildNode = ((htmlChildNode != null) ? htmlChildNode.NextSibling : null))
			{
				htmlChildNode = AddBlock(xamlTableCellElement, htmlChildNode, currentProperties, stylesheet, sourceContext);
			}
		}

		private static ArrayList AnalyzeTableStructure(XmlElement htmlTableElement, CssStylesheet stylesheet)
		{
			if (!htmlTableElement.HasChildNodes)
			{
				return null;
			}
			bool columnWidthsAvailable = true;
			ArrayList columnStarts = new ArrayList();
			ArrayList activeRowSpans = new ArrayList();
			XmlNode htmlChildNode = htmlTableElement.FirstChild;
			double tableWidth = 0.0;
			while (htmlChildNode != null && columnWidthsAvailable)
			{
				switch (htmlChildNode.LocalName.ToLower())
				{
				case "tbody":
				{
					double tbodyWidth = AnalyzeTbodyStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans, tableWidth, stylesheet);
					if (tbodyWidth > tableWidth)
					{
						tableWidth = tbodyWidth;
					}
					else if (tbodyWidth == 0.0)
					{
						columnWidthsAvailable = false;
					}
					break;
				}
				case "tr":
				{
					double trWidth = AnalyzeTRStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans, tableWidth, stylesheet);
					if (trWidth > tableWidth)
					{
						tableWidth = trWidth;
					}
					else if (trWidth == 0.0)
					{
						columnWidthsAvailable = false;
					}
					break;
				}
				case "td":
					columnWidthsAvailable = false;
					break;
				}
				htmlChildNode = htmlChildNode.NextSibling;
			}
			if (columnWidthsAvailable)
			{
				columnStarts.Add(tableWidth);
				VerifyColumnStartsAscendingOrder(columnStarts);
			}
			else
			{
				columnStarts = null;
			}
			return columnStarts;
		}

		private static double AnalyzeTbodyStructure(XmlElement htmlTbodyElement, ArrayList columnStarts, ArrayList activeRowSpans, double tableWidth, CssStylesheet stylesheet)
		{
			double tbodyWidth = 0.0;
			bool columnWidthsAvailable = true;
			if (!htmlTbodyElement.HasChildNodes)
			{
				return tbodyWidth;
			}
			ClearActiveRowSpans(activeRowSpans);
			XmlNode htmlChildNode = htmlTbodyElement.FirstChild;
			while (htmlChildNode != null && columnWidthsAvailable)
			{
				switch (htmlChildNode.LocalName.ToLower())
				{
				case "tr":
				{
					double trWidth = AnalyzeTRStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans, tbodyWidth, stylesheet);
					if (trWidth > tbodyWidth)
					{
						tbodyWidth = trWidth;
					}
					break;
				}
				case "td":
					columnWidthsAvailable = false;
					break;
				}
				htmlChildNode = htmlChildNode.NextSibling;
			}
			ClearActiveRowSpans(activeRowSpans);
			if (!columnWidthsAvailable)
			{
				return 0.0;
			}
			return tbodyWidth;
		}

		private static double AnalyzeTRStructure(XmlElement htmlTRElement, ArrayList columnStarts, ArrayList activeRowSpans, double tableWidth, CssStylesheet stylesheet)
		{
			if (!htmlTRElement.HasChildNodes)
			{
				return 0.0;
			}
			bool columnWidthsAvailable = true;
			double columnStart = 0.0;
			XmlNode htmlChildNode = htmlTRElement.FirstChild;
			int columnIndex = 0;
			double trWidth = 0.0;
			checked
			{
				if (columnIndex < activeRowSpans.Count && (double)columnStarts[columnIndex] == columnStart)
				{
					while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
					{
						activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
						columnIndex++;
						columnStart = (double)columnStarts[columnIndex];
					}
				}
				while (htmlChildNode != null && columnWidthsAvailable)
				{
					VerifyColumnStartsAscendingOrder(columnStarts);
					string text;
					if ((text = htmlChildNode.LocalName.ToLower()) != null && text == "td")
					{
						if (columnIndex < columnStarts.Count)
						{
							if (columnStart < (double)columnStarts[columnIndex])
							{
								columnStarts.Insert(columnIndex, columnStart);
								activeRowSpans.Insert(columnIndex, 0);
							}
						}
						else
						{
							columnStarts.Add(columnStart);
							activeRowSpans.Add(0);
						}
						double columnWidth = GetColumnWidth((XmlElement)htmlChildNode);
						if (columnWidth != -1.0)
						{
							int rowSpan = GetRowSpan((XmlElement)htmlChildNode);
							int nextColumnIndex = GetNextColumnIndex(columnIndex, columnWidth, columnStarts, activeRowSpans);
							if (nextColumnIndex != -1)
							{
								for (int spannedColumnIndex = columnIndex; spannedColumnIndex < nextColumnIndex; spannedColumnIndex++)
								{
									activeRowSpans[spannedColumnIndex] = rowSpan - 1;
								}
								columnIndex = nextColumnIndex;
								columnStart += columnWidth;
								if (columnIndex < activeRowSpans.Count && (double)columnStarts[columnIndex] == columnStart)
								{
									while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
									{
										activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
										columnIndex++;
										columnStart = (double)columnStarts[columnIndex];
									}
								}
							}
							else
							{
								columnWidthsAvailable = false;
							}
						}
						else
						{
							columnWidthsAvailable = false;
						}
					}
					htmlChildNode = htmlChildNode.NextSibling;
				}
				if (columnWidthsAvailable)
				{
					return columnStart;
				}
				return 0.0;
			}
		}

		private static int GetRowSpan(XmlElement htmlTDElement)
		{
			string rowSpanAsString = GetAttribute(htmlTDElement, "rowspan");
			if (rowSpanAsString != null)
			{
				int rowSpan;
				if (!int.TryParse(rowSpanAsString, out rowSpan))
				{
					return 1;
				}
				return rowSpan;
			}
			return 1;
		}

		private static int GetNextColumnIndex(int columnIndex, double columnWidth, ArrayList columnStarts, ArrayList activeRowSpans)
		{
			double columnStart = (double)columnStarts[columnIndex];
			checked
			{
				int spannedColumnIndex = columnIndex + 1;
				while (spannedColumnIndex < columnStarts.Count && (double)columnStarts[spannedColumnIndex] < columnStart + columnWidth && spannedColumnIndex != -1)
				{
					spannedColumnIndex = (((int)activeRowSpans[spannedColumnIndex] <= 0) ? (spannedColumnIndex + 1) : (-1));
				}
				return spannedColumnIndex;
			}
		}

		private static void ClearActiveRowSpans(ArrayList activeRowSpans)
		{
			for (int columnIndex = 0; columnIndex < activeRowSpans.Count; columnIndex = checked(columnIndex + 1))
			{
				activeRowSpans[columnIndex] = 0;
			}
		}

		private static void InitializeActiveRowSpans(ArrayList activeRowSpans, int count)
		{
			for (int columnIndex = 0; columnIndex < count; columnIndex = checked(columnIndex + 1))
			{
				activeRowSpans.Add(0);
			}
		}

		private static double GetNextColumnStart(XmlElement htmlTDElement, double columnStart)
		{
			double nextColumnStart = -1.0;
			double columnWidth = GetColumnWidth(htmlTDElement);
			if (columnWidth == -1.0)
			{
				return -1.0;
			}
			return columnStart + columnWidth;
		}

		private static double GetColumnWidth(XmlElement htmlTDElement)
		{
			string columnWidthAsString = null;
			double columnWidth = -1.0;
			columnWidthAsString = GetAttribute(htmlTDElement, "width");
			if (columnWidthAsString == null)
			{
				columnWidthAsString = GetCssAttribute(GetAttribute(htmlTDElement, "style"), "width");
			}
			if (!TryGetLengthValue(columnWidthAsString, out columnWidth) || columnWidth == 0.0)
			{
				columnWidth = -1.0;
			}
			return columnWidth;
		}

		private static int CalculateColumnSpan(int columnIndex, double columnWidth, ArrayList columnStarts)
		{
			int columnSpanningIndex = columnIndex;
			double columnSpanningValue = 0.0;
			int columnSpan = 0;
			double subColumnWidth = 0.0;
			checked
			{
				while (columnSpanningValue < columnWidth && columnSpanningIndex < columnStarts.Count - 1)
				{
					subColumnWidth = (double)columnStarts[columnSpanningIndex + 1] - (double)columnStarts[columnSpanningIndex];
					columnSpanningValue += subColumnWidth;
					columnSpanningIndex++;
				}
				return columnSpanningIndex - columnIndex;
			}
		}

		private static void VerifyColumnStartsAscendingOrder(ArrayList columnStarts)
		{
			for (int columnIndex = 0; columnIndex < columnStarts.Count; columnIndex = checked(columnIndex + 1))
			{
				double num = (double)columnStarts[columnIndex];
			}
		}

		private static void ApplyLocalProperties(XmlElement xamlElement, Hashtable localProperties, bool isBlock)
		{
			bool marginSet = false;
			string marginTop = "0";
			string marginBottom = "0";
			string marginLeft = "0";
			string marginRight = "0";
			bool paddingSet = false;
			string paddingTop = "0";
			string paddingBottom = "0";
			string paddingLeft = "0";
			string paddingRight = "0";
			string borderColor = null;
			bool borderThicknessSet = false;
			string borderThicknessTop = "0";
			string borderThicknessBottom = "0";
			string borderThicknessLeft = "0";
			string borderThicknessRight = "0";
			IDictionaryEnumerator propertyEnumerator = localProperties.GetEnumerator();
			while (propertyEnumerator.MoveNext())
			{
				switch ((string)propertyEnumerator.Key)
				{
				case "font-family":
					xamlElement.SetAttribute("FontFamily", (string)propertyEnumerator.Value);
					break;
				case "font-style":
					xamlElement.SetAttribute("FontStyle", (string)propertyEnumerator.Value);
					break;
				case "font-weight":
					xamlElement.SetAttribute("FontWeight", (string)propertyEnumerator.Value);
					break;
				case "font-size":
					xamlElement.SetAttribute("FontSize", (string)propertyEnumerator.Value);
					break;
				case "color":
					SetPropertyValue(xamlElement, TextElement.ForegroundProperty, (string)propertyEnumerator.Value);
					break;
				case "background-color":
					SetPropertyValue(xamlElement, TextElement.BackgroundProperty, (string)propertyEnumerator.Value);
					break;
				case "text-decoration-underline":
					if (!isBlock && (string)propertyEnumerator.Value == "true")
					{
						xamlElement.SetAttribute("TextDecorations", "Underline");
					}
					break;
				case "text-decoration-none":
				case "text-decoration-overline":
				case "text-decoration-line-through":
				case "text-decoration-blink":
					if (isBlock)
					{
					}
					break;
				case "text-indent":
					if (isBlock)
					{
						xamlElement.SetAttribute("TextIndent", (string)propertyEnumerator.Value);
					}
					break;
				case "text-align":
					if (isBlock)
					{
						xamlElement.SetAttribute("TextAlignment", (string)propertyEnumerator.Value);
					}
					break;
				case "margin-top":
					marginSet = true;
					marginTop = (string)propertyEnumerator.Value;
					break;
				case "margin-right":
					marginSet = true;
					marginRight = (string)propertyEnumerator.Value;
					break;
				case "margin-bottom":
					marginSet = true;
					marginBottom = (string)propertyEnumerator.Value;
					break;
				case "margin-left":
					marginSet = true;
					marginLeft = (string)propertyEnumerator.Value;
					break;
				case "padding-top":
					paddingSet = true;
					paddingTop = (string)propertyEnumerator.Value;
					break;
				case "padding-right":
					paddingSet = true;
					paddingRight = (string)propertyEnumerator.Value;
					break;
				case "padding-bottom":
					paddingSet = true;
					paddingBottom = (string)propertyEnumerator.Value;
					break;
				case "padding-left":
					paddingSet = true;
					paddingLeft = (string)propertyEnumerator.Value;
					break;
				case "border-color-top":
					borderColor = (string)propertyEnumerator.Value;
					break;
				case "border-color-right":
					borderColor = (string)propertyEnumerator.Value;
					break;
				case "border-color-bottom":
					borderColor = (string)propertyEnumerator.Value;
					break;
				case "border-color-left":
					borderColor = (string)propertyEnumerator.Value;
					break;
				case "border-width-top":
					borderThicknessSet = true;
					borderThicknessTop = (string)propertyEnumerator.Value;
					break;
				case "border-width-right":
					borderThicknessSet = true;
					borderThicknessRight = (string)propertyEnumerator.Value;
					break;
				case "border-width-bottom":
					borderThicknessSet = true;
					borderThicknessBottom = (string)propertyEnumerator.Value;
					break;
				case "border-width-left":
					borderThicknessSet = true;
					borderThicknessLeft = (string)propertyEnumerator.Value;
					break;
				case "list-style-type":
					if (xamlElement.LocalName == "List")
					{
						string markerStyle;
						switch (((string)propertyEnumerator.Value).ToLower())
						{
						case "disc":
							markerStyle = "Disc";
							break;
						case "circle":
							markerStyle = "Circle";
							break;
						case "none":
							markerStyle = "None";
							break;
						case "square":
							markerStyle = "Square";
							break;
						case "box":
							markerStyle = "Box";
							break;
						case "lower-latin":
							markerStyle = "LowerLatin";
							break;
						case "upper-latin":
							markerStyle = "UpperLatin";
							break;
						case "lower-roman":
							markerStyle = "LowerRoman";
							break;
						case "upper-roman":
							markerStyle = "UpperRoman";
							break;
						case "decimal":
							markerStyle = "Decimal";
							break;
						default:
							markerStyle = "Disc";
							break;
						}
						xamlElement.SetAttribute("MarkerStyle", markerStyle);
					}
					break;
				}
			}
			if (isBlock)
			{
				if (marginSet)
				{
					ComposeThicknessProperty(xamlElement, "Margin", marginLeft, marginRight, marginTop, marginBottom);
				}
				if (paddingSet)
				{
					ComposeThicknessProperty(xamlElement, "Padding", paddingLeft, paddingRight, paddingTop, paddingBottom);
				}
				if (borderColor != null)
				{
					xamlElement.SetAttribute("BorderBrush", borderColor);
				}
				if (borderThicknessSet)
				{
					ComposeThicknessProperty(xamlElement, "BorderThickness", borderThicknessLeft, borderThicknessRight, borderThicknessTop, borderThicknessBottom);
				}
			}
		}

		private static void ComposeThicknessProperty(XmlElement xamlElement, string propertyName, string left, string right, string top, string bottom)
		{
			if (left[0] == '0' || left[0] == '-')
			{
				left = "0";
			}
			if (right[0] == '0' || right[0] == '-')
			{
				right = "0";
			}
			if (top[0] == '0' || top[0] == '-')
			{
				top = "0";
			}
			if (bottom[0] == '0' || bottom[0] == '-')
			{
				bottom = "0";
			}
			string thickness = ((!(left == right) || !(top == bottom)) ? (left + "," + top + "," + right + "," + bottom) : ((!(left == top)) ? (left + "," + top) : left));
			xamlElement.SetAttribute(propertyName, thickness);
		}

		private static void SetPropertyValue(XmlElement xamlElement, DependencyProperty property, string stringValue)
		{
			TypeConverter typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
			try
			{
				object convertedValue = typeConverter.ConvertFromInvariantString(stringValue);
				if (convertedValue != null)
				{
					xamlElement.SetAttribute(property.Name, stringValue);
				}
			}
			catch (Exception)
			{
			}
		}

		private static Hashtable GetElementProperties(XmlElement htmlElement, Hashtable inheritedProperties, out Hashtable localProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			Hashtable currentProperties = new Hashtable();
			IDictionaryEnumerator propertyEnumerator = inheritedProperties.GetEnumerator();
			while (propertyEnumerator.MoveNext())
			{
				currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;
			}
			string elementName = htmlElement.LocalName.ToLower();
			string namespaceURI = htmlElement.NamespaceURI;
			localProperties = new Hashtable();
			switch (elementName)
			{
			case "i":
			case "italic":
			case "em":
				localProperties["font-style"] = "italic";
				break;
			case "b":
			case "bold":
			case "strong":
			case "dfn":
				localProperties["font-weight"] = "bold";
				break;
			case "u":
			case "underline":
				localProperties["text-decoration-underline"] = "true";
				break;
			case "font":
			{
				string attributeValue = GetAttribute(htmlElement, "face");
				if (attributeValue != null)
				{
					localProperties["font-family"] = attributeValue;
				}
				attributeValue = GetAttribute(htmlElement, "size");
				if (attributeValue != null)
				{
					double fontSize = double.Parse(attributeValue) * 4.0;
					if (fontSize < 1.0)
					{
						fontSize = 1.0;
					}
					else if (fontSize > 1000.0)
					{
						fontSize = 1000.0;
					}
					localProperties["font-size"] = fontSize.ToString();
				}
				attributeValue = GetAttribute(htmlElement, "color");
				if (attributeValue != null)
				{
					localProperties["color"] = attributeValue;
				}
				break;
			}
			case "samp":
				localProperties["font-family"] = "Courier New";
				localProperties["font-size"] = "8pt";
				localProperties["text-align"] = "Left";
				break;
			case "pre":
				localProperties["font-family"] = "Courier New";
				localProperties["font-size"] = "8pt";
				localProperties["text-align"] = "Left";
				break;
			case "blockquote":
				localProperties["margin-left"] = "16";
				break;
			case "h1":
				localProperties["font-size"] = "22pt";
				break;
			case "h2":
				localProperties["font-size"] = "20pt";
				break;
			case "h3":
				localProperties["font-size"] = "18pt";
				break;
			case "h4":
				localProperties["font-size"] = "16pt";
				break;
			case "h5":
				localProperties["font-size"] = "12pt";
				break;
			case "h6":
				localProperties["font-size"] = "10pt";
				break;
			case "ul":
				localProperties["list-style-type"] = "disc";
				break;
			case "ol":
				localProperties["list-style-type"] = "decimal";
				break;
			}
			HtmlCssParser.GetElementPropertiesFromCssAttributes(htmlElement, elementName, stylesheet, localProperties, sourceContext);
			propertyEnumerator = localProperties.GetEnumerator();
			while (propertyEnumerator.MoveNext())
			{
				currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;
			}
			return currentProperties;
		}

		private static string GetCssAttribute(string cssStyle, string attributeName)
		{
			if (cssStyle != null)
			{
				attributeName = attributeName.ToLower();
				string[] styleValues = cssStyle.Split(';');
				for (int styleValueIndex = 0; styleValueIndex < styleValues.Length; styleValueIndex = checked(styleValueIndex + 1))
				{
					string[] styleNameValue = styleValues[styleValueIndex].Split(':');
					if (styleNameValue.Length == 2 && styleNameValue[0].Trim().ToLower() == attributeName)
					{
						return styleNameValue[1].Trim();
					}
				}
			}
			return null;
		}

		private static bool TryGetLengthValue(string lengthAsString, out double length)
		{
			length = double.NaN;
			checked
			{
				if (lengthAsString != null)
				{
					lengthAsString = lengthAsString.Trim().ToLower();
					if (lengthAsString.EndsWith("pt"))
					{
						lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
						if (double.TryParse(lengthAsString, out length))
						{
							length = length * 96.0 / 72.0;
						}
						else
						{
							length = double.NaN;
						}
					}
					else if (lengthAsString.EndsWith("px"))
					{
						lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
						if (!double.TryParse(lengthAsString, out length))
						{
							length = double.NaN;
						}
					}
					else if (!double.TryParse(lengthAsString, out length))
					{
						length = double.NaN;
					}
				}
				return !double.IsNaN(length);
			}
		}

		private static string GetColorValue(string colorValue)
		{
			return colorValue;
		}

		private static void ApplyPropertiesToTableCellElement(XmlElement htmlChildNode, XmlElement xamlTableCellElement)
		{
			xamlTableCellElement.SetAttribute("BorderThickness", "1,1,1,1");
			xamlTableCellElement.SetAttribute("BorderBrush", "Black");
			string rowSpanString = GetAttribute(htmlChildNode, "rowspan");
			if (rowSpanString != null)
			{
				xamlTableCellElement.SetAttribute("RowSpan", rowSpanString);
			}
		}
	}
}
