using System;
using System.Collections.Generic;

namespace Launcher
{
	internal class Parameter
	{
		private readonly string _ParameterName;

		private readonly List<string> _ParameterValues = new List<string>();

		internal string Name
		{
			get
			{
				return _ParameterName;
			}
		}

		internal string Value
		{
			get
			{
				if (_ParameterValues.Count <= 0)
				{
					return "";
				}
				return _ParameterValues[0];
			}
		}

		internal string[] Values
		{
			get
			{
				return _ParameterValues.ToArray();
			}
		}

		internal Parameter(string ParameterName, string ParameterValue)
		{
			_ParameterName = ParameterName;
			_ParameterValues.Add(ParameterValue);
		}

		internal void AddValue(string ParameterValue)
		{
			_ParameterValues.Add(ParameterValue);
		}

		internal void SetValue(string ParameterValue)
		{
			if (_ParameterValues.Count != 1)
			{
				throw new Exception("Expected exactly 1 parameter value.");
			}
			_ParameterValues[0] = ParameterValue;
		}

		public override string ToString()
		{
			return string.Format("{0}={1}", _ParameterName, (_ParameterValues.Count > 0) ? "Multiple Values" : Value);
		}
	}
}
