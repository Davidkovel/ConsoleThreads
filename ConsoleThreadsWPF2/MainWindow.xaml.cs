using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace TemperatureVisualizer
{
    public partial class MainWindow : Window
    {
        private readonly string _dataStoragePath = "temperature_dataset.csv";
        private ObservableCollection<TemperatureRecord> _temperatureRecords;
        
        public SeriesCollection Series { get; set; }
        public ObservableCollection<string> Labels { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SetupApplication();
        }

        private void SetupApplication()
        {
            _temperatureRecords = new ObservableCollection<TemperatureRecord>();
            DataTable.ItemsSource = _temperatureRecords;
            
            Series = new SeriesCollection();
            Labels = new ObservableCollection<string>();
            
            DataContext = this;
            
            LoadSampleDataset();
        }

        private void LoadSampleDataset()
        {
            _temperatureRecords.Add(new TemperatureRecord { TimePoint = "Morning", Value = 18.5 });
            _temperatureRecords.Add(new TemperatureRecord { TimePoint = "Noon", Value = 22.3 });
            _temperatureRecords.Add(new TemperatureRecord { TimePoint = "Evening", Value = 19.7 });
            _temperatureRecords.Add(new TemperatureRecord { TimePoint = "Night", Value = 16.2 });
            
            UpdateStatus("Sample dataset loaded");
        }

        private void StoreDataset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var writer = new StreamWriter(_dataStoragePath))
                {
                    foreach (var record in _temperatureRecords)
                    {
                        writer.WriteLine($"{record.TimePoint};{record.Value}");
                    }
                }
                UpdateStatus($"Dataset stored to {_dataStoragePath}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error storing data: {ex.Message}", true);
            }
        }

        private void RetrieveDataset_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(_dataStoragePath))
            {
                UpdateStatus("No stored dataset found", true);
                return;
            }

            try
            {
                _temperatureRecords.Clear();
                foreach (var line in File.ReadAllLines(_dataStoragePath))
                {
                    var parts = line.Split(';');
                    if (parts.Length == 2 && double.TryParse(parts[1], out double value))
                    {
                        _temperatureRecords.Add(new TemperatureRecord
                        {
                            TimePoint = parts[0],
                            Value = value
                        });
                    }
                }
                UpdateStatus($"Dataset loaded from {_dataStoragePath}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading data: {ex.Message}", true);
            }
        }

        private void VisualizeData_Click(object sender, RoutedEventArgs e)
        {
            if (_temperatureRecords.Count == 0)
            {
                UpdateStatus("No data to visualize", true);
                return;
            }

            new Thread(() => 
            {
                Dispatcher.Invoke(() => GenerateTemperatureChart());
            })
            {
                IsBackground = true
            }.Start();
        }

        private void GenerateTemperatureChart()
        {
            try
            {
                Series.Clear();
                Labels.Clear();

                var values = new ChartValues<double>();
                foreach (var record in _temperatureRecords)
                {
                    values.Add(record.Value);
                    Labels.Add(record.TimePoint);
                }

                Series.Add(new LineSeries
                {
                    Title = "Temperature",
                    Values = values,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.5,
                    Stroke = System.Windows.Media.Brushes.LightSeaGreen,
                    Fill = System.Windows.Media.Brushes.Transparent
                });

                UpdateStatus("Data visualization complete");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Visualization error: {ex.Message}", true);
            }
        }

        private void AddSampleData_Click(object sender, RoutedEventArgs e)
        {
            LoadSampleDataset();
            UpdateStatus("Added sample dataset");
        }

        private void UpdateStatus(string message, bool isError = false)
        {
            StatusText.Text = message;
            StatusText.Foreground = isError ? 
                System.Windows.Media.Brushes.IndianRed : 
                System.Windows.Media.Brushes.LightGreen;
        }
    }

    public class TemperatureRecord
    {
        public string TimePoint { get; set; }
        public double Value { get; set; }
    }
}