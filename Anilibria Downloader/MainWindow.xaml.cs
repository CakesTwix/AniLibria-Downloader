using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Anilibria_Downloader
{
    public class ProcessAsync
    {

        private string _fileName;
        private string _arguments;

        public ProcessAsync(string fileName, string arguments)
        {
            _fileName = fileName;
            _arguments = arguments;
        }

        public async Task<string> Run(StringBuilder stdin = null)
        {

            // Initialise
            var cmd = new Process();
            cmd.StartInfo.FileName = _fileName;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Arguments = _arguments;

            // Create a task that waits for the Process to finish
            var cmdExited = new CmdExitedTaskWrapper();
            cmd.EnableRaisingEvents = true;
            cmd.Exited += cmdExited.EventHandler;

            // Start process
            cmd.Start();

            // Pass any stdin if necessary
            if (stdin != null)
            {
                await cmd.StandardInput.WriteAsync(stdin.ToString());
                await cmd.StandardInput.FlushAsync();
                cmd.StandardInput.Close();
            }

            // Wait for process to end and return stdout
            await cmdExited.Task;
            return cmd.StandardOutput.ReadToEnd();

        }

        /// <remarks>
        /// We can't wait on a Process directly, so create a wrapper for a
        /// task that waits for the <see cref="Process.Exited"/> Event to be
        /// raised.
        /// </remarks>
        private class CmdExitedTaskWrapper
        {

            private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

            public void EventHandler(object sender, EventArgs e)
            {
                _tcs.SetResult(true);
            }

            public Task Task => _tcs.Task;

        }

    }
    public class Torrent
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Series { get; set; }
        public string Quality { get; set; }
    }
    public partial class MainWindow : Window
    {
        
        public string NameTitle = "";
        public string NameTitleEN = "";

        //Евент для оповещения всех окон приложения
        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get
            {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri(String.Format("Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("Resources/lang.")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged(Application.Current, new EventArgs());
            }
        }
        public ObservableCollection<Torrent> torrent { get; set; }
        public MainWindow()
        {
            InitializeComponent(); 
            //Программно нажимаю на кнопку рандома
            RandomTitle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            App.LanguageChanged += LanguageChanged;

            CultureInfo currLang = App.Language;

            //Заполняем меню смены языка:
            LanguageMenuItem.Items.Clear();
            foreach (var lang in App.Languages)
            {
                MenuItem menuLang = new MenuItem();
                menuLang.Header = lang.DisplayName;
                menuLang.Tag = lang;
                menuLang.Click += ChangeLanguageClick;
                LanguageMenuItem.Items.Add(menuLang);
            }
        }

        private void ChangeLanguageClick(Object sender, EventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                CultureInfo lang = mi.Tag as CultureInfo;
                if (lang != null)
                {
                    App.Language = lang;
                }
            }

        }

        private void ChangeAnime_JArray(JArray jsonAnime)
        {
            Console.WriteLine(jsonAnime);
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


            for (int i = 0; i <= (int)jsonAnime[0]["type"]["series"]; i++)
            {
                Series.Items.Add(i+1); 
            }
            //SeriesListTorrentLabel.Content = jsonAnime["torrents"]["list"][0]["series"]["string"];
            //SeriesListTorrentLabel.Content = jsonAnime[0]["torrents"]["list"][0]["series"]["string"].ToString();
            ChangeImage(jsonAnime[0]["poster"]["url"].ToString());
            Series.SelectedIndex = 0;
            QualityComboBox.SelectedIndex = 0;
            torrent = new ObservableCollection<Torrent>();
            foreach (var Item in jsonAnime[0]["torrents"]["list"])
            {
                torrent.Add(new Torrent
                {
                    ID = (int)Item["torrent_id"],
                    Name = Item["metadata"]["name"].ToString(),
                    Series = Item["series"]["string"].ToString(),
                    Quality = Item["quality"].ToString(),
                });
            }

            Test.ItemsSource = torrent;
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
            //SeriesListTorrentLabel.Content = jsonAnime["torrents"]["list"][0]["series"]["string"];
            ChangeImage(jsonAnime["poster"]["url"].ToString());
            Series.SelectedIndex = 0;
            QualityComboBox.SelectedIndex = 0;
            torrent = new ObservableCollection<Torrent>();
            foreach(var Item in jsonAnime["torrents"]["list"])
            {
                torrent.Add(new Torrent
                {
                    ID = (int)Item["torrent_id"],
                    Name = Item["metadata"]["name"].ToString(),
                    Series = Item["series"]["string"].ToString(),
                    Quality = Item["quality"].ToString(),
                });
            }
            
            Test.ItemsSource = torrent;
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
                ChangeAnime_JArray(qqq);
            }
            

        }

        public async void Download(object sender, RoutedEventArgs e)
        {
            WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            string json2 = wc.DownloadString("https://api.anilibria.tv/v2/searchTitles" + "?search=" + NameTitle);
            JArray qqq = JArray.Parse(json2);

            if (qqq.Count != 0)
            {
                progressSeries.IsIndeterminate = true;
                DownloadButton.IsEnabled = false;
                var p1 = new ProcessAsync("cmd.exe", "/C ffmpeg -i https://" + qqq[0]["player"]["hosts"]["hls"].ToString() + qqq[0]["player"]["playlist"][Series.SelectedItem.ToString()]["hls"][QualityComboBox.SelectedValue.ToString().ToLower()].ToString() + " -n -c copy file:" + (NameTitleEN.Replace(" ", "_") + "_" + Series.SelectedValue + "_" + QualityComboBox.SelectedValue + ".mp4").Replace(":",""));
                Console.WriteLine(await p1.Run());
                //Console.WriteLine("ffmpeg -i https://" + qqq[0]["player"]["hosts"]["hls"].ToString() + qqq[0]["player"]["playlist"][Series.SelectedItem.ToString()]["hls"][QualityComboBox.SelectedValue.ToString().ToLower()].ToString() + " -n -c copy file:" + (NameTitleEN.Replace(" ", "_") + "_" + Series.SelectedValue + "_" + QualityComboBox.SelectedValue + ".mp4").Replace(":", ""));
                DownloadButton.IsEnabled = true;
                progressSeries.IsIndeterminate = false;

            }   
        }
        public void DownloadTorrent(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.DataContext is Torrent)
            {

                Torrent deleteme = (Torrent)button.DataContext;
                WebClient DownloadTorrent = new WebClient();
                DownloadTorrent.DownloadFileAsync(new Uri("https://anilibria.tv/upload/torrents/" + deleteme.ID.ToString() + ".torrent")
                    ,deleteme.ID.ToString()+".torrent");
                Console.WriteLine("https:/anilibria.tv/upload/torrents/"+ deleteme.ID.ToString()+ ".torrent");

            }
        }

            private void AboutProgram_MenuItem(object sender, RoutedEventArgs e)
        {
            AboutProgram aboutProgram = new AboutProgram();
            aboutProgram.ShowDialog();
        }
    }
}
