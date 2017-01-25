using System;
using System.Collections;
using System.Collections.Generic;

namespace DataParsing
{
	public static class DataNodeDictionary
	{
		public static IDictionary<string, object> ToDictionary(DataNode dataNode)
		{
			return DataNodeDictionary.ContentToDictionary(dataNode) as IDictionary<string, object>;
		}

		private static object ContentToDictionary(DataNode dataNode)
		{
			if (dataNode.Nodes.Count == 0)
			{
				return dataNode.Content;
			}
			if (dataNode.IsList)
			{
				List<object> list = new List<object>();
				for (int i = 0; i < dataNode.Nodes.Count; i++)
				{
					DataNode dataNode2 = dataNode.Nodes[i];
					list.Add(DataNodeDictionary.ContentToDictionary(dataNode2));
				}
				return list;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			for (int j = 0; j < dataNode.Nodes.Count; j++)
			{
				DataNode dataNode3 = dataNode.Nodes[j];
				dictionary[dataNode3.Name] = DataNodeDictionary.ContentToDictionary(dataNode3);
			}
			return dictionary;
		}

		public static DataNode FromDictionary(IDictionary<string, object> dictionary)
		{
			DataNode dataNode = new DataNode("rval");
			foreach (KeyValuePair<string, object> current in dictionary)
			{
				dataNode.AddNode(DataNodeDictionary.FromObject(current.Key, current.Value));
			}
			return dataNode;
		}

		private static DataNode FromObject(string name, object obj)
		{
			IList list = obj as IList;
			if (list != null)
			{
				DataNode dataNode = new DataNode(name, true);
				foreach (object current in list)
				{
					dataNode.AddNode(DataNodeDictionary.FromObject(null, current));
				}
				return dataNode;
			}
			Dictionary<string, object> dictionary = obj as Dictionary<string, object>;
			if (dictionary != null)
			{
				DataNode dataNode2 = new DataNode(name, false);
				foreach (KeyValuePair<string, object> current2 in dictionary)
				{
					dataNode2.AddNode(DataNodeDictionary.FromObject(current2.Key, current2.Value));
				}
				return dataNode2;
			}
			DataNode dataNode3 = new DataNode(name, false);
			if (obj != null)
			{
				dataNode3.Content = obj.ToString();
			}
			else
			{
				dataNode3.Content = string.Empty;
			}
			return dataNode3;
		}
	}
}
