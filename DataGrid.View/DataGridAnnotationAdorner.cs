using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;



/* 
 *  https://stackoverflow.com/questions/9998691/wpf-adorner-with-controls-inside
 *  
 * Unlike most controls in WPF, adorners don't have any out-of-the-box way of assigning child elements. Without adding anything to adorners, you can only 
 * override their OnRender method and draw stuff within the DrawingContext that gets passed into it. (stuff like creating drag handles around an object).
 * 
 * The trick to doing this is to create a VisualCollection and set your adorner as its owner by passing it into the constructor for the collection.

This isn't mentioned in the article, but note that it is possible to combine the VisualCollection technique along with drawing in the OnRender method 
of the adorner. I'm using OnRender to achieve the side and top borders described in my diagram above and using VisualCollection to place and create the 
controls.

Edit: here is the source code from the mentioned blog post since it is no longer available:

public class AdornerContentPresenter : Adorner
{
  private VisualCollection _Visuals;
  private ContentPresenter _ContentPresenter;

  public AdornerContentPresenter(UIElement adornedElement)
    : base(adornedElement)
  {
    _Visuals = new VisualCollection(this);
    _ContentPresenter = new ContentPresenter();
    _Visuals.Add(_ContentPresenter);
  }

  public AdornerContentPresenter(UIElement adornedElement, Visual content)
    : this(adornedElement)
  { Content = content; }

  protected override Size MeasureOverride(Size constraint)
  {
    _ContentPresenter.Measure(constraint);
    return _ContentPresenter.DesiredSize;
  }

  protected override Size ArrangeOverride(Size finalSize)
  {
    _ContentPresenter.Arrange(new Rect(0, 0,
         finalSize.Width, finalSize.Height));
    return _ContentPresenter.RenderSize;
  }

  protected override Visual GetVisualChild(int index)
  { return _Visuals[index]; }

  protected override int VisualChildrenCount
  { get { return _Visuals.Count; } }

*/

/*
 * Binding myBinding = new Binding();
myBinding.Source = ViewModel;
myBinding.Path = new PropertyPath("SomeString");
myBinding.Mode = BindingMode.TwoWay;
myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
BindingOperations.SetBinding(txtText, TextBox.TextProperty, myBinding);
 */


namespace DataGrid.View
{
    public class DataGridAnnotationAdorner : Adorner
    {
        private ArrayList _logicalChildren;
        private Point _location;
        public DataGridAnnotationControl Control { get; set; }

        public DataGridAnnotationAdorner(System.Windows.Controls.DataGrid adornedDataGrid)
            : base(adornedDataGrid)
        {
            // The "control" can be a controltemplate from a contentcontrol, a datatemplate, a style, or a custom control.
            // The Adorner content is the DataGridAnnotationControl from project SchedullingCustomControls.
            // The purpose of the DataGridAnnotationControl is to allow user editing of the selected visit from the appointment book.
            Control = new DataGridAnnotationControl();

            // Apply the binding to the DataGridAnnotationControl Control FrameworkElement.
            // Very Important ! Note: Memory leaks will occur when the property in the Path is a not a DependencyProperty or does not implement INotifyPropertyChanged.

            /* VERY IMPORTANT! When the following bindings are made, the source property will be IMMEDIATELY read and the target property (and any callback) will
             * be IMMEDIATELY updated. Thus, any properties set in the Control constructor (or dependency properties) will be IMMEDIATELY overwritten from the source
             * of the binding. */

            // AllPatientNames provides the ItemsSource for the drop-down comboboxes.
            Binding myBinding = new Binding("AllPatientNames");
            myBinding.Source = adornedDataGrid.DataContext;                 // The DataContext for this user control is the AppointmentEditor.                   
            myBinding.Mode = BindingMode.TwoWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.AllPatientNamesProperty, myBinding);

            // A name selected from the combobox.
            Binding myBinding1 = new Binding("SelectedPatientName");
            myBinding1.Source = adornedDataGrid.DataContext;                 // The DataContext for this user control is the AppointmentEditor.           
            myBinding1.Mode = BindingMode.TwoWay;
            myBinding1.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.selectedItemProperty, myBinding1);

