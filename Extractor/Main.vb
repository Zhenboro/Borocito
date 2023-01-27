Public Class Main
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Hide()
        parameters = Command()
        ReadParameters()
        Init()
    End Sub

    Sub ReadParameters()
        Me.Hide()
        Try
            If My.Application.CommandLineArgs.Count > 0 Then
                For i As Integer = 0 To My.Application.CommandLineArgs.Count - 1
                    Dim parameter As String = My.Application.CommandLineArgs(i)
                    If parameter.ToLower Like "*/override*" Then
                        OverrideOwner = True
                    End If
                Next
            End If
        Catch ex As Exception
            AddToLog("ReadParameters@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
End Class
