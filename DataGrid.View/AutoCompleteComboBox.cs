using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Timers;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;



/// <summary>
/// AutocompleteComboBox (C:\Users\Alan Wayne\Downloads\AutoComplete\AutoComplete\AutoComplete.sln) is from
/// https://www.codeproject.com/Articles/47481/WPF-Auto-complete-Control
/// WPF Auto-complete Control,  Yang Yu, 14 Dec 2009
/// 
/// Implementing an auto-complete control in WPF by extending the ComboBox control.
/// Introduction
/// There seems to be a lack of support for an auto-complete control in WPF.The closest one to it is the ComboBox which is the 
/// base of our implementation for this article.
/// 
/// Background
/// An auto-complete control is one that allows the user to enter text while querying for a possible selection based on what the user has already entered.
/// The most popular auto-complete implementation deals with querying of a "Starts With" of the current text in the control.
/// 
/// How it Works
/// Here are some of the properties we care about in the ComboBox:
/// IsEditable- This allows the user to input text into the control.
/// StaysOpenOnEdit - This will force the combobox to stay open when typing.
/// IsTextSearchEnabled - This uses the default "auto-complete" behavior of the ComboBox.
/// 
/// The Magic
/// By using a combination of the above properties(pretty self-explanatory) and a timer to control the delay of the query, an event which allows the user to attach 
/// a new data source, and some styles, we could implement an auto-complete control.
/// 
/// Using the Control
/// XAML
/// <Window x:Class= "Gui.TestWindow"
///    xmlns= "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
///    xmlns:x= "http://schemas.microsoft.com/winfx/2006/xaml"
///    xmlns:ctr= "clr-namespace:Gui.Controls"
///    Title= "Auto Complete Test"
///    Height= "200" Width= "300"
///    Loaded= "Window_Loaded" >
///       < StackPanel >
///       < StackPanel.Resources >
///       < ResourceDictionary
///       Source= "/Gui.Controls;component/Styles/AutoComplete.Styles.xaml" />
///      </ StackPanel.Resources >
///   < Label > Cities:</Label>
///   <ctr:AutoComplete x:Name= "autoCities"
///
///     SelectedValuePath= "CityID" DisplayMemberPath= "Name"
///         PatternChanged= "autoCities_PatternChanged"
///          Style= "{StaticResource AutoCompleteComboBox}"
///          Delay= "500" />
///      < !--can also do binding on selected value -->
/// </StackPanel>
/// </Window>
/// 
/// Similar to a combo box, an auto-complete control utilizes the DisplayMemberPath and SelectValuePath properties to bind to a specific data source.
/// Code-Behind
/// <summary>
/// occurs when the user stops typing after a delayed timespan
/// </summary>
/// <param name="sender"></param>
/// <param name="args"></param>
///protected void autoCities_PatternChanged(object sender,
///      Gui.Controls.AutoComplete.AutoCompleteArgs args)
///{
///    //check
///    if (string.IsNullOrEmpty(args.Pattern))
///        args.CancelBinding = true;
///    else
///        args.DataSource = TestWindow.GetCities(args.Pattern);
///}
/// We can utilize the PatternChanged event to subscribe to changes to the the data source.This data source is also equal to a pattern the user has 
/// currently entered into the control.
///
///  Points of Interest
///  We utilize the MVVM pattern to create a ViewModel of any entity bound to the data source which contains the HighLight property. Through the use of styles, 
///  this highlighted section will be reflected in the dropdown.
/// </summary>
/// <seealso cref="System.Windows.Controls.ComboBox" />



/*
 * Logic
 *          XAML:
 * <local:AutoCompleteComboBox x:Name="cboLastName" 
 *      Style="{StaticResource AutoCompleteComboBox}" 
 *      Margin="10,0" Width="150" FontSize="18" 
 *      DisplayMemberPath="lastName" 
 *      Delay="500"/>
 * 
 * The AutoCompleteComboBox is defined in C:\Nova6\2.0\SchedulingCustomControls\AutoComplete.Styles.xaml
 *       <Style x:Key="AutoCompleteComboBox" TargetType="ComboBox" BasedOn="{StaticResource ComboBoxEx}">
 *       
 * Keypress input is managed by:
 *          this.EditableTextBox.PreviewKeyDown += new KeyEventHandler(EditableTextBox_PreviewKeyDown);
 *          this.EditableTextBox.TextChanged += new TextChangedEventHandler(EditableTextBox_TextChanged);
 *          
 *  Upon initialization, a timer is set. A delay of 500ms is set from the DataGridAnnotationControl. The timer
 *  has autoreset set to true. Upon firing, the timer event handler will call the bound PatternChanged event
 *  handler in DataGridAnnotationControl.
 * 
 */

