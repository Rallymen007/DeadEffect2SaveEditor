using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using FakeUnityEngine;

namespace DataParsing
{
	public static class DataNodeBinary
	{
		public enum BinaryFormat
		{
			Simple,
			SimpleCompresssed,
			Encrypted
		}

		private const string BinaryHeaderId = "DEF2";

		private const int UncompressedHeader = 1;

		private const int CompressedHeader = 2;

		private const int CompressedEncrypted = 3;

		private const byte EmptyNodeMarker = 0;

		private const byte ContentNodeMarker = 1;

		private const byte ListNodeMarker = 2;

		private const byte ObjectNodeMarker = 3;

		private const byte HeaderEndMarker = 4;

		private const byte FileEndMarker = 5;

		private static int m_HeaderIndex = 1;

		private static byte[] m_EncryptKey = new byte[]
		{
			153,
			75,
			98,
			45,
			67,
			128,
			98,
			35,
			123,
			11,
			207,
			19,
			178,
			117,
			241,
			1
		};

		private static byte[] m_EncryptVector = new byte[]
		{
			17,
			187,
			98,
			254,
			111,
			7,
			201,
			205,
			32,
			48,
			159,
			74,
			251,
			167,
			162,
			11
		};

		private static DataNode ReadCompressed(BinaryReader reader)
		{
			List<string> list = DataNodeBinary.ReadHeader(reader);
			if (list == null)
			{
				return null;
			}
			return DataNodeBinary.ReadFromBinary(reader, list);
		}

		private static DataNode ReadEncrypted(BinaryReader reader)
		{
			DataNode result = null;
			using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
			{
				rijndaelManaged.Key = DataNodeBinary.m_EncryptKey;
				rijndaelManaged.IV = DataNodeBinary.m_EncryptVector;
				ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
				CryptoStream input = new CryptoStream(reader.BaseStream, cryptoTransform, CryptoStreamMode.Read);
				BinaryReader binaryReader = new BinaryReader(input);
				int num = binaryReader.ReadInt32();
				switch (num)
				{
				case 1:
					result = DataNodeBinary.ReadFromBinary(binaryReader, null);
					break;
				case 2:
					result = DataNodeBinary.ReadCompressed(binaryReader);
					break;
				case 3:
					Debug.LogError("Double encryption!!!");
					break;
				default:
					Debug.LogError("Unknown version " + num);
					break;
				}
				cryptoTransform.Dispose();
			}
			return result;
		}

		private static List<string> ReadHeader(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			List<string> list = new List<string>(num + 1);
			list.Add(string.Empty);
			for (int i = 0; i < num; i++)
			{
				string item = reader.ReadString();
				list.Add(item);
			}
			byte b = reader.ReadByte();
			if (b != 4)
			{
				throw new Exception("Malformed header!!!!");
			}
			return list;
		}

		private static DataNode ReadFromBinary(BinaryReader br, List<string> header)
		{
			byte b = br.ReadByte();
			switch (b)
			{
			case 0:
				return DataNodeBinary.ReadEmptyNode(br, header);
			case 1:
				return DataNodeBinary.ReadContentNode(br, header);
			case 2:
				return DataNodeBinary.ReadSubNodes(br, true, header);
			case 3:
				return DataNodeBinary.ReadSubNodes(br, false, header);
			case 5:
				throw new Exception("Unknown node type " + b);
			}
			throw new Exception("Unknown node type " + b);
		}

		private static string ReadString(BinaryReader br, List<string> header)
		{
			if (header == null)
			{
				return br.ReadString();
			}
			int num;
			if (header.Count < 65535)
			{
				num = (int)br.ReadUInt16();
			}
			else
			{
				num = br.ReadInt32();
			}
			if (num < 0 || num > header.Count)
			{
				throw new Exception("Wrong index counter in compressed file.");
			}
			return header[num];
		}

		private static DataNode ReadEmptyNode(BinaryReader br, List<string> header)
		{
			return new DataNode(DataNodeBinary.ReadString(br, header));
		}

		private static DataNode ReadContentNode(BinaryReader br, List<string> header)
		{
			return new DataNode(DataNodeBinary.ReadString(br, header))
			{
				Content = DataNodeBinary.ReadString(br, header)
			};
		}

