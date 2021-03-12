'Imports System.Managemen
Imports System.Security.Cryptography
Imports System.Text

Module Funciones
  
    Public BaseDatos As String
    Public Servidor As String
    Public Password As String
    Public UserBD As String
    Public SeparadorDecimal As Char
    Public formatoLectura As String = "yyyyMMdd" ' Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
    Public formatoEscritura As String = "yyyyMMddHHmmss" '"yyyyMMdd"
    Public conexion As clsSQLDataManagement


    Private strClave As String = "%ü&/@#$A"
    Private des As New TripleDESCryptoServiceProvider 'Algorithmo TripleDES
    Private hashmd5 As New MD5CryptoServiceProvider 'objeto md5


    Public Function Encriptar(ByVal strPass As String) As String

        Dim i As Integer
        Dim strPass2 As String
        Dim strCAR As String, strCodigo As String

        strPass2 = ""

        For i = 1 To Len(strPass)
            strCAR = Mid(strPass, i, 1)
            strCodigo = Mid(strClave, ((i - 1) Mod Len(strClave)) + 1, 1)
            strPass2 = strPass2 & Right("0" & Hex(Asc(strCodigo) Xor Asc(strCAR)), 2)
        Next i
        Encriptar = strPass2
    End Function

    Public Function Desencriptar(ByVal strPass As String) As String

        Dim i As Integer
        Dim strPass2 As String
        Dim strCAR As String, strCodigo As String
        Dim j As Integer

        strPass2 = ""
        j = 1
        For i = 1 To Len(strPass) Step 2
            strCAR = Mid(strPass, i, 2)
            strCodigo = Mid(strClave, ((j - 1) Mod Len(strClave)) + 1, 1)
            strPass2 = strPass2 & Chr(Asc(strCodigo) Xor Val("&h" + strCAR))
            j = j + 1
        Next i
        Desencriptar = strPass2
    End Function

    Public Function DecimalSeparator() As Char
        DecimalSeparator = Mid$(1 / 2, 2, 1)
    End Function

    ''' <summary>
    ''' Obtiene la fecha del servidor de base de datos.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetServerDate() As Date
        Dim oReader As OleDb.OleDbDataReader
        Dim dteFecha As Date

        Try
            oReader = conexion.ExecuteQuery("SELECT GETDATE()")

            oReader.Read()
            dteFecha = Format(CDate(oReader(0)), formatoLectura)
            oReader.Close()

        Catch ex As Exception
            ErrorLog.Create("GeneradorCAE", ex)
            dteFecha = Today

        End Try
        oReader = Nothing
        Return dteFecha
    End Function


    ''' <summary>
    ''' Encripta los datos para generar la licencia utilizando MD5
    ''' </summary>
    ''' <param name="strPass"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function EncriptarMD5(ByVal strPass As String) As String
        Dim strEncriptada As String
        des.Key = hashmd5.ComputeHash((New UnicodeEncoding).GetBytes(strClave))
        des.Mode = CipherMode.ECB
        Dim encrypt As ICryptoTransform = des.CreateEncryptor()
        Dim buff() As Byte = UnicodeEncoding.ASCII.GetBytes(strPass)
        strEncriptada = Convert.ToBase64String(encrypt.TransformFinalBlock(buff, 0, buff.Length))
        Return strEncriptada
    End Function
    Public Function DesencriptarMD5(ByVal strPass As String) As String
        Dim strDesencriptada As String

        des.Key = hashmd5.ComputeHash((New UnicodeEncoding).GetBytes(strClave))
        des.Mode = CipherMode.ECB
        Dim desencrypta As ICryptoTransform = des.CreateDecryptor()
        Dim buff() As Byte = Convert.FromBase64String(strPass)
        strDesencriptada = UnicodeEncoding.ASCII.GetString(desencrypta.TransformFinalBlock(buff, 0, buff.Length))
        Return strDesencriptada
    End Function


    ''' <summary>
    ''' Funcion que valida cuit
    ''' </summary>
    ''' <param name="strNroCuit">cuit a verificar</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ValidarCUIT(ByVal strNroCuit As String) As Boolean
        Dim mk_suma As Integer
        Dim mk_valido As String

        If IsNumeric(strNroCuit) Then
            If strNroCuit.Length <> 11 Then
                mk_valido = False
            Else
                mk_suma = 0
                mk_suma += CInt(strNroCuit.Substring(0, 1)) * 5
                mk_suma += CInt(strNroCuit.Substring(1, 1)) * 4
                mk_suma += CInt(strNroCuit.Substring(2, 1)) * 3
                mk_suma += CInt(strNroCuit.Substring(3, 1)) * 2
                mk_suma += CInt(strNroCuit.Substring(4, 1)) * 7
                mk_suma += CInt(strNroCuit.Substring(5, 1)) * 6
                mk_suma += CInt(strNroCuit.Substring(6, 1)) * 5
                mk_suma += CInt(strNroCuit.Substring(7, 1)) * 4
                mk_suma += CInt(strNroCuit.Substring(8, 1)) * 3
                mk_suma += CInt(strNroCuit.Substring(9, 1)) * 2
                mk_suma += CInt(strNroCuit.Substring(10, 1)) * 1
      
                If Math.Round(mk_suma / 11, 0) = (mk_suma / 11) Then
                    mk_valido = True
                Else
                    mk_valido = False
                End If
            End If
        Else
            mk_valido = False
        End If
        Return (mk_valido)
    End Function




End Module
