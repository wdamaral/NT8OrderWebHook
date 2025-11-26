using NinjaTrader.Custom.Indicators.OrderWebHook.Interfaces;
using NinjaTrader.Gui.Chart;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NinjaTrader.Custom.Indicators.OrderWebHook.UI
{
    public class ChartUiManager
    {
        private readonly ILogger _logger;
        private readonly ChartControl _chartControl;
        private readonly IProviderConfig _atsConfig;
        private readonly IProviderConfig _qlConfig;
        private readonly IProviderConfig _pipeConfig;

        private Chart _chartWindow;
        private Grid _chartTraderGrid;
        private Grid _mainGrid;
        private Button _atsButton;
        private Button _qlButton;
        private Button _pipeButton;
        private ListBox _logBox;

        private bool _panelActive;
        private DispatcherTimer _watcher;

        public ChartUiManager(ILogger logger, ChartControl chartControl, IProviderConfig atsConfig, IProviderConfig qlConfig, IProviderConfig pipeConfig)
        {
            _logger = logger;
            _chartControl = chartControl;
            _atsConfig = atsConfig;
            _qlConfig = qlConfig;
            _pipeConfig = pipeConfig;
        }

        public void LoadControlPanel()
        {
            _watcher = new DispatcherTimer();
            _watcher.Interval = TimeSpan.FromSeconds(1);
            _watcher.Tick += (s, e) => TryInjectControls();
            _watcher.Start();
        }

        public void UnloadControlPanel()
        {
            if (_watcher != null) { _watcher.Stop(); _watcher = null; }
            if (_chartWindow != null) _chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
            RemoveWPFControls();
        }

        public void LogMessage(string message)
        {
            if (_logBox == null) return;

            // Check for errors to color code
            bool isError = message.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           message.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0;

            var item = new ListBoxItem
            {
                Content = message,
                Foreground = isError ? Brushes.Salmon : Brushes.WhiteSmoke,
                FontSize = 10,
                Padding = new Thickness(2, 0, 0, 0)
            };

            // Insert at top
            _logBox.Items.Insert(0, item);

            // Limit to last 20 items to save memory/space
            if (_logBox.Items.Count > 20)
            {
                _logBox.Items.RemoveAt(_logBox.Items.Count - 1);
            }
        }

        private void TryInjectControls()
        {
            if (_panelActive)
            {
                if (_chartTraderGrid != null && _mainGrid != null && !_chartTraderGrid.Children.Contains(_mainGrid))
                    _panelActive = false;
                else
                    return;
            }

            _chartWindow = Window.GetWindow(_chartControl.Parent) as Chart;
            if (_chartWindow == null) return;

            var chartTraderObj = FindChildByAutomationId(_chartWindow, "ChartWindowChartTraderControl");
            if (chartTraderObj == null) return;

            var contentControl = chartTraderObj as ContentControl;
            if (contentControl == null) return;

            _chartTraderGrid = contentControl.Content as Grid;
            if (_chartTraderGrid == null) return;

            CreateWPFControls();
            _chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
            _chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;

            if (TabSelected()) InsertWPFControls();
        }

        private void CreateWPFControls()
        {
            // 3 Columns Grid (ATS | QL | PIPE)
            _mainGrid = new Grid
            {
                Margin = new Thickness(0, 60, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Row 0: Buttons
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            // Row 1: Buttons
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            // Row 2: Log Grid
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });

            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // ATS Button
            _atsButton = CreateButton("ATS", _atsConfig.Enabled, OnAtsClick);
            Grid.SetRow(_atsButton, 0);
            Grid.SetColumn(_atsButton, 0);
            _mainGrid.Children.Add(_atsButton);

            // QL Button
            _qlButton = CreateButton("QL", _qlConfig.Enabled, OnQlClick);
            Grid.SetRow(_qlButton, 0);
            Grid.SetColumn(_qlButton, 1);
            _mainGrid.Children.Add(_qlButton);

            // QL Button
            _pipeButton = CreateButton("PIPE", _pipeConfig.Enabled, OnPipeClick);
            Grid.SetRow(_pipeButton, 1);
            Grid.SetColumn(_pipeButton, 0);
            Grid.SetColumnSpan(_pipeButton, 2);
            _mainGrid.Children.Add(_pipeButton);

            // Log ListBox
            _logBox = new ListBox
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)), // Dark background
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Foreground = Brushes.WhiteSmoke,
                Margin = new Thickness(2),
                Height = 95,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Correctly set the attached property
            ScrollViewer.SetVerticalScrollBarVisibility(_logBox, ScrollBarVisibility.Auto);

            Grid.SetRow(_logBox, 2);
            Grid.SetColumn(_logBox, 0);
            Grid.SetColumnSpan(_logBox, 2); // Span across both buttons
            _mainGrid.Children.Add(_logBox);
        }

        private Button CreateButton(string label, bool isOn, RoutedEventHandler onClick)
        {
            var btn = new Button
            {
                Content = string.Format("{0}: {1}", label, isOn ? "ON" : "OFF"),
                Background = isOn ? Brushes.SeaGreen : Brushes.Maroon,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Height = 25,
                Margin = new Thickness(2),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0)
            };
            btn.Click += onClick;
            return btn;
        }

        private void InsertWPFControls()
        {
            if (_panelActive || _chartTraderGrid == null || _mainGrid == null) return;
            try
            {
                if (_chartTraderGrid.RowDefinitions.Count > 0)
                {
                    int lastRow = _chartTraderGrid.RowDefinitions.Count - 1;
                    Grid.SetRow(_mainGrid, lastRow);
                    if (_chartTraderGrid.ColumnDefinitions.Count > 0)
                        Grid.SetColumnSpan(_mainGrid, _chartTraderGrid.ColumnDefinitions.Count);
                }
                _chartTraderGrid.Children.Add(_mainGrid);
                _panelActive = true;
            }
            catch { }
        }

        private void RemoveWPFControls()
        {
            if (!_panelActive || _chartTraderGrid == null || _mainGrid == null) return;
            try { if (_chartTraderGrid.Children.Contains(_mainGrid)) _chartTraderGrid.Children.Remove(_mainGrid); } catch { }
            _panelActive = false;
        }

        private void UpdateButtonState(Button btn, string label, bool enabled)
        {
            btn.Content = string.Format("{0}: {1}", label, enabled ? "ON" : "OFF");
            btn.Background = enabled ? Brushes.SeaGreen : Brushes.Maroon;
        }

        private void OnAtsClick(object sender, RoutedEventArgs e)
        {
            _atsConfig.Enabled = !_atsConfig.Enabled;
            UpdateButtonState(_atsButton, "ATS", _atsConfig.Enabled);

            string state = _atsConfig.Enabled ? "ENABLED" : "DISABLED";
            string shortState = _atsConfig.Enabled ? "On" : "Off";
            _logger.Log(string.Format("ATS Service: {0}", state), string.Format("ATS: {0}", shortState));

            e.Handled = true;
        }

        private void OnPipeClick(object sender, RoutedEventArgs e)
        {
            _pipeConfig.Enabled = !_pipeConfig.Enabled;
            UpdateButtonState(_pipeButton, "PIPE", _pipeConfig.Enabled);

            string state = _pipeConfig.Enabled ? "ENABLED" : "DISABLED";
            string shortState = _pipeConfig.Enabled ? "On" : "Off";
            _logger.Log(string.Format("Pipe Service: {0}", state), string.Format("PIPE: {0}", shortState));

            e.Handled = true;
        }

        private void OnQlClick(object sender, RoutedEventArgs e)
        {
            _qlConfig.Enabled = !_qlConfig.Enabled;
            UpdateButtonState(_qlButton, "QL", _qlConfig.Enabled);

            string state = _qlConfig.Enabled ? "ENABLED" : "DISABLED";
            string shortState = _qlConfig.Enabled ? "On" : "Off";
            _logger.Log(string.Format("QuantLynk Service: {0}", state), string.Format("QL: {0}", shortState));

            e.Handled = true;
        }

        private bool TabSelected()
        {
            if (_chartWindow == null || _chartWindow.MainTabControl == null) return false;
            foreach (TabItem tab in _chartWindow.MainTabControl.Items)
                if ((tab.Content as ChartTab).ChartControl == _chartControl && tab == _chartWindow.MainTabControl.SelectedItem) return true;
            return false;
        }

        private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (TabSelected()) InsertWPFControls(); else RemoveWPFControls();
        }

        private DependencyObject FindChildByAutomationId(DependencyObject parent, string id)
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (AutomationProperties.GetAutomationId(child) == id) return child;
                var res = FindChildByAutomationId(child, id);
                if (res != null) return res;
            }
            return null;
        }
    }
}

