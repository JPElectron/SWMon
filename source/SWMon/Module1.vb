Module Module1

    Sub Main()

        Console.WriteLine("Initializing SWMon")

        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf HandleUnhandledExceptions
        Dim swMon As New SWMon
        RemoveHandler AppDomain.CurrentDomain.UnhandledException, AddressOf HandleUnhandledExceptions

        swMon.Start()

        Console.WriteLine("SWMon started, press enter to exit")
        Console.ReadLine()

        swMon.Stop()
    End Sub

    Private Sub HandleUnhandledExceptions(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
        Console.WriteLine(e.ExceptionObject)
    End Sub

End Module
