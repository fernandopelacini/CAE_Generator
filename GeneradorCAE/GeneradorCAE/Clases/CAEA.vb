Public Class CAEA
#Region "Propiedades"
    Public Property IDCaea As Integer
    Public Property CAEA As String
    Public Property Cuit As String
    Public Property FechaProceso As DateTime
    Public Property FechaTopeInforme As DateTime
    Public Property FechaVigenteDesde As DateTime
    Public Property FechaVigenteHasta As DateTime
    Public Property UniqueID As String
    Public Property Activo As Boolean
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
    ''' Lista el CAEA habilitado de la quincena
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ListarCAEAdisponible(ByRef oCaea As CAEA)

        Try
            conexion.Command.Parameters.Clear()
            Dim dsCAEA As DataSet
            dsCAEA = New DataSet

            dsCAEA = conexion.ExecuteDataset(LISTAR_CAEA_DISPONIBLE, _
                                             TABLA_CAEA, _
                                             True, _
                                             Nothing, _
                                             Nothing)


            If dsCAEA.Tables(TABLA_CAEA).Rows.Count > 0 Then
                oCaea.CAEA = dsCAEA.Tables(TABLA_CAEA).Rows(0).Item(0).ToString()
                oCaea.FechaVigenteHasta = dsCAEA.Tables(TABLA_CAEA).Rows(0).Item(1).ToString() 'fecha fin
            Else
                oCaea.CAEA = 0
            End If
            dsCAEA.Dispose()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub


    ''' <summary>
    ''' Inserta un nuevo codigo CAE
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function InsertarCAE() As Boolean
        Try
            Dim oParametros As OleDb.OleDbParameterCollection
            Dim oParameter As OleDb.OleDbParameter

            conexion.Command.Parameters.Clear()
            oParametros = conexion.Command.Parameters

            oParameter = New OleDb.OleDbParameter("@cae", CAEA)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@cuit", Cuit)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@fechaproceso", FechaProceso)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@fechatopeinforme", FechaTopeInforme)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@fechavigentedesde", FechaVigenteDesde)
            oParametros.Add(oParameter)
            oParameter = Nothing

            oParameter = New OleDb.OleDbParameter("@fechavigentehasta", FechaVigenteHasta)
            oParametros.Add(oParameter)
            oParameter = Nothing

            conexion.ExecuteSPNonQuery(INSERTAR_CAEA, oParametros)

            oParametros = Nothing


        Catch ex As Exception
            Throw ex
        End Try
        Return True
    End Function

    ''' <summary>
    ''' Obtiene el CAE desde el webservice de AFIP
    ''' </summary>
    ''' <param name="oAuthentication"></param>
    ''' <remarks></remarks>
    Public Sub GenerarCAEA(ByVal oAuthentication As AuthenticationRequest)
        Try
            Dim objFECAEAGetResponse As wsfe.FECAEAGetResponse
            Dim FEAuthRequest As wsfe.FEAuthRequest
            Dim wsFactura As wsfe.Service
            wsFactura = New wsfe.Service

            Dim orden As Short
            'Dim periodo As Integer

            If Month(oAuthentication.GenerationTime) <= 11 Then
                If Day(oAuthentication.GenerationTime) <= 15 Then
                    orden = 2
                Else
                    orden = 1
                End If
            Else
                If Day(oAuthentication.GenerationTime) <= 15 Then
                    orden = 2 '2da de diciembre
                Else
                    orden = 1
                End If
            End If


            ' orden = oAuthentication.Quincena.ToString()


            FEAuthRequest = New wsfe.FEAuthRequest

            FEAuthRequest = Inicializar(oAuthentication, CUIT_VERIFICADORA_DEL_NORTE)
            wsFactura.Url = "https://servicios1.afip.gov.ar/wsfev1/service.asmx "

            'Dim objStreamWriter As New IO.StreamWriter("C:\Certificados\LALALA.xml")
            'MsgBox(wsFactura.GetType().ToString())
            'Dim x As New Xml.Serialization.XmlSerializer(wsFactura.GetType().ReflectionOnlyGetType("GeneradorCAE.wsfe.Service", True, True))

            'x.Serialize(objStreamWriter, FEAuthRequest)
            'objStreamWriter.Close()


            '''''"http://wswhomo.afip.gov.ar/wsfev1/service.asmx"



            objFECAEAGetResponse = wsFactura.FECAEASolicitar(FEAuthRequest, oAuthentication.Periodo, orden)
 
            If objFECAEAGetResponse.Errors IsNot Nothing Then
                ErrorLog.Create("GeneradorCAE", objFECAEAGetResponse.Errors(0).Code.ToString, objFECAEAGetResponse.Errors(0).Msg)

                objFECAEAGetResponse = wsFactura.FECAEAConsultar(FEAuthRequest, oAuthentication.Periodo, orden)

                If objFECAEAGetResponse.Errors IsNot Nothing Then
                    ErrorLog.Create("GeneradorCAE", objFECAEAGetResponse.Errors(0).Code.ToString, objFECAEAGetResponse.Errors(0).Msg)
                End If
            End If
            'Serialize object to a text file.
            'Dim objStreamWriter As New StreamWriter(Application.StartupPath & "Response.xml")
            'Dim x As New XmlSerializer(objFECAEAGetResponse.GetType)
            'x.Serialize(objStreamWriter, objFECAEAGetResponse)
            'objStreamWriter.Close()
            'MessageBox.Show("Se generó el archivo C:\WSFEV1_objFECAEAGetResponse.xml")
            Dim oCae As CAEA
            oCae = New CAEA

            With objFECAEAGetResponse
                oCae.CAEA = .ResultGet.CAEA
                oCae.Cuit = CUIT_VERIFICADORA_DEL_NORTE
                oCae.FechaProceso = DateTime.ParseExact(.ResultGet.FchProceso, formatoEscritura, System.Globalization.CultureInfo.InvariantCulture)
                oCae.FechaTopeInforme = Date.ParseExact(.ResultGet.FchTopeInf.ToString(), formatoLectura, System.Globalization.CultureInfo.InvariantCulture)
                oCae.FechaVigenteDesde = Date.ParseExact(.ResultGet.FchVigDesde.ToString(), formatoLectura, System.Globalization.CultureInfo.InvariantCulture)
                oCae.FechaVigenteHasta = Date.ParseExact(.ResultGet.FchVigHasta.ToString(), formatoLectura, System.Globalization.CultureInfo.InvariantCulture)
            End With

            oCae.InsertarCAE()

            oCae.Dispose()
            ' End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function Inicializar(ByVal oAuthentication As AuthenticationRequest, ByVal strCuit As String) As wsfe.FEAuthRequest
        Try
            Dim oFactura As wsfe.FEAuthRequest
            oFactura = New wsfe.FEAuthRequest

            oFactura.Token = oAuthentication.Token
            oFactura.Sign = oAuthentication.Sign
            oFactura.Cuit = strCuit
            Return oFactura
        Catch ex As Exception
            Throw ex
        End Try
        Return Nothing
    End Function

#End Region
End Class