namespace DataGrid.View
{
    /// <summary>
    /// See: C:\Nova6\2.0\SchedulingCustomControls\Resource Dictionaries\AutoComplete.Styles.xaml for ControlTemplate.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.ComboBox" />
    /// 

    [TemplatePart(Name = "DropDownBorder", Type = typeof(Border))]
    [TemplatePart(Name = "Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
    public class AutoCompleteComboBox2 : ComboBox, IDisposable
    {
        private const string EditableTextBoxPartName = "PART_EditableTextBox";

        public delegate void PatternChangedEventHandler(object sender, PatternChangedEventArgs e);

        /// <summary>
        /// event handler for auto complete pattern changed
        ///    Useage: public event AutoCompleteHandler PatternChanged;
        ///    completed = new TaskCompletionSource<object>();
        ///    PatternChanged(this, args, completed);
        ///    completed.TrySetResult(null);    // Signal that task/event has completed.
        ///    
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void AutoCompleteHandler(object sender, PatternChangedEventArgs args, TaskCompletionSource<object> completed);

        /// <summary>
        /// Event handler when ONLY ONE SUBSCRIBER. Does not work well for multiple subscribers. It returns a TASK and not a void.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate Task AsyncEventHandler(object sender, PatternChangedEventArgs e);

        #region [Events]
        // The DataGridAnnotationControl is listening with AddHandler.
        public static readonly RoutedEvent PatternChangedEvent = EventManager.RegisterRoutedEvent(
           "PatternChanged", RoutingStrategy.Bubble, typeof(PatternChangedEventHandler), typeof(AutoCompleteComboBox2));

        /// <summary>
        /// Occurs when [text changed].
        /// </summary>
        public event Action<object, TextChangedEventArgs> TextChanged;

        /// <summary>
        /// Used to pass arguments for autocomplete control
        /// </summary>

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

        #endregion

        #region Fields
        private Timer _interval;
        private bool IsKeyEvent = false;
        protected TextBox EditableTextBox;
        private const int DEFAULT_DELAY = 800; // 1 seconds delay
        #endregion

        static AutoCompleteComboBox2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteComboBox2), new FrameworkPropertyMetadata(typeof(AutoCompleteComboBox2)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            EditableTextBox = GetTemplateChild(EditableTextBoxPartName) as TextBox;

            LostFocus += AutoCompleteComboBox_LostFocus;
            SelectionChanged += AutoCompleteComboBox_SelectionChanged;

            // Listen for key down before the texbox gets it.
            EditableTextBox.PreviewKeyDown += new KeyEventHandler(EditableTextBox_PreviewKeyDown);
            EditableTextBox.TextChanged += new TextChangedEventHandler(EditableTextBox_TextChanged);
            EditableTextBox.KeyUp += EditableTextBox_KeyUp;
        }

        private void EditableTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // The TextChanged event will only be called if there IS text. Hence, if the backspace or delete
            // key removes the last character, there is no text to be changed.
            if (e.Key == Key.Back && string.IsNullOrWhiteSpace(Text))
            {
                ResetTimer();
            }
            if (e.Key == Key.Delete && string.IsNullOrWhiteSpace(Text))
            {
                ResetTimer();
            }
        }

        public AutoCompleteComboBox2()
        {
            Loaded += AutoCompleteComboBox_Loaded;
        }

        private void AutoCompleteComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // Wait until the AutoCompleteComboBox is loaded to read the Delay from the XAML.
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _interval = new Timer(Delay);
                _interval.AutoReset = true;
                _interval.Elapsed += new ElapsedEventHandler(_interval_ElapsedAsync);
                // The timer is not started until the TextChanged event.
            }
        }

        #region Properties
        /// <summary>
        /// The timespan between keypress and pattern changed event
        /// </summary>
        [DefaultValue(DEFAULT_DELAY)]
        public int Delay { get; set; } = DEFAULT_DELAY;
        /// <summary>
        /// The maximum number of records to show in the drop down
        /// </summary>
        public int MaxRecords { get; set; } = 10;
        /// <summary>
        /// Determines weather textbox does type ahead
        /// </summary>
        public bool TypeAhead { get; set; } = false;
        /// <summary>
        /// returns the selected items text representation
        /// </summary>
        public string SelectedText
        {
            get
            {
                if (SelectedIndex == -1) return string.Empty;

                return this.SelectedItem.GetType().GetProperty(this.DisplayMemberPath).GetValue(this.SelectedItem, null).ToString();

            }
        }

        #endregion

        private async void _interval_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            // pause/stop the timer
            _interval.Stop();

            IsKeyEvent = false;

            await Dispatcher.BeginInvoke(new Action(() =>
            {
                // EVERYTHING INSIDE A DISPATCHER WILL BE RUN ON THE UI THREAD UNLESS CHANGED BY CONFIGUREAWAIT(FALSE).

                // Text can only be accessed from the UI thread.
                // Events are handled on the thread that raised them. Hence, RaiseRefreshAutoCompleteComboBox event will be bound
                // to this (the UI) thread. This is a routed event.

                RaisePatternChangedEvent();

                if (!(Text is null))
                    EditableTextBox.CaretIndex = Text.Length;

                // show the drop down list.
                this.IsDropDownOpen = true;
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Handles the LostFocus event of the AutoCompleteComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AutoCompleteComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IsKeyEvent = false;
            IsDropDownOpen = false;
            // release timer resources
            _interval.Close();

            try
            {
                EditableTextBox.CaretIndex = 0;
            }
            catch { }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the AutoCompleteComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This event handler is called AFTER the SelectionChanged handler in DataGridAnnotationControl.
        /// </remarks>

        private void AutoCompleteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(_interval is null))
                _interval.Stop();
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the EditableTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void EditableTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IsKeyEvent = true;

            // If a key is pressed AFTER a selection is made, then assume no selection was made.
            if (!(SelectedItem is null))
            {
                var box = (TextBox)sender;
                var t = box.Text;
                var s = box.SelectionStart;  // position of cursor at time of key press.

                SelectedItem = null;    // making this null clears the DataGridAnnotationControl and all comboboxes.

                box.Text = t;
                box.SelectionStart = s;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the EditableTextBox control. 
        /// The xaml bound properties in the DataGridAnnotationControl will be set before the EditableTextBox_TextChanged event occurs in AutoCompleteComboBox.
        /// The EditableTextBox_TextChanged event is called by both keyboard input and programmatic change to the text, hence the need for IsKeyEvent
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>     
        void EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TextChanged event can only be called when there is text TO BE CHANGED!. So if the textbox is already empty,
            // delete and backspace will NOT trigger a textchanged event.
            if (string.IsNullOrWhiteSpace(EditableTextBox.Text.Trim()))
                TextChanged?.Invoke(sender, e);     // call the DataGrid to decide what to do with the other search boxes.
            else if (IsKeyEvent)
                ResetTimer();                       // start interval timer.
        }

        /// <summary>
        /// Resets the timer.
        /// </summary>
        protected void ResetTimer()
        {
            _interval.Stop();
            _interval.Start();
        }


        #region [CLR accessors for the events]
        //add remove handlers. Provide CLR accessors to event.
        public event PatternChangedEventHandler PatternChanged
        {
            add { AddHandler(PatternChangedEvent, value); }
            remove { RemoveHandler(PatternChangedEvent, value); }
        }

        void RaisePatternChangedEvent()
        {
            // Text is the ComboBox.Text
            PatternChangedEventArgs args = new PatternChangedEventArgs(Text);
            RaiseEvent(args);
        }

        #endregion


        #region [IDisposable]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _interval.Close();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

