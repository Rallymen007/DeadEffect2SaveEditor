using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using FakeUnityEngine;

namespace DataParsing
{
	public class CsvSerializer
	{
		public static string SerializeData<T>(IEnumerable<T> data, CsvDataDescriptor descriptor = null, CsvParser.DelimiterType delimiter = CsvParser.DelimiterType.Comma)
		{
			List<List<string>> list = new List<List<string>>();
			if (descriptor == null)
			{
				descriptor = CsvDataDescriptor.CreateDescriptor(typeof(T), null);
			}
			list.Add(descriptor.CreateCsvHeader());
			foreach (T current in data)
			{
				list.Add(CsvSerializer.Serialize(current, descriptor));
			}
			return CsvParser.ConvertToCsv(list, delimiter);
		}

		public static string SerializeData(IEnumerable<object> data, CsvDataDescriptor descriptor = null, CsvParser.DelimiterType delimiter = CsvParser.DelimiterType.Comma)
		{
			List<List<string>> list = new List<List<string>>();
			if (descriptor == null)
			{
				List<Type> list2 = new List<Type>();
				foreach (object current in data)
				{
					Type type = current.GetType();
					if (!list2.Contains(type))
					{
						list2.Add(type);
					}
				}
				descriptor = CsvDataDescriptor.CreateDescriptor(list2, null);
			}
			list.Add(descriptor.CreateCsvHeader());
			foreach (object current2 in data)
			{
				list.Add(CsvSerializer.Serialize(current2, descriptor));
			}
			return CsvParser.ConvertToCsv(list, delimiter);
		}

		public static List<string> Serialize(object item, CsvDataDescriptor descriptor)
		{
			List<string> list = new List<string>();
			Type type = item.GetType();
			if (descriptor.ItemTypeColumn != -1)
			{
				while (list.Count < descriptor.ItemTypeColumn + 1)
				{
					list.Add(null);
				}
				list[descriptor.ItemTypeColumn] = type.Name;
			}
			foreach (CsvDataDescriptor.DataDescriptorItem current in descriptor.Descriptor)
			{
				if (current.Type == type)
				{
					while (list.Count < current.ColumnIndex + 1)
					{
						list.Add(null);
					}
					string value = CsvSerializer.GetValue(item, current.Field);
					list[current.ColumnIndex] = value;
				}
			}
			return list;
		}

		public static string GetValue(object obj, FieldInfo field)
		{
			Type fieldType = field.FieldType;
			if (fieldType == typeof(string) || fieldType == typeof(bool) || fieldType == typeof(int) || fieldType.IsEnum)
			{
				return field.GetValue(obj).ToString();
			}
			if (fieldType == typeof(float))
			{
				return ((float)field.GetValue(obj)).ToString(CultureInfo.InvariantCulture);
			}
			Debug.LogWarning("Field type " + fieldType.Name + " serialization not supported!");
			return null;
		}

		public static List<T> DeserializeData<T>(string csvString, CsvDataDescriptor descriptor = null, int headerRow = 0, CsvParser.DelimiterType delimiter = CsvParser.DelimiterType.Auto) where T : new()
		{
			List<List<string>> parsedCsv = CsvParser.ParseString(csvString, delimiter, true);
			return CsvSerializer.DeserializeData<T>(parsedCsv, descriptor, headerRow);
		}