		private static DataNode ReadSubNodes(BinaryReader br, bool isList, List<string> header)
		{
			DataNode dataNode = new DataNode(DataNodeBinary.ReadString(br, header));
			dataNode.IsList = isList;
			int num = br.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				DataNode node = DataNodeBinary.ReadFromBinary(br, header);
				dataNode.AddNode(node);
			}
			return dataNode;
		}

		private static void WriteSimpleFormat(DataNode mainNode, BinaryWriter writer)
		{
			writer.Write(1);
			DataNodeBinary.WriteDataToBinary(mainNode, writer, null);
			writer.Write((byte)5);
			writer.Flush();
		}

		private static void WriteCompressedFormat(DataNode mainNode, BinaryWriter writer)
		{
			writer.Write(2);
			Dictionary<string, int> header = DataNodeBinary.WriteCompressedHeader(mainNode, writer);
			DataNodeBinary.WriteDataToBinary(mainNode, writer, header);
			writer.Write((byte)5);
			writer.Flush();
		}

		private static void WriteEncryptedFormat(DataNode mainNode, BinaryWriter writer)
		{
			writer.Write(3);
			using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
			{
				rijndaelManaged.Key = DataNodeBinary.m_EncryptKey;
				rijndaelManaged.IV = DataNodeBinary.m_EncryptVector;
				ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
					{
						BinaryWriter binaryWriter = new BinaryWriter(cryptoStream);
						DataNodeBinary.WriteCompressedFormat(mainNode, binaryWriter);
						binaryWriter.Flush();
						cryptoStream.Flush();
					}
					memoryStream.Flush();
					writer.Write(memoryStream.ToArray());
				}
				cryptoTransform.Dispose();
			}
			writer.Flush();
		}

		private static Dictionary<string, int> WriteCompressedHeader(DataNode mainNode, BinaryWriter writer)
		{
			int totalCount = mainNode.TotalCount;
			Dictionary<string, int> dictionary = new Dictionary<string, int>(totalCount);
			List<string> list = new List<string>(totalCount);
			DataNodeBinary.m_HeaderIndex = 1;
			DataNodeBinary.AddToHeader(dictionary, list, mainNode);
			writer.Write(dictionary.Count);
			foreach (string current in list)
			{
				writer.Write(current);
			}
			writer.Write((byte)4);
			return dictionary;
		}

		private static void AddToHeader(Dictionary<string, int> header, List<string> keys, DataNode node)
		{
			if (!string.IsNullOrEmpty(node.Name) && !header.ContainsKey(node.Name))
			{
				header.Add(node.Name, DataNodeBinary.m_HeaderIndex);
				keys.Add(node.Name);
				DataNodeBinary.m_HeaderIndex++;
			}
			if (!string.IsNullOrEmpty(node.Content) && !header.ContainsKey(node.Content))
			{
				header.Add(node.Content, DataNodeBinary.m_HeaderIndex);
				keys.Add(node.Content);
				DataNodeBinary.m_HeaderIndex++;
			}
			foreach (DataNode current in node.Nodes)
			{
				DataNodeBinary.AddToHeader(header, keys, current);
			}
		}

		private static void WriteCachedString(string val, BinaryWriter bw, Dictionary<string, int> header)
		{
			int num = 0;
			if (!string.IsNullOrEmpty(val))
			{
				num = header[val];
			}
			if (header.Count < 65535)
			{
				bw.Write((ushort)num);
			}
			else
			{
				bw.Write(num);
			}
		}

		private static void WriteEmptyNode(DataNode node, BinaryWriter bw, Dictionary<string, int> header)
		{
			bw.Write((byte)0);
			DataNodeBinary.WriteNodeName(node, bw, header);
		}

		private static void WriteNodeName(DataNode node, BinaryWriter bw, Dictionary<string, int> header)
		{
			if (header == null)
			{
				bw.Write(node.Name);
			}
			else
			{
				DataNodeBinary.WriteCachedString(node.Name, bw, header);
			}
		}

		private static void WriteSubNodes(DataNode node, BinaryWriter bw, Dictionary<string, int> header)
		{
			if (node.IsList)
			{
				bw.Write((byte)2);
			}
			else
			{
				bw.Write((byte)3);
			}
			DataNodeBinary.WriteNodeName(node, bw, header);
			bw.Write(node.Count);
			for (int i = 0; i < node.Nodes.Count; i++)
			{
				DataNode node2 = node.Nodes[i];
				DataNodeBinary.WriteDataToBinary(node2, bw, header);
			}
		}

		private static void WriteContentNode(DataNode node, BinaryWriter bw, Dictionary<string, int> header)
		{
			if (node.Nodes != null && node.Nodes.Count > 0)
			{
				throw new Exception("Node can't contain content and subnodes");
			}
			bw.Write((byte)1);
			DataNodeBinary.WriteNodeName(node, bw, header);
			if (header != null)
			{
				DataNodeBinary.WriteCachedString(node.Content, bw, header);
			}
			else
			{
				bw.Write(node.Content);
			}
		}

		public static void WriteDataToBinary(DataNode node, BinaryWriter bw, Dictionary<string, int> header)
		{
			if (node == null)
			{
				throw new ArgumentNullException();
			}
			if (string.IsNullOrEmpty(node.Content))
			{
				if (node.Nodes == null || node.Nodes.Count == 0)
				{
					DataNodeBinary.WriteEmptyNode(node, bw, header);
				}
				else
				{
					DataNodeBinary.WriteSubNodes(node, bw, header);
				}
			}
			else
			{
				DataNodeBinary.WriteContentNode(node, bw, header);
			}
		}

		public static void ToBinaryFile(DataNode mainNode, string fileName, DataNodeBinary.BinaryFormat format = DataNodeBinary.BinaryFormat.Encrypted)
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(fileName, FileMode.Create)))
			{
				DataNodeBinary.ToBinaryStream(mainNode, binaryWriter, format);
			}
		}

		public static void ToBinaryStream(DataNode mainNode, BinaryWriter writer, DataNodeBinary.BinaryFormat format)
		{
			writer.Write("DEF2");
			switch (format)
			{
			case DataNodeBinary.BinaryFormat.Simple:
				DataNodeBinary.WriteSimpleFormat(mainNode, writer);
				break;
			case DataNodeBinary.BinaryFormat.SimpleCompresssed:
				DataNodeBinary.WriteCompressedFormat(mainNode, writer);
				break;
			case DataNodeBinary.BinaryFormat.Encrypted:
				DataNodeBinary.WriteEncryptedFormat(mainNode, writer);
				break;
			default:
				throw new ArgumentOutOfRangeException("format");
			}
		}

		public static byte[] ToBinaryBytes(DataNode mainNode, DataNodeBinary.BinaryFormat format = DataNodeBinary.BinaryFormat.Encrypted)
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream(65536))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					DataNodeBinary.ToBinaryStream(mainNode, binaryWriter, format);
					result = memoryStream.ToArray();
				}
			}
			return result;
		}

		public static DataNode FromBinaryFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				Debug.LogError("File " + fileName + " doesn't exist");
				return null;
			}
			DataNode result;
			try
			{
				using (BinaryReader binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
				{
					result = DataNodeBinary.FromBinaryStream(binaryReader);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Binary file load failed!");
				Debug.LogException(exception);
				result = null;
			}
			return result;
		}

		public static DataNode FromBinaryStream(BinaryReader reader)
		{
			string text = reader.ReadString();
			if (text != "DEF2")
			{
				Debug.LogError("Unknown header " + text);
				return null;
			}
			int num = reader.ReadInt32();
			switch (num)
			{
			case 1:
				return DataNodeBinary.ReadFromBinary(reader, null);
			case 2:
				return DataNodeBinary.ReadCompressed(reader);
			case 3:
				return DataNodeBinary.ReadEncrypted(reader);
			default:
				Debug.LogError("Unknown version " + num);
				return null;
			}
		}

		public static DataNode FromBinaryBytes(byte[] bytes)
		{
			DataNode result;
			try
			{
				using (MemoryStream memoryStream = new MemoryStream(bytes))
				{
					using (BinaryReader binaryReader = new BinaryReader(memoryStream))
					{
						result = DataNodeBinary.FromBinaryStream(binaryReader);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Binary bytes load failed!");
				Debug.LogException(exception);
				result = null;
			}
			return result;
		}
	}
}
