
// -----------------------------------------------------------------------
// © 2017 ANDE Corporation.All rights reserved
// -----------------------------------------------------------------------


namespace ANDE.FAIRS.Athena.UserControls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DateSelector.xaml
    /// </summary>
    public partial class DateRangeSelector : UserControl
    {
        private bool _displayTitle;
        
        public Double TitleHeight { get; set; }
        public Double ControlHeight { get; set; }
        public Double FontSize { get; set; }
        public bool DisplayTitle
        {
            get { return _displayTitle; }
            set { _displayTitle = value; }
        }

        public DateRangeSelector()
        {
            InitializeComponent();
            this.Loaded += DateRangeSelector_Loaded;
        }

        private void DateRangeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            GrdDatepicker.RowDefinitions[0].Height = new GridLength(TitleHeight);
            GrdDatepicker.RowDefinitions[1].Height = new GridLength(ControlHeight);
            if (!DisplayTitle)
            {
                GrdDatepicker.RowDefinitions[0].Height = new GridLength(0);
            }
            if(FontSize>0)
            {
                LblStartDate.FontSize = FontSize;
                startMonth_.FontSize = FontSize;
                startYear_.FontSize = FontSize;
                startDay_.FontSize = FontSize;
                LblEndDate.FontSize = FontSize;
                endMonth_.FontSize = FontSize;
                endDay_.FontSize = FontSize;
                endYear_.FontSize = FontSize;
            }
        }

        public delegate void DateChangeCompleteHandler();
        public event DateChangeCompleteHandler DateChanged;

        private SelectionChangedEventHandler fowardSelChg_;

        public event SelectionChangedEventHandler ForwardSelectionChanged
        {
            add
            {
                fowardSelChg_ += value;
            }

            remove
            {
                fowardSelChg_ -= value;
            }
        }

        private void ForwardSelChg(object sender, SelectionChangedEventArgs e)
        {
            if (fowardSelChg_ != null)
            {
                fowardSelChg_(sender, e);
            }
        }
        
        private bool ignoreSelectionChanged_ = false;

        private void SetCBIndex(ComboBox cb, int index)
        {
            ignoreSelectionChanged_ = true;
            cb.SelectedIndex = index;
            if (ignoreSelectionChanged_)
            {
                ignoreSelectionChanged_ = false;
            }
        }

        public void InitStartEnd(DateTime start, DateTime end)
        {
            InitStartEnd(start, end, false);
        }

        public void InitStartEnd(DateTime start, DateTime end, bool showNone)
        {
            startYear_.Items.Clear();
            endYear_.Items.Clear();

            if (   start == DateTime.MinValue
                || end == DateTime.MaxValue)
            {
                startYear_.Items.Add("None");
                startDay_.Visibility = Visibility.Hidden;
                startMonth_.Visibility = Visibility.Hidden;
                SetCBIndex(startYear_, 0);

                endYear_.Items.Add("None");
                endDay_.Visibility = Visibility.Hidden;
                endMonth_.Visibility = Visibility.Hidden;
                SetCBIndex(endYear_, 0);
                return;
            }

            if (showNone)
            {
                startYear_.Items.Add("None");
                startDay_.Visibility = Visibility.Hidden;
                startMonth_.Visibility = Visibility.Hidden;
            }
            else
            {
                startDay_.Visibility = Visibility.Visible;
                startMonth_.Visibility = Visibility.Visible;
            }

            // Years
            for (int year = start.Year; year <= end.Year; ++year)
            {
                startYear_.Items.Add(year);
                endYear_.Items.Add(year);
            }

            SetCBIndex(startYear_, 0);

            if (showNone)
            {
                endYear_.Items.Add("None");
                SetCBIndex(endYear_, endYear_.Items.Count - 2);
            }
            else
            {
                SetCBIndex(endYear_, endYear_.Items.Count - 1);
            }

            endDay_.Visibility = Visibility.Visible;
            endMonth_.Visibility = Visibility.Visible;

            // Months
            if (!startMonth_.HasItems && !endMonth_.HasItems)
            {
                for (int month = 1; month <= 12; ++month)
                {
                    var start_mitem = new ComboBoxItem();
                    var end_mitem = new ComboBoxItem();

                    string month_name = new DateTime(1, month, 1).ToString("MMMM");

                    start_mitem.Content = month_name;
                    start_mitem.Tag = month;

                    end_mitem.Content = month_name;
                    end_mitem.Tag = month;

                    startMonth_.Items.Add(start_mitem);
                    endMonth_.Items.Add(end_mitem);
                }
            }
            SetCBIndex(startMonth_, start.Month - 1);
            SetCBIndex(endMonth_, end.Month - 1);

            // Days - based on month and year
            UpdateDays(startDay_, startYear_, startMonth_);
            SetCBIndex(startDay_, start.Day - 1);

            UpdateDays(endDay_, endYear_, endMonth_);
            SetCBIndex(endDay_, end.Day - 1);


        }

        private void UpdateDays(ComboBox daycb, ComboBox yearcb, ComboBox monthcb)
        {
            if (yearcb.SelectedItem is string)
            {
                return;
            }

            int year = (int)yearcb.SelectedItem;
            var monthcbi = (ComboBoxItem)monthcb.SelectedItem;
            int month = (int)monthcbi.Tag;

            int num_days = DateTime.DaysInMonth(year, month);

            if (daycb.Items.Count < num_days)
            {
                int add = num_days - daycb.Items.Count;
                int x = daycb.Items.Count + 1;
                int y = daycb.Items.Count + add;

                for (int i = x; i <= y; ++i)
                {
                    daycb.Items.Add(i);
                }
            }
            else
            {
                if (daycb.SelectedIndex >= num_days)
                {
                    daycb.SelectedIndex = num_days - 1;
                }

                int rem = daycb.Items.Count - num_days;
                for (int i = 0; i < rem; ++i)
                {
                    daycb.Items.RemoveAt(daycb.Items.Count - 1);
                }
            }

            // Ensure that a day is selected - removing days may
            // have reset the selection
            if (daycb.SelectedIndex < 0)
            {
                SetCBIndex(daycb, daycb.Items.Count - 1);
            }
        }

        public DateTime GetCurrStart()
        {
            int yyyy = 1900;
            if (startYear_.Items.Count > 0)
            {
                string none = startYear_.SelectedItem as string;
                if (none != null || startYear_.SelectedItem == null)
                {
                    return DateTime.MinValue;
                }
                yyyy = (int)startYear_.SelectedItem;
            }

            int mm = 1;
            if (startMonth_.Items.Count > 0)
            {
                var cbitem = (ComboBoxItem)startMonth_.SelectedItem;
                mm = (int)cbitem.Tag;
            }

            int dd = 1;
            if (startDay_.Items.Count > 0)
            {
                dd = (int)startDay_.SelectedItem;
            }

            int max_dd = DateTime.DaysInMonth(yyyy, mm);

            if (dd > max_dd)
            {
                dd = max_dd;
            }

            return new DateTime(yyyy, mm, dd, 0, 0, 0, 0);
        }

        private void startYear__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged_)
            {
                ignoreSelectionChanged_ = false;
                return;
            }

            if (startYear_.Items.Count == 0)
            {
                return;
            }

            if (startYear_.SelectedItem is string)
            {
                startDay_.Visibility = Visibility.Hidden;
                startMonth_.Visibility = Visibility.Hidden;
                ForwardSelChg(sender, e);
                return;
            }

            startDay_.Visibility = Visibility.Visible;
            startMonth_.Visibility = Visibility.Visible;
            UpdateDays(startDay_, startYear_, startMonth_);
            ForwardSelChg(sender, e);
            NotifyDateChangeCompleted();
        }

        private void startMonth__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged_)
            {
                ignoreSelectionChanged_ = false;
                return;
            }

            if (startMonth_.Items.Count == 0)
            {
                return;
            }

            UpdateDays(startDay_, startYear_, startMonth_);
            ForwardSelChg(sender, e);
            NotifyDateChangeCompleted();
        }

        private void startDay__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged_)
            {
                ignoreSelectionChanged_ = false;
                return;
            }

            if (startDay_.Items.Count == 0)
            {
                return;
            }

            if (startDay_.SelectedIndex < 0)
            {
                // no selection because we changed to a month with fewer days
                // update day accordingly
                SetCBIndex(startDay_, startDay_.Items.Count - 1);
            }
            ForwardSelChg(sender, e);
            NotifyDateChangeCompleted();
        }

        public DateTime GetCurrEnd()
        {
            int yyyy = 9999;
            if (endYear_.Items.Count > 0)
            {
                string none = endYear_.SelectedItem as string;
                if (none != null || endYear_.SelectedItem == null)
                {
                    return DateTime.MaxValue;
                }
                yyyy = (int)endYear_.SelectedItem;
            }

            int mm = 12;
            if (endMonth_.Items.Count > 0)
            {
                var cbitem = (ComboBoxItem)endMonth_.SelectedItem;
                mm = (int)cbitem.Tag;
            }

            int max_dd = DateTime.DaysInMonth(yyyy, mm);

            int dd = max_dd;
            if (endDay_.Items.Count > 0)
            {
                dd = (int)endDay_.SelectedItem;
            }

            if (dd > max_dd)
            {
                dd = max_dd;
            }

            return new DateTime(yyyy, mm, dd, 23, 59, 59, 0);
        }

        private void endYear__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged_)
            {
                ignoreSelectionChanged_ = false;
                return;
            }

            if (endYear_.Items.Count == 0)
            {
                return;
            }

            if (endYear_.SelectedItem is string)
            {
                endDay_.Visibility = Visibility.Hidden;
                endMonth_.Visibility = Visibility.Hidden;
                ForwardSelChg(sender, e);
                return;
            }

            endDay_.Visibility = Visibility.Visible;
            endMonth_.Visibility = Visibility.Visible;
            UpdateDays(endDay_, endYear_, endMonth_);
            ForwardSelChg(sender, e);
            NotifyDateChangeCompleted();
        }

        private void endMonth__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged_)
            {
                ignoreSelectionChanged_ = false;
                return;
            }

            if (endMonth_.Items.Count == 0)
            {
                return;
            }

            UpdateDays(endDay_, endYear_, endMonth_);
            ForwardSelChg(sender, e);
            NotifyDateChangeCompleted();
        }

        private void endDay__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged_)
            {
                ignoreSelectionChanged_ = false;
                return;
            }

            if (endDay_.Items.Count == 0)
            {
                return;
            }

            ForwardSelChg(sender, e);
            NotifyDateChangeCompleted();
        }

        private void NotifyDateChangeCompleted()
        {
            if (null != DateChanged)
            {
                DateChanged();
            }
        }
    }
}

// -----------------------------------------------------------------------
// © 2017 ANDE Corporation.All rights reserved
// -----------------------------------------------------------------------
