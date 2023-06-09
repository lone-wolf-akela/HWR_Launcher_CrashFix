using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Launcher
{
	public class IniFile
	{
		private readonly List<Section> _Sections;

		public string FileName { get; private set; }

		public Section this[string parm]
		{
			get
			{
				return GetSection(parm);
			}
		}

		private IniFile(string InFileName, List<Section> Sections)
		{
			_Sections = Sections;
			FileName = InFileName;
		}

		internal void Dump()
		{
			foreach (Section S in _Sections)
			{
				Console.WriteLine(S);
				Parameter[] parameters = S.Parameters;
				foreach (Parameter P in parameters)
				{
					Console.WriteLine(P);
				}
			}
		}

		internal bool HasSection(string SectionName)
		{
			return GetSections(SectionName).Count() > 0;
		}

		internal void AddSection(string SectionName)
		{
			if (GetSections(SectionName).Count() == 0)
			{
				_Sections.Add(new Section(SectionName));
			}
		}

		internal Section GetSection(string SectionName)
		{
			IEnumerable<Section> Matches = GetSections(SectionName);
			if (Matches.Count() > 1)
			{
				throw new Exception(string.Format("Expected exactly 1 section for {0}", SectionName));
			}
			if (Matches.Count() <= 0)
			{
				return null;
			}
			return Matches.First();
		}

		internal IEnumerable<Section> GetSections(string SectionName)
		{
			foreach (Section S in _Sections)
			{
				if (S.Name.Equals(SectionName, StringComparison.InvariantCultureIgnoreCase))
				{
					yield return S;
				}
			}
		}

		internal void Save(string SaveAsFileName = null, params string[] OnlyWriteSection)
		{
			string ActualFileName = (string.IsNullOrWhiteSpace(SaveAsFileName) ? FileName : SaveAsFileName);
			using (StreamWriter Writer = new StreamWriter(ActualFileName))
			{
				foreach (Section sect in _Sections)
				{
					if (OnlyWriteSection.Length != 0 && !OnlyWriteSection.Contains(sect.Name))
					{
						continue;
					}
					Writer.WriteLine(string.Format("[{0}]", sect.Name));
					Parameter[] parameters = sect.Parameters;
					foreach (Parameter parm in parameters)
					{
						string[] values = parm.Values;
						foreach (string val in values)
						{
							Writer.WriteLine(string.Format("{0}={1}", parm.Name, val));
						}
					}
					Writer.WriteLine();
				}
			}
		}
	}
}
