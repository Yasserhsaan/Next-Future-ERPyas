using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Core.Controls
{
    /// <summary>
    /// شريط التنقل الأنيق لنظام ERP
    /// يوفر أزرار تنقل متقدمة مع اختصارات لوحة المفاتيح
    /// </summary>
    public partial class NavigationBar : UserControl, INotifyPropertyChanged
    {
        #region Dependency Properties

        // السجل الحالي
        public static readonly DependencyProperty CurrentRecordProperty =
            DependencyProperty.Register(nameof(CurrentRecord), typeof(int), typeof(NavigationBar),
                new PropertyMetadata(1, OnNavigationPropertyChanged));

        public int CurrentRecord
        {
            get => (int)GetValue(CurrentRecordProperty);
            set => SetValue(CurrentRecordProperty, value);
        }

        // إجمالي السجلات
        public static readonly DependencyProperty TotalRecordsProperty =
            DependencyProperty.Register(nameof(TotalRecords), typeof(int), typeof(NavigationBar),
                new PropertyMetadata(0, OnNavigationPropertyChanged));

        public int TotalRecords
        {
            get => (int)GetValue(TotalRecordsProperty);
            set => SetValue(TotalRecordsProperty, value);
        }

        // إظهار خاصية الانتقال المباشر
        public static readonly DependencyProperty ShowGoToProperty =
            DependencyProperty.Register(nameof(ShowGoTo), typeof(bool), typeof(NavigationBar),
                new PropertyMetadata(true));

        public bool ShowGoTo
        {
            get => (bool)GetValue(ShowGoToProperty);
            set => SetValue(ShowGoToProperty, value);
        }

        #endregion

        #region Commands as Dependency Properties

        public static readonly DependencyProperty FirstCommandProperty =
            DependencyProperty.Register(nameof(FirstCommand), typeof(ICommand), typeof(NavigationBar));

        public ICommand FirstCommand
        {
            get => (ICommand)GetValue(FirstCommandProperty);
            set => SetValue(FirstCommandProperty, value);
        }

        public static readonly DependencyProperty PreviousCommandProperty =
            DependencyProperty.Register(nameof(PreviousCommand), typeof(ICommand), typeof(NavigationBar));

        public ICommand PreviousCommand
        {
            get => (ICommand)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }

        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register(nameof(NextCommand), typeof(ICommand), typeof(NavigationBar));

        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        public static readonly DependencyProperty LastCommandProperty =
            DependencyProperty.Register(nameof(LastCommand), typeof(ICommand), typeof(NavigationBar));

        public ICommand LastCommand
        {
            get => (ICommand)GetValue(LastCommandProperty);
            set => SetValue(LastCommandProperty, value);
        }

        public static readonly DependencyProperty GoToCommandProperty =
            DependencyProperty.Register(nameof(GoToCommand), typeof(ICommand), typeof(NavigationBar));

        public ICommand GoToCommand
        {
            get => (ICommand)GetValue(GoToCommandProperty);
            set => SetValue(GoToCommandProperty, value);
        }

        #endregion

        #region Properties

        // الخصائص المحسوبة للتحكم في حالة الأزرار
        public bool CanGoFirst => CurrentRecord > 1 && TotalRecords > 0;
        public bool CanGoPrevious => CurrentRecord > 1 && TotalRecords > 0;
        public bool CanGoNext => CurrentRecord < TotalRecords && TotalRecords > 0;
        public bool CanGoLast => CurrentRecord < TotalRecords && TotalRecords > 0;

        #endregion

        #region Constructor

        public NavigationBar()
        {
            InitializeComponent();
            
            // ربط اختصارات لوحة المفاتيح
            this.KeyDown += NavigationBar_KeyDown;
            this.Focusable = true;
            
            // تحديث حالة الأزرار
            UpdateButtonStates();
            UpdateRecordCountText();
        }

        #endregion

        #region Event Handlers

        private static void OnNavigationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NavigationBar navBar)
            {
                navBar.UpdateButtonStates();
                navBar.UpdateRecordCountText();
                navBar.OnPropertyChanged(nameof(navBar.CanGoFirst));
                navBar.OnPropertyChanged(nameof(navBar.CanGoPrevious));
                navBar.OnPropertyChanged(nameof(navBar.CanGoNext));
                navBar.OnPropertyChanged(nameof(navBar.CanGoLast));
            }
        }

        // اختصارات لوحة المفاتيح
        private void NavigationBar_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Home:
                    if (CanGoFirst)
                        FirstButton_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;

                case Key.Left:
                    if (CanGoPrevious)
                        PreviousButton_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;

                case Key.Right:
                    if (CanGoNext)
                        NextButton_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;

                case Key.End:
                    if (CanGoLast)
                        LastButton_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;

                case Key.G when Keyboard.Modifiers == ModifierKeys.Control:
                    GoToTextBox.Focus();
                    GoToTextBox.SelectAll();
                    e.Handled = true;
                    break;
            }
        }

        // أحداث الأزرار
        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstCommand?.CanExecute(null) == true)
                FirstCommand.Execute(null);
            else if (CanGoFirst)
                CurrentRecord = 1;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviousCommand?.CanExecute(null) == true)
                PreviousCommand.Execute(null);
            else if (CanGoPrevious)
                CurrentRecord--;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (NextCommand?.CanExecute(null) == true)
                NextCommand.Execute(null);
            else if (CanGoNext)
                CurrentRecord++;
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            if (LastCommand?.CanExecute(null) == true)
                LastCommand.Execute(null);
            else if (CanGoLast)
                CurrentRecord = TotalRecords;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteGoTo();
        }

        private void GoToTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteGoTo();
                e.Handled = true;
            }
        }

        #endregion

        #region Private Methods

        private void ExecuteGoTo()
        {
            if (int.TryParse(GoToTextBox.Text, out int recordNumber))
            {
                if (recordNumber >= 1 && recordNumber <= TotalRecords)
                {
                    if (GoToCommand?.CanExecute(recordNumber) == true)
                        GoToCommand.Execute(recordNumber);
                    else
                        CurrentRecord = recordNumber;
                    
                    GoToTextBox.Clear();
                }
                else
                {
                    MessageBox.Show($"يرجى إدخال رقم بين 1 و {TotalRecords}",
                                  "رقم غير صحيح",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    GoToTextBox.SelectAll();
                }
            }
            else
            {
                MessageBox.Show("يرجى إدخال رقم صحيح",
                              "خطأ في الإدخال",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                GoToTextBox.SelectAll();
            }
        }

        private void UpdateButtonStates()
        {
            FirstButton.IsEnabled = CanGoFirst;
            PreviousButton.IsEnabled = CanGoPrevious;
            NextButton.IsEnabled = CanGoNext;
            LastButton.IsEnabled = CanGoLast;
            GoButton.IsEnabled = TotalRecords > 0;
            GoToTextBox.IsEnabled = TotalRecords > 0;
        }

        private void UpdateRecordCountText()
        {
            if (RecordCountText == null)
                return;

            // نبني المحتوى بشكل أوضح باستخدام Runs لدعم RTL وتنسيق الأرقام
            RecordCountText.Inlines.Clear();

            if (TotalRecords <= 0)
            {
                RecordCountText.Text = "لا توجد سجلات";
                return;
            }

            RecordCountText.FlowDirection = FlowDirection.RightToLeft;

            RecordCountText.Inlines.Add(new System.Windows.Documents.Run("السجل "));
            RecordCountText.Inlines.Add(new System.Windows.Documents.Run(CurrentRecord.ToString())
            {
                FontWeight = FontWeights.SemiBold
            });
            RecordCountText.Inlines.Add(new System.Windows.Documents.Run(" من "));
            RecordCountText.Inlines.Add(new System.Windows.Documents.Run(TotalRecords.ToString("N0"))
            {
                FontWeight = FontWeights.SemiBold
            });
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// تحديث معلومات التنقل
        /// </summary>
        /// <param name="current">السجل الحالي</param>
        /// <param name="total">إجمالي السجلات</param>
        public void UpdateNavigation(int current, int total)
        {
            CurrentRecord = Math.Max(1, Math.Min(current, total));
            TotalRecords = Math.Max(0, total);
        }

        /// <summary>
        /// إعادة تعيين التنقل
        /// </summary>
        public void Reset()
        {
            CurrentRecord = 1;
            TotalRecords = 0;
            GoToTextBox.Clear();
        }

        /// <summary>
        /// التركيز على شريط التنقل لتفعيل اختصارات لوحة المفاتيح
        /// </summary>
        public new void Focus()
        {
            base.Focus();
        }

        #endregion
    }
}
