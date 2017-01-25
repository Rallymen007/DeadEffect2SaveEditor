using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FakeUnityEngine;

namespace DataParsing
{
	public class CsvParser
	{
		public enum DelimiterType
		{
			Auto,
			Comma,
			Semicolon,
			Tab
		}

		public delegate void ParserCallback(List<string> dataRow);

		public static string ConvertToCsv(List<List<string>> sourceData, CsvParser.DelimiterType delimiterType = CsvParser.DelimiterType.Comma)
		{
			StringBuilder stringBuilder = new StringBuilder();
			char c;
			switch (delimiterType)
			{
			case CsvParser.DelimiterType.Comma:
				c = ',';
				break;
			case CsvParser.DelimiterType.Semicolon:
				c = ';';
				break;
			case CsvParser.DelimiterType.Tab:
				c = '\t';
				break;
			default:
				throw new ArgumentException("Auto or unknown type of delimiter can't be used for export", "delimiterType");
			}
			char[] anyOf = new char[]
			{
				'\n',
				'"',
				c
			};
			foreach (List<string> current in sourceData)
			{
				for (int i = 0; i < current.Count; i++)
				{
					string text = current[i];
					if (!string.IsNullOrEmpty(text))
					{
						int num = text.IndexOfAny(anyOf);
						if (num != -1)
						{
							string value = text.Replace("\"", "\"\"");
							stringBuilder.Append('"');
							stringBuilder.Append(value);
							stringBuilder.Append('"');
						}
						else
						{
							stringBuilder.Append(text);
						}
					}
					if (i != current.Count - 1)
					{
						stringBuilder.Append(c);
					}
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		public static List<List<string>> ParseFile(string fileName, CsvParser.DelimiterType delimiterType = CsvParser.DelimiterType.Auto, bool ignoreEmptyLines = true)
		{
			string csvString = File.ReadAllText(fileName);
			return CsvParser.ParseString(csvString, delimiterType, true);
		}

		public static List<List<string>> ParseString(string csvString, CsvParser.DelimiterType delimiterType = CsvParser.DelimiterType.Auto, bool ignoreEmptyLines = true)
		{
			List<List<string>> list = new List<List<string>>();
			CsvParser.ParseString(csvString, new CsvParser.ParserCallback(list.Add), delimiterType, ignoreEmptyLines);
			return list;
		}

		public static void ParseString(string csvString, CsvParser.ParserCallback dataHandler, CsvParser.DelimiterType delimiterType = CsvParser.DelimiterType.Auto, bool ignoreEmptyLines = true)
		{
			csvString = csvString.Replace("\r", string.Empty);
			CsvParser.ParserCallback parserCallback = delegate(List<string> dataRow)
			{
				if (ignoreEmptyLines && dataRow.Count == 0)
				{
					return;
				}
				dataHandler(dataRow);
			};
			char c;
			switch (delimiterType)
			{
			case CsvParser.DelimiterType.Auto:
				c = CsvParser.DetectDelimiter(csvString);
				break;
			case CsvParser.DelimiterType.Comma:
				c = ',';
				break;
			case CsvParser.DelimiterType.Semicolon:
				c = ';';
				break;
			case CsvParser.DelimiterType.Tab:
				c = '\t';
				break;
			default:
				c = ',';
				break;
			}
			List<string> list = new List<string>();
			char[] array = new char[]
			{
				c,
				'\n'
			};
			int i = 0;
			while (i < csvString.Length)
			{
				int num;
				string text;
				if (csvString[i] == '"' && CsvParser.GetEndQuoteIndex(csvString, i, array, out num))
				{
					if (num == -1)
					{
						Debug.LogError("Possibly corrupted CSV file, correct quote missing!");
						text = csvString.Substring(i + 1, num - i - 1);
					}
					else
					{
						text = csvString.Substring(i + 1, num - i - 1);
						i = num + 1;
						text = text.Replace("\r", string.Empty);
						text = text.Replace("\"\"", "\"");
					}
				}
				else
				{
					int num2 = csvString.IndexOfAny(array, i);
					if (num2 == -1)
					{
						text = csvString.Substring(i);
						i += text.Length;
						text = text.Replace("\r", string.Empty);
					}
					else if (num2 == i)
					{
						text = string.Empty;
					}
					else
					{
						text = csvString.Substring(i, num2 - i);
						i += text.Length;
						text = text.Replace("\r", string.Empty);
					}
				}
				list.Add(text);
				if (i == csvString.Length)
				{
					parserCallback(list);
					break;
				}
				if (csvString[i] == c)
				{
					i++;
				}
				else
				{
					if (csvString[i] != '\n')
					{
						Debug.LogError("This shouldn't happen!");
						int num3 = i - 50;
						if (num3 < 0)
						{
							num3 = 0;
						}
						Debug.Log(csvString.Substring(num3, 100));
						break;
					}
					parserCallback(list);
					list = new List<string>();
					if (csvString[i] == '\r')
					{
						i++;
					}
					i++;
				}
				if (i == csvString.Length)
				{
					if (list.Count > 0)
					{
						parserCallback(list);
					}
					break;
				}
			}
		}

		protected static char DetectDelimiter(string csv)
		{
			char[] anyOf = new char[]
			{
				',',
				';',
				'\t'
			};
			int num = csv.IndexOfAny(anyOf);
			if (num == -1)
			{
				throw new Exception("CSV string doesn't contain any delimiter!");
			}
			return csv[num];
		}

		public static bool GetEndQuoteIndex(string str, int startpos, char[] delimiters, out int endIndex)
		{
			for (int i = startpos + 1; i < str.Length; i++)
			{
				if (str[i] == '"')
				{
					if (i != str.Length - 1 && str[i + 1] == '"')
					{
						i++;
					}
					else
					{
                        throw new Exception("commented code");
						/*if (!delimiters.Contains(str[i + 1]))
						{
							endIndex = i;
							return false;
						}*/
						endIndex = i;
						return true;
					}
				}
			}
			endIndex = str.Length;
			return false;
		}
	}
}
