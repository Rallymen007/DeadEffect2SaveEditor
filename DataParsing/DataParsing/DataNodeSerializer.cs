using System;
using System.Globalization;
using System.Reflection;
using FakeUnityEngine;

namespace DataParsing
{
	public class DataNodeSerializer
	{
		public static DataNode Serialize<T>(T obj, string nodeName = null)
		{
			DataNode dataNode = new DataNode((!string.IsNullOrEmpty(nodeName)) ? nodeName : obj.GetType().Name);
			DataNodeSerializer.SerializeToNode<T>(dataNode, obj);
			return dataNode;
		}

		public static void SerializeToNode<T>(DataNode holder, T obj)
		{
			Type type = obj.GetType();
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
						goto IL_78;
					}
				}
				else
				{
					object[] customAttributes2 = fieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true);
					if (customAttributes2.Length == 0)
					{
						goto IL_78;
					}
				}
				IL_1E5:
				i++;
				continue;
				IL_78:
				Type fieldType = fieldInfo.FieldType;
				if (fieldType == typeof(string) || fieldType == typeof(bool) || fieldType == typeof(int) || fieldType.IsEnum)
				{
					string value = fieldInfo.GetValue(obj).ToString();
					holder.AddString(fieldInfo.Name, value);
					goto IL_1E5;
				}
				if (fieldType == typeof(float))
				{
					string value2 = ((float)fieldInfo.GetValue(obj)).ToString(CultureInfo.InvariantCulture);
					holder.AddString(fieldInfo.Name, value2);
					goto IL_1E5;
				}
				if (fieldType.IsArray)
				{
					Type elementType = fieldType.GetElementType();
					object value3 = fieldInfo.GetValue(obj);
					if (elementType == typeof(float))
					{
						holder.AddFloatArray(fieldInfo.Name, value3 as float[]);
					}
					else if (elementType == typeof(int))
					{
						holder.AddIntArray(fieldInfo.Name, value3 as int[]);
					}
					else
					{
						Debug.LogError("Unsupported array type " + elementType);
					}
					goto IL_1E5;
				}
				if (fieldType.IsSerializable)
				{
				}
				Debug.LogError("Unsupported serializable object " + fieldInfo.Name + " in " + type.Name);
				goto IL_1E5;
			}
		}

		public static void Deserialize<T>(T obj, DataNode node)
		{
			if (obj == null)
			{
				Debug.LogError("Can't deserialize null object!");
				return;
			}
			Type type = obj.GetType();
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
						goto IL_8E;
					}
				}
				else
				{
					object[] customAttributes2 = fieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true);
					if (customAttributes2.Length == 0)
					{
						goto IL_8E;
					}
				}
				IL_29A:
				i++;
				continue;
				IL_8E:
				if (node == null)
				{
					goto IL_29A;
				}
				if (node.FindNode(fieldInfo.Name) == null)
				{
					goto IL_29A;
				}
				Type fieldType = fieldInfo.FieldType;
				if (fieldType.IsEnum)
				{
					fieldInfo.SetValue(obj, node.FindEnumValue(fieldInfo.Name, fieldType));
					goto IL_29A;
				}
				if (fieldType == typeof(string))
				{
					fieldInfo.SetValue(obj, node.FindValue(fieldInfo.Name));
					goto IL_29A;
				}
				if (fieldType == typeof(bool))
				{
					fieldInfo.SetValue(obj, node.FindBoolValue(fieldInfo.Name, false));
					goto IL_29A;
				}
				if (fieldType == typeof(int))
				{
					fieldInfo.SetValue(obj, node.FindIntValue(fieldInfo.Name, 0));
					goto IL_29A;
				}
				if (fieldType == typeof(float))
				{
					fieldInfo.SetValue(obj, node.FindFloatValue(fieldInfo.Name, 0f));
					goto IL_29A;
				}
				if (fieldType.IsArray)
				{
					Type elementType = fieldType.GetElementType();
					DataNode dataNode = node.FindNode(fieldInfo.Name);
					object value = null;
					if (dataNode != null && dataNode.IsList)
					{
						if (elementType == typeof(float))
						{
							value = dataNode.ReadAsFloatArray();
						}
						else if (elementType == typeof(int))
						{
							value = dataNode.ReadAsIntArray();
						}
						else
						{
							Debug.LogError("Unsupported array type " + elementType);
						}
					}
					fieldInfo.SetValue(obj, value);
					goto IL_29A;
				}
				Debug.LogError(string.Concat(new object[]
				{
					"Field type ",
					fieldInfo.Name,
					" in ",
					obj.GetType(),
					" is of unsupported type ",
					fieldInfo.FieldType
				}));
				goto IL_29A;
			}
		}

		public static T Deserizalize<T>(DataNode node)
		{
			T t = Activator.CreateInstance<T>();
			DataNodeSerializer.Deserialize<T>(t, node);
			return t;
		}
	}
}
