using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Onvif.Controls
{
    public partial class MediaClientProfilesForm : Form
    {
        deviceio.Profile[] mediaProfiles;

        public MediaClientProfilesForm(deviceio.Profile[] profiles)
        {
            mediaProfiles = profiles;
            InitializeComponent();
            MediaProfilesTreeViewInit();
        }
        private void MediaProfilesTreeViewInit()
        {
            XmlSerializer serialization = new XmlSerializer(mediaProfiles.GetType());

            MemoryStream memory = new MemoryStream();
            try
            {
                serialization.Serialize(memory, mediaProfiles);
                memory.Seek(0, System.IO.SeekOrigin.Begin);

                XmlDocument xml = new XmlDocument();
                xml.Load(memory);

                MediaProfilesTreeView.Nodes.Clear();
                MediaProfilesTreeView.Nodes.Add(new TreeNode(xml.DocumentElement.Name));
                TreeNode node = new TreeNode();
                node = MediaProfilesTreeView.Nodes[0];

                AddNode(xml.DocumentElement, node);
                MediaProfilesTreeView.ExpandAll();
            }
            catch (XmlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                memory.Close();
            }
           
        }

        private void AddNode(XmlNode xNode, TreeNode tNode)
        {
            XmlNode xmlNode;
            TreeNode treeNode;
            XmlNodeList nodeList;

            if (xNode.HasChildNodes)
            {
                nodeList = xNode.ChildNodes;
                for (int index = 0; index <= nodeList.Count - 1; index++)
                {
                    xmlNode = xNode.ChildNodes[index];
                    tNode.Nodes.Add(new TreeNode(xmlNode.Name));
                    treeNode = tNode.Nodes[index];
                    AddNode(xmlNode, treeNode);
                }
            }
            else
            {
                tNode.Text = (xNode.OuterXml).Trim();
            }
        }       
    }
}

//foreach (deviceio.Profile profile in mediaProfiles)
//{
//    TreeNode root = new TreeNode(profile.Name);
//    MediaProfilesTreeView.Nodes.Add(root);

//    //foreach (PropertyInfo info in profile.GetType().GetProperties())
//    //{
//    //    string name = info.Name;
//    //    string value = info.GetValue(profile, null).ToString();
//    //    TreeNode node = new TreeNode(name + " " + value);// + " " + info.GetValue(profile, null).ToString());
//    //    root.Nodes.Add(node);
//    //}
//}