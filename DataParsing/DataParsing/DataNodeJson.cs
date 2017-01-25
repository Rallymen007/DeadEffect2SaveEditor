using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FakeUnityEngine;

namespace DataParsing
{
	public static class DataNodeJson
	{
		public static string ToJson(DataNode node)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('{');
			DataNodeJson.ToJson(node, stringBuilder, false);
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		public static void ToJson(DataNode node, StringBuilder sb, bool listItem = false)
		{
			string arg = "nullName";
			if (!string.IsNullOrEmpty(node.Name))
			{
				arg = node.Name;
			}
			if (string.IsNullOrEmpty(node.Content))
			{
				if (node.Nodes == null || node.Nodes.Count == 0)
				{
					if (listItem)
					{
						if (node.IsList)
						{
							sb.Append("[]");
						}
						else
						{
							sb.Append("\"\"");
						}
					}
					else if (node.IsList)
					{
						sb.AppendFormat("\"{0}\":[]", arg);
					}
					else
					{
						sb.AppendFormat("\"{0}\":\"\"", arg);
					}
				}
				else
				{
					if (!listItem)
					{
						sb.AppendFormat("\"{0}\":", arg);
					}
					if (node.IsList)
					{
						sb.Append('[');
					}
					else
					{
						sb.Append('{');
					}
					for (int i = 0; i < node.Nodes.Count; i++)
					{
						DataNode node2 = node.Nodes[i];
						DataNodeJson.ToJson(node2, sb, node.IsList);
						if (i != node.Nodes.Count - 1)
						{
							sb.Append(',');
						}
					}
					if (node.IsList)
					{
						sb.Append("]");
					}
					else
					{
						sb.Append('}');
					}
				}
			}
			else
			{
				if (node.Nodes != null && node.Nodes.Count > 0)
				{
					throw new Exception("Node can't contain content and subnodes");
				}
				if (listItem)
				{
					sb.AppendFormat("\"{0}\"", node.Content.Replace("\"", "\\\""));
				}
				else
				{
					sb.AppendFormat("\"{0}\":\"{1}\"", arg, node.Content.Replace("\"", "\\\""));
				}
			}
		}

		public static void ToJsonFile(DataNode node, string fileName)
		{
			string value = DataNodeJson.ToJson(node);
			using (StreamWriter streamWriter = new StreamWriter(fileName))
			{
				streamWriter.Write(value);
				streamWriter.Close();
			}
		}

		public static DataNode FromJsonResource(string resourceName)
		{
            throw new Exception("Commented code");
			/*TextAsset textAsset = Resources.Load<TextAsset>(resourceName);
			if (textAsset == null)
			{
				Debug.LogError("Can't find resource file " + resourceName);
				return null;
			}
			string text = textAsset.text;
			if (string.IsNullOrEmpty(text))
			{
				Debug.LogError("Resource file empty: " + resourceName);
				return null;
			}
			DataNode result;
			try
			{
				DataNode dataNode = DataNodeJson.FromJson(text);
				result = dataNode;
			}
			catch (Exception exception)
			{
				Debug.LogError("Failure while parsing json resource " + resourceName);
				Debug.LogException(exception);
				result = null;
			}
			return result;*/
		}

		public static DataNode FromJsonFile(string fileName)
		{
			string jsonString;
			using (StreamReader streamReader = new StreamReader(fileName))
			{
				jsonString = streamReader.ReadToEnd();
				streamReader.Close();
			}
			return DataNodeJson.FromJson(jsonString);
		}

		public static DataNode FromJson(string jsonString)
		{
            throw new Exception("fuck this");
			/*int i = 0;
			string text = string.Empty;
			string text2 = string.Empty;
			DataNode dataNode = null;
			Stack<DataNode> stack = new Stack<DataNode>();
			bool flag = false;
			while (i < jsonString.Length)
			{
				char c = jsonString[i];
				char c2;
				if (!flag)
				{
					c2 = c;
					switch (c2)
					{
					case '\t':
					case '\n':
					case '\r':
						goto IL_2F7;
					case '\v':
					case '\f':
						IL_186:
						switch (c2)
						{
						case ' ':
							goto IL_2F7;
						case '!':
							IL_19C:
							switch (c2)
							{
							case '[':
								break;
							case '\\':
								i++;
								goto IL_318;
							case ']':
								goto IL_239;
							default:
								switch (c2)
								{
								case '{':
									goto IL_1E7;
								case '|':
									IL_1C8:
									if (c2 == ',')
									{
										if (text != string.Empty)
										{
											if (dataNode == null)
											{
												throw new Exception("JSON Parse: Closing brackets without context!");
											}
											dataNode.AddString(text2, text);
										}
										text2 = string.Empty;
										text = string.Empty;
										goto IL_318;
									}
									if (c2 != ':')
									{
										text += c;
										goto IL_318;
									}
									text2 = text;
									text = string.Empty;
									goto IL_318;
								case '}':
									goto IL_239;
								}
								goto IL_1C8;
							}
							IL_1E7:
							stack.Push(new DataNode(null, c == '['));
							if (dataNode != null)
							{
								text2 = text2.Trim();
								DataNode dataNode2 = stack.Peek();
								dataNode2.Name = text2;
								dataNode.AddNode(dataNode2);
							}
							text2 = string.Empty;
							text = string.Empty;
							dataNode = stack.Peek();
							goto IL_318;
							IL_239:
							if (stack.Count == 0)
							{
								throw new Exception("JSON Parse: Too many closing brackets");
							}
							stack.Pop();
							if (text != string.Empty)
							{
								text2 = text2.Trim();
								if (dataNode == null)
								{
									throw new Exception("JSON Parse: Closing brackets without context!");
								}
								dataNode.AddString(text2, text);
							}
							text2 = string.Empty;
							text = string.Empty;
							if (stack.Count > 0)
							{
								dataNode = stack.Peek();
							}
							goto IL_318;
						case '"':
							flag = true;
							goto IL_318;
						}
						goto IL_19C;
					}
					goto IL_186;
					IL_318:
					i++;
					continue;
					IL_2F7:
					goto IL_318;
				}
				c2 = c;
				if (c2 != '"')
				{
					if (c2 != '\\')
					{
						text += c;
					}
					else
					{
						i++;
						char c3 = jsonString[i];
						char c4 = c3;
						switch (c4)
						{
						case 'n':
							text += '\n';
							goto IL_143;
						case 'o':
						case 'p':
						case 'q':
						case 's':
							IL_8D:
							if (c4 == 'b')
							{
								text += '\b';
								goto IL_143;
							}
							if (c4 != 'f')
							{
								text += c3;
								goto IL_143;
							}
							text += '\f';
							goto IL_143;
						case 'r':
							text += '\r';
							goto IL_143;
						case 't':
							text += '\t';
							goto IL_143;
						case 'u':
						{
							string s = jsonString.Substring(i + 1, 4);
							text += (char)int.Parse(s, NumberStyles.AllowHexSpecifier);
							i += 4;
							goto IL_143;
						}
						}
						goto IL_8D;
						IL_143:;
					}
				}
				else
				{
					flag = false;
				}
				i++;
			}
			if (flag)
			{
				throw new Exception("JSON Parse: too many quotation marks.");
			}
			if (dataNode == null)
			{
				Debug.LogError("No context in json:'" + jsonString + "'");
				return null;
			}
			return dataNode.Nodes[0];*/
		}
	}
}