            // EncounterRecord provides the data source for the DataGridAnnotationControl.
            Binding myBinding2 = new Binding("ER");
            myBinding2.Source = adornedDataGrid.DataContext;                 // The DataContext for this user control is the AppointmentEditor.           
            myBinding2.Mode = BindingMode.TwoWay;
            myBinding2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.ERProperty, myBinding2);

            // Policies provides the data source for the DataGridAnnotationControl.
            Binding myBinding3 = new Binding("Policies");
            myBinding3.Source = adornedDataGrid.DataContext;                 // The DataContext for this user control is the AppointmentEditor.            
            myBinding3.Mode = BindingMode.TwoWay;
            myBinding3.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.PoliciesProperty, myBinding3);

            Binding cmd1 = new Binding("LookUpCommand");
            cmd1.Source = adornedDataGrid.DataContext;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.LookUpCommandProperty, cmd1);

            // Tell the AppointmentEditor.cs to make a new appointment for the patient.
            Binding cmd2 = new Binding("SaveAppointmentCommand");
            cmd2.Source = adornedDataGrid.DataContext;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.SaveAppointmentCommandProperty, cmd2);

            // Tell the AppointmentEditor.cs to create a doctor service for the patient.
            Binding cmd3 = new Binding("CheckInCommand");
            cmd3.Source = adornedDataGrid.DataContext;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.CheckInCommandProperty, cmd3);

            Binding cmd4 = new Binding("ClearCommand");
            cmd4.Source = adornedDataGrid.DataContext;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.ClearCommandProperty, cmd4);

            Binding cmd5 = new Binding("UpdateEncounterCommand");
            cmd5.Source = adornedDataGrid.DataContext;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.UpdateEncounterCommandProperty, cmd5);

            Binding cmd6 = new Binding("DeleteEncounterCommand");
            cmd6.Source = adornedDataGrid.DataContext;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.DeleteEncounterCommandProperty, cmd6);

            Binding cmd7 = new Binding("CloseCommand");
            cmd7.Source = adornedDataGrid.DataContext;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.CloseCommandProperty, cmd7);

            Binding myBinding4 = new Binding("CanEditName");
            myBinding4.Source = adornedDataGrid.DataContext;                 // The DataContext for this user control is the AppointmentEditor.           
            myBinding4.Mode = BindingMode.TwoWay;
            myBinding4.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.CanEditNameProperty, myBinding4);


            // THE ORDER OF THE BINDINGS IS EXTREMELEY IMPORTANT, especially since each binding will be evaluated immediately when the binding is created.
            // The binding to the VISIT needs to be the LAST BINDING MADE to initiallize the control.
            Binding myBinding5 = new Binding("Visit");
            myBinding5.Source = adornedDataGrid.DataContext;                 // The DataContext for this user control is the AppointmentEditor.           
            myBinding5.Mode = BindingMode.TwoWay;
            myBinding5.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(Control, DataGridAnnotationControl.VisitProperty, myBinding5);

            AddLogicalChild(Control);
            AddVisualChild(Control);
        }

        #region Measure/Arrange
        /// <summary>
        /// Allows the control to determine how big it wants to be. Note: Measuring will call up the bindings. The Loaded event
        /// occurs AFTER measuring and layout.
        /// </summary>
        /// <param name="constraint">A limiting size for the control.</param>
        protected override System.Windows.Size MeasureOverride(Size constraint)
        {
            Control.Measure(constraint); // OnApplyTemplate() is called from here.
            return Control.DesiredSize;
        }

        /// <summary>
        /// Positions and sizes the control.
        /// </summary>
        /// <param name="finalSize">The actual size of the adorner control.</param>		
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Position the adorner control to the center of the adorned element.
            double xloc = (((System.Windows.Controls.DataGrid)AdornedElement).ActualWidth - Control.DesiredSize.Width) / 2;
            double yloc = (((System.Windows.Controls.DataGrid)AdornedElement).ActualHeight - Control.DesiredSize.Height) / 2;

            _location = new Point(xloc, yloc);

            // a location of O(0,0) is top-left corner of the adorned DataGrid.
            Rect rect = new Rect(_location, finalSize);

            Control.Arrange(rect);

            return finalSize;
        }

        // The OnRender method is called as the Rendering of the Adorner. The Adorner is a FrameworkElement. 
        // The OnRender method handles drawing our rectangle, and the  GetLayoutClip method handles setting a clipping Geometry 
        // that avoids drawing on top of the adorned control. (i.e., the GetLayoutClip allow a space in the adorner to get to the
        // AdornedElement--not what I need here. See: SmokeScreenAdorner).
        protected override void OnRender(DrawingContext drawingContext)
        {
            SolidColorBrush screenBrush_ = new SolidColorBrush();
            screenBrush_.Color = Colors.Crimson;
            screenBrush_.Opacity = 0.3;
            screenBrush_.Freeze();

            // The origin O(0,0) for the drawingContext is the origin of the AdornedElement.
            // The axes of the drawingContext is parrallel to the adorned element -- even when rotated.
            // Hence, this will draw the grayed out WindowRect parallel to the rotated adorned element.
            //        drawingContext.DrawRectangle(screenBrush_, null, WindowRect());

            drawingContext.DrawRectangle(screenBrush_, null, WindowRect());

            base.OnRender(drawingContext);
        }


        private Rect WindowRect()
        {
            // The AdornedElement is the AppointmentDataGrid.
            if (AdornedElement is null)
                throw new ArgumentException("cannot adorn a null control");

            // Coordinates are relative to the AdornedElement (i.e., the AppointmentDataGrid).
            return new Rect(new Point(0, 0), AdornedElement.DesiredSize);
        }

        #endregion // Measure/Arrange

        #region [Visual Children]
        /// <summary>
        /// Required for the element to be rendered.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Required for the element to be rendered.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException("index");

            return Control;
        }

        #endregion // Visual Children

        #region Logical Children
        /// <summary>
        /// Required for the displayed element to inherit property values
        /// from the logical tree, such as FontSize.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (_logicalChildren == null)
                {
                    _logicalChildren = new ArrayList();
                    _logicalChildren.Add(Control);
                }

                return _logicalChildren.GetEnumerator();
            }
        }

        #endregion // Logical Children
    }
}

