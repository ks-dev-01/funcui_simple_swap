namespace FuncuiSimpleSwap
open System
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish
open Avalonia.Input
open Avalonia.Layout
open Avalonia
open Avalonia.FuncUI.Components
open Avalonia.FuncUI.Builder

module PageOne =
    type State = { Text : string }

    type Msg = | OnTextChanged of string
    
    let init : State = { Text = "Abc" }

    let update (msg: Msg) (state: State): State =
        match msg with | OnTextChanged text -> { state with Text = text }

    let view (state: State) (dispatch: Msg -> unit) =
        let otherText = "Another Text View" + state.Text
        StackPanel.create [
            StackPanel.orientation Orientation.Horizontal
            StackPanel.children [
                TextBox.create [ 
                    TextBox.text state.Text 
                    TextBox.onTextChanged (fun text -> dispatch (OnTextChanged text))
                    TextBox.watermark "Enter here"
                    ]
                TextBlock.create [ 
                    TextBlock.text otherText
                    ]         
            ]
        ]

module PageTwo =
    type State = { Text : string }

    type Msg = | OnTextChanged of string
    
    let init : State = { Text = "Dfg" }

    let update (msg: Msg) (state: State): State =
        match msg with | OnTextChanged text -> { state with Text = text }

    let view (state: State) (dispatch: Msg -> unit) =
        let otherText = "Another Text View" + state.Text
        StackPanel.create [
            StackPanel.orientation Orientation.Vertical
            StackPanel.children [
                TextBox.create [ 
                    TextBox.text state.Text 
                    TextBox.onTextChanged (fun text -> dispatch (OnTextChanged text))
                    TextBox.watermark "Enter here"
                    ]
                TextBlock.create [ 
                    TextBlock.text otherText
                    ]         
            ]
        ]

module Shell =
    
    type PageType = 
    | PageOneType
    | PageTwoType

    type State =
        { CurrentPage : PageType 
          PageOneState : PageOne.State
          PageTwoState : PageTwo.State }

    type Msg =
        | ChangePageToggle
        | PageOneMsg of PageOne.Msg
        | PageTwoMsg of PageTwo.Msg
    
    let init : State * Cmd<Msg> =
        { CurrentPage = PageOneType; PageOneState = PageOne.init; PageTwoState = PageTwo.init }, Cmd.none

    let update (msg: Msg) (state: State): State * Cmd<_> =
        match msg with
        | ChangePageToggle ->
            match state.CurrentPage with
            | PageOneType -> {state with CurrentPage = PageTwoType }, Cmd.none
            | PageTwoType -> {state with CurrentPage = PageOneType }, Cmd.none
        | PageOneMsg subMsg ->
            let updatedState = PageOne.update subMsg state.PageOneState
            { state with PageOneState = updatedState }, Cmd.none
        | PageTwoMsg subMsg ->
            let updatedState = PageTwo.update subMsg state.PageTwoState
            { state with PageTwoState = updatedState }, Cmd.none

    let private tabs state otherPageState selectorPageState dispatch : Types.IView list = [
                  
        TabItem.create [
            TabItem.header "PageOther"
            TabItem.content (PageOne.view otherPageState (PageOneMsg >> dispatch) )
        ]
        TabItem.create [
            TabItem.header "PageSelector"
            TabItem.content (PageTwo.view selectorPageState (PageTwoMsg >> dispatch) )
        ]
    ]

    let view (state: State) (dispatch: Msg -> unit) =
            StackPanel.create [
                StackPanel.children [
                    Button.create [
                        Button.content "Swap View"
                        Button.onClick (fun _ -> dispatch ChangePageToggle )
                    ]
                    StackPanel.create [
                        StackPanel.children [
                            match state.CurrentPage with
                            | PageOneType -> PageOne.view state.PageOneState (PageOneMsg >> dispatch)
                            | PageTwoType -> PageTwo.view state.PageTwoState (PageTwoMsg >> dispatch)
                        ]
                    ]
                ]
            ]

    let viewT (state: State) (dispatch: Msg -> unit) =
        StackPanel.create [
            StackPanel.children [
                TabControl.create [
                    
                   TabControl.tabStripPlacement Dock.Left
                   TabControl.viewItems (tabs state state.PageOneState state.PageTwoState dispatch)
                ]
            ]
        ]
          
    type MainWindow() as this =
        inherit HostWindow()
        do
            base.Title <- "FuncuiSimpleSwap A"
            base.Width <- 800.0
            base.Height <- 600.0
            base.MinWidth <- 800.0
            base.MinHeight <- 600.0

            //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
            //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true
            // you can use the following DEBUG helpers to trace elmish updates and to 
            // open Avalonia DevTools
#if DEBUG
            this.AttachDevTools(KeyGesture.Parse("F12"))
#endif
            Elmish.Program.mkProgram (fun () -> init) update view
            |> Program.withHost this
#if DEBUG
            |> Program.withConsoleTrace
#endif
            |> Program.run
