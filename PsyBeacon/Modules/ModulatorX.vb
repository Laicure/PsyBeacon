Module ModulatorX
#Region "Global Declarations"
    Friend sqlCon As String = "Integrated Security=False; Data Source=; Initial Catalog=; User ID=; Password="
    Friend Uzer As String = Environment.UserDomainName & "\" & Environment.UserName
    Friend Namm As String = ""
    Friend MasterHazher As New HashSet(Of String)

    Friend ProcessWhiteList() As String = {""}
#End Region

#Region "Get Window Title; stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c"
    <Runtime.InteropServices.DllImport("user32.dll", SetLastError:=True)> Private Function GetForegroundWindow() As IntPtr
    End Function

    <Runtime.InteropServices.DllImport("user32.dll", EntryPoint:="GetWindowText")> Private Function GetWindowText(hWnd As IntPtr, text As System.Text.StringBuilder, count As Integer) As Integer
    End Function

    Friend Function GetActiveWindowTitle() As String
        Const nChars As Integer = 256
        Dim Buff As New System.Text.StringBuilder(nChars)
        Dim handle As IntPtr = GetForegroundWindow()

        If GetWindowText(handle, Buff, nChars) > 0 Then
            Return Buff.ToString()
        Else
            Return Nothing
        End If
    End Function
#End Region

#Region "Get Window Process Name; stackoverflow.com/questions/16680356/get-the-process-name-of-the-window-that-is-currently-active-and-in-focus-using-v"
    <Runtime.InteropServices.DllImport("user32.dll", SetLastError:=True)> Private Function GetWindowThreadProcessId(ByVal hwnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
    End Function

    Friend Function GetActiveWindowProcessName() As String
        Dim handle As IntPtr = GetForegroundWindow()
        Dim ProcID As Integer = Nothing
        GetWindowThreadProcessId(handle, ProcID)

        Return Process.GetProcessById(ProcID).ProcessName
    End Function
#End Region

#Region "Last Input; http://stackoverflow.com/questions/22878502/detecting-mouse-moves-and-key-strokes-in-vb-net"
    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)> Structure LASTINPUTINFO
        <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.U4)> Friend cbSize As Integer
        <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.U4)> Friend dwTime As Integer
    End Structure

    <Runtime.InteropServices.DllImport("user32.dll")> Private Function GetLastInputInfo(ByRef plii As LASTINPUTINFO) As Boolean
    End Function

    Dim idletime As Long
    Dim lastInputInf As New LASTINPUTINFO()
    Friend Function GetLastInputTime() As Long
        idletime = 0
        lastInputInf.cbSize = Runtime.InteropServices.Marshal.SizeOf(lastInputInf)
        lastInputInf.dwTime = 0

        If GetLastInputInfo(lastInputInf) Then
            idletime = Environment.TickCount - lastInputInf.dwTime
        End If

        If idletime > 0 Then
            Return idletime
        Else : Return 0
        End If
    End Function

#End Region

#Region "http://www.pinvoke.net/default.aspx/user32.lockworkstation"
    <Runtime.InteropServices.DllImport("user32.dll", SetLastError:=True)> Friend Function LockWorkStation() As Boolean
    End Function
#End Region

End Module