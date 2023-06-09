using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace HTMLConverter
{
	internal class CssStylesheet
	{
		private class StyleDefinition
		{
			public string Selector;

			public string Definition;

			public StyleDefinition(string selector, string definition)
			{
				Selector = selector;
				Definition = definition;
			}
		}

		private List<StyleDefinition> _styleDefinitions;

		public CssStylesheet(XmlElement htmlElement)
		{
			if (htmlElement != null)
			{
				DiscoverStyleDefinitions(htmlElement);
			}
		}

		public void DiscoverStyleDefinitions(XmlElement htmlElement)
		{
			if (htmlElement.LocalName.ToLower() == "link")
			{
				return;
			}
			if (htmlElement.LocalName.ToLower() != "style")
			{
				for (XmlNode htmlChildNode2 = htmlElement.FirstChild; htmlChildNode2 != null; htmlChildNode2 = htmlChildNode2.NextSibling)
				{
					if (htmlChildNode2 is XmlElement)
					{
						DiscoverStyleDefinitions((XmlElement)htmlChildNode2);
					}
				}
				return;
			}
			StringBuilder stylesheetBuffer = new StringBuilder();
			for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
			{
				if (htmlChildNode is XmlText || htmlChildNode is XmlComment)
				{
					stylesheetBuffer.Append(RemoveComments(htmlChildNode.Value));
				}
			}
			int nextCharacterIndex = 0;
			checked
			{
				while (nextCharacterIndex < stylesheetBuffer.Length)
				{
					int selectorStart = nextCharacterIndex;
					for (; nextCharacterIndex < stylesheetBuffer.Length && stylesheetBuffer[nextCharacterIndex] != '{'; nextCharacterIndex++)
					{
						if (stylesheetBuffer[nextCharacterIndex] == '@')
						{
							for (; nextCharacterIndex < stylesheetBuffer.Length && stylesheetBuffer[nextCharacterIndex] != ';'; nextCharacterIndex++)
							{
							}
							selectorStart = nextCharacterIndex + 1;
						}
					}
					if (nextCharacterIndex < stylesheetBuffer.Length)
					{
						int definitionStart = nextCharacterIndex;
						for (; nextCharacterIndex < stylesheetBuffer.Length && stylesheetBuffer[nextCharacterIndex] != '}'; nextCharacterIndex++)
						{
						}
						if (nextCharacterIndex - definitionStart > 2)
						{
							AddStyleDefinition(stylesheetBuffer.ToString(selectorStart, definitionStart - selectorStart), stylesheetBuffer.ToString(definitionStart + 1, nextCharacterIndex - definitionStart - 2));
						}
						if (nextCharacterIndex < stylesheetBuffer.Length)
						{
							nextCharacterIndex++;
						}
					}
				}
			}
		}

		private string RemoveComments(string text)
		{
			int commentStart = text.IndexOf("/*");
			if (commentStart < 0)
			{
				return text;
			}
			checked
			{
				int commentEnd = text.IndexOf("*/", commentStart + 2);
				if (commentEnd < 0)
				{
					return text.Substring(0, commentStart);
				}
				return text.Substring(0, commentStart) + " " + RemoveComments(text.Substring(commentEnd + 2));
			}
		}

		public void AddStyleDefinition(string selector, string definition)
		{
			selector = selector.Trim().ToLower();
			definition = definition.Trim().ToLower();
			if (selector.Length == 0 || definition.Length == 0)
			{
				return;
			}
			if (_styleDefinitions == null)
			{
				_styleDefinitions = new List<StyleDefinition>();
			}
			string[] simpleSelectors = selector.Split(',');
			for (int i = 0; i < simpleSelectors.Length; i = checked(i + 1))
			{
				string simpleSelector = simpleSelectors[i].Trim();
				if (simpleSelector.Length > 0)
				{
					_styleDefinitions.Add(new StyleDefinition(simpleSelector, definition));
				}
			}
		}

		public string GetStyle(string elementName, List<XmlElement> sourceContext)
		{
			checked
			{
				if (_styleDefinitions != null)
				{
					for (int i = _styleDefinitions.Count - 1; i >= 0; i--)
					{
						string selector = _styleDefinitions[i].Selector;
						string[] selectorLevels = selector.Split(' ');
						int indexInSelector = selectorLevels.Length - 1;
						int num = sourceContext.Count - 1;
						string selectorLevel = selectorLevels[indexInSelector].Trim();
						if (MatchSelectorLevel(selectorLevel, sourceContext[sourceContext.Count - 1]))
						{
							return _styleDefinitions[i].Definition;
						}
					}
				}
				return null;
			}
		}

		private bool MatchSelectorLevel(string selectorLevel, XmlElement xmlElement)
		{
			if (selectorLevel.Length == 0)
			{
				return false;
			}
			int indexOfDot = selectorLevel.IndexOf('.');
			int indexOfPound = selectorLevel.IndexOf('#');
			string selectorClass = null;
			string selectorId = null;
			string selectorTag = null;
			checked
			{
				if (indexOfDot >= 0)
				{
					if (indexOfDot > 0)
					{
						selectorTag = selectorLevel.Substring(0, indexOfDot);
					}
					selectorClass = selectorLevel.Substring(indexOfDot + 1);
				}
				else if (indexOfPound >= 0)
				{
					if (indexOfPound > 0)
					{
						selectorTag = selectorLevel.Substring(0, indexOfPound);
					}
					selectorId = selectorLevel.Substring(indexOfPound + 1);
				}
				else
				{
					selectorTag = selectorLevel;
				}
				if (selectorTag != null && selectorTag != xmlElement.LocalName)
				{
					return false;
				}
				if (selectorId != null && HtmlToXamlConverter.GetAttribute(xmlElement, "id") != selectorId)
				{
					return false;
				}
				if (selectorClass != null && HtmlToXamlConverter.GetAttribute(xmlElement, "class") != selectorClass)
				{
					return false;
				}
				return true;
			}
		}
	}
}
