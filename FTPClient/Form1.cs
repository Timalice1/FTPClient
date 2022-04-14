using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FTPClient {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, System.EventArgs e) {

        }

        class User {
            public string Host { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }

        private void btnSave_Click(object sender, System.EventArgs e) {
            try {
                CheckData();
                var user = new User() {
                    Host = host.Text,
                    Login = login.Text,
                    Password = password.Text
                };

                //Serialize user class to Json file
                string filePath = $@"../../UserData/{user.Login}.json";
                string data = JsonSerializer.Serialize(user, new JsonSerializerOptions() { WriteIndented = true});
                File.WriteAllText(filePath, data);

                //Add saved user to listView
                foreach(var item in usersList.Items) {
                    if (item.Equals(user.Login))
                        throw new Exception("This profile already exist");
                }
                usersList.Items.Add(user.Login);
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnLoad_Click(object sender, System.EventArgs e) {
            try {
                if (usersList.SelectedItem == null)
                    throw new Exception("User not selected");

                string path = $@"../../UserData/{usersList.SelectedItem}.json";
                string data = File.ReadAllText(path);
                var user = JsonSerializer.Deserialize<User>(data);

                host.Text = user.Host;
                login.Text = user.Login;
                password.Text = user.Password;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }

        }

        private void Form1_Load(object sender, EventArgs e) {
            var files = Directory.GetFiles(@"../../UserData");
            foreach(var file in files) {
                string name = file.Remove(0, file.LastIndexOf("\\") + 1);
                usersList.Items.Add(name.Remove(name.LastIndexOf(".")));
            }
        }

        private void CheckData() {
            //Check input data
            var regex = new Regex(@"ftp://\d{3}.\d{3}.\d+.\d+");
            if (!regex.IsMatch(host.Text))
                throw new Exception("Host not valid");
            if (login.Text == "")
                throw new Exception("Login cannot be empty");
            if (password.Text == "")
                throw new Exception("Password cannot be empty");
        }
    }
}
