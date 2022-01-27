Public Class Init

    Private Sub Init_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        parameters = Command()
        ReadParameters()
        StartUp.Init()
    End Sub

    Sub ReadParameters()
        Try
            If My.Application.CommandLineArgs.Count > 0 Then
                For i As Integer = 0 To My.Application.CommandLineArgs.Count - 1
                    Dim parameter As String = My.Application.CommandLineArgs(i)
                    'If parameter.ToLower Like "**" Then
                    '
                    'End If
                Next
            End If
        Catch ex As Exception
            AddToLog("ReadParameters@Init", "Error: " & ex.Message, True)
        End Try
    End Sub
End Class