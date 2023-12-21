using MusciPlayerWpf.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace MusciPlayerWpf.ViewModel
{
    class MainViewModel : PropertyAuto
    {

        private ObservableCollection<string> _musicCollection = new ObservableCollection<string>();
        public ObservableCollection<string> MusicCollection 
        {
            get { return _musicCollection; }
            set
            {
                _musicCollection = value;
                OnPropertyChanged("MusicCollection");
            }
        }
        public MainViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5); 
            _timer.Tick += Timer_Tick;
            _totalTime = TimeSpan.Zero; 
            Volume = 0.5; 
        }
        private string _selectedSong;
        public string SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;

                foreach (string song in MusicsPath)
                {
                    if (song.Contains(_selectedSong))
                    {
                        SelectedSongPath = song;

                        var file = TagLib.File.Create(SelectedSongPath);
                        if (file.Properties.Duration != null)
                        {
                            TotalTime = file.Properties.Duration;
                        }
                        else
                        {
                            TotalTime = TimeSpan.Zero;
                        }
                        _timer.Start();
                        Play.Execute(song);
          
                        break;
                      
                    }
                }
            
            }
        }

        private BitmapImage _selectedSongImage = new BitmapImage();
        public BitmapImage SelectedSongImage 
        {
            get { return _selectedSongImage; }
            set
            {
                _selectedSongImage = value;
                OnPropertyChanged("SelectedSongImage");
            }
        }

        private string _selectedSongName;
        public string SelectedSongName 
        {
            get { return _selectedSongName; }
            set
            {
                _selectedSongName = value;
                OnPropertyChanged("SelectedSongName");
            }
        }

        private string _selectedSongArtist;
        public string SelectedSongArtist 
        {
            get { return _selectedSongArtist; }
            set
            {
                _selectedSongArtist = value;
                OnPropertyChanged("SelectedSongArtist");
            }
        }

        private string _selectedSongPath;
        public string SelectedSongPath 
        {
            get { return _selectedSongPath; }
            set
            {
                _selectedSongPath = value;
                OnPropertyChanged("SelectedSongPath");
            }
        }

        private List<string> _musicsPath = new List<string>();
        public List<string> MusicsPath 
        {
            get { return _musicsPath; }
            set
            {
                _musicsPath = value;
                OnPropertyChanged("MusicsPath");
            }
        }

        private RelayCommand _loadMusic;
        public RelayCommand LoadMusic
        {
            get
            {
                _loadMusic = new RelayCommand(obj =>
                {
                    using (OpenFileDialog browser = new OpenFileDialog())
                    {
                        browser.Multiselect = true;
                        browser.Filter = "MP3 files (*.mp3)|*.mp3";

                        if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            foreach (string selectedFilePath in browser.FileNames)
                            {
                                string selectedFileName = Path.GetFileName(selectedFilePath);

                                MusicCollection.Add(selectedFileName);
                                MusicsPath.Add(selectedFilePath);
                            }
                        }
                    }
                });
                return _loadMusic;
            }
        }


        private MediaPlayer _player = new MediaPlayer();
        public MediaPlayer Player 
        {
            get { return _player; }
            set
            {
                _player = value;
                OnPropertyChanged("Player");
            }
        }
        private RelayCommand _play;
        public RelayCommand Play
        {
            get
            {
                _play = new RelayCommand(obj =>
                {
                    try
                    {
                        Player.Open(new Uri(SelectedSongPath));
                        Player.Play();

                        SetSongPicture();
                        TrimSongName(SelectedSong);

                        if (Player.NaturalDuration.HasTimeSpan)
                        {
                            TotalTime = Player.NaturalDuration.TimeSpan;
                            _timer.Start();
                        }
                        CurrentPosition = 0;
                        TrackProgress = 0;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Выберите папку с музыкой и загрузите её!");
                    }
                });
                return _play;
            }
        }

        private RelayCommand _pause;
        public RelayCommand Pause 
        {
            get
            {
                _pause = new RelayCommand(obj =>
                {
                    Player.Pause();
                });
                
                return _pause;
            }
        }

        private RelayCommand _resume;
        public RelayCommand Resume 
        {
            get
            {
                _resume = new RelayCommand(obj =>
                {
                    Player.Play();
                });
                return _resume;
            }
        }

        private double _trackProgress;
        public double TrackProgress
        {
            get { return _trackProgress; }
            set
            {
                _trackProgress = value;
                OnPropertyChanged("TrackProgress");
            }
        }
        private DispatcherTimer _timer; 
        private void Timer_Tick(object sender, EventArgs e)
        {

            CurrentPosition = Player.Position.TotalSeconds;

            var startTimeSpan = TimeSpan.FromSeconds(CurrentPosition);
            StartTime = $"{startTimeSpan:mm\\:ss}";

            var remainingTimeSpan = TotalTime;
            RemainingTime = $"{remainingTimeSpan:mm\\:ss}";

            if (Player.Position >= _totalTime)
            {
                _timer.Stop();
                CurrentPosition = TotalTime.TotalSeconds; 
                TrackProgress = 100 ; 
            }
            else
            {
                TrackProgress = (CurrentPosition / TotalTime.TotalSeconds) ;
            }
        }
        private RelayCommand _next;
        public RelayCommand Next
        {
            get
            {
                _next = new RelayCommand(obj =>
                {
                    try
                    {
                        int indexNext = MusicsPath.IndexOf(SelectedSongPath) + 1;
                        Player.Open(new Uri(MusicsPath[indexNext]));
                        SelectedSong = MusicCollection[indexNext];
                        Player.Play();

                        SetSongPicture();
                        TrimSongName(SelectedSong);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show($"Ошибка при переключении на следующий трек: Выберите трек");
                    }
                });
                return _next;
            }
        }
        private RelayCommand _previous;
        public RelayCommand Previous
        {
            get
            {
                _previous = new RelayCommand(obj =>
                {
                    try
                    {
                        int indexPrevious = MusicsPath.IndexOf(SelectedSongPath) - 1;
                        if (indexPrevious >= 0) 
                        {
                            Player.Open(new Uri(MusicsPath[indexPrevious]));
                            SelectedSong = MusicCollection[indexPrevious];
                            Player.Play();

                            SetSongPicture();
                            TrimSongName(SelectedSong);
                        }
                        else
                        {
                            MessageBox.Show("Это первый трек в плейлисте.", "Информация");
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show($"Ошибка при переключении на предыдущий трек: Выберите трек");
                    }
                });
                return _previous;
            }
        }

        private void SetSongPicture() 
        {
            var file = TagLib.File.Create(SelectedSongPath);

            try
            {
                using (MemoryStream stream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                {
                    BitmapImage bitmap = new BitmapImage();

                    stream.Position = 0;

                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    SelectedSongImage = bitmap;
                }
            }
            catch 
            {
                BitmapImage bitmap = new BitmapImage(new Uri(@"/Resources/Default.png", UriKind.Relative));

                SelectedSongImage = bitmap;
            }
        }
        private TimeSpan _totalTime;
        public TimeSpan TotalTime
        {
            get { return _totalTime; }
            set
            {
                _totalTime = value;
                OnPropertyChanged("TotalTime");
                OnPropertyChanged("ProgressBarMaximum"); 
            }
        }
        public double ProgressBarMaximum
        {
            get { return TotalTime.TotalSeconds; }
        }
        private double _currentPosition;
        public double CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                OnPropertyChanged("CurrentPosition");
                OnPropertyChanged("ProgressBarMaximum");
                Player.Position = TimeSpan.FromSeconds(value);
            }
        }
        private string _startTime;
        public string StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged("StartTime");
            }
        }
        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set
            {
                _remainingTime = value;
                OnPropertyChanged("RemainingTime");
            }
        }
        private double _volume;
        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged("Volume");
                Player.Volume = value;
            }
        }
        private RelayCommand _minimizeWindow;
        public RelayCommand MinimizeWindow
        {
            get
            {
                _minimizeWindow = new RelayCommand(obj =>
                {
                    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                });
                return _minimizeWindow;
            }
        }

        private RelayCommand _closeApplication;
        public RelayCommand CloseApplication
        {
            get
            {
                _closeApplication = new RelayCommand(obj =>
                {
                    Application.Current.Shutdown();
                });
                return _closeApplication;
            }
        }
        private void TrimSongName(string song) 
        {
            song = song.TrimEnd('.', 'm', 'p', '3');

            for (int i = 0; i < song.Length; i++)
            {
                if (char.IsPunctuation(song[i]))
                {
                    SelectedSongArtist = song.Remove(i);
                    SelectedSongName = song.Substring(i + 2);
                    break;
                }
            }
        }

    }
}
