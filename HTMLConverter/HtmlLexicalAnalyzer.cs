using System;
using System.IO;
using System.Text;

namespace HTMLConverter
{
	internal class HtmlLexicalAnalyzer
	{
		private StringReader _inputStringReader;

		private int _nextCharacterCode;

		private char _nextCharacter;

		private int _lookAheadCharacterCode;

		private char _lookAheadCharacter;

		private char _previousCharacter;

		private bool _ignoreNextWhitespace;

		private bool _isNextCharacterEntity;

		private StringBuilder _nextToken;

		private HtmlTokenType _nextTokenType;

		internal HtmlTokenType NextTokenType
		{
			get
			{
				return _nextTokenType;
			}
		}

		internal string NextToken
		{
			get
			{
				return _nextToken.ToString();
			}
		}

		private char NextCharacter
		{
			get
			{
				return _nextCharacter;
			}
		}

		private bool IsAtEndOfStream
		{
			get
			{
				return _nextCharacterCode == -1;
			}
		}

		private bool IsAtTagStart
		{
			get
			{
				if (_nextCharacter == '<' && (_lookAheadCharacter == '/' || IsGoodForNameStart(_lookAheadCharacter)))
				{
					return !_isNextCharacterEntity;
				}
				return false;
			}
		}

		private bool IsAtTagEnd
		{
			get
			{
				if (_nextCharacter == '>' || (_nextCharacter == '/' && _lookAheadCharacter == '>'))
				{
					return !_isNextCharacterEntity;
				}
				return false;
			}
		}

		private bool IsAtDirectiveStart
		{
			get
			{
				if (_nextCharacter == '<' && _lookAheadCharacter == '!')
				{
					return !IsNextCharacterEntity;
				}
				return false;
			}
		}

		private bool IsNextCharacterEntity
		{
			get
			{
				return _isNextCharacterEntity;
			}
		}

		internal HtmlLexicalAnalyzer(string inputTextString)
		{
			_inputStringReader = new StringReader(inputTextString);
			_nextCharacterCode = 0;
			_nextCharacter = ' ';
			_lookAheadCharacterCode = _inputStringReader.Read();
			_lookAheadCharacter = (char)checked((ushort)_lookAheadCharacterCode);
			_previousCharacter = ' ';
			_ignoreNextWhitespace = true;
			_nextToken = new StringBuilder(100);
			_nextTokenType = HtmlTokenType.Text;
			GetNextCharacter();
		}

		internal void GetNextContentToken()
		{
			_nextToken.Length = 0;
			if (IsAtEndOfStream)
			{
				_nextTokenType = HtmlTokenType.EOF;
				return;
			}
			if (IsAtTagStart)
			{
				GetNextCharacter();
				if (NextCharacter == '/')
				{
					_nextToken.Append("</");
					_nextTokenType = HtmlTokenType.ClosingTagStart;
					GetNextCharacter();
					_ignoreNextWhitespace = false;
				}
				else
				{
					_nextTokenType = HtmlTokenType.OpeningTagStart;
					_nextToken.Append("<");
					_ignoreNextWhitespace = true;
				}
				return;
			}
			if (IsAtDirectiveStart)
			{
				GetNextCharacter();
				if (_lookAheadCharacter == '[')
				{
					ReadDynamicContent();
				}
				else if (_lookAheadCharacter == '-')
				{
					ReadComment();
				}
				else
				{
					ReadUnknownDirective();
				}
				return;
			}
			_nextTokenType = HtmlTokenType.Text;
			while (!IsAtTagStart && !IsAtEndOfStream && !IsAtDirectiveStart)
			{
				if (NextCharacter == '<' && !IsNextCharacterEntity && _lookAheadCharacter == '?')
				{
					SkipProcessingDirective();
					continue;
				}
				if (NextCharacter <= ' ')
				{
					if (!_ignoreNextWhitespace)
					{
						_nextToken.Append(' ');
					}
					_ignoreNextWhitespace = true;
				}
				else
				{
					_nextToken.Append(NextCharacter);
					_ignoreNextWhitespace = false;
				}
				GetNextCharacter();
			}
		}

