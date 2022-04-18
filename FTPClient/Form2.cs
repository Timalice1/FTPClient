using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FTPClient {
    public partial class Form2 : Form {

        private string host;
        private NetworkCredential credential;

        public Form2(string name, string host, NetworkCredential credential) {
            InitializeComponent();
            this.Text = name;
            progressBar1.Hide();
            this.host = host;
            this.credential = credential;
            treeView1.AfterSelect += TreeView1_AfterSelect;
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            throw new NotImplementedException();
        }

        private void Form2_Load(object sender, EventArgs e) {
            treeView1.Nodes.Clear();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host);
            request.Credentials = credential;
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251))) {
                var regex = new Regex(@".+\d\d:\d\d\s");
                var file = new Regex(@".+\..+");
                while (!reader.EndOfStream) {
                    string nodeName = regex.Replace(reader.ReadLine(), "");

                    if (nodeName.Equals(".") || nodeName.Equals(".."))
                        continue;

                    TreeNode node = new TreeNode(nodeName);
                    treeView1.Nodes.Add(node);

                    if (file.IsMatch(nodeName))
                        continue;

                    FillNode(node);
                }
            }
        }

        private void FillNode(TreeNode parentNode) {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{host}/{parentNode.FullPath}");
            request.Credentials = credential;
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251))) {
                var regex = new Regex(@".+\d\d:\d\d\s");
                var file = new Regex(@".+\..+");
                while (!reader.EndOfStream) {
                    string nodeName = regex.Replace(reader.ReadLine(), "");

                    if (nodeName.Equals(".") || nodeName.Equals(".."))
                        continue;

                    TreeNode node = new TreeNode(nodeName);
                    parentNode.Nodes.Add(node);

                    if (file.IsMatch(nodeName))
                        continue;

                    FillNode(node);
                }
            }
        }

        private void btnDelete2_Click(object sender, EventArgs e) {
            try {
                var res = MessageBox.Show($"You realy want to delete \"{treeView1.SelectedNode.FullPath}?\"",
                    "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if(res == DialogResult.OK) {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{host}/{treeView1.SelectedNode.FullPath}");
                    request.Credentials = credential;
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    //Update treeView
                    Form2_Load(sender, e);

                }

            }catch (Exception ex){
                MessageBox.Show(ex.Message);
            }
        }

    }
}
