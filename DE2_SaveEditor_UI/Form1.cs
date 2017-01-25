using DataParsing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DE2_SaveEditor_UI {
    public partial class Form1 : Form {
        //public readonly string BASE_PATH = @"F:\dl\DE2 Save Editor\";
        public readonly string BASE_PATH = @"C:\Users\Franck\AppData\LocalLow\BadFly Interactive\Dead Effect 2\";
        public readonly string PROFILE_FILE = "profile0.sav";
        public readonly string ACHIEVEMENT_FILE = "achievement.data";
        public readonly string DUMP_FILE = "profile_dump.txt";
        public bool isBackupOK = false;

        public DataNode saveFile, achievements;

        public Form1() {
            InitializeComponent();

            saveFile = DataNodeBinary.FromBinaryFile(BASE_PATH + PROFILE_FILE);
            achievements = DataNodeBinary.FromBinaryFile(BASE_PATH + ACHIEVEMENT_FILE);

            InitializeTreeView();

            InitializeBasicStats();

            InitializeRawAchievements();
        }

        private void InitializeRawAchievements() {
            foreach(DataNode node in achievements) {
                TreeNode tNode = new TreeNode(node.Name);
                tNode.Tag = node;
                AddNodes(node, tNode);
                treeView2.Nodes.Add(tNode);
            }
            treeView2.AfterLabelEdit += TreeView1_AfterLabelEdit;
            treeView2.NodeMouseDoubleClick += TreeView1_NodeMouseDoubleClick;
        }

        public void InitializeTreeView() {
            foreach (DataNode node in saveFile) {
                TreeNode tNode = new TreeNode(node.Name);
                tNode.Tag = node;
                AddNodes(node, tNode);
                treeView1.Nodes.Add(tNode);
            }

            treeView1.NodeMouseDoubleClick += TreeView1_NodeMouseDoubleClick;
            treeView1.AfterLabelEdit += TreeView1_AfterLabelEdit;
        }

        private void TreeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if (e.Label == null) {
                e.Node.Text = (e.Node.Tag as DataNode).Name;
                e.CancelEdit = true;
                return;
            }

            (e.Node.Tag as DataNode).Content = e.Label;
            e.Node.Text = (e.Node.Tag as DataNode).Name;
            e.CancelEdit = true;

            if (!isBackupOK) {
                isBackupOK = true;
                File.Copy(BASE_PATH + PROFILE_FILE, BASE_PATH + PROFILE_FILE + ".bak", true);
            }
        }

        private void TreeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
            e.Node.Text = (e.Node.Tag as DataNode).Content;
            e.Node.BeginEdit();
        }

        private void AddNodes(DataNode dataNode, TreeNode parent) {
            foreach (DataNode node in dataNode) {
                TreeNode tNode = new TreeNode(node.Name);
                tNode.Tag = node;
                AddNodes(node, tNode);
                parent.Nodes.Add(tNode);
            }
        }

        private void button1_Click(object sender, EventArgs e) {

        }

        private void button2_Click(object sender, EventArgs e) {
            PrintDataNode(new StreamWriter(BASE_PATH + DUMP_FILE), saveFile);
        }


        #region print
        static void PrintDataNode(StreamWriter f, DataNode node) {
            PrintDataNode(f, node, 0);
        }

        static void PrintDataNode(StreamWriter f, DataNode node, int level) { 
            f.Write(new string('\t', level) + node.Name);
            if (node.Count > 0) {
                f.Write("\n");
                foreach (DataNode child in node.Nodes) {
                    PrintDataNode(f, child, level + 1);
                }
            } else {
                f.Write(" -> " + node.Content + "\n");
            }
        }
        #endregion

        private void button3_Click(object sender, EventArgs e) {
            string invalidIDs = "";
            ListInvalidIDs(ref invalidIDs, saveFile, "");

            MessageBox.Show(invalidIDs);
        }


        private void ListInvalidIDs(ref string invalidIDs, DataNode file, string path) {
            invalidIDs += CheckID(file, path) ? path + "\\" + file.Name + "\n" : "";
            
            foreach (DataNode child in file.Nodes) {
                ListInvalidIDs(ref invalidIDs, child, path + "\\" + file.Name);
            }
        }

        private bool CheckID(DataNode file, string path) {
            if (file.Content != null) return false;

            switch (file.Name) {
                case "Level":
                case "BonusLevel":
                case "ItemType":
                case "DescriptionGuid":
                    return true;
                default:
                    return false;
            }
        }

        public void InitializeBasicStats() {
            GroupBox boundingBox;
            FlowLayoutPanel parentPanel;
            TextBox text;
            Label caption;

            boundingBox = new GroupBox();
            boundingBox.Layout += BoundingBox_Layout;
            boundingBox.Text = "Ammo counters";
            boundingBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            boundingBox.Width = tabPage1.Width - 8;
            boundingBox.Left = 4;
            boundingBox.Top = 2;
            
            foreach(DataNode ammo in saveFile["CharacterInventory"]["Slots"]["Ammo"]["Ammos"]) {
                parentPanel = new FlowLayoutPanel();
                text = new TextBox();
                caption = new Label();

                caption.Text = ammo["Type"].Content;
                caption.Height = 15;
                text.Text = ammo["Ammo"].Content;
                text.Width = 90;

                parentPanel.Controls.Add(caption);
                parentPanel.Controls.Add(text);
                parentPanel.Width = 100;
                parentPanel.Height = 40;
                
                boundingBox.Controls.Add(parentPanel);
            }
            tabPage1.Controls.Add(boundingBox);
        }

        private void BoundingBox_Layout(object sender, LayoutEventArgs e) {
            int top = 20, left = 10;
            Random r = new Random();

            foreach(Control item in (sender as GroupBox).Controls) {
                if (left + 100 > (sender as GroupBox).Width) {
                    top += 40; left = 10;
                }
                item.Top = top;
                item.Left = left;
                left += 100;
            }

            (sender as GroupBox).Height = top + 50;
        }

        private void button4_Click(object sender, EventArgs e) {
            DataNodeBinary.ToBinaryFile(saveFile, BASE_PATH + PROFILE_FILE, DataNodeBinary.BinaryFormat.Encrypted);
        }
    }
}