		internal void GetNextTagToken()
		{
			_nextToken.Length = 0;
			if (IsAtEndOfStream)
			{
				_nextTokenType = HtmlTokenType.EOF;
				return;
			}
			SkipWhiteSpace();
			if (NextCharacter == '>' && !IsNextCharacterEntity)
			{
				_nextTokenType = HtmlTokenType.TagEnd;
				_nextToken.Append('>');
				GetNextCharacter();
			}
			else if (NextCharacter == '/' && _lookAheadCharacter == '>')
			{
				_nextTokenType = HtmlTokenType.EmptyTagEnd;
				_nextToken.Append("/>");
				GetNextCharacter();
				GetNextCharacter();
				_ignoreNextWhitespace = false;
			}
			else if (IsGoodForNameStart(NextCharacter))
			{
				_nextTokenType = HtmlTokenType.Name;
				while (IsGoodForName(NextCharacter) && !IsAtEndOfStream)
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
				}
			}
			else
			{
				_nextTokenType = HtmlTokenType.Atom;
				_nextToken.Append(NextCharacter);
				GetNextCharacter();
			}
		}

		internal void GetNextEqualSignToken()
		{
			_nextToken.Length = 0;
			_nextToken.Append('=');
			_nextTokenType = HtmlTokenType.EqualSign;
			SkipWhiteSpace();
			if (NextCharacter == '=')
			{
				GetNextCharacter();
			}
		}

