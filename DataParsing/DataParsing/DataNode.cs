using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FakeUnityEngine;

namespace DataParsing
{

	public class DataNode : IEnumerable<DataNode>, IEnumerable
	{
		public string Name;

		public string Content;

		public List<DataNode> Nodes = new List<DataNode>();

		public bool IsList;

		public int Count
		{
			get
			{
				return this.Nodes.Count;
			}
		}

		public int TotalCount
		{
			get
			{
				int num = 1;
				foreach (DataNode current in this.Nodes)
				{
					num += current.TotalCount;
				}
				return num;
			}
		}

		public DataNode this[string id]
		{
			get
			{
				return this.FindNode(id);
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value.Name != id)
				{
					throw new ArgumentException();
				}
				if (this.Nodes == null || this.Nodes.Count == 0)
				{
					this.AddNode(value);
					return;
				}
				for (int i = 0; i < this.Nodes.Count; i++)
				{
					if (this.Nodes[i].Name == id)
					{
						this.Nodes[i] = value;
						return;
					}
				}
				this.AddNode(value);
			}
		}

		public DataNode(string name)
		{
			this.Name = name;
		}

		public DataNode(string name, bool list)
		{
			this.Name = name;
			this.IsList = list;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void AddNode(DataNode node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (this.Nodes == null)
			{
				this.Nodes = new List<DataNode>();
			}
			this.Nodes.Add(node);
		}

		public DataNode AddNode(string nodeName)
		{
			DataNode dataNode = new DataNode(nodeName);
			this.AddNode(dataNode);
			return dataNode;
		}

		public DataNode AddString(string id, string value)
		{
			DataNode dataNode = new DataNode(id)
			{
				Content = value
			};
			this.AddNode(dataNode);
			return dataNode;
		}

		public DataNode AddBool(string id, bool value)
		{
			DataNode dataNode = new DataNode(id)
			{
				Content = ((!value) ? "False" : "True")
			};
			this.AddNode(dataNode);
			return dataNode;
		}

		public DataNode AddInt(string name, int val)
		{
			DataNode dataNode = this.AddNode(name);
			dataNode.Content = val.ToString(CultureInfo.InvariantCulture);
			return dataNode;
		}

		public DataNode AddIntArray(string name, int[] val)
		{
			DataNode dataNode = this.AddNode(name);
			dataNode.IsList = true;
			if (val == null || val.Length == 0)
			{
				return dataNode;
			}
			for (int i = 0; i < val.Length; i++)
			{
				dataNode.AddInt(null, val[i]);
			}
			return dataNode;
		}

		public DataNode AddFloatArray(string name, float[] val)
		{
			DataNode dataNode = this.AddNode(name);
			dataNode.IsList = true;
			if (val == null || val.Length == 0)
			{
				return dataNode;
			}
			for (int i = 0; i < val.Length; i++)
			{
				dataNode.AddFloat(null, val[i]);
			}
			return dataNode;
		}

		public DataNode AddFloat(string name, float val)
		{
			DataNode dataNode = this.AddNode(name);
			dataNode.Content = val.ToString(CultureInfo.InvariantCulture);
			return dataNode;
		}

		public DataNode AddVector(string name, Vector3 val)
		{
			DataNode dataNode = this.AddNode(name);
			dataNode.AddFloat("X", val.x);
			dataNode.AddFloat("Y", val.y);
			dataNode.AddFloat("Z", val.z);
			return dataNode;
		}

		public DataNode AddVector2D(string name, Vector2 val)
		{
			DataNode dataNode = this.AddNode(name);
			dataNode.AddFloat("X", val.x);
			dataNode.AddFloat("Y", val.y);
			return dataNode;
		}

		public DataNode AddQuaternion(string name, Quaternion val)
		{
			DataNode dataNode = this.AddNode(name);
			dataNode.AddFloat("X", val.x);
			dataNode.AddFloat("Y", val.y);
			dataNode.AddFloat("Z", val.z);
			dataNode.AddFloat("W", val.w);
			return dataNode;
		}

		public bool ReadAsInt(out int outValue)
		{
			if (string.IsNullOrEmpty(this.Content))
			{
				outValue = 0;
				return false;
			}
			return int.TryParse(this.Content, out outValue);
		}

		public bool ReadAsEnum<T>(out T outValue) where T : struct
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("Enum required!");
			}
			outValue = default(T);
			if (string.IsNullOrEmpty(this.Content))
			{
				return false;
			}
			T t;
			try
			{
				t = (T)((object)Enum.Parse(typeof(T), this.Content));
			}
			catch (Exception)
			{
				Debug.LogError("Can't parse " + this.Content + " as " + typeof(T).ToString());
				return false;
			}
			outValue = t;
			return true;
		}

		public bool ReadAsFloat(out float outValue)
		{
			if (string.IsNullOrEmpty(this.Content))
			{
				outValue = 0f;
				return false;
			}
			return float.TryParse(this.Content, out outValue);
		}

		public bool ReadAsVector(out Vector3 outValue)
		{
			outValue = Vector3.zero;
			if (this.Nodes.Count != 3)
			{
				return false;
			}
			outValue.x = this.FindFloatValue("X", 0f);
			outValue.y = this.FindFloatValue("Y", 0f);
			outValue.z = this.FindFloatValue("Z", 0f);
			return true;
		}

		public bool ReadAsVector2D(out Vector2 outValue)
		{
			outValue = Vector2.zero;
			if (this.Nodes.Count != 2)
			{
				return false;
			}
			outValue.x = this.FindFloatValue("X", 0f);
			outValue.y = this.FindFloatValue("Y", 0f);
			return true;
		}

		public bool ReadAsQuaternion(out Quaternion outValue)
		{
			outValue = Quaternion.identity;
			if (this.Nodes.Count != 4)
			{
				return false;
			}
			outValue.x = this.FindFloatValue("X", 0f);
			outValue.y = this.FindFloatValue("Y", 0f);
			outValue.z = this.FindFloatValue("Z", 0f);
			outValue.w = this.FindFloatValue("W", 0f);
			return true;
		}

        public static Dictionary<string, int> switchMap = null;

		public bool ReadAsBool(out bool outValue)
		{
			string content = this.Content;
			if (content != null)
			{
				if (DataNode.switchMap == null)
				{
					DataNode.switchMap = new Dictionary<string, int>(6)
					{
						{
							"True",
							0
						},
						{
							"true",
							0
						},
						{
							"1",
							0
						},
						{
							"False",
							1
						},
						{
							"false",
							1
						},
						{
							"0",
							1
						}
					};
				}
				int num;
				if (DataNode.switchMap.TryGetValue(content, out num))
				{
					if (num == 0)
					{
						outValue = true;
						return true;
					}
					if (num == 1)
					{
						outValue = false;
						return true;
					}
				}
			}
			outValue = false;
			return false;
		}

		public int[] ReadAsIntArray()
		{
			if (!this.IsList)
			{
				return null;
			}
			int[] array = new int[this.Nodes.Count];
			for (int i = 0; i < this.Nodes.Count; i++)
			{
				int num;
				this.Nodes[i].ReadAsInt(out num);
				array[i] = num;
			}
			return array;
		}

		public float[] ReadAsFloatArray()
		{
			if (!this.IsList)
			{
				return null;
			}
			float[] array = new float[this.Nodes.Count];
			for (int i = 0; i < this.Nodes.Count; i++)
			{
				float num;
				this.Nodes[i].ReadAsFloat(out num);
				array[i] = num;
			}
			return array;
		}

		public DataNode FindNode(string id)
		{
			if (this.Nodes == null || this.Nodes.Count == 0)
			{
				return null;
			}
			foreach (DataNode current in this.Nodes)
			{
				if (current.Name == id)
				{
					return current;
				}
			}
			return null;
		}

		public DataNode FindOrCreateNode(string id)
		{
			return this[id];
		}

		public string FindValue(string id)
		{
			DataNode dataNode = this.FindNode(id);
			if (dataNode == null)
			{
				return null;
			}
			return dataNode.Content;
		}

		public bool FindFloatValue(string id, out float outValue)
		{
			outValue = 0f;
			DataNode dataNode = this.FindNode(id);
			return dataNode != null && dataNode.ReadAsFloat(out outValue);
		}

		public bool FindIntValue(string id, out int outValue)
		{
			outValue = 0;
			DataNode dataNode = this.FindNode(id);
			return dataNode != null && dataNode.ReadAsInt(out outValue);
		}

		public int FindIntValue(string id, int defValue = 0)
		{
			DataNode dataNode = this.FindNode(id);
			if (dataNode == null)
			{
				return defValue;
			}
			int result;
			if (dataNode.ReadAsInt(out result))
			{
				return result;
			}
			return defValue;
		}

		public float FindFloatValue(string id, float defValue = 0f)
		{
			DataNode dataNode = this.FindNode(id);
			if (dataNode == null)
			{
				return defValue;
			}
			float result;
			if (dataNode.ReadAsFloat(out result))
			{
				return result;
			}
			return defValue;
		}

		public bool FindBoolValue(string id, bool defValue = false)
		{
			DataNode dataNode = this.FindNode(id);
			if (dataNode == null)
			{
				return defValue;
			}
			bool result;
			if (dataNode.ReadAsBool(out result))
			{
				return result;
			}
			return defValue;
		}

		public int FindEnumValue(string id, Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Enum required!");
			}
			string text = this.FindValue(id);
			if (string.IsNullOrEmpty(text))
			{
				return 0;
			}
			int result;
			try
			{
				result = (int)Enum.Parse(enumType, text);
			}
			catch (Exception)
			{
				Debug.LogError("Can't parse " + text + " as " + enumType.ToString());
				return 0;
			}
			return result;
		}

		public T FindEnumValue<T>(string id, T defValue) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("Enum required!");
			}
			string text = this.FindValue(id);
			if (string.IsNullOrEmpty(text))
			{
				return defValue;
			}
			T result;
			try
			{
				result = (T)((object)Enum.Parse(typeof(T), text));
			}
			catch (Exception)
			{
				Debug.LogError("Can't parse " + text + " as " + typeof(T).ToString());
				return defValue;
			}
			return result;
		}

		public Vector2 FindVector2DValue(string id)
		{
			return this.FindVector2DValue(id, Vector2.zero);
		}

		public Vector2 FindVector2DValue(string id, Vector2 defValue)
		{
			DataNode dataNode = this.FindNode(id);
			if (dataNode == null)
			{
				return defValue;
			}
			Vector2 result;
			if (dataNode.ReadAsVector2D(out result))
			{
				return result;
			}
			return defValue;
		}

		public Vector3 FindVectorValue(string id)
		{
			return this.FindVectorValue(id, Vector3.zero);
		}

		public Vector3 FindVectorValue(string id, Vector3 defValue)
		{
			DataNode dataNode = this.FindNode(id);
			if (dataNode == null)
			{
				return defValue;
			}
			Vector3 result;
			if (dataNode.ReadAsVector(out result))
			{
				return result;
			}
			return defValue;
		}

		public Quaternion FindQuaternionValue(string id, Quaternion defValue)
		{
			DataNode dataNode = this.FindNode(id);
			if (dataNode == null)
			{
				return defValue;
			}
			Quaternion result;
			if (dataNode.ReadAsQuaternion(out result))
			{
				return result;
			}
			return defValue;
		}

		public List<string> FindStringList(string id)
		{
			List<string> list = new List<string>();
			DataNode dataNode = this.FindNode(id);
			if (dataNode != null)
			{
				if (!dataNode.IsList)
				{
					Debug.LogError("Data node is not a list - id " + id);
				}
				else
				{
					for (int i = 0; i < dataNode.Nodes.Count; i++)
					{
						list.Add(dataNode.Nodes[i].Content);
					}
				}
			}
			return list;
		}

		public void DeleteNode(DataNode node)
		{
			if (this.Nodes != null)
			{
				this.Nodes.Remove(node);
			}
		}

		public void DeleteNode(string id, bool all = true)
		{
			if (this.Nodes == null)
			{
				return;
			}
			int i = 0;
			while (i < this.Nodes.Count)
			{
				if (this.Nodes[i].Name == id)
				{
					this.Nodes.RemoveAt(i);
					if (!all)
					{
						return;
					}
				}
				else
				{
					i++;
				}
			}
		}

		public IEnumerator<DataNode> GetEnumerator()
		{
			return this.Nodes.GetEnumerator();
		}

		public void FlushToLog()
		{
			Debug.Log(this.Name + " : " + this.Content);
			foreach (DataNode current in this.Nodes)
			{
				current.FlushToLog();
			}
			Debug.Log("End " + this.Name);
		}
	}
}
