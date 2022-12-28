// -----------------------------------------------------------------------
// © 2017 ANDE Corporation.All rights reserved
// -----------------------------------------------------------------------


using ANDE.FAIRS.Domain;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace ANDE.FAIRS.Athena.UserControls
{
    /// <summary>
    /// Interaction logic for PagingElements.xaml
    /// </summary>
    public partial class PagingElements : UserControl
    {
        #region Private members
        private int totalItems;
        private int pageSize;
        #endregion

        #region Object constructions
        public PagingElements()
        {
            InitializeComponent();
            btnFirst.IsEnabled = false;
            btnPrev.IsEnabled = false;
        }
        #endregion

        #region Public Properties
        public int CurrentPageIndex { get; set; }

        public int CurrentPage
        {
            get
            {
                return TotalPages > 0 ? CurrentPageIndex + 1 : 0;
            }
        }

        public int TotalPages
        {
            get
            {
                if (TotalItems % PageSize == 0)
                {
                    return TotalItems / PageSize;
                }
                else
                {
                    return TotalItems / PageSize + 1;
                }
            }
        }

        public int TotalItems
        {
            get
            {
                return totalItems;
            }
            set
            {
                totalItems = value;
            }
        }

        public int PageSize
        {
            get
            {
                return pageSize <= 0 ? System.Convert.ToInt32(ConfigurationManager.AppSettings["DefaultPageSize"]) : pageSize;
            }
            set
            {
                pageSize = value;
            }
        }
        #endregion

        #region Events and delegates
        /// <summary>
        /// Page Change Handler
        /// </summary>
        public delegate void PageChangeHandler();

        /// <summary>
        /// PageChange event to trigger
        /// </summary>
        public event PageChangeHandler PageChanged;

        private void Notify()
        {
            if (null != PageChanged)
            {
                PageChanged();
            }
        }

        private void FirstClick(object sender, RoutedEventArgs e)
        {
            this.CurrentPageIndex = 0;
            Notify();
            ConfigureButtons();
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageIndex > 0)
            {
                this.CurrentPageIndex--;
                Notify();
            }
            ConfigureButtons();
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            if (this.CurrentPageIndex < TotalPages - 1)
            {
                this.CurrentPageIndex++;
                Notify();
            }
            ConfigureButtons();
        }

        private void LastClick(object sender, RoutedEventArgs e)
        {
            if (TotalPages > 0)
            {
                this.CurrentPageIndex = TotalPages - 1;
                Notify();
            }
            ConfigureButtons();
        }
        #endregion

        #region Public methods
        public void ConfigureButtons()
        {
            btnLast.IsEnabled = true;
            btnFirst.IsEnabled = true;
            btnNext.IsEnabled = true;
            btnPrev.IsEnabled = true;

            if (CurrentPageIndex == 0)
            {
                btnFirst.IsEnabled = false;
                btnPrev.IsEnabled = false;
                if (TotalPages <= 1)
                {
                    btnNext.IsEnabled = false;
                    btnLast.IsEnabled = false;
                }
            }

            if (CurrentPageIndex == TotalPages - 1)
            {
                btnNext.IsEnabled = false;
                btnLast.IsEnabled = false;
                if (TotalPages == 1)
                {
                    btnFirst.IsEnabled = false;
                    btnPrev.IsEnabled = false;
                }
            }
        }
        #endregion
    }
}

// -----------------------------------------------------------------------
// © 2017 ANDE Corporation.All rights reserved
// -----------------------------------------------------------------------
