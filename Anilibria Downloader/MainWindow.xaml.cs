using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Anilibria_Downloader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public string NameTitle = "";
        public string NameTitleEN = "";

        public MainWindow()
        {
            InitializeComponent();
            //Программно нажимаю на кнопку рандома
            RandomTitle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        public void Downloader(string hls, string m3u8)
        {
            // Start the child process.
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C ffmpeg -i http://" + hls + m3u8 + " -y -c copy file:" + NameTitleEN.Replace(" ","_") + "_" + Series.SelectedValue + "_" + QualityComboBox.SelectedValue + ".mp4";
            p.Start();
            p.Close();
            // Read the output stream first and then wait.
            //return p.StandardOutput.ReadToEnd();
    }
        public JObject GetRandom()
        {
            try
            {
                WebClient wc = new System.Net.WebClient();
                wc.Encoding = System.Text.Encoding.UTF8;
                var json = wc.DownloadString("https://api.anilibria.tv/v2/getRandomTitle");
                //Console.WriteLine(json);
                return JObject.Parse(json);
            }
            catch(Exception e)
            {
                MessageBox.Show("Нету подключения к серверам Либрии");
                var ErrorJson = new JObject();
                ErrorJson.Add("error", e.ToString());
                return ErrorJson;
            }
        } 

        //Метод, при нажатии кнопки рандома
        private void getRandom_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                JObject qqq = GetRandom();
                if (qqq.ContainsKey("error"))
                {

                }
                else
                {
                    NameTitle = qqq["names"]["ru"].ToString();
                    NameTitleEN = qqq["names"]["en"].ToString();
                    GroupTitleName.Text = NameTitle;
                    String Content = "Статус: " + qqq["status"]["string"] + "\n";
                    Content += "Серий: " + qqq["type"]["full_string"] + "\n";
                    Content += qqq["description"] + "\n";
                    Opicanie.Text = Content.ToString();
                    Series.Items.Clear();
                    QualityComboBox.Items.Clear();
                    if (qqq["player"]["playlist"]["1"]["hls"]["sd"].ToString() != "") QualityComboBox.Items.Add("SD");
                    if (qqq["player"]["playlist"]["1"]["hls"]["hd"].ToString() != "") QualityComboBox.Items.Add("HD");
                    if (qqq["player"]["playlist"]["1"]["hls"]["fhd"].ToString() != "") QualityComboBox.Items.Add("FHD");


                    for (int i = 1; i <= (int)qqq["type"]["series"]; i++)
                    {
                        Series.Items.Add(i);
                    }
                    //Меняем картинку
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("https://www.anilibria.tv" + qqq["poster"]["url"]);
                    bitmap.EndInit();
                    ImageTitle.Source = bitmap;
                    Series.SelectedIndex = 0;
                    QualityComboBox.SelectedIndex = 0;
                }
            }
            catch(WebException e1)
            {
                
            }
        }

        //Метод, ищущий тайтлы
        private void Search(object sender, RoutedEventArgs e)
        {
            SearchComboBox.Items.Clear();
            string SearchTitle = SearchComboBox.Text;
            WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            try
            {
                string json2 = wc.DownloadString("https://api.anilibria.tv/v2/searchTitles" + "?search=" + SearchTitle);
                JArray qqq = JArray.Parse(json2);
                //Console.WriteLine(qqq.Count);
                for (int i = 0; i < qqq.Count; i++)
                {
                    SearchComboBox.Items.Add(qqq[i]["names"]["ru"]);
                }
                SearchComboBox.Text = "";
                SearchComboBox.IsDropDownOpen = true;
                Series.SelectedIndex = 0;
                QualityComboBox.SelectedIndex = 0;
            }
            catch(WebException e2)
            {
                MessageBox.Show("Нету подключения к серверам Либрии\n" + e2.ToString());
            }

        }

        //Метод, вызывающейся при изменении итема в комбоБоксе
        void SearhComboBox_Click(object sender, SelectionChangedEventArgs args)
        {
            WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            string json2 = wc.DownloadString("https://api.anilibria.tv/v2/searchTitles" + "?search=" + SearchComboBox.SelectedValue);
            JArray qqq = JArray.Parse(json2);
            if (qqq.Count != 0)
            {
                //Console.WriteLine(qqq.ToString());
                Series.Items.Clear();
                SearchComboBox.Items.Clear();
                NameTitle = qqq[0]["names"]["ru"].ToString();
                NameTitleEN = qqq[0]["names"]["en"].ToString();
                GroupTitleName.Text = NameTitle;
                String Content = "Статус: " + qqq[0]["status"]["string"].ToString() + "\n";
                Content += qqq[0]["description"] + "\n";
                Opicanie.Text = Content.ToString();
                //Меняем картинку
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("https://www.anilibria.tv" + qqq[0]["poster"]["url"]);
                bitmap.EndInit();
                ImageTitle.Source = bitmap;
                for (int i = 0; i <= (int)qqq[0]["type"]["series"]; i++)
                {
                    Series.Items.Add(i+1);
                }
                QualityComboBox.Items.Clear();
                if (qqq[0]["player"]["playlist"]["1"]["hls"]["sd"].ToString() != "") QualityComboBox.Items.Add("SD");
                if (qqq[0]["player"]["playlist"]["1"]["hls"]["hd"].ToString() != "") QualityComboBox.Items.Add("HD");
                if (qqq[0]["player"]["playlist"]["1"]["hls"]["fhd"].ToString() != "") QualityComboBox.Items.Add("FHD");
                QualityComboBox.SelectedIndex = 0;
                Series.SelectedIndex = 0;
            }
            

        }
        public void Download(object sender, RoutedEventArgs e)
        {
            WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            string json2 = wc.DownloadString("https://api.anilibria.tv/v2/searchTitles" + "?search=" + NameTitle);
            JArray qqq = JArray.Parse(json2);
            ProgressDownload.IsIndeterminate = true;
            if (qqq.Count != 0)
            {

                /*Console.WriteLine(
                    Series.SelectedItem.ToString() + " "
                    + QualityComboBox.SelectedValue + " "
                    + qqq[0]["player"]["hosts"]["hls"].ToString() + " "
                    + qqq[0]["player"]["playlist"][Series.SelectedItem.ToString()]["hls"][QualityComboBox.SelectedValue.ToString().ToLower()]);
                    */
                
                Downloader(qqq[0]["player"]["hosts"]["hls"].ToString(),
                    qqq[0]["player"]["playlist"][Series.SelectedItem.ToString()]["hls"][QualityComboBox.SelectedValue.ToString().ToLower()].ToString());
                
            }
            ProgressDownload.IsIndeterminate = false;
               
        }

        private void AboutProgram_MenuItem(object sender, RoutedEventArgs e)
        {
            AboutProgram aboutProgram = new AboutProgram();
            aboutProgram.ShowDialog();
        }
    }
}
