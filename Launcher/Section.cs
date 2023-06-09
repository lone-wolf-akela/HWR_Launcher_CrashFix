using System;
using System.Collections.Generic;

namespace Launcher
{
	public class Section
	{
		private readonly string _SectionName;

		private readonly List<Parameter> _Parameters = new List<Parameter>();

		internal string Name
		{
			get
			{
				return _SectionName;
			}
		}

		internal Parameter[] Parameters
		{
			get
			{
				return _Parameters.ToArray();
			}
		}

		public string this[string param]
		{
			get
			{
				foreach (Parameter P in _Parameters)
				{
					if (P.Name.Equals(param, StringComparison.InvariantCultureIgnoreCase))
					{
						return P.Value.Trim();
					}
				}
				return "";
			}
		}

		internal Section(string SectionName)
		{
			_SectionName = SectionName;
		}

		internal Parameter GetParameter(string ParameterName)
		{
			foreach (Parameter P in _Parameters)
			{
				if (P.Name.Equals(ParameterName, StringComparison.InvariantCultureIgnoreCase))
				{
					return P;
				}
			}
			return null;
		}

		internal void AddParameter(Parameter NewParameter)
		{
			Parameter ExistingParameter = GetParameter(NewParameter.Name);
			if (ExistingParameter != null)
			{
				ExistingParameter.AddValue(NewParameter.Value);
			}
			else
			{
				_Parameters.Add(NewParameter);
			}
		}

		public override string ToString()
		{
			return string.Format("[{0}]", _SectionName);
		}

		public string[] GetParameterValues(string ParameterName)
		{
			Parameter P = GetParameter(ParameterName);
			if (P != null)
			{
				return P.Values;
			}
			return new string[0];
		}
	}
}
