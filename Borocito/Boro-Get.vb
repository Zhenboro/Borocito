Imports System.IO.Compression
Imports Microsoft.Win32
Namespace Boro_Get

    Module Manager
        Private DIRBoroGetInstallFolder As String = DIRCommons & "\boro-get"
        Private zipPackageFile As String = DIRBoroGetInstallFolder & "\boro-get.zip"

        Function BORO_GET_ADMIN(ByVal command As String) As String
            Try
                AddToLog("BOROGET", "Processing: " & command, False)
                Dim boroGETcommand As String = command.Replace("boro-get", Nothing)
                boroGETcommand = boroGETcommand.TrimStart()
                boroGETcommand = boroGETcommand.TrimEnd()
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
                ElseIf boroGETcommand = "reset" Then
                    If My.Computer.FileSystem.DirectoryExists(DIRBoroGetInstallFolder) Then
                        My.Computer.FileSystem.DeleteDirectory(DIRBoroGetInstallFolder, FileIO.DeleteDirectoryOption.DeleteAllContents)
                    End If
                    Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
                    regKey.DeleteSubKeyTree("boro-get")
                    Return "Local repository has been cleared!"
                Else
                    If isBoroGetInstalled() Then
                        'paquete
                        Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                        Process.Start(regKey.GetValue("boro-get"), boroGETcommand)
                        Return "Processing (" & boroGETcommand & ")"
                    Else
                        Return "boro-get is not installed."
                    End If
                End If
            Catch ex As Exception
                AddToLog("BORO_GET_ADMIN@BOROGET", "Error: " & ex.Message, True)
                Return "Error processing 'boro-get' command."
            End Try
        End Function
        Function isBoroGetInstalled()
            Try
                Dim check_boroget As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                If check_boroget Is Nothing Then
                    Return False
                Else
                    If check_boroget.GetValue("boro-get") = Nothing Then
                        Return False
                    Else
                        Return True
                    End If
                End If
            Catch ex As Exception
                AddToLog("isBoroGetInstalled@BOROGET", "Error: " & ex.Message, True)
                Return False
            End Try
        End Function
        Function InstallBOROGET() As String
            Try
                AddToLog("BOROGET", "Installing boro-get...", False)
                If Not My.Computer.FileSystem.DirectoryExists(DIRBoroGetInstallFolder) Then
                    My.Computer.FileSystem.CreateDirectory(DIRBoroGetInstallFolder)
                End If
                If My.Computer.FileSystem.FileExists(zipPackageFile) Then
                    My.Computer.FileSystem.DeleteFile(zipPackageFile)
                End If
                'Descargar desde el servidor
                AddToLog("BOROGET", "Downloading boro-get...", False)
                My.Computer.Network.DownloadFile(GetIniValue("Components", "boro-get", DIRCommons & "\General.ini"), zipPackageFile)
                'Instalar
                AddToLog("BOROGET", "Extracting boro-get...", False)
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
                AddToLog("Install@BOROGET", "Error: " & ex.Message, True)
                Return "Error installing boro-get."
            End Try
        End Function
        Function UninstallBOROGET() As String
            Try
                AddToLog("BOROGET", "Uninstalling boro-get...", False)
                'Eliminar directorio
                If My.Computer.FileSystem.DirectoryExists(DIRBoroGetInstallFolder) Then
                    My.Computer.FileSystem.DeleteDirectory(DIRBoroGetInstallFolder, FileIO.DeleteDirectoryOption.DeleteAllContents)
                End If
                'Eliminar registros
                Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
                regKey.DeleteSubKeyTree("boro-get")
                Return "boro-get has been uninstalled!"
            Catch ex As Exception
                AddToLog("Uninstall@BOROGET", "Error: " & ex.Message, True)
                Return "Error uninstalling boro-get."
            End Try
        End Function
        Function SetBOROGET(ByVal regKey As String, ByVal regValue As String) As String
            Try
                AddToLog("BOROGET", "Setting boro-get...", False)
                Dim RegeditBoroGet As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                If RegeditBoroGet Is Nothing Then
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito\\boro-get")
                    RegeditBoroGet = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                End If
                RegeditBoroGet.SetValue(regKey, regValue)
                Return "Key: " & regKey & " Value: " & regValue & " has been setted!"
            Catch ex As Exception
                AddToLog("Set@BOROGET", "Error: " & ex.Message, True)
                Return "Error setting registry."
            End Try
        End Function
    End Module

End Namespace
