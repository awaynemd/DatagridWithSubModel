using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;



// The DataGridAnnotationControl is the control displayed by the adorner to the datagrid.



namespace DataGrid.View
{
    public class PatternChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The current pattern in the auto complete
        /// </summary>
        public string Pattern { get; }

        public PatternChangedEventArgs(string Pattern) :
            base(AutoCompleteComboBox2.PatternChangedEvent)
        {
            this.Pattern = Pattern;
        }
    }

    public interface IVisitService { }

    public interface IInsurancePolicy
    {
        DateTime? verificationDate { get; }
        string policyNumber { get; }
        string name { get; }
        decimal? deductible { get; }
        decimal? coPay { get; }
        string relationToPolicyHolder { get; }
    }

    public interface IService { }

    public interface IViewName
    {
        string firstName { get; }
        string lastName { get; }
        DateTime birthDate { get; }
    }

    public interface IEncounterRecord
    {
        string sex { get; }
        bool? selfPay { get; }
        bool? cashOnly { get; }
        DateTime? lastEncounter { get; }
        int? encounterCount { get; }
        DateTime? firstEncounter { get; }
        DateTime? tcompleted { get; }
        string status { get; }
        bool notSeen { get; }
        string patientAge { get; }
        TimeSpan? timeSinceCheckIn { get; }
        string description { get; }
        string provider { get; }
        string strBirthDate { get; }
        int? missedAppointments { get; }
        bool? fileOnInsurance { get; }
        DateTime? appointmentTime { get; }
        string phoneNumber { get; }
        DateTime? postingTime { get; }
        string lastName { get; }
        string firstName { get; }
        string mi { get; }
        DateTime? birthDate { get; }
        int? chartNumber { get; }
        bool? doNotSee { get; }
        DateTime? tservice { get; }
    }

    public interface IContactInformation 
    { }

    // TemplatePart defines the parts needed for the LOGIC of the control to work.
    [TemplatePart(Name = "PART_BirthDate", Type = typeof(AutoCompleteComboBox2))]
    [TemplatePart(Name = "PART_FirstName", Type = typeof(AutoCompleteComboBox2))]
    [TemplatePart(Name = "PART_LastName", Type = typeof(AutoCompleteComboBox2))]
    [TemplatePart(Name = "PART_SaveBtn", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_CheckInBtn", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_DeleteBtn", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_CloseBtn", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ClearBtn", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_LookupBtn", Type = typeof(ButtonBase))]
    public class DataGridAnnotationControl : Control, INotifyPropertyChanged
    {
        #region [INotifyPropertyChanged]
        public event PropertyChangedEventHandler PropertyChanged;

        // using "virtual" here results in call chain violation CA2214
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region [Fields]
        // Const strings needed for GetTemplateChild() in OnApplyTemplate and defined with TemplatePart.
        private const string BirthDatePartName = "PART_BirthDate";
        private const string FirstNamePartName = "PART_FirstName";
        private const string LastNamePartName = "PART_LastName";
        private const string BtnSavePartName = "PART_SaveBtn";
        private const string BtnCheckInPartName = "PART_CheckInBtn";
        private const string BtnDeletePartName = "PART_DeleteBtn";
        private const string BtnClosePartName = "PART_CloseBtn";
        private const string BtnClearPartName = "PART_ClearBtn";
        private const string BtnLookupPartName = "PART_LookupBtn";
        private const string BtnUpdatePartName = "PART_UpdateBtn";

        private AutoCompleteComboBox2 _cboBirthDate;
        private AutoCompleteComboBox2 _cboFirstName;
        private AutoCompleteComboBox2 _cboLastName;
        private Button _btnClose;
        private Button _btnClear;
        private Button _btnLookup;
        private Button _btnDelete;
        private Button _btnCheckIn;
        private object _selectedName;
        private ObservableCollection<IViewName> _patientNames;
        #endregion

        static DataGridAnnotationControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridAnnotationControl), new FrameworkPropertyMetadata(typeof(DataGridAnnotationControl)));
        }

        // The purpose of the DataGridAnnotationControl is to all editing of the selected visit from the Appointment book.
        public DataGridAnnotationControl()
        {
            /* VERY IMPORTANT! The bindings in the DataGridAnnotationAdorner will be made AFTER this constructor of the DataGridAnnotationControl. When the bindings are made, the source 
             * property will be IMMEDIATELY be read and the target property here in the DataGridAnnotationControl (and any callback) will
             * be IMMEDIATELY updated. Thus, any properties set in the  this constructor (or dependency properties) will be IMMEDIATELY overwritten from the source
             * of the binding in the DataGridAnnotationAdorner. */

            BorderBrush = Brushes.Black;
            Background = Brushes.AliceBlue;
            BorderThickness = new Thickness(20, 20, 20, 20);

        //    LastVisit = new VisitService();

            // Add a handler in the DataGridAnnotationControl to the routed event from the AutoCompleteComboBox.
        //    AddHandler(PatternChangedEvent, new PatternChangedEventHandler(PatternChanged));

            CI = new ContactInformation();

            ListByFirstName = new ObservableCollection<IViewName>();
            ListByLastName = new ObservableCollection<IViewName>();
            ListByBirthDate = new ObservableCollection<IViewName>();
        }

        /// <summary>
        /// ContactInformation class is presented on the display.
        /// </summary>
        public class ContactInformation : IContactInformation
        {
            public ContactInformation() { }

            public ContactInformation(IEncounterRecord eR)
            {
                sex = eR.sex;
                selfPay = eR.selfPay;
                cashOnly = eR.cashOnly;
                lastEncounter = eR.lastEncounter;
                encounterCount = eR.encounterCount;
                firstEncounter = eR.firstEncounter;
                tcompleted = eR.tcompleted;
                status = eR.status;
                notSeen = eR.notSeen;
                patientAge = eR.patientAge;
                timeSinceCheckIn = eR.timeSinceCheckIn;
                description = eR.description;
                provider = eR.provider;
                strBirthDate = eR.strBirthDate;
                missedAppointments = eR.missedAppointments;
                fileOnInsurance = eR.fileOnInsurance;
                appointmentTime = eR.appointmentTime;
                phoneNumber = eR.phoneNumber;
                postingTime = eR.postingTime;
                lastName = eR.lastName;
                firstName = eR.firstName;
                mi = eR.mi;
                birthDate = eR.birthDate;
                chartNumber = eR.chartNumber;
                doNotSee = eR.doNotSee;
                tservice = eR.tservice;
            }

            public string sex { get; set; }
            public bool? selfPay { get; set; }
            public bool? cashOnly { get; set; }
            public DateTime? lastEncounter { get; set; }
            public int? encounterCount { get; set; }
            public DateTime? firstEncounter { get; set; }
            public DateTime? tcompleted { get; set; }
            public string status { get; set; }
            public bool notSeen { get; set; }
            public string patientAge { get; set; }
            public TimeSpan? timeSinceCheckIn { get; set; }
            public string description { get; set; }
            public string provider { get; set; }
            public string strBirthDate { get; set; }
            public int? missedAppointments { get; set; }
            public bool? fileOnInsurance { get; set; }
            public DateTime? appointmentTime { get; set; }
            public string phoneNumber { get; set; }
            public DateTime? postingTime { get; set; }
            public string lastName { get; set; }
            public string firstName { get; set; }
            public string mi { get; set; }
            public DateTime? birthDate { get; set; }
            public int? chartNumber { get; set; }
            public bool? doNotSee { get; set; }
            public DateTime? tservice { get; set; }

            public bool ValidateAppointment()
            {
                if (appointmentTime.HasValue && birthDate.HasValue && !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
                    return true;

                return false;
            }

            public bool CanUpdate()
            {
                // The posting time is only assigned when an appointment is saved. A service without an appointment does not have a posting time.
                var time = tservice ?? postingTime;

                if (time.HasValue && birthDate.HasValue && !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
                    return true;

                return false;
            }

            public string ProperName()
            {
                throw new NotImplementedException();
            }

            public IService Clone()
            {
                throw new NotImplementedException();
            }

            public string PatientName()
            {
                throw new NotImplementedException();
            }
        }

        // OnApplyTemplate() is first called by the control.Measure().
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _cboBirthDate = GetTemplateChild(BirthDatePartName) as AutoCompleteComboBox2;
            _cboFirstName = GetTemplateChild(FirstNamePartName) as AutoCompleteComboBox2;
            _cboLastName = GetTemplateChild(LastNamePartName) as AutoCompleteComboBox2;
            _btnCheckIn = GetTemplateChild(BtnCheckInPartName) as Button;
            _btnDelete = GetTemplateChild(BtnDeletePartName) as Button;
            _btnClose = GetTemplateChild(BtnClosePartName) as Button;
            _btnClear = GetTemplateChild(BtnClearPartName) as Button;
            _btnLookup = GetTemplateChild(BtnLookupPartName) as Button;

            _btnClear.Click += btnClear_Click;              // Clear DataGridAnnotationControl.
            _btnLookup.Click += btnLookup_Click;            // Does a lookup on user entered name, if valid.
        }

        // Update the ItemsSource drop-down comboboxes.
        private async Task LoadComboBoxesAsync(ObservableCollection<IViewName> names)
        {
            if (names is null)
                return;

            await Task.Run(() =>
            {
                ListByFirstName = new ObservableCollection<IViewName>(names.OrderBy(q => q.firstName).ThenBy(q => q.lastName).ThenBy(q => q.birthDate));
                ListByLastName = new ObservableCollection<IViewName>(names.OrderBy(q => q.lastName).ThenBy(q => q.firstName).ThenBy(q => q.birthDate));
                ListByBirthDate = new ObservableCollection<IViewName>(names.OrderBy(q => q.birthDate).ThenBy(q => q.lastName).ThenBy(q => q.firstName));
            });

            patientListByFirstName = ListByFirstName;
            patientListByLastName = ListByLastName;
            patientListByBirthDate = ListByBirthDate;
        }

        /// <summary>
        /// The PatternChangedEvent handler --- called from the routed event. Update the patient list when text input from AutoCompleteComboBox.
        /// This event is run from the UI thread. It has access to the patientListByLastName and patientListByFirstName which are UI thread bound.
        /// ListByLastName and ListByFirstName are filled by a background thread before calling RefreshAutoCompleteComboBox.
        /// 
        /// Came here from: SchedulingCustomControls.dll!SchedulingCustomControls.AutoCompleteComboBox.RaisePatternChangedEvent()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PatternChanged(object sender, PatternChangedEventArgs e)
        {
            await LoadingComboBoxes;
            await FilterPatientNamesAsync(AllPatientNames);

            patientListByFirstName = ListByFirstName;
            patientListByLastName = ListByLastName;
            patientListByBirthDate = ListByBirthDate;
        }

        public event Action ClearEvent;
        public void RaiseClearEvent()
        {
            ClearEvent?.Invoke();
        }

        /// <summary>
        /// Handles the Click event of the _btnClear control. 
        /// </summary>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            CanEditName = true;
            ClearScreen();

            OnPropertyChanged(null);
            RaiseClearEvent();
        }


        /// <summary>
        /// Handles name lookup on user enetered data, if valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLookup_Click(object sender, RoutedEventArgs e)
        {
            /*
            var n = new NameValidatorService();
            if (!n.validate(firstName))
            {
                MessageBox.Show("First Name is invalid");
                return;
            }

            if (!n.validate(lastName))
            {
                MessageBox.Show("Last Name is invalid");
                return;
            }

            var b = new DateValidatorService();
            if (!b.validate(strBirthDate))
            {
                MessageBox.Show("BirthDate is invalid");
                return;
            }

            string mi = null;

            var patientName = new PatientNameService(lastName, firstName, mi, DateTime.Parse(strBirthDate)).GetPatientName();

            if (LookUpCommand.CanExecute(patientName))
                LookUpCommand.Execute(patientName);

            */
        }

        /// <summary>
        /// Create a time for an arbitrary date string.
        /// </summary>
        Func<string[], int[]> calendar = (i) =>
        {
            int x;
            int[] time = new int[3];

            for (int k = 0; k < i.Length; k++)
            {
                if (int.TryParse(i[k], out x))
                {
                    if (k == 0 && x < 13 && x > 0)           // Months
                        time[k] = x;
                    if (k == 1 && x < 31 && x > 0)          // Day
                        time[k] = x;
                    if (k == 2 && x < 2050 && x > 1900)     // Year
                        time[k] = x;
                }
            }
            return time;
        };

        private async Task FilterPatientNamesAsync(ObservableCollection<IViewName> allPatientNames)
        {
            await Task.Run(() =>
            {
                #region [Filter]
                IEnumerable<IViewName> names;

                var lastname = (lastName ?? string.Empty).ToUpper();
                var firstname = (firstName ?? string.Empty).ToUpper();
                var arBirthDate = (strBirthDate ?? string.Empty).Split(new char[] { '/' }, 3);

                var t = calendar(arBirthDate);

                if (t[2] > 0)
                    names = allPatientNames.Where(q => q.lastName.StartsWith(lastname) && q.firstName.StartsWith(firstname) && ((DateTime)q.birthDate).Month == t[0] && ((DateTime)q.birthDate).Day == t[1] && ((DateTime)q.birthDate).Year == t[2]);
                else if (t[1] > 0)
                    names = allPatientNames.Where(q => q.lastName.StartsWith(lastname) && q.firstName.StartsWith(firstname) && ((DateTime)q.birthDate).Month == t[0] && ((DateTime)q.birthDate).Day == t[1]);
                else if (t[0] > 0)
                    names = allPatientNames.Where(q => q.lastName.StartsWith(lastname) && q.firstName.StartsWith(firstname) && ((DateTime)q.birthDate).Month == t[0]);
                else
                    names = allPatientNames.Where(q => q.lastName.StartsWith(lastname) && q.firstName.StartsWith(firstname));
                #endregion

                // The FilterPatientNames is being run on a background thread. It will NOT have access to the
                // properties bound to the UI. (I.e., patientListByLastName and patientListByFirstName are part of the UI thread).
                var byLastName = names.OrderBy(q => q.lastName).ThenBy(q => q.firstName).ThenBy(q => q.birthDate);
                ListByLastName = new ObservableCollection<IViewName>(byLastName);

                var byFirstName = names.OrderBy(q => q.firstName).ThenBy(q => q.lastName).ThenBy(q => q.birthDate);
                ListByFirstName = new ObservableCollection<IViewName>(byFirstName);

                var byBirthDate = names.OrderBy(q => q.birthDate).ThenBy(q => q.lastName).ThenBy(q => q.firstName);
                ListByBirthDate = new ObservableCollection<IViewName>(byBirthDate);
            });
        }

        private void ClearScreen()
        {
            if (selectedItem is null)
            {
                lastName = null; firstName = null; strBirthDate = null;

                if (ClearCommand.CanExecute(null))
                    ClearCommand.Execute(null);
            }
            else
                selectedItem = null;

            OnPropertyChanged(null);
        }

        public string GetPatientName()
        {
            if (CI is null)
                return null;

            // return new PatientNameService(CI).GetTitlePatientName();
            return "What the fuck Name";
        }

        // The visit is data-bound in the DataGridAnnotationAdorner to the AppointmentEditor from which it gets its initial value.
        // User selection in the Appointments.xaml datagrid will initially set this value directly in the AppointmentEditor by way of SelectedVisitCommand.
        public IVisit Visit
        {
            get { return (IVisit)GetValue(VisitProperty); }
            set { SetValue(VisitProperty, value); }
        }
        public static readonly DependencyProperty VisitProperty =
            DependencyProperty.Register("Visit", typeof(IVisit), typeof(DataGridAnnotationControl), new PropertyMetadata(null, (s, e) =>
            {
                var sender = s as DataGridAnnotationControl;
                var visit = (IVisit)e.NewValue;

             //   var v = new VisitService(visit);
             //   sender.VisitHasAccount = v.HasAccount();
             //   sender.CanEditName = !sender.VisitHasAccount;
            }));

        /// <summary>
        /// Gets or sets a value indicating whether the appointment line is open and available or already assigned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public bool VisitHasAccount
        {
            get { return (bool)GetValue(VisitHasAccountProperty); }
            set { SetValue(VisitHasAccountProperty, value); }
        }
        public static readonly DependencyProperty VisitHasAccountProperty =
            DependencyProperty.Register("VisitHasAccount", typeof(bool), typeof(DataGridAnnotationControl), new PropertyMetadata(false));

        public bool CanEditName
        {
            get { return (bool)GetValue(CanEditNameProperty); }
            set { SetValue(CanEditNameProperty, value); }
        }
        public static readonly DependencyProperty CanEditNameProperty =
            DependencyProperty.Register("CanEditName", typeof(bool), typeof(DataGridAnnotationControl), new PropertyMetadata(true,
                (s, e) =>
                {
                    var sender = s as DataGridAnnotationControl;
                    var ee = (bool)e.NewValue;
                }

                ));

        // HasInsurance enables/disables the user selection of fileOnInsurance
        public bool HasInsurance
        {
            get { return (bool)GetValue(HasInsuranceProperty); }
            set { SetValue(HasInsuranceProperty, value); }
        }
        public static readonly DependencyProperty HasInsuranceProperty =
            DependencyProperty.Register("HasInsurance", typeof(bool), typeof(DataGridAnnotationControl), new PropertyMetadata(false));

        public ObservableCollection<IViewName> patientListByFirstName
        {
            get { return (ObservableCollection<IViewName>)GetValue(patientListByFirstNameProperty); }
            set { SetValue(patientListByFirstNameProperty, value); }
        }
        public static readonly DependencyProperty patientListByFirstNameProperty =
            DependencyProperty.Register("patientListByFirstName", typeof(ObservableCollection<IViewName>), typeof(DataGridAnnotationControl), new PropertyMetadata(null));

        public ObservableCollection<IViewName> patientListByLastName
        {
            get { return (ObservableCollection<IViewName>)GetValue(patientListByLastNameProperty); }
            set { SetValue(patientListByLastNameProperty, value); }
        }
        public static readonly DependencyProperty patientListByLastNameProperty =
            DependencyProperty.Register("patientListByLastName", typeof(ObservableCollection<IViewName>), typeof(DataGridAnnotationControl), new PropertyMetadata(null));

        public ObservableCollection<IViewName> patientListByBirthDate
        {
            get { return (ObservableCollection<IViewName>)GetValue(patientListByBirthDateProperty); }
            set { SetValue(patientListByBirthDateProperty, value); }
        }
        public static readonly DependencyProperty patientListByBirthDateProperty =
            DependencyProperty.Register("patientListByBirthDate", typeof(ObservableCollection<IViewName>), typeof(DataGridAnnotationControl), new PropertyMetadata(null));

        // The patientName is read only to the display in Generic.xaml. It presents the formatted name in bold type.
        public string patientName
        {
            get { return (string)GetValue(patientNameProperty); }
            set { SetValue(patientNameProperty, value); }
        }
        public static readonly DependencyProperty patientNameProperty =
            DependencyProperty.Register("patientName", typeof(string), typeof(DataGridAnnotationControl), new FrameworkPropertyMetadata(
            string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /*  AllPatientNames is bound programmatically in the DataGridAnnotationAdorner.
         *          Binding myBinding = new Binding();
                    myBinding.Source = adornedDataGrid.DataContext;                 // The DataContext for this user control is the AppointmentEditor. 
                    myBinding.Path = new PropertyPath("AllPatientNames");
                    myBinding.Mode = BindingMode.TwoWay;
                    myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    // Apply the binding to the DataGridAnnotationControl Control FrameworkElement.
                    BindingOperations.SetBinding(Control, DataGridAnnotationControl.AllPatientNamesProperty, myBinding);
         */

        public ObservableCollection<IViewName> AllPatientNames
        {
            get { return (ObservableCollection<IViewName>)GetValue(AllPatientNamesProperty); }
            set { SetValue(AllPatientNamesProperty, value); }
        }
        public static readonly DependencyProperty AllPatientNamesProperty =
            DependencyProperty.Register("AllPatientNames", typeof(ObservableCollection<IViewName>), typeof(DataGridAnnotationControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (f, e) =>
                {
                    var ff = f as DataGridAnnotationControl;
                    var ee = e;

                    ff.LoadingComboBoxes = ff.LoadComboBoxesAsync((ObservableCollection<IViewName>)ee.NewValue);
                }));

        public IEncounterRecord ER
        {
            get { return (IEncounterRecord)GetValue(ERProperty); }
            set { SetValue(ERProperty, value); }
        }
        public static readonly DependencyProperty ERProperty =
            DependencyProperty.Register("ER", typeof(IEncounterRecord), typeof(DataGridAnnotationControl), new PropertyMetadata(null, (s, e) =>
            {
                var sender = s as DataGridAnnotationControl;
                var data = (IEncounterRecord)e.NewValue;

                if (data is null)
                    sender.CI = new ContactInformation();
                else
                    sender.CI = new ContactInformation(data);

                sender.patientName = sender.GetPatientName();

                sender.OnPropertyChanged(null);
            }));

        // selectedItem is the selected item from the drop-down comboboxes. It is bound to SelectedPatientName via the DataGridAnnotationAdorner.
        public IViewName selectedItem
        {
            get { return (IViewName)GetValue(selectedItemProperty); }
            set { SetValue(selectedItemProperty, value); }
        }
        public static readonly DependencyProperty selectedItemProperty = DependencyProperty.Register("selectedItem", typeof(IViewName), typeof(DataGridAnnotationControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Policies will display insurance information to the user.
        public List<IInsurancePolicy> Policies
        {
            get { return (List<IInsurancePolicy>)GetValue(PoliciesProperty); }
            set { SetValue(PoliciesProperty, value); }
        }
        public static readonly DependencyProperty PoliciesProperty =
            DependencyProperty.Register("Policies", typeof(List<IInsurancePolicy>), typeof(DataGridAnnotationControl), new PropertyMetadata(null, (s, e) =>
            {
                var sender = s as DataGridAnnotationControl;
                var data = (List<IInsurancePolicy>)e.NewValue;

                if (data is null)
                    sender.HasInsurance = false;
                else
                    sender.HasInsurance = data.Count > 0;

                sender.OnPropertyChanged(null);
            }));

        public ICommand SelectionChangedCommand
        {
            get { return (ICommand)GetValue(SelectionChangedCommandProperty); }
            set { SetValue(SelectionChangedCommandProperty, value); }
        }
        public static DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register("SelectionChangedCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        public ICommand SaveAppointmentCommand
        {
            get { return (ICommand)GetValue(SaveAppointmentCommandProperty); }
            set { SetValue(SaveAppointmentCommandProperty, value); }
        }
        public static DependencyProperty SaveAppointmentCommandProperty = DependencyProperty.Register("SaveAppointmentCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        public ICommand UpdateEncounterCommand
        {
            get { return (ICommand)GetValue(UpdateEncounterCommandProperty); }
            set { SetValue(UpdateEncounterCommandProperty, value); }
        }
        public static DependencyProperty UpdateEncounterCommandProperty = DependencyProperty.Register("UpdateEncounterCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        public ICommand DeleteEncounterCommand
        {
            get { return (ICommand)GetValue(DeleteEncounterCommandProperty); }
            set { SetValue(DeleteEncounterCommandProperty, value); }
        }
        public static DependencyProperty DeleteEncounterCommandProperty = DependencyProperty.Register("DeleteEncounterCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }
        public static DependencyProperty CloseCommandProperty = DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        public ICommand CheckInCommand
        {
            get { return (ICommand)GetValue(CheckInCommandProperty); }
            set { SetValue(CheckInCommandProperty, value); }
        }
        public static DependencyProperty CheckInCommandProperty = DependencyProperty.Register("CheckInCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        public ICommand LookUpCommand
        {
            get { return (ICommand)GetValue(LookUpCommandProperty); }
            set { SetValue(LookUpCommandProperty, value); }
        }
        public static DependencyProperty LookUpCommandProperty = DependencyProperty.Register("LookUpCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        public ICommand ClearCommand
        {
            get { return (ICommand)GetValue(ClearCommandProperty); }
            set { SetValue(ClearCommandProperty, value); }
        }
        public static DependencyProperty ClearCommandProperty = DependencyProperty.Register("ClearCommand", typeof(ICommand), typeof(DataGridAnnotationControl));

        private ContactInformation _CI;
        public ContactInformation CI
        {
            get { return _CI; }
            private set { if (_CI == value) return; _CI = value; OnPropertyChanged(); }
        }

        public bool lastInsuranceFiling { get; private set; }

        public IVisitService LastVisit { get; private set; }
        public Task LoadingComboBoxes { get; private set; }
        public Task<IEnumerable<IViewName>> PatientNamesLoaded { get; private set; }
        public Task<bool> PatientListUpdated { get; private set; }

        // AutoCompleteComboBox -- ItemsSource
        public ObservableCollection<IViewName> ListByLastName { get; private set; }
        public ObservableCollection<IViewName> ListByFirstName { get; private set; }
        public ObservableCollection<IViewName> ListByBirthDate { get; private set; }

        // EncounterRecord/ContactInformation
        public DateTime? AppointmentTime => CI.appointmentTime;
        public int? chartNumber => CI.chartNumber;
        public bool? selfPay => CI.selfPay;
        public bool? doNotSee => CI.doNotSee;
        public bool? cashOnly => CI.cashOnly;
        public string sex => CI.sex;
        public string patientAge => CI.patientAge;
        public DateTime? lastEncounter => CI.lastEncounter;
        public int? missedAppointments => CI.missedAppointments;
        public DateTime? postingTime => CI.postingTime;
        public DateTime? tservice => CI.tservice;   // CheckinTime is tservice.

        // Primary Insurance
        public DateTime? verificationDate => (Policies.Count() == 0) ? null : Policies[0].verificationDate;
        public string primaryMemberId => (Policies.Count() == 0) ? null : Policies[0].policyNumber;
        public string primaryInsuranceName => (Policies.Count() == 0) ? null : Policies[0].name;
        public decimal? deductible => (Policies.Count() == 0) ? null : Policies[0].deductible;
        public decimal? coPay => (Policies.Count() == 0) ? null : Policies[0].coPay;
        public string relationToPolicyHolder => (Policies.Count() == 0) ? null : Policies[0].relationToPolicyHolder;

        // Secondary Insurance
        public string secondaryMemberId => (Policies.Count() > 1) ? Policies[1].policyNumber : null;
        public string secondaryInsuranceName => (Policies.Count() > 1) ? Policies[1].name : null;

        // AutoCompleteComboBox
        public IViewName selectedName
        {
           get { return (IViewName)_selectedName; }
           set { if (_selectedName == value) return; _selectedName = value; OnPropertyChanged(); }
        }
        public ObservableCollection<IViewName> PatientNames
        {
            get { return _patientNames; }
            set { if (_patientNames == value) return; _patientNames = value; OnPropertyChanged(); }
        }

        // The AutoCompleteComboBox Text is bound directly to the strBirthDate in the Generic.xaml
        public string strBirthDate
        {
            get { return CI.strBirthDate; }
            set { CI.strBirthDate = value; OnPropertyChanged(); }
        }

        // The AutoCompleteComboBox Text is bound directly to the firstName in the Generic.xaml
        public string firstName
        {
            get { return CI.firstName; }
            set { if (CI.firstName == value) return; CI.firstName = value; OnPropertyChanged(); }
        }

        // The AutoCompleteComboBox Text is bound directly to the lastName in the Generic.xaml
        public string lastName
        {
            get { return CI.lastName; }
            set { if (CI.lastName == value) return; CI.lastName = value; OnPropertyChanged(); }
        }

        // User input
        public string phoneNumber
        {
            get { return CI.phoneNumber; }
            set { CI.phoneNumber = value; }
        }
        public bool? fileOnInsurance
        {
            get { return CI.fileOnInsurance; }
            set { CI.fileOnInsurance = value; }
        }
    }
}

