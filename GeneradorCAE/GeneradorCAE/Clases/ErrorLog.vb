Imports System.Text

Public Class ErrorLog

#Region "Propiedades"
    Public Shared Property IDerror As Integer
    Public Shared Property IDusuario As String
    Public Shared Property StackTrace As String
    Public Shared Property Type As String
    Public Shared Property Descripcion As String
    Public Shared Property FechaError As DateTime
#End Region

#Region "Metodos"

    Public Shared Sub Create(ByVal User As String, ByVal ex As Exception)
        Try

            Dim separador As String = "','"
            Dim query As New StringBuilder

            conexion.Command.Parameters.Clear()

            query.Append("INSERT INTO tblErrorLog values ('")
            query.Append(User)
            query.Append(separador)
            query.Append(Replace(ex.StackTrace, "'", "''"))
            query.Append(separador)
            query.Append(ex.GetType().Name)
            query.Append(separador)
            query.Append(Replace(ex.Message, "'", "''"))
            query.Append("',")
            query.Append("GetDate()")
            query.Append(")")
            conexion.ExecuteNonQuery(query.ToString())
            query = Nothing
        Catch ex1 As Exception
            Dim archivoError As String = "C:\archivoError.txt"
            Dim sw As IO.StreamWriter

            If Not IO.File.Exists(archivoError) Then IO.File.CreateText(archivoError)

            sw = New IO.StreamWriter(archivoError, True, System.Text.Encoding.Default)

            sw.WriteLine(ex1.Message)

            sw.Close()
        End Try
    End Sub


    Public Shared Sub Create(ByVal User As String, ByVal code As String, mensaje As String)
        Try

            Dim separador As String = "','"
            Dim query As New StringBuilder

            conexion.Command.Parameters.Clear()

            query.Append("INSERT INTO tblErrorLog values ('")
            query.Append(User)
            query.Append(separador)
            query.Append(code)
            query.Append(separador)
            query.Append("wsfe")
            query.Append(separador)
            query.Append(Replace(mensaje, "'", "''"))
            query.Append("',GetDate()")
            query.Append(")")
            conexion.ExecuteNonQuery(query.ToString())
            query = Nothing
        Catch ex1 As Exception
            Dim archivoError As String = "C:\archivoError.txt"
            Dim sw As IO.StreamWriter

            If Not IO.File.Exists(archivoError) Then IO.File.CreateText(archivoError)

            sw = New IO.StreamWriter(archivoError, True, System.Text.Encoding.Default)

            sw.WriteLine(ex1.Message)

            sw.Close()
        End Try
    End Sub

    Public Sub Dispose()
        MyBase.Finalize()
    End Sub
#End Region
End Class
