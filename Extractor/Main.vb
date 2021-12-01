Public Class Main
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddToLog("Main", "Extractor iniciado!", True)
        parameters = Command()
        'ReadParameters()
        Init()
    End Sub

    Sub ReadParameters()
        Try



        Catch ex As Exception
            AddToLog("ReadParameters@Main", "Error: " & ex.Message, True)
        End Try
    End Sub
End Class
