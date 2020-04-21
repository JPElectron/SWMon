Imports System.Net
Imports System.Threading
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.ServiceProcess
Imports System.Xml


Public Class SWMon

    Public Const SettingsFilePath As String = "swmondnsr.ini"
    Public Const DefaultPort As Integer = 80
    Public Const DefaultPollInterval As Integer = 3
    Public Const DefaultRetryFaultActions = 1

    Public Property IP As IPAddress
    Public Property Port As Integer = DefaultPort
    Public Property Password As String
    Public Property Interval As Integer = DefaultPollInterval
    'Commands
    Public Property RelayOn As String
    Public Property RelayOff As String
    Public Property Retry As Integer
    Public Property Fault As String
    'Service command and option
    Public Property PreDelay As Integer
    Public Property Service As String
    Public Property PostDelay As Integer

    Public Property Log As String


    Private _timer As Timer
    Private _pollSync As New Object

    Private _queryStateCommandBytes As Byte()

    Private _lastRelayState As String
    Private _FailedConnectionAttempts As Integer = 0

    Private _LogWriter As StreamWriter


    Public Sub New()
        'initialize settings from ini file
        Using settings = IO.File.OpenText(SettingsFilePath)
            While Not settings.Peek = -1
                Dim settingsLine As String = settings.ReadLine
                If Not String.IsNullOrWhiteSpace(settingsLine) AndAlso Not settingsLine.StartsWith(";") Then
                    Dim setting() As String = settingsLine.Split("=".ToCharArray, StringSplitOptions.RemoveEmptyEntries)

                    If setting.Length > 1 Then

                        Select Case setting(0).ToUpperInvariant
                            Case "IP"
                                If Not IPAddress.TryParse(setting(1), _IP) Then
                                    Throw New Exception("IP Address could not be parsed")
                                End If
                            Case "PORT"
                                If Not Integer.TryParse(setting(1), _Port) Then
                                    _Port = DefaultPort
                                End If
                            Case "PASSWORD"
                                _Password = setting(1)
                            Case "INTERVAL"
                                If Not Integer.TryParse(setting(1), _Interval) Then
                                    _Interval = DefaultPollInterval
                                End If
                            Case "RELAYON"
                                _RelayOn = setting(1)
                            Case "RELAYOFF"
                                _RelayOff = setting(1)
                            Case "RETRY"
                                If Not Integer.TryParse(setting(1), _Retry) Then
                                    _Retry = DefaultRetryFaultActions
                                End If
                            Case "FAULT"
                                _Fault = setting(1)
                            Case "PREDELAY"
                                Integer.TryParse(setting(1), _PreDelay)
                            Case "SERVICE"
                                _Service = setting(1)
                            Case "POSTDELAY"
                                Integer.TryParse(setting(1), _PostDelay)
                            Case "LOG"
                                _Log = setting(1)
                                If Not IO.File.Exists(_Log) Then
                                    _LogWriter = IO.File.CreateText(_Log)
                                Else
                                    _LogWriter = New StreamWriter(_Log, True)
                                End If
                                _LogWriter.AutoFlush = True
                        End Select
                    End If
                End If
            End While
        End Using

        If IP Is Nothing Then
            Throw New Exception("IP Address not specified")
        End If

        If String.IsNullOrWhiteSpace(_Password) OrElse _Password.Equals("webrelay", StringComparison.OrdinalIgnoreCase) Then
            Throw New Exception("Password not defined or invalid")
        End If

        _queryStateCommandBytes = Encoding.ASCII.GetBytes("GET /state.xml?noReply=0 HTTP/1.1" & vbCrLf & "Authorization: Basic " & Convert.ToBase64String(Encoding.ASCII.GetBytes("none:" & _Password)) & vbCrLf & vbCrLf)
    End Sub


    Public Sub Start()

        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledExceptionHandler

        LogEntry(System.Reflection.Assembly.GetCallingAssembly().GetName().Name & " " & System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString() & " started")
        _timer = New Timer(AddressOf Poll, Nothing, 0, _Interval * 1000)
        LogEntry("Checking WebRelay every " & _Interval & " seconds")
    End Sub

    Public Sub [Stop]()
        _timer.Change(Timeout.Infinite, Timeout.Infinite)
        _timer = Nothing
        LogEntry(System.Reflection.Assembly.GetCallingAssembly().GetName().Name & " " & System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString() & " stopped")
    End Sub

    Private Sub Poll(ByVal state As Object)
        'Allow only one thread at a time to poll the device, incase the previous poll took longer than the interval
        SyncLock _pollSync

            Dim response As String
            Using client = New TcpClient

                Try
                    client.Connect(_IP, _Port)
                Catch ex As Exception
                    If Not String.IsNullOrWhiteSpace(_Fault) Then
                        _FailedConnectionAttempts += 1
                        LogEntry("Fault " & _FailedConnectionAttempts.ToString & " of " & _Retry.ToString & " contacting WebRelay at " & _IP.ToString & ":" & _Port.ToString)
                        If _FailedConnectionAttempts = _Retry Then
                            LogEntry("Starting Fault action")
                            RunCommand(_Fault)
                        End If
                        Exit Sub
                    Else
                        LogException(ex)
                        Exit Sub
                    End If
                End Try

                Dim responseBytes(client.ReceiveBufferSize) As Byte
                Using connection = client.GetStream
                    Try
                        connection.Write(_queryStateCommandBytes, 0, _queryStateCommandBytes.Length)
                        'Get the response from webrelay
                        connection.Read(responseBytes, 0, client.ReceiveBufferSize)
                    Catch ex As Exception
                        If Not String.IsNullOrWhiteSpace(_Fault) Then
                            _FailedConnectionAttempts += 1
                            LogEntry("Fault " & _FailedConnectionAttempts.ToString & " of " & _Retry.ToString & " contacting WebRelay at " & _IP.ToString & ":" & _Port.ToString)
                            If _FailedConnectionAttempts = _Retry Then
                                LogEntry("Starting Fault action")
                                RunCommand(_Fault)
                            End If
                            Exit Sub
                        Else
                            LogException(ex)
                            Exit Sub
                        End If
                    End Try
                End Using

                'Parse the response and update the webrelay state and input text boxes
                response = Encoding.ASCII.GetString(responseBytes)
            End Using

            Dim responseDoc As New XmlDocument
            Try
                responseDoc.LoadXml(response)
            Catch ex As Exception
                LogException(ex)
                Exit Sub
            End Try

            Dim readyStateNode As XmlNode = responseDoc.SelectSingleNode("/datavalues/relaystate")
            If readyStateNode IsNot Nothing Then
                Dim relayState As String = If(readyStateNode.InnerText = "1", "ON", "OFF")

                'If it made it this far the connection was successful
                _FailedConnectionAttempts = 0

                If Not relayState.Equals(_lastRelayState, StringComparison.OrdinalIgnoreCase) Then
                    If _lastRelayState IsNot Nothing Then
                        Select Case relayState
                            Case "ON"
                                If Not String.IsNullOrWhiteSpace(_RelayOn) Then
                                    LogEntry("Starting RelayOn action")
                                    RunCommand(_RelayOn)
                                End If
                            Case "OFF"
                                If Not String.IsNullOrWhiteSpace(_RelayOff) Then
                                    LogEntry("Starting RelayOff action")
                                    RunCommand(_RelayOff)
                                End If
                        End Select
                    End If

                    _lastRelayState = relayState
                End If
            End If
        End SyncLock
    End Sub

    Private Sub RunCommand(ByVal command As String)
        Try
            Process.Start(command)
        Catch ex As Exception
            LogException(ex)
        End Try

        If Not String.IsNullOrWhiteSpace(_Service) Then
            RestartService()
        End If
    End Sub

    Private Sub RestartService()
        LogEntry("Stopping service:" & _Service & " in " & PreDelay & " seconds")
        Dim serviceStopTimer As New Timer(AddressOf StopService, Nothing, PreDelay * 1000, Timeout.Infinite)
    End Sub

    Private Sub StopService(ByVal state As Object)

        Dim service As New ServiceController(_Service)

        Try
            service.Stop()
        Catch ex As Exception
            LogException(ex)
            Exit Sub
        End Try

        service.WaitForStatus(ServiceControllerStatus.Stopped)
        LogEntry("Service:" & _Service & " stopped")

        LogEntry("Starting service:" & _Service & " in " & PostDelay & " seconds")
        Dim serviceStartTime As New Timer(AddressOf StartService, Nothing, PostDelay * 1000, Timeout.Infinite)
    End Sub

    Private Sub StartService(ByVal state As Object)

        Dim service As New ServiceController(_Service)

        Try
            Service.Start()
        Catch ex As Exception
            LogException(ex)
            Exit Sub
        End Try

        service.WaitForStatus(ServiceControllerStatus.Running)
        LogEntry("Service:" & _Service & " started")
    End Sub

    Public Sub LogEntry(ByVal entry As String)
        If _LogWriter IsNot Nothing Then
            SyncLock _LogWriter
                _LogWriter.WriteLine("{0} {1}", Now.ToString("MMddyy hh:mm:ss"), entry)
            End SyncLock
        End If
    End Sub

    Public Sub LogException(ByVal ex As Exception)
        If _LogWriter IsNot Nothing Then
            SyncLock _LogWriter
                _LogWriter.WriteLine("{0} {1}", Now.ToString("MMddyy hh:mm:ss"), ex.Message)
            End SyncLock
        End If
    End Sub

    Private Sub UnhandledExceptionHandler(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
        LogException(e.ExceptionObject)
    End Sub

End Class
