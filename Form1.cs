using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace e621Downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string query = QueryTextBox.Text;
            int pages = Int16.Parse(pageTextBox.Text);
            string fileName = fileTextbox.Text;
            string folderName = folderTextBox.Text;
            string user = usernameTextBox.Text;
            bool useMultipleFolder = ratingtFolderCheckBox.Checked;

            int downloadedFile = 0;
            fileCountLb.Visible = true;
            DownloadBtn.Visible = false;

            if (!Directory.Exists( folderName ))
            {
                Directory.CreateDirectory( folderName );
            }

            string folder =  folderName;

            if (useMultipleFolder)
            {
                CreateSubfolderIfNotExists(folder, "rating_s");
                CreateSubfolderIfNotExists(folder, "rating_q");
                CreateSubfolderIfNotExists(folder, "rating_e");
            }

            string url = "https://e621.net/posts.json?tags=" + query;
            string userAgent = "e621_downloader.cs";
            int u = 1;

            while (u <= pages)
            {
                string finalUrl = url + "&page=" + u;
                Console.WriteLine($"\n\nPage N°{u}\n");

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", userAgent);
                    client.Headers.Add("From", user);
                    string resultJson = client.DownloadString(finalUrl);
                    JObject resultObject = JObject.Parse(resultJson);

                    JArray posts = (JArray)resultObject["posts"];

                    foreach (JToken post in posts)
                    {
                        string imageUrl = post["file"]["url"].ToString();

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            string file;
                            if (useMultipleFolder)
                            {
                                file = Path.Combine(folder, "rating_" + post["rating"].ToString(), fileName + post["id"] + "." + post["file"]["ext"]);
                            }
                            else
                            {
                                file = Path.Combine(folder, fileName + post["id"] + "." + post["file"]["ext"]);
                            }

                            using (WebClient downloadClient = new WebClient())
                            {
                                using (Stream stream = downloadClient.OpenRead(imageUrl))
                                {
                                    using (FileStream fileStream = File.Create(file))
                                    {
                                        stream.CopyTo(fileStream);
                                    }
                                }
                            }

                            Console.WriteLine("Image successfully Downloaded: " + file);
                            downloadedFile += 1;
                            fileCountLb.Text = $"{downloadedFile} Files downloaded";
                        }
                        else
                        {
                            Console.WriteLine("Image Couldn't be retrieved");
                        }
                    }
                }

                u++;
            }
        }

        

        static void CreateSubfolderIfNotExists(string parentFolder, string subfolder)
        {
            string subfolderPath = Path.Combine(parentFolder, subfolder);
            if (!Directory.Exists(subfolderPath))
            {
                Directory.CreateDirectory(subfolderPath);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            folderTextBox.Text = Path.Combine(Directory.GetCurrentDirectory(), "yiff");
        }

        private void ratingtFolderCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void openFolderBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = folderTextBox.Text ?? Path.Combine(Directory.GetCurrentDirectory(), "yiff");
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                folderTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
