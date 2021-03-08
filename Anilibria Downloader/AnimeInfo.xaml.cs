using Anilibria_Downloader.Models;
using Anilibria_Downloader.Utility;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Anilibria_Downloader
{
    public partial class AnimeInfo : Page
    {

        public string NameTitle = "";
        public string NameTitleEN = "";

        //Евент для оповещения всех окон приложения
        
        public ObservableCollection<Torrent> torrent { get; set; }
        public AnimeInfo()
        {
            InitializeComponent();
            //Программно нажимаю на кнопку рандома
            RandomTitle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        public void ChangeAnime_JArray(JArray jsonAnime)
        {
            NameTitle = jsonAnime[0]["names"]["ru"].ToString();
            NameTitleEN = jsonAnime[0]["names"]["en"].ToString();
            GroupTitleName.Text = NameTitle;
            String Content = "Статус: " + jsonAnime[0]["status"]["string"] + "\n";
            Content += "Серий: " + jsonAnime[0]["type"]["full_string"] + "\n";
            Content += jsonAnime[0]["description"] + "\n";
            Opicanie.Text = Content.ToString();
            Series.Items.Clear();
            QualityComboBox.Items.Clear();
            if (jsonAnime[0]["player"]["playlist"]["1"]["hls"]["sd"].ToString() != "") QualityComboBox.Items.Add("SD");
            if (jsonAnime[0]["player"]["playlist"]["1"]["hls"]["hd"].ToString() != "") QualityComboBox.Items.Add("HD");
            if (jsonAnime[0]["player"]["playlist"]["1"]["hls"]["fhd"].ToString() != "") QualityComboBox.Items.Add("FHD");

            Convectors convectors = new Convectors();

            for (int i = 0; i <= (int)jsonAnime[0]["type"]["series"]; i++)
            {
                Series.Items.Add(i + 1);
            }
            ChangeImage(jsonAnime[0]["poster"]["url"].ToString());
            Series.SelectedIndex = 0;
            QualityComboBox.SelectedIndex = 0;
            torrent = new ObservableCollection<Torrent>();
            foreach (var Item in jsonAnime[0]["torrents"]["list"])
            {
                torrent.Add(new Torrent
                {
                    ID = (int)Item["torrent_id"],
                    Name = NameTitleEN + " " + Item["quality"]["string"].ToString(),
                    Series = Item["series"]["string"].ToString(),
                    Quality = Item["quality"]["string"].ToString(),
                    Size = convectors.ConvertSize((double)Item["total_size"]),
                });
            }

            TorrentsItem.ItemsSource = torrent;
        }
        public void ChangeAnime_JObject(JObject jsonAnime)
        {
            NameTitle = jsonAnime["names"]["ru"].ToString();
            NameTitleEN = jsonAnime["names"]["en"].ToString();
            GroupTitleName.Text = NameTitle;
            String Content = "Статус: " + jsonAnime["status"]["string"] + "\n";
            Content += "Серий: " + jsonAnime["type"]["full_string"] + "\n";
            Content += jsonAnime["description"] + "\n";
            Opicanie.Text = Content.ToString();
            Series.Items.Clear();
            QualityComboBox.Items.Clear();
            if (jsonAnime["player"]["playlist"]["1"]["hls"]["sd"].ToString() != "") QualityComboBox.Items.Add("SD");
            if (jsonAnime["player"]["playlist"]["1"]["hls"]["hd"].ToString() != "") QualityComboBox.Items.Add("HD");
            if (jsonAnime["player"]["playlist"]["1"]["hls"]["fhd"].ToString() != "") QualityComboBox.Items.Add("FHD");
            if (torrent != null) torrent.Clear();
            for (int i = 1; i <= (int)jsonAnime["type"]["series"]; i++)
            {
                Series.Items.Add(i);
            }

            Convectors convectors = new Convectors();

            ChangeImage(jsonAnime["poster"]["url"].ToString());
            Series.SelectedIndex = 0;
            QualityComboBox.SelectedIndex = 0;
            torrent = new ObservableCollection<Torrent>();
            foreach (var Item in jsonAnime["torrents"]["list"])
            {
                torrent.Add(new Torrent
                {
                    ID = (int)Item["torrent_id"],
                    Name = NameTitleEN + " " + Item["quality"]["string"].ToString(),
                    Series = Item["series"]["string"].ToString(),
                    Quality = Item["quality"]["string"].ToString(),
                    Size = convectors.ConvertSize((double)Item["total_size"]),
                });
            }

            TorrentsItem.ItemsSource = torrent;
        }
        public void ChangeImage(string poster)
        {
            //Меняем картинку
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("https://www.anilibria.tv" + poster);
            bitmap.EndInit();
            ImageTitle.Source = bitmap;
        }
        public JObject GetRandom()
        {
            WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            var json = wc.DownloadString("https://api.anilibria.tv/v2/getRandomTitle");
            //Console.WriteLine(json);
            return JObject.Parse(json);
        }

        //Метод, при нажатии кнопки рандома
        private void getRandom_Click(object sender, RoutedEventArgs e)
        {
            JObject qqq = GetRandom();
            ChangeAnime_JObject(qqq);
        }

        //Метод, ищущий тайтлы
        private void Search(object sender, RoutedEventArgs e)
        {
            SearchComboBox.Items.Clear();
            string SearchTitle = SearchComboBox.Text;
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            try
            {
                string json2 = wc.DownloadString("https://api.anilibria.tv/v2/searchTitles" + "?search=" + SearchTitle);
                JArray qqq = JArray.Parse(json2);
                for (int i = 0; i < qqq.Count; i++)
                {
                    SearchComboBox.Items.Add(qqq[i]["names"]["ru"]);
                }
                SearchComboBox.Text = "";
                SearchComboBox.IsDropDownOpen = true;
                Series.SelectedIndex = 0;
                QualityComboBox.SelectedIndex = 0;
            }
            catch (WebException e2)
            {
                MessageBox.Show("Нету подключения к серверам Либрии\n" + e2.ToString());
            }

        }

        //Метод, вызывающейся при изменении итема в комбоБоксе
        void SearhComboBox_Click(object sender, SelectionChangedEventArgs args)
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            string json2 = wc.DownloadString("https://api.anilibria.tv/v2/searchTitles" + "?search=" + SearchComboBox.SelectedValue);
            JArray SearchCombo = JArray.Parse(json2);
            if (SearchCombo.Count != 0)
            {
                ChangeAnime_JArray(SearchCombo);
            }
        }

        public async void Download(object sender, RoutedEventArgs e)
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            string json2 = wc.DownloadString("https://api.anilibria.tv/v2/searchTitles" + "?search=" + NameTitle);
            JArray DownloadJson = JArray.Parse(json2);

            if (DownloadJson.Count != 0)
            {
                DownloadButton.IsEnabled = false;

                var ffmpeg = new Engine("ffmpeg.exe");

                ffmpeg.Progress += OnProgress;
                ffmpeg.Error += OnError;
                ffmpeg.Complete += OnComplete;

                string url = "https://" + DownloadJson[0]["player"]["host"].ToString() + DownloadJson[0]["player"]["playlist"][Series.SelectedItem.ToString()]["hls"][QualityComboBox.SelectedValue.ToString().ToLower()].ToString();
                await ffmpeg.ExecuteAsync("-i " + url + " -c copy -y " + (NameTitleEN.Replace(" ", "_") + "_" + Series.SelectedValue + "_" + QualityComboBox.SelectedValue + ".mp4").Replace(":", ""));
                DownloadButton.IsEnabled = true;
            }
        }
        private void OnProgress(object sender, ConversionProgressEventArgs e)
        {
            Convectors convectors = new Convectors();
            Dispatcher.BeginInvoke(new ThreadStart(delegate { SizeDownload.Content = convectors.ConvertSize(Convert.ToDouble(e.SizeKb * 1024)); }));
            Dispatcher.BeginInvoke(new ThreadStart(delegate { TimeDownload.Content = e.ProcessedDuration; }));
        }
        private void OnComplete(object sender, ConversionCompleteEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.TaskbarLibria.ShowBalloonTip("Закачка заверщена", NameTitle, BalloonIcon.Info);
        }
        private void OnError(object sender, ConversionErrorEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.TaskbarLibria.ShowBalloonTip("Закачка заверщена", NameTitle, BalloonIcon.Info);
        }
        public void DownloadTorrent(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.DataContext is Torrent)
            {
                Torrent deleteme = (Torrent)button.DataContext;
                WebClient DownloadTorrent = new WebClient();
                DownloadTorrent.DownloadFileAsync(new Uri("https://anilibria.tv/upload/torrents/" + deleteme.ID.ToString() + ".torrent")
                    , deleteme.Name.ToString().Replace(":", "_") + ".torrent");
                Console.WriteLine("https:/anilibria.tv/upload/torrents/" + deleteme.ID.ToString() + ".torrent");

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}