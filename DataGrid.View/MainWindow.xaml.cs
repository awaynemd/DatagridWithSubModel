using System.Windows;
using System;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace DataGrid.View
{
    public interface ICloseable
    {
        event Action RequestClose;
    }

    public partial class MainWindow : Window
    {
        private AdornerLayer _adornerLayer;
        private DataGridAnnotationAdorner _adorner;

        // The Appointments AppointmentDate is xaml bound (see: DoctorView.xaml) to the SelectedAppointmentDate of the AppointmentEditor.
        public MainWindow()
        {
            InitializeComponent();

            // show a clock
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                CurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
            }, Dispatcher);
        }

        /// <summary>
        /// This Handles the ScheduledAppointment event of the AppointmentDataGrid control. This event is triggered when an item selection is made.
        /// This is XAML bound in Appointments.xaml
        ///       <customcontrols:AppointmentDataGrid ...customcontrols:AppointmentListView.ScheduledAppointment  ="AppointmentDataGrid_ScheduledAppointment"...  
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ScheduledAppointmentEventArgs"/> instance containing the event data.</param>
        private void AppointmentDataGrid_ScheduledAppointment(object sender, ScheduledAppointmentEventArgs args)
        {
            AdornerClose();
            AppointmentDataGrid fe = (AppointmentDataGrid)sender;

            ((ICloseable)DataContext).RequestClose += Appointments_RequestClose;

            // The appointmentKey comes directly from the selected listview element. 
            IVisit visit = (IVisit)args.appointmentKey;

            // Must set the selected visit in the AppointmentEditor before the bindings of the adorner bind it to the DataGridAnnotationControl.
            // RaiseCommand here causes the "Command" (defined below) of the Appointments.XAML to call the RelayCommand via the DoctorView.Xaml.
            // The Command Target is SelectedVisit is the AppointmentEditor.cs.
            RaiseVisitCommand(visit);

            // Creation of the adorner will create the DatGridAnnotationControl and immediately create the bindings.
            _adorner = new DataGridAnnotationAdorner(fe);

            // put adorner into the adornerlayer.
            InstallAdorner(fe, _adorner);
        }

        /// <summary>
        /// Installs the adorner.
        /// </summary>
        /// <param name="fe">The fe.</param>
        /// <param name="adorner">The adorner.</param>
        /// <exception cref="ArgumentException">datagrid does not have have an adorner layer.</exception>
        private void InstallAdorner(FrameworkElement fe, Adorner adorner)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(fe);

            if (_adornerLayer == null)
            {
                // if we don't have an adorner layer it's probably because it's too early in the window's construction
                // Let's re-run at a slightly later time
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(() => InstallAdorner(fe, adorner)));
                return;
            }

            if (_adornerLayer == null)
                throw new ArgumentException("datagrid does not have have an adorner layer.");

            // Add the adorner to the DataGrid's adorner layer.
            _adornerLayer.Add(adorner);
        }


        private void Appointments_RequestClose()
        {
            AdornerClose();
        }

        /// <summary>
        /// Remove all the attached events and close the adorner and its control.
        /// </summary>
        private void AdornerClose()
        {
            if (_adorner != null)
            {
                _adorner.Control = null;

                // remove adorner
                _adornerLayer.Remove(_adorner);
                _adornerLayer = null;
                _adorner = null;
            }
        }


        private void RaiseVisitCommand(IVisit visit)
        {
            if (visit is IVisit Visit)
            {
                var target = CommandTarget;

                var routedCmd = VisitCommand as RoutedCommand;
                if (routedCmd != null && routedCmd.CanExecute(Visit, target))
                {
                    routedCmd.Execute(Visit, target);
                }
                else if (VisitCommand != null && VisitCommand.CanExecute(Visit))
                {
                    // This is NOT a routed command. The command "VisitCommand" itself is defined
                    // below as a command source. The target is defined in DoctorView.xaml as "SelectedName" in AppointmentEditor.
                    VisitCommand.Execute(Visit);
                }
            }
        }

      

        #region [Implementation of ICommandSource for Appointments.Command]
        //  ICommand ICommandSource.Command => throw new NotImplementedException();

        //  object ICommandSource.CommandParameter => throw new NotImplementedException();

        //  IInputElement ICommandSource.CommandTarget => throw new NotImplementedException();

        /* I am defining a "Command" for the Appointments.XAML. It is used as:
         *        <scheduling:Appointments Grid.Column="2" Grid.Row="0" Grid.RowSpan="2" 
         *                                    AppointmentDate="{Binding SelectedAppointmentDate}"
         *                                    VisitCommand="{Binding SelectName}"
         *                                    />
         */



        public ICommand CloseWindowCommand
        {
            get { return (ICommand)GetValue(CloseWindowCommandProperty); }
            set { SetValue(CloseWindowCommandProperty, value); }
        }
        public static DependencyProperty CloseWindowCommandProperty = DependencyProperty.Register("CloseWindowCommand", typeof(ICommand), typeof(MainWindow), new UIPropertyMetadata(null));



        public object CloseWindowCommandParameter
        {
            get { return (object)GetValue(CloseWindowCommandParameterProperty); }
            set { SetValue(CloseWindowCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseWindowCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseWindowCommandParameterProperty =
            DependencyProperty.Register("CloseWindowCommandParameter", typeof(object), typeof(MainWindow), new UIPropertyMetadata(null));


        /// <summary>
        /// Create a command source for the Appointments.xaml. This is "executed" when an item is selected from the AppointmentListView and will be handled by the "SelectedName" relaycommand
        /// in the DoctorView.xaml datacontext of AppointmentEditor.
        /// </summary>
        public ICommand VisitCommand
        {
            get { return (ICommand)GetValue(VisitCommandProperty); }
            set { SetValue(VisitCommandProperty, value); }
        }

        public static readonly DependencyProperty VisitCommandProperty =
            DependencyProperty.Register("VisitCommand", typeof(ICommand), typeof(MainWindow), new UIPropertyMetadata(null));


        public object VisitCommandParameter
        {
            get { return (object)GetValue(VisitCommandParameterProperty); }
            set { SetValue(VisitCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisitCommandParameterProperty =
            DependencyProperty.Register("VisitCommandParameter", typeof(object), typeof(MainWindow), new UIPropertyMetadata(null));


        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(MainWindow), new UIPropertyMetadata(null));


        #endregion

        #region [Dependency Properties -- Only TARGETS of bindings must be Dependeny Properties]
        // The AppointmentDate is xaml (see: DoctorView.xaml) bound to the SelectedAppointmentDate of the AppointmentEditor for the Table Date.
        // The AppointmentDate is set by selection of a calendar date in the DoctorView.xaml.
        public DateTime AppointmentDate
        {
            get { return (DateTime)GetValue(AppointmentDateProperty); }
            set { SetValue(AppointmentDateProperty, value); }
        }

        // The AppointmentDate for the Appointments screen is only read into it (i.e., source to target -- not twoway binding).
        public static readonly DependencyProperty AppointmentDateProperty =
            DependencyProperty.Register("AppointmentDate", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now,
                (s, e) =>
                {
                    var ss = (MainWindow)s;
                    var _appointmentDate = (DateTime)e.NewValue;

                  

                    // Do not rebuild the "rows" of the DataGrid. Doing so causes the DataGrid to first appear empty before being filled--so it flashes.
                    // ss.FillScheduleAsync(_appointmentDate);

                }));
        #endregion					

    }
}


