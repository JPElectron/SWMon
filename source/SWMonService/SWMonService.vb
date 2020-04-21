Imports System.ServiceProcess
Imports System.Threading
Imports System.IO

Public Class SWMonService

    Private _Worker As New Worker()
    'Used to pass a stop message from the service thread to the worker thread
    Private Shared _Stop As New ManualResetEventSlim(False)

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.

        'Debugger.Launch()

        'The current directory will be system32 for the service so change it to the exe's directory
        Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.Location)

        'We need to start a new thread to do the work on so OnStart can return within 30 seconds (otherwise the service manager will stop the service)
        Dim WorkerThread As System.Threading.Thread
        Dim WorkerStart As System.Threading.ThreadStart
        WorkerStart = AddressOf _Worker.DoWork
        WorkerThread = New System.Threading.Thread(WorkerStart)
        WorkerThread.Start()

    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        _Stop.Set()
    End Sub

    Public Class Worker
        Private _thMain As System.Threading.Thread
        Private _swMon As SWMon.SWMon

        Public Sub DoWork()

            _swMon = New SWMon.SWMon

            _swMon.Start()

            'Make this thread wait until _Stop is signaled. It will then continue execution from this point
            _Stop.Wait()

            _swMon.Stop()
        End Sub


    End Class

End Class
