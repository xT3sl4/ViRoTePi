using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace frontend
{
    public partial class HelpWindow : Window
    {
        private bool _videoLoaded = false;
        private List<string> _videoPaths = new List<string>();
        private int _currentVideoIndex = 0;

        public HelpWindow(string videoPath = null)
        {
            InitializeComponent();
            DiscoverVideos();

            if (_videoPaths.Count > 0)
            {
                _currentVideoIndex = 0;
                LoadVideo(_videoPaths[0]);
            }
            else if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
            {
                _videoPaths.Add(videoPath);
                LoadVideo(videoPath);
            }
            else
            {
                NoVideoOverlay.Visibility = Visibility.Visible;
                _videoLoaded = false;
            }

            UpdateVideoIndexText();
        }

        private void DiscoverVideos()
        {
            string videosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Videos");

            if (!Directory.Exists(videosDir))
                return;


            var numbered = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                string path = Path.Combine(videosDir, $"help{i}.mp4");
                if (File.Exists(path))
                    numbered.Add(path);
                else
                    break; 
            }

            if (numbered.Count > 0)
            {
                _videoPaths = numbered;
                return;
            }


            string fallback = Path.Combine(videosDir, "help.mp4");
            if (File.Exists(fallback))
                _videoPaths.Add(fallback);
        }

        private void LoadVideo(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    HelpVideo.Stop();
                    HelpVideo.Source = new Uri(path, UriKind.Absolute);
                    NoVideoOverlay.Visibility = Visibility.Collapsed;
                    _videoLoaded = true;
                    HelpVideo.Play();
                }
                else
                {
                    NoVideoOverlay.Visibility = Visibility.Visible;
                    _videoLoaded = false;
                }
            }
            catch
            {
                NoVideoOverlay.Visibility = Visibility.Visible;
                _videoLoaded = false;
            }

            UpdateVideoIndexText();
        }

        private void UpdateVideoIndexText()
        {
            if (_videoPaths.Count <= 1)
                VideoIndexText.Text = _videoPaths.Count == 1 ? "Film 1 / 1" : "Brak filmow";
            else
                VideoIndexText.Text = $"Film {_currentVideoIndex + 1} / {_videoPaths.Count}";
        }

        private void PrevVideo_Click(object sender, RoutedEventArgs e)
        {
            if (_videoPaths.Count <= 1) return;

            _currentVideoIndex--;
            if (_currentVideoIndex < 0)
                _currentVideoIndex = _videoPaths.Count - 1;

            LoadVideo(_videoPaths[_currentVideoIndex]);
        }

        private void NextVideo_Click(object sender, RoutedEventArgs e)
        {
            if (_videoPaths.Count <= 1) return;

            _currentVideoIndex++;
            if (_currentVideoIndex >= _videoPaths.Count)
                _currentVideoIndex = 0;

            LoadVideo(_videoPaths[_currentVideoIndex]);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_videoLoaded)
                HelpVideo.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (_videoLoaded)
                HelpVideo.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (_videoLoaded)
                HelpVideo.Stop();
        }

        private void HelpVideo_MediaEnded(object sender, RoutedEventArgs e)
        {

            if (_videoPaths.Count > 1)
            {
                _currentVideoIndex++;
                if (_currentVideoIndex >= _videoPaths.Count)
                    _currentVideoIndex = 0;

                LoadVideo(_videoPaths[_currentVideoIndex]);
            }
            else
            {
                HelpVideo.Stop();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
