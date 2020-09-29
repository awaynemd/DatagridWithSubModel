module MyDataGrid.DataGrid

open Elmish
open Elmish.WPF
open System

module Visit = 
    
    type Model =
       { ServiceTime: DateTime option
         DoNotSee: Boolean option
         ChartNumber: int option
         LastName: string option
         FirstName: string option
         Mi: string option
         BirthDate: DateTime option
         PostingTime: DateTime option 
         AppointmentTime: DateTime option 
         Id: int}

    let SetVisits appointmentsPerCell  = [for x in 1 .. appointmentsPerCell -> 
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
                                                  Id = x
                                              }]   

    type Msg = unit
    
    let bindings() = [
        "FirstName" |> Binding.oneWayOpt( fun (_,v) -> v.FirstName)
        "LastName"  |> Binding.oneWayOpt( fun (_,v) -> v.LastName)
        "BirthDate" |> Binding.oneWayOpt( fun (_,v) -> v.BirthDate) 
        "ServiceTime" |> Binding.oneWayOpt( fun (_,v) -> v.ServiceTime)
    ]

module Cell =

    type Model =
        { RowNumber: int
          ColumnNumber: int
          AppointmentKeys: Visit.Model list
          ColumnTime: TimeSpan
          AppointmentCount: int
          AppointmentTime: DateTime option  // all lines in the cell have the same appointment time.  
          Id: int
          SelectedAppointmentKey: int option
        }

    let SetCell (rowNumber: int, columnNumber: int) =
        let AppointmentsPerCell = 4
        {RowNumber = rowNumber
         ColumnNumber = columnNumber
         AppointmentKeys =  Visit.SetVisits AppointmentsPerCell  
         ColumnTime = System.TimeSpan.FromMinutes(float(columnNumber * 15))
         AppointmentCount = 4
         AppointmentTime = Some(DateTime.Now)
         Id=rowNumber*10 + columnNumber
         SelectedAppointmentKey = None
         }

    type Msg =
        | SetAppointmentKey  of int option

    
    let update msg m =
        match msg with
        | SetAppointmentKey keyId -> {m with SelectedAppointmentKey = keyId}


    let bindings() =[
        "AppointmentKeys"  |> Binding.subModelSeq(
                                (fun (m,_) -> m.AppointmentKeys),
                                (fun v -> v.Id),
                                 Visit.bindings )

        "SelectedAppointmentKey" |> Binding.subModelSelectedItem("AppointmentKeys", (fun (m,_) -> m.SelectedAppointmentKey), SetAppointmentKey)
    ]

module Row =

    type Model =
      { RowTime: string
        Columns: Cell.Model list 
        Id: int }

    let SetRow (rowNumber: int, startTime: System.TimeSpan)= 
        let columnCount = 4
        let hr = System.TimeSpan.FromHours(1.0)
        let rowTime = startTime + System.TimeSpan.FromTicks(hr.Ticks * int64(rowNumber))
        { RowTime = rowTime.ToString("h':00'")
          Columns = [for columnNumber in 1 .. columnCount -> Cell.SetCell(rowNumber, columnNumber) ]
          Id = rowNumber
        }

    
    let bindings () = [
        "RowTime" |> Binding.oneWay( fun (m,r) -> r.RowTime)
        "Columns" |> Binding.subModelSeq(
                                            (fun (_,r) -> r.Columns),
                                            (fun c -> c.Id),
                                             Cell.bindings                            
                                          )
    
    ] 
    

type Model =
  { AppointmentDate: DateTime
    Rows: Row.Model list
    SelectedRow: int option}

type Msg =
  | SetAppointmentDate of DateTime
  | SetSelectedRow of int option

let init () =
    let rowCount = 9
    let startTime = TimeSpan.FromHours(float(8))
    { AppointmentDate = DateTime.Now 
      Rows = [for rowNumber in 0 .. rowCount -> Row.SetRow(rowNumber, startTime)]
      SelectedRow = None
    }

let update msg m =
  match msg with
  | SetAppointmentDate d -> {m with AppointmentDate = d}
  | SetSelectedRow rowId -> {m with SelectedRow = rowId}

let bindings () : Binding<Model, Msg> list = [
  "SelectedAppointmentDate" |> Binding.twoWay( (fun m -> m.AppointmentDate), SetAppointmentDate)

  "Rows" |> Binding.subModelSeq(
                                 (fun m -> m.Rows),
                                 (fun r -> r.Id),
                                  Row.bindings
                               )                          
                    
  "SelectedRow" |> Binding.subModelSelectedItem("Rows", (fun (m,_) -> m.SelectedRow), SetSelectedRow)
]

let main window =
  Program.mkSimpleWpf init update bindings
  |> Program.withConsoleTrace
  |> Program.runWindowWithConfig
    { ElmConfig.Default with LogConsole = true; Measure = true }
    window