		public static List<T> DeserializeData<T>(List<List<string>> parsedCsv, CsvDataDescriptor descriptor = null, int headerRow = 0) where T : new()
		{
			if (headerRow >= parsedCsv.Count)
			{
				throw new ArgumentException("Header row index bigger then data!", "headerRow");
			}
			if (descriptor == null)
			{
				descriptor = CsvDataDescriptor.CreateDescriptor(typeof(T), parsedCsv[headerRow]);
			}
			List<T> list = new List<T>(parsedCsv.Count - headerRow - 1);
			for (int i = headerRow + 1; i < parsedCsv.Count; i++)
			{
				List<string> list2 = parsedCsv[i];
				if (list2.Count != 0)
				{
					T t = (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
					CsvSerializer.Deserialize(t, list2, descriptor);
					list.Add(t);
				}
			}
			return list;
		}

		public static List<object> DeserializeData(string csvString, CsvDataDescriptor descriptor, int headerRow = 0, CsvParser.DelimiterType delimiter = CsvParser.DelimiterType.Auto, IEnumerable<Type> customTypes = null)
		{
			List<List<string>> list = CsvParser.ParseString(csvString, delimiter, true);
			if (headerRow >= list.Count)
			{
				throw new ArgumentException("Header row index bigger then data!", "headerRow");
			}
			return CsvSerializer.DeserializeData(list, descriptor, headerRow, delimiter, customTypes);
		}

		public static List<object> DeserializeData(List<List<string>> parsedCsv, CsvDataDescriptor descriptor, int headerRow = 0, CsvParser.DelimiterType delimiter = CsvParser.DelimiterType.Auto, IEnumerable<Type> customTypes = null)
		{
			if (descriptor == null)
			{
				if (customTypes == null)
				{
					throw new NullReferenceException("descriptor for generic import can't be null, you must at least specify types!");
				}
				descriptor = CsvDataDescriptor.CreateDescriptor(customTypes, parsedCsv[headerRow]);
			}
			if (descriptor.ItemTypeColumn == -1)
			{
				throw new ArgumentException("descriptor for generic import must have type column set!");
			}
			List<object> list = new List<object>();
			for (int i = headerRow + 1; i < parsedCsv.Count; i++)
			{
				List<string> list2 = parsedCsv[i];
				if (list2.Count >= descriptor.ItemTypeColumn + 1)
				{
					string text = list2[descriptor.ItemTypeColumn];
					if (!string.IsNullOrEmpty(text))
					{
						Type itemType = descriptor.GetItemType(text);
						if (itemType == null)
						{
							Debug.LogWarning("Can't instantiate type " + text + " this type is not in the descriptor!");
						}
						else
						{
							object obj = Activator.CreateInstance(itemType);
							CsvSerializer.Deserialize(obj, list2, descriptor);
							list.Add(obj);
						}
					}
				}
			}
			return list;
		}

		public static void Deserialize(object obj, List<string> dataRow, CsvDataDescriptor descriptor)
		{
			Type type = obj.GetType();
			foreach (CsvDataDescriptor.DataDescriptorItem current in descriptor.Descriptor)
			{
				if (current.ColumnIndex < dataRow.Count)
				{
					if (type == current.Type)
					{
						string value = dataRow[current.ColumnIndex];
						CsvSerializer.SetValue(obj, current.Field, value);
					}
				}
			}
		}

		public static void SetValue(object obj, FieldInfo field, string value)
		{
			Type fieldType = field.FieldType;
			if (fieldType.IsEnum)
			{
				value = value.Trim();
				try
				{
					object value2 = Enum.Parse(fieldType, value);
					field.SetValue(obj, value2);
				}
				catch
				{
					Debug.LogError("Failed to parse enumeration " + value);
				}
				return;
			}
			if (fieldType == typeof(string))
			{
				field.SetValue(obj, value);
				return;
			}
			if (fieldType == typeof(bool))
			{
				value = value.Trim().ToLowerInvariant();
				bool flag = value == "true" || value == "yes";
				field.SetValue(obj, flag);
				return;
			}
			if (fieldType == typeof(int))
			{
				value = value.Replace(" ", string.Empty);
				int num;
				if (int.TryParse(value, out num))
				{
					field.SetValue(obj, num);
					return;
				}
				return;
			}
			else
			{
				if (fieldType == typeof(float))
				{
					value = value.Replace(" ", string.Empty);
					value = value.Replace(",", ".");
					float num2;
					if (float.TryParse(value, out num2))
					{
						field.SetValue(obj, num2);
					}
					return;
				}
				Debug.LogError(string.Concat(new object[]
				{
					"Field type ",
					field.Name,
					" in ",
					obj.GetType(),
					" is of unsupported type ",
					field.FieldType
				}));
				return;
			}
		}
	}
}
