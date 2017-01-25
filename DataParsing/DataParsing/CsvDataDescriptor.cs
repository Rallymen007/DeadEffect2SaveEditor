using System;
using System.Collections.Generic;
using System.Reflection;
using FakeUnityEngine;

namespace DataParsing
{
	public sealed class CsvDataDescriptor
	{
		public struct DataDescriptorItem
		{
			public int ColumnIndex;

			public string HeaderName;

			public Type Type;

			public FieldInfo Field;
		}

		private const string ItemTypeHeaderName = "itemType?";

		public readonly List<CsvDataDescriptor.DataDescriptorItem> Descriptor = new List<CsvDataDescriptor.DataDescriptorItem>();

		private int _totalColumns;

		private List<string> _csvHeader;

		private readonly Dictionary<string, Type> _typeDictionary = new Dictionary<string, Type>();

		private readonly Dictionary<string, int> _columnDictionary = new Dictionary<string, int>();

		public int ItemTypeColumn
		{
			get;
			private set;
		}

		private CsvDataDescriptor(List<string> headerRow)
		{
			this.ItemTypeColumn = -1;
			if (headerRow == null)
			{
				return;
			}
			for (int i = 0; i < headerRow.Count; i++)
			{
				string text = headerRow[i];
				if (text == "itemType?")
				{
					if (this.ItemTypeColumn != -1)
					{
						Debug.LogWarning("Header contains duplicate " + this.ItemTypeColumn + " second is ignored!");
					}
					else
					{
						this.ItemTypeColumn = i;
					}
				}
				else
				{
					if (this._columnDictionary.ContainsKey(text))
					{
						Debug.LogWarning("Header contains duplicate " + this.ItemTypeColumn + " !");
						string arg = text;
						int num = 0;
						while (this._columnDictionary.ContainsKey(text))
						{
							text = arg + "_dup_" + num;
							num++;
						}
					}
					this._columnDictionary.Add(text, i);
					this.Descriptor.Add(new CsvDataDescriptor.DataDescriptorItem
					{
						ColumnIndex = i,
						Field = null,
						Type = null,
						HeaderName = text
					});
					this._totalColumns = i + 1;
				}
			}
		}

		public Type GetItemType(string typeValue)
		{
			Type result;
			if (this._typeDictionary.TryGetValue(typeValue, out result))
			{
				return result;
			}
			return null;
		}

		public List<string> CreateCsvHeader()
		{
			if (this._csvHeader == null)
			{
				this._csvHeader = new List<string>();
				foreach (CsvDataDescriptor.DataDescriptorItem current in this.Descriptor)
				{
					while (this._csvHeader.Count < current.ColumnIndex + 1)
					{
						this._csvHeader.Add(null);
					}
					this._csvHeader[current.ColumnIndex] = current.HeaderName;
				}
			}
			return this._csvHeader;
		}

		private void AddType(Type type)
		{
			this._typeDictionary.Add(type.Name, type);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo[] array = fields;
			int i = 0;
			while (i < array.Length)
			{
				FieldInfo fieldInfo = array[i];
				if (!fieldInfo.IsPublic)
				{
					object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(SerializeField), true);
					if (customAttributes.Length != 0)
					{
						goto IL_7A;
					}
				}
				else
				{
					object[] customAttributes2 = fieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true);
					if (customAttributes2.Length == 0)
					{
						goto IL_7A;
					}
				}
				IL_F2:
				i++;
				continue;
				IL_7A:
				string name = fieldInfo.Name;
				int totalColumns;
				if (!this._columnDictionary.TryGetValue(name, out totalColumns))
				{
					totalColumns = this._totalColumns;
					this._totalColumns++;
					this._columnDictionary.Add(name, totalColumns);
				}
				this.Descriptor.Add(new CsvDataDescriptor.DataDescriptorItem
				{
					ColumnIndex = totalColumns,
					Field = fieldInfo,
					Type = type,
					HeaderName = name
				});
				goto IL_F2;
			}
		}

		public static CsvDataDescriptor CreateDescriptor(Type type, List<string> headerRow = null)
		{
			CsvDataDescriptor csvDataDescriptor = new CsvDataDescriptor(headerRow);
			csvDataDescriptor.AddType(type);
			return csvDataDescriptor;
		}

		public static CsvDataDescriptor CreateDescriptor(IEnumerable<Type> types, List<string> headerRow = null)
		{
			CsvDataDescriptor csvDataDescriptor = new CsvDataDescriptor(headerRow);
			csvDataDescriptor.Descriptor.Add(new CsvDataDescriptor.DataDescriptorItem
			{
				ColumnIndex = 0,
				Field = null,
				Type = null,
				HeaderName = "itemType?"
			});
			csvDataDescriptor.ItemTypeColumn = 0;
			csvDataDescriptor._totalColumns = 1;
			foreach (Type current in types)
			{
				csvDataDescriptor.AddType(current);
			}
			return csvDataDescriptor;
		}
	}
}
