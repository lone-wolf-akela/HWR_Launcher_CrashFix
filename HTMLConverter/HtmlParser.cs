using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace HTMLConverter
{
	internal class HtmlParser
	{
		internal const string HtmlHeader = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n";

		internal const string HtmlStartFragmentComment = "<!--StartFragment-->";

		internal const string HtmlEndFragmentComment = "<!--EndFragment-->";

		internal const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

		private HtmlLexicalAnalyzer _htmlLexicalAnalyzer;

		private XmlDocument _document;

		private Stack<XmlElement> _openedElements;

		private Stack<XmlElement> _pendingInlineElements;

		private HtmlParser(string inputString)
		{
			_document = new XmlDocument();
			_openedElements = new Stack<XmlElement>();
			_pendingInlineElements = new Stack<XmlElement>();
			_htmlLexicalAnalyzer = new HtmlLexicalAnalyzer(inputString);
			_htmlLexicalAnalyzer.GetNextContentToken();
		}

		internal static XmlElement ParseHtml(string htmlString)
		{
			HtmlParser htmlParser = new HtmlParser(htmlString);
			return htmlParser.ParseHtmlContent();
		}

		internal static string ExtractHtmlFromClipboardData(string htmlDataString)
		{
			int startHtmlIndex = htmlDataString.IndexOf("StartHTML:");
			if (startHtmlIndex < 0)
			{
				return "ERROR: Urecognized html header";
			}
			checked
			{
				startHtmlIndex = int.Parse(htmlDataString.Substring(startHtmlIndex + "StartHTML:".Length, "0123456789".Length));
				if (startHtmlIndex < 0 || startHtmlIndex > htmlDataString.Length)
				{
					return "ERROR: Urecognized html header";
				}
				int endHtmlIndex = htmlDataString.IndexOf("EndHTML:");
				if (endHtmlIndex < 0)
				{
					return "ERROR: Urecognized html header";
				}
				endHtmlIndex = int.Parse(htmlDataString.Substring(endHtmlIndex + "EndHTML:".Length, "0123456789".Length));
				if (endHtmlIndex > htmlDataString.Length)
				{
					endHtmlIndex = htmlDataString.Length;
				}
				return htmlDataString.Substring(startHtmlIndex, endHtmlIndex - startHtmlIndex);
			}
		}

		internal static string AddHtmlClipboardHeader(string htmlString)
		{
			StringBuilder stringBuilder = new StringBuilder();
			checked
			{
				int startHTML = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n".Length + 6 * ("0123456789".Length - "{0:D10}".Length);
				int endHTML = startHTML + htmlString.Length;
				int startFragment = htmlString.IndexOf("<!--StartFragment-->", 0);
				startFragment = ((startFragment < 0) ? startHTML : (startHTML + startFragment + "<!--StartFragment-->".Length));
				int endFragment = htmlString.IndexOf("<!--EndFragment-->", 0);
				endFragment = ((endFragment < 0) ? endHTML : (startHTML + endFragment));
				stringBuilder.AppendFormat("Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n", startHTML, endHTML, startFragment, endFragment, startFragment, endFragment);
				stringBuilder.Append(htmlString);
				return stringBuilder.ToString();
			}
		}

		private void InvariantAssert(bool condition, string message)
		{
			if (!condition)
			{
				throw new Exception("Assertion error: " + message);
			}
		}

		private XmlElement ParseHtmlContent()
		{
			XmlElement htmlRootElement = _document.CreateElement("html", "http://www.w3.org/1999/xhtml");
			OpenStructuringElement(htmlRootElement);
			while (_htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EOF)
			{
				if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.OpeningTagStart)
				{
					_htmlLexicalAnalyzer.GetNextTagToken();
					if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
					{
						string htmlElementName2 = _htmlLexicalAnalyzer.NextToken.ToLower();
						_htmlLexicalAnalyzer.GetNextTagToken();
						XmlElement htmlElement = _document.CreateElement(htmlElementName2, "http://www.w3.org/1999/xhtml");
						ParseAttributes(htmlElement);
						if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.EmptyTagEnd || HtmlSchema.IsEmptyElement(htmlElementName2))
						{
							AddEmptyElement(htmlElement);
						}
						else if (HtmlSchema.IsInlineElement(htmlElementName2))
						{
							OpenInlineElement(htmlElement);
						}
						else if (HtmlSchema.IsBlockElement(htmlElementName2) || HtmlSchema.IsKnownOpenableElement(htmlElementName2))
						{
							OpenStructuringElement(htmlElement);
						}
					}
				}
				else if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.ClosingTagStart)
				{
					_htmlLexicalAnalyzer.GetNextTagToken();
					if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
					{
						string htmlElementName = _htmlLexicalAnalyzer.NextToken.ToLower();
						_htmlLexicalAnalyzer.GetNextTagToken();
						CloseElement(htmlElementName);
					}
				}
				else if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Text)
				{
					AddTextContent(_htmlLexicalAnalyzer.NextToken);
				}
				else if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Comment)
				{
					AddComment(_htmlLexicalAnalyzer.NextToken);
				}
				_htmlLexicalAnalyzer.GetNextContentToken();
			}
			if (htmlRootElement.FirstChild is XmlElement && htmlRootElement.FirstChild == htmlRootElement.LastChild && htmlRootElement.FirstChild.LocalName.ToLower() == "html")
			{
				htmlRootElement = (XmlElement)htmlRootElement.FirstChild;
			}
			return htmlRootElement;
		}

		private XmlElement CreateElementCopy(XmlElement htmlElement)
		{
			XmlElement htmlElementCopy = _document.CreateElement(htmlElement.LocalName, "http://www.w3.org/1999/xhtml");
			for (int i = 0; i < htmlElement.Attributes.Count; i = checked(i + 1))
			{
				XmlAttribute attribute = htmlElement.Attributes[i];
				htmlElementCopy.SetAttribute(attribute.Name, attribute.Value);
			}
			return htmlElementCopy;
		}

		private void AddEmptyElement(XmlElement htmlEmptyElement)
		{
			InvariantAssert(_openedElements.Count > 0, "AddEmptyElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
			XmlElement htmlParent = _openedElements.Peek();
			htmlParent.AppendChild(htmlEmptyElement);
		}

		private void OpenInlineElement(XmlElement htmlInlineElement)
		{
			_pendingInlineElements.Push(htmlInlineElement);
		}

		private void OpenStructuringElement(XmlElement htmlElement)
		{
			if (HtmlSchema.IsBlockElement(htmlElement.LocalName))
			{
				while (_openedElements.Count > 0 && HtmlSchema.IsInlineElement(_openedElements.Peek().LocalName))
				{
					XmlElement htmlInlineElement = _openedElements.Pop();
					InvariantAssert(_openedElements.Count > 0, "OpenStructuringElement: stack of opened elements cannot become empty here");
					_pendingInlineElements.Push(CreateElementCopy(htmlInlineElement));
				}
			}
			if (_openedElements.Count > 0)
			{
				XmlElement htmlParent = _openedElements.Peek();
				if (HtmlSchema.ClosesOnNextElementStart(htmlParent.LocalName, htmlElement.LocalName))
				{
					_openedElements.Pop();
					htmlParent = ((_openedElements.Count > 0) ? _openedElements.Peek() : null);
				}
				if (htmlParent != null)
				{
					htmlParent.AppendChild(htmlElement);
				}
			}
			_openedElements.Push(htmlElement);
		}

		private bool IsElementOpened(string htmlElementName)
		{
			foreach (XmlElement openedElement in _openedElements)
			{
				if (openedElement.LocalName == htmlElementName)
				{
					return true;
				}
			}
			return false;
		}

		private void CloseElement(string htmlElementName)
		{
			InvariantAssert(_openedElements.Count > 0, "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
			if (_pendingInlineElements.Count > 0 && _pendingInlineElements.Peek().LocalName == htmlElementName)
			{
				XmlElement htmlInlineElement = _pendingInlineElements.Pop();
				InvariantAssert(_openedElements.Count > 0, "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
				XmlElement htmlParent = _openedElements.Peek();
				htmlParent.AppendChild(htmlInlineElement);
			}
			else
			{
				if (!IsElementOpened(htmlElementName))
				{
					return;
				}
				while (_openedElements.Count > 1)
				{
					XmlElement htmlOpenedElement = _openedElements.Pop();
					if (htmlOpenedElement.LocalName == htmlElementName)
					{
						break;
					}
					if (HtmlSchema.IsInlineElement(htmlOpenedElement.LocalName))
					{
						_pendingInlineElements.Push(CreateElementCopy(htmlOpenedElement));
					}
				}
			}
		}

		private void AddTextContent(string textContent)
		{
			OpenPendingInlineElements();
			InvariantAssert(_openedElements.Count > 0, "AddTextContent: Stack of opened elements cannot be empty, as we have at least one artificial root element");
			XmlElement htmlParent = _openedElements.Peek();
			XmlText textNode = _document.CreateTextNode(textContent);
			htmlParent.AppendChild(textNode);
		}

		private void AddComment(string comment)
		{
			OpenPendingInlineElements();
			InvariantAssert(_openedElements.Count > 0, "AddComment: Stack of opened elements cannot be empty, as we have at least one artificial root element");
			XmlElement htmlParent = _openedElements.Peek();
			XmlComment xmlComment = _document.CreateComment(comment);
			htmlParent.AppendChild(xmlComment);
		}

		private void OpenPendingInlineElements()
		{
			if (_pendingInlineElements.Count > 0)
			{
				XmlElement htmlInlineElement = _pendingInlineElements.Pop();
				OpenPendingInlineElements();
				InvariantAssert(_openedElements.Count > 0, "OpenPendingInlineElements: Stack of opened elements cannot be empty, as we have at least one artificial root element");
				XmlElement htmlParent = _openedElements.Peek();
				htmlParent.AppendChild(htmlInlineElement);
				_openedElements.Push(htmlInlineElement);
			}
		}

		private void ParseAttributes(XmlElement xmlElement)
		{
			while (_htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EOF && _htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.TagEnd && _htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EmptyTagEnd)
			{
				if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
				{
					string attributeName = _htmlLexicalAnalyzer.NextToken;
					_htmlLexicalAnalyzer.GetNextEqualSignToken();
					_htmlLexicalAnalyzer.GetNextAtomToken();
					string attributeValue = _htmlLexicalAnalyzer.NextToken;
					xmlElement.SetAttribute(attributeName, attributeValue);
				}
				_htmlLexicalAnalyzer.GetNextTagToken();
			}
		}
	}
}