		internal void GetNextAtomToken()
		{
			_nextToken.Length = 0;
			SkipWhiteSpace();
			_nextTokenType = HtmlTokenType.Atom;
			if ((NextCharacter == '\'' || NextCharacter == '"') && !IsNextCharacterEntity)
			{
				char startingQuote = NextCharacter;
				GetNextCharacter();
				while ((NextCharacter != startingQuote || IsNextCharacterEntity) && !IsAtEndOfStream)
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
				}
				if (NextCharacter == startingQuote)
				{
					GetNextCharacter();
				}
			}
			else
			{
				while (!IsAtEndOfStream && !char.IsWhiteSpace(NextCharacter) && NextCharacter != '>')
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
				}
			}
		}

		private void GetNextCharacter()
		{
			if (_nextCharacterCode == -1)
			{
				throw new InvalidOperationException("GetNextCharacter method called at the end of a stream");
			}
			_previousCharacter = _nextCharacter;
			_nextCharacter = _lookAheadCharacter;
			_nextCharacterCode = _lookAheadCharacterCode;
			_isNextCharacterEntity = false;
			ReadLookAheadCharacter();
			if (_nextCharacter != '&')
			{
				return;
			}
			if (_lookAheadCharacter == '#')
			{
				int entityCode = 0;
				ReadLookAheadCharacter();
				checked
				{
					for (int j = 0; j < 7; j++)
					{
						if (!char.IsDigit(_lookAheadCharacter))
						{
							break;
						}
						entityCode = 10 * entityCode + (_lookAheadCharacterCode - 48);
						ReadLookAheadCharacter();
					}
				}
				if (_lookAheadCharacter == ';')
				{
					ReadLookAheadCharacter();
					_nextCharacterCode = entityCode;
					_nextCharacter = (char)checked((ushort)_nextCharacterCode);
					_isNextCharacterEntity = true;
				}
				else
				{
					_nextCharacter = _lookAheadCharacter;
					_nextCharacterCode = _lookAheadCharacterCode;
					ReadLookAheadCharacter();
					_isNextCharacterEntity = false;
				}
			}
			else
			{
				if (!char.IsLetter(_lookAheadCharacter))
				{
					return;
				}
				string entity = "";
				for (int i = 0; i < 10; i = checked(i + 1))
				{
					if (!char.IsLetter(_lookAheadCharacter) && !char.IsDigit(_lookAheadCharacter))
					{
						break;
					}
					entity += _lookAheadCharacter;
					ReadLookAheadCharacter();
				}
				if (_lookAheadCharacter == ';')
				{
					ReadLookAheadCharacter();
					if (HtmlSchema.IsEntity(entity))
					{
						_nextCharacter = HtmlSchema.EntityCharacterValue(entity);
						_nextCharacterCode = _nextCharacter;
						_isNextCharacterEntity = true;
					}
					else
					{
						_nextCharacter = _lookAheadCharacter;
						_nextCharacterCode = _lookAheadCharacterCode;
						ReadLookAheadCharacter();
						_isNextCharacterEntity = false;
					}
				}
				else
				{
					_nextCharacter = _lookAheadCharacter;
					ReadLookAheadCharacter();
					_isNextCharacterEntity = false;
				}
			}
		}

		private void ReadLookAheadCharacter()
		{
			if (_lookAheadCharacterCode != -1)
			{
				_lookAheadCharacterCode = _inputStringReader.Read();
				if (_lookAheadCharacterCode > 0)
				{
					_lookAheadCharacter = (char)checked((ushort)_lookAheadCharacterCode);
				}
				else
				{
					_lookAheadCharacter = '\0';
				}
			}
		}

		private void SkipWhiteSpace()
		{
			while (true)
			{
				if (_nextCharacter == '<' && (_lookAheadCharacter == '?' || _lookAheadCharacter == '!'))
				{
					GetNextCharacter();
					if (_lookAheadCharacter == '[')
					{
						while (!IsAtEndOfStream && (_previousCharacter != ']' || _nextCharacter != ']' || _lookAheadCharacter != '>'))
						{
							GetNextCharacter();
						}
						if (_nextCharacter == '>')
						{
							GetNextCharacter();
						}
					}
					else
					{
						while (!IsAtEndOfStream && _nextCharacter != '>')
						{
							GetNextCharacter();
						}
						if (_nextCharacter == '>')
						{
							GetNextCharacter();
						}
					}
				}
				if (!char.IsWhiteSpace(NextCharacter))
				{
					break;
				}
				GetNextCharacter();
			}
		}

		private bool IsGoodForNameStart(char character)
		{
			if (character != '_')
			{
				return char.IsLetter(character);
			}
			return true;
		}

		private bool IsGoodForName(char character)
		{
			if (!IsGoodForNameStart(character) && character != '.' && character != '-' && character != ':' && !char.IsDigit(character) && !IsCombiningCharacter(character))
			{
				return IsExtender(character);
			}
			return true;
		}

		private bool IsCombiningCharacter(char character)
		{
			return false;
		}

		private bool IsExtender(char character)
		{
			return false;
		}

		private void ReadDynamicContent()
		{
			_nextTokenType = HtmlTokenType.Text;
			_nextToken.Length = 0;
			GetNextCharacter();
			GetNextCharacter();
			while ((_nextCharacter != ']' || _lookAheadCharacter != '>') && !IsAtEndOfStream)
			{
				GetNextCharacter();
			}
			if (!IsAtEndOfStream)
			{
				GetNextCharacter();
				GetNextCharacter();
			}
		}

		private void ReadComment()
		{
			_nextTokenType = HtmlTokenType.Comment;
			_nextToken.Length = 0;
			GetNextCharacter();
			GetNextCharacter();
			GetNextCharacter();
			while (true)
			{
				if (!IsAtEndOfStream && (_nextCharacter != '-' || _lookAheadCharacter != '-') && (_nextCharacter != '!' || _lookAheadCharacter != '>'))
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
					continue;
				}
				GetNextCharacter();
				if (_previousCharacter == '-' && _nextCharacter == '-' && _lookAheadCharacter == '>')
				{
					GetNextCharacter();
					break;
				}
				if (_previousCharacter == '!' && _nextCharacter == '>')
				{
					break;
				}
				_nextToken.Append(_previousCharacter);
			}
			if (_nextCharacter == '>')
			{
				GetNextCharacter();
			}
		}

		private void ReadUnknownDirective()
		{
			_nextTokenType = HtmlTokenType.Text;
			_nextToken.Length = 0;
			GetNextCharacter();
			while ((_nextCharacter != '>' || IsNextCharacterEntity) && !IsAtEndOfStream)
			{
				GetNextCharacter();
			}
			if (!IsAtEndOfStream)
			{
				GetNextCharacter();
			}
		}

		private void SkipProcessingDirective()
		{
			GetNextCharacter();
			GetNextCharacter();
			while (((_nextCharacter != '?' && _nextCharacter != '/') || _lookAheadCharacter != '>') && !IsAtEndOfStream)
			{
				GetNextCharacter();
			}
			if (!IsAtEndOfStream)
			{
				GetNextCharacter();
				GetNextCharacter();
			}
		}
	}
}
