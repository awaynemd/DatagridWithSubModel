module MyDataGrid.DataGrid

open Elmish
open Elmish.WPF
open System

type Visit = 
    {   ServiceTime: DateTime option
        DoNotSee: Boolean option
        ChartNumber: int option
        LastName: string option
        FirstName: string option
        Mi: string option
        BirthDate: DateTime option
        PostingTime: DateTime option 
        AppointmentTime: DateTime option }

module Row = 
    type Cell = 
          {RowNumber: int
           ColumnNumber: int
           AppointmentKeys: Visit list
           ColumnTime: TimeSpan
           AppointmentCount: int
           AppointmentTime: DateTime option  // all lines in the cell have the same appointment time.     
          }

    let SetCell (rowNumber: int, columnNumber: int) =
        let AppointmentsPerCell = 4
        { RowNumber = rowNumber
          ColumnNumber = columnNumber
          AppointmentKeys = [for x in 1 .. AppointmentsPerCell -> 
                             {
                                ServiceTime = Some System.DateTime.Now 
                                DoNotSee = Some false 
                                ChartNumber = Some 8812 
                                LastName= Some ("LastName" + string x)
                                FirstName= Some ("FirstName" + string x)
                                Mi = Some "J" 
                                BirthDate = Some(DateTime(2020,09,14))
                                PostingTime = Some DateTime.Now
                                AppointmentTime = Some DateTime.Now
                             }]      
          ColumnTime = System.TimeSpan.FromMinutes(float(columnNumber * 15))
          AppointmentCount = 4
          AppointmentTime = Some(DateTime.Now)
        }

    type Model =
        { RowTime: string
          Columns: Cell list}

    type Msg = unit

    let bindings () : Binding<Model, Msg> list = [
        "Columns" |> Binding.oneWay( fun m -> m.Columns)
        "RowTime" |> Binding.oneWay( fun m -> m.RowTime)
    ]


type Model =
  { AppointmentDate: DateTime
    Rows:  Row.Model  list                             //Row list
    SelectedRow: Row.Model option}

type Msg =
  | SetAppointmentDate of DateTime
  | SetSelectedRow of Row.Model option
  | RowsMsg of Row.Msg

let SetRow (rowNumber: int, startTime: System.TimeSpan)= 
    let columnCount = 4
    let hr = System.TimeSpan.FromHours(1.0)
    let rowTime = startTime + System.TimeSpan.FromTicks(hr.Ticks * int64(rowNumber))
    { Row.RowTime = rowTime.ToString("h':00'")
      Row.Columns = [for columnNumber in 1 .. columnCount -> Row.SetCell(rowNumber, columnNumber) ]
    }

let init =
      let rowCount = 9
      let startTime = TimeSpan.FromHours(float(8))
      { AppointmentDate = DateTime.Now 
        Rows = [for rowNumber in 0 .. rowCount -> SetRow(rowNumber, startTime)]
        SelectedRow = None
      }

let update msg m =
  match msg with
  | SetAppointmentDate d -> {m with AppointmentDate = d}
  | SetSelectedRow r -> {m with SelectedRow = r}
  | _ -> m

let bindings () : Binding<Model, Msg> list = [
  "SelectedAppointmentDate" |> Binding.twoWay( (fun m -> m.AppointmentDate), SetAppointmentDate)
  "Rows" |> Binding.subModel( (fun m -> m.Rows), snd, RowsMsg, Row.bindings)
  "SelectedRow" |> Binding.twoWay( (fun m -> m.SelectedRow), SetSelectedRow)
]

let designVm = ViewModel.designInstance init (bindings ())


let main window =
  Program.mkSimpleWpf (fun () -> init) update bindings
  |> Program.withConsoleTrace
  |> Program.runWindowWithConfig
    { ElmConfig.Default with LogConsole = true; Measure = true }
    window
