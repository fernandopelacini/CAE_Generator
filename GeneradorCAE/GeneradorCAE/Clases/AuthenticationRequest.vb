Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Xml
Imports System.Net
Imports System.Security
Imports System.Security.Cryptography
Imports System.Security.Cryptography.pkcs
Imports System.Security.Cryptography.X509Certificates
Imports System.IO
Imports System.Runtime.InteropServices

Public Class AuthenticationRequest

#Region "Propiedades"
    Public Property Token As String
    Public Property Sign As String
    Public Property ExpirationTime As DateTime
    Public Property Service As String
    Public Property UniqueID As String
    Public Property Periodo As String
    Public Property Quincena As Char
    Public Property Activo As Boolean
    Public Property GenerationTime As DateTime

    Public XmlLoginTicketRequest As XmlDocument = Nothing
    Public XmlLoginTicketResponse As XmlDocument = Nothing
    Public RutaDelCertificadoFirmante As String
    Public XmlStrLoginTicketRequestTemplate As String = "<loginTicketRequest><header><uniqueId></uniqueId><generationTime></generationTime><expirationTime></expirationTime></header><service></service></loginTicketRequest>"

#End Region

#Region "Metodos"
    ''' <summary>
    ''' Destruye el objeto
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Dispose()
        MyBase.Finalize()
    End Sub


    ''' <summary>
    ''' Obtiene el authentication request, si se encuentra valido
    ''' </summary>
    ''' <param name="strTipoWebService">Codigo del webservice que se va a utilizar. 
    ''' Ej: wsfe para factura electronica.</param>
    ''' <param name="oAuthentication">Datos devueltos</param>
    ''' <remarks></remarks>
    Public Sub ObtenerAuthenticationRequest(ByVal strTipoWebService As String, ByRef oAuthentication As AuthenticationRequest)
        Try
            conexion.Command.Parameters.Clear()
            Dim oDatos As DataSet
            oDatos = New DataSet

            oDatos = conexion.ExecuteDataset(TRAER_AUTHENTICATION_REQUEST, _
                                            TABLA_AUTHENTICATION_REQUEST, _
                                            "@servicio", _
                                            strTipoWebService, _
                                            True, _
                                            Nothing)
            If oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows.Count > 0 Then
                With oAuthentication
                    .Token = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(0)
                    .Sign = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(1)
                    .ExpirationTime = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(2)
                    .Service = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(3)
                    .UniqueID = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(4)
                    .Periodo = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(5)
                    .Quincena = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(6)
                    .Activo = oDatos.Tables(TABLA_AUTHENTICATION_REQUEST).Rows(0).Item(7)
                End With
            Else
                With oAuthentication
                    .Token = 0
                    .Sign = 0
                    .Service = 0
                    .UniqueID = 0
                    .Periodo = 0
                    .Activo = 0
                End With
            End If

        Catch ex As Exception
            Throw ex
        End Try

    End Sub


    ''' <summary>
    ''' Obtiene el token y signature para el Webservice elegido
    ''' </summary>
    ''' <param name="strTipoWebService"></param>
    ''' <param name="oParametros">Parametros para obtener el token</param>
    ''' <param name="oAuthentication">Datos devueltos</param>
    ''' <remarks></remarks>
    Public Sub GenerarTokenYsignature(ByVal strTipoWebService As String, ByVal oParametros As CertificadoParametro, ByRef oAuthentication As AuthenticationRequest)
        Try
            Console.WriteLine("***Servicio a acceder: {0}", oParametros.Servicio)
            Console.WriteLine("***URL del WSAA: {0}", oParametros.WebServiceURL)
            Console.WriteLine("***Ruta del certificado: {0}", oParametros.CertificadoP12)
            Console.WriteLine("***Modo verbose: {0}", True)
            Console.WriteLine("**************************************************************************")
            Console.WriteLine("***Accediendo a {0}", oParametros.WebServiceURL)


            Dim xmlNodoUniqueId As XmlNode
            Dim xmlNodoGenerationTime As XmlNode
            Dim xmlNodoExpirationTime As XmlNode
            Dim xmlNodoService As XmlNode

            XmlLoginTicketRequest = New XmlDocument()
            XmlLoginTicketRequest.LoadXml(XmlStrLoginTicketRequestTemplate)

            xmlNodoUniqueId = XmlLoginTicketRequest.SelectSingleNode("//uniqueId")
            xmlNodoGenerationTime = XmlLoginTicketRequest.SelectSingleNode("//generationTime")
            xmlNodoExpirationTime = XmlLoginTicketRequest.SelectSingleNode("//expirationTime")
            xmlNodoService = XmlLoginTicketRequest.SelectSingleNode("//service")

            xmlNodoGenerationTime.InnerText = DateTime.Now.AddMinutes(-10).ToString("s")
            xmlNodoExpirationTime.InnerText = DateTime.Now.AddMinutes(+10).ToString("s")
            xmlNodoUniqueId.InnerText = oAuthentication.UniqueID + 1
            xmlNodoService.InnerText = oParametros.Servicio


            Console.WriteLine("***Leyendo certificado: {0}", oParametros.CertificadoP12)

            Dim certFirmante As X509Certificate2 = CertificadosX509Lib.ObtieneCertificadoDesdeArchivo(oParametros.CertificadoP12, oParametros.Password)

            Console.WriteLine("***Firmando: ")
            Console.WriteLine(XmlLoginTicketRequest.OuterXml)


            ' Convierto el login ticket request a bytes, para firmar
            Dim EncodedMsg As Encoding = Encoding.UTF8
            Dim msgBytes As Byte() = EncodedMsg.GetBytes(XmlLoginTicketRequest.OuterXml)

            ' Firmo el msg y paso a Base64
            Dim cmsFirmadoBase64 As String
            Dim encodedSignedCms As Byte() = CertificadosX509Lib.FirmaBytesMensaje(msgBytes, certFirmante)
            cmsFirmadoBase64 = Convert.ToBase64String(encodedSignedCms)
            Console.WriteLine("***Llamando al WSAA en URL: {0}", oParametros.WebServiceURL)
            Console.WriteLine("***Argumento en el request:")
            Console.WriteLine(cmsFirmadoBase64)


            Dim servicioWsaa As New Wsaa.LoginCMSService
            Dim loginTicketResponse As String

            servicioWsaa.Url = oParametros.WebServiceURL

            loginTicketResponse = servicioWsaa.loginCms(cmsFirmadoBase64)

            ' PASO 4: Analizo el Login Ticket Response recibido del WSAA
            XmlLoginTicketResponse = New XmlDocument()
            XmlLoginTicketResponse.LoadXml(loginTicketResponse)

            oAuthentication.UniqueID = UInt32.Parse(XmlLoginTicketResponse.SelectSingleNode("//uniqueId").InnerText)
            oAuthentication.GenerationTime = DateTime.Parse(XmlLoginTicketResponse.SelectSingleNode("//generationTime").InnerText)
            oAuthentication.ExpirationTime = DateTime.Parse(XmlLoginTicketResponse.SelectSingleNode("//expirationTime").InnerText)
            oAuthentication.Service = oParametros.Servicio 'XmlLoginTicketResponse.SelectSingleNode("//service").InnerText
            oAuthentication.Sign = XmlLoginTicketResponse.SelectSingleNode("//sign").InnerText
            oAuthentication.Token = XmlLoginTicketResponse.SelectSingleNode("//token").InnerText


            oAuthentication.Activo = True
            oAuthentication.Periodo = Year(oAuthentication.GenerationTime)
            If Month(oAuthentication.GenerationTime) < 10 Then
                oAuthentication.Periodo += 0 & Month(oAuthentication.GenerationTime)
            Else
                oAuthentication.Periodo += Month(oAuthentication.GenerationTime)
            End If

            If Day(oAuthentication.GenerationTime) <= 15 Then
                oAuthentication.Quincena = "1"
            Else
                oAuthentication.Quincena = "2"
            End If



        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    ''' <summary>
    ''' Inserta los nuevos datos de authentication Requesst
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function InsertarAuthenticationRequest() As Boolean
        Try
            Dim oParametros As OleDb.OleDbParameterCollection
            Dim oParameter As OleDb.OleDbParameter

            conexion.Command.Parameters.Clear()
            oParametros = conexion.Command.Parameters

            oParameter = New OleDb.OleDbParameter("@token", Token)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@sign", Sign)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@expirationtime", ExpirationTime)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@service", Service)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@uniqueID", UniqueID)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@periodo", Periodo)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@quincena", Quincena)
            oParametros.Add(oParameter)
            oParameter = Nothing

            conexion.ExecuteSPNonQuery(INSERTAR_AUTHENTICATION_REQUEST, oParametros)

            oParametros = Nothing


        Catch ex As Exception
            Throw ex
        End Try
        Return True
    End Function



