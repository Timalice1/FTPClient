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
        private Regex file = new Regex(@".+\..+");


        public Form2(string name, string host, NetworkCredential credential) {
            InitializeComponent();
            this.Text = name;
            progressBar1.Hide();
            this.host = host;
            this.credential = credential;
            treeView1.AfterSelect += TreeView1_AfterSelect;
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            try {
                if (!file.IsMatch(e.Node.FullPath)) {
                    dirName.Text = e.Node.FullPath + "\\";
                    fileName.Text = e.Node.FullPath + "\\";
                }
                else {
                    dirName.Text = e.Node.Parent.FullPath;
                    fileName.Text = e.Node.Parent.FullPath;
                }
            }
            catch { }
        }

        private void Form2_Load(object sender, EventArgs e) {
            treeView1.Nodes.Clear();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host);
            request.Credentials = credential;
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251))) {
                var regex = new Regex(@".+\d\d:\d\d\s");
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

                string name;
                if(treeView1.SelectedNode.FullPath.Contains("\\"))
                    name = treeView1.SelectedNode.FullPath.Remove(0, treeView1.SelectedNode.FullPath.LastIndexOf("\\"));
                else name = treeView1.SelectedNode.FullPath;

                var deleteFile = WebRequestMethods.Ftp.DeleteFile;
                var deleteDir = WebRequestMethods.Ftp.RemoveDirectory;

                if (res == DialogResult.OK) {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{host}/{treeView1.SelectedNode.FullPath}");
                    request.Credentials = credential;
                    if (file.IsMatch(name))
                        request.Method = deleteFile;
                    else
                        request.Method = deleteDir;
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    MessageBox.Show($"\"{treeView1.SelectedNode.FullPath}\" deleted");

                    //Update treeView
                    Form2_Load(sender, e);
                }

            }catch (Exception ex){
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateDir_Click(object sender, EventArgs e) {
            try {
                if (dirName.Text == treeView1.SelectedNode.FullPath)
                    throw new Exception("This folder already exist");
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{host}/{dirName.Text}");
                request.Credentials = credential;
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                //Update treeView
                Form2_Load(sender, e);

                MessageBox.Show($"\"{dirName.Text}\" created");

            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private string fullFilePath = "";

        private void btnUpload_Click(object sender, EventArgs e) {
            try {
                string name = "";

                if(name.Contains("\\"))
                    name = fileName.Text.Remove(0, fileName.Text.LastIndexOf("\\"));
                else name = fileName.Text;

                if (!file.IsMatch(name))
                    throw new Exception("Invalid file name");

                var createFile = WebRequestMethods.Ftp.AppendFile;
                var uploadFile = WebRequestMethods.Ftp.UploadFile;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{host}/{fileName.Text}");
                request.Credentials = credential;

                if (fileName.Text.StartsWith(treeView1.SelectedNode.FullPath) || file.IsMatch(fileName.Text))
                    request.Method = createFile;
                else request.Method = uploadFile;

                byte[] _file = File.ReadAllBytes(fullFilePath);
                Stream strz = request.GetRequestStream();
                strz.Write(_file, 0, _file.Length);
                strz.Close();
                strz.Dispose();
                
                //Update treeView
                Form2_Load(sender, e);

                MessageBox.Show($"\"{fileName.Text}\" created");
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e) {
            var ofd= new OpenFileDialog();
            var res = ofd.ShowDialog();

            if(res == DialogResult.OK) {
                fullFilePath = ofd.FileName;
                fileName.Text += $"\\{ofd.SafeFileName}";
            }
        }
    }
}
