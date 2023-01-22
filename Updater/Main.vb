Public Class Main
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        parameters = Command()
        ReadParameters()
        Init()
    End Sub

    Sub ReadParameters()
        Try
            If parameters.Contains("/ForceUpdate") Then
                forceUpdate = True
            End If
        Catch ex As Exception
            AddToLog("ReadParameters@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
End Class
