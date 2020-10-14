using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Anilibria_Downloader
{
    public class AnimeUpdate
    {
        public string Image { get; set; }
        public string Name { get; set; }
    }
    /// <summary>
    /// Логика взаимодействия для AnimeList.xaml
    /// </summary>
    public partial class AnimeList : Page
    {
        public List<AnimeUpdate> Animelist { get; set; }
        public AnimeList()
        {
            InitializeComponent();
        }
        public JArray getUpdates()
        {
            WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            var json = wc.DownloadString("https://api.anilibria.tv/v2/getUpdates");
            //Console.WriteLine(json);
            return JArray.Parse(json);
        }

        private void Updates(object sender, RoutedEventArgs e)
        {
            JArray updates = getUpdates();
            Animelist = new List<AnimeUpdate>();
            foreach (var item in updates)
            {
                Console.WriteLine(item);
                Animelist.Add(new AnimeUpdate
                {
                    Name = item["names"]["ru"].ToString(),
                    Image = "https://www.anilibria.tv" + item["poster"]["url"]
                });
            }
            AnimeControl.ItemsSource = Animelist;
        }
    }
}