#End Region
End Class


Class CertificadosX509Lib

    Public Shared VerboseMode As Boolean = False

    ''' <summary>
    ''' Firma mensaje
    ''' </summary>
    ''' <param name="argBytesMsg">Bytes del mensaje</param>
    ''' <param name="argCertFirmante">Certificado usado para firmar</param>
    ''' <returns>Bytes del mensaje firmado</returns>
    ''' <remarks></remarks>
    Public Shared Function FirmaBytesMensaje( _
    ByVal argBytesMsg As Byte(), _
    ByVal argCertFirmante As X509Certificate2 _
    ) As Byte()
        Try
            ' Pongo el mensaje en un objeto ContentInfo (requerido para construir el obj SignedCms)
            Dim infoContenido As New ContentInfo(argBytesMsg)
            Dim cmsFirmado As New SignedCms(infoContenido)

            ' Creo objeto CmsSigner que tiene las caracteristicas del firmante
            Dim cmsFirmante As New CmsSigner(argCertFirmante)
            cmsFirmante.IncludeOption = X509IncludeOption.EndCertOnly

            If VerboseMode Then
                Console.WriteLine("***Firmando bytes del mensaje...")
            End If
            ' Firmo el mensaje PKCS #7
            cmsFirmado.ComputeSignature(cmsFirmante)

            If VerboseMode Then
                Console.WriteLine("***OK mensaje firmado")
            End If

            ' Encodeo el mensaje PKCS #7.
            Return cmsFirmado.Encode()
        Catch excepcionAlFirmar As Exception
            Throw New Exception("***Error al firmar: " & excepcionAlFirmar.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Lee certificado de disco
    ''' </summary>
    ''' <param name="argArchivo">Ruta del certificado a leer.</param>
    ''' <returns>Un objeto certificado X509</returns>
    ''' <remarks></remarks>
    Public Shared Function ObtieneCertificadoDesdeArchivo( _
    ByVal argArchivo As String, _
    ByVal argPassword As String) As X509Certificate2

        Dim objCert As New X509Certificate2
        Try
            'If argPassword.IsReadOnly Then
            objCert.Import(My.Computer.FileSystem.ReadAllBytes(argArchivo), argPassword, X509KeyStorageFlags.PersistKeySet)
            'Else
            'objCert.Import(My.Computer.FileSystem.ReadAllBytes(argArchivo))
            'End If
            Return objCert
        Catch excepcionAlImportarCertificado As Exception
            Throw New Exception(excepcionAlImportarCertificado.Message & " " & excepcionAlImportarCertificado.StackTrace)
            Return Nothing
        End Try
    End Function

End Class
