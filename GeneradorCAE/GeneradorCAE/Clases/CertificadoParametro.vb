Public Class CertificadoParametro
#Region "Propiedades"
    Public Property IDservicio As Integer
    Public Property WebServiceURL As String
    Public Property Servicio As String
    Public Property SignerPFX As String
    Public Property Password As String
    Public Property CertificadoP12 As String
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
    ''' Obtiene la configuracion de los datos de un webservice para generar el token y sign
    ''' </summary>
    ''' <param name="strTipoWebService">Codigo del webservice que se va a utilizar. 
    ''' Ej: wsfe para factura electronica.</param>
    ''' <param name="oCertificado">Datos devueltos</param>
    ''' <remarks></remarks>
    Public Sub ObtenerParametrosCertificado(ByVal strTipoWebService As String, ByRef oCertificado As CertificadoParametro)
        Try
            conexion.Command.Parameters.Clear()
            Dim oDatos As DataSet
            oDatos = New DataSet

            oDatos = conexion.ExecuteDataset(TRAER_CERTIFICADO_PARAMETROS, _
                                            TABLA_CERTIFICADO_PARAMETROS, _
                                            "@servicio", _
                                            strTipoWebService, _
                                            True, _
                                            Nothing)
            If oDatos.Tables(TABLA_CERTIFICADO_PARAMETROS).Rows.Count > 0 Then
                With oCertificado
                    .IDservicio = oDatos.Tables(TABLA_CERTIFICADO_PARAMETROS).Rows(0).Item(0)
                    .WebServiceURL = oDatos.Tables(TABLA_CERTIFICADO_PARAMETROS).Rows(0).Item(1)
                    .Servicio = oDatos.Tables(TABLA_CERTIFICADO_PARAMETROS).Rows(0).Item(2)
                    .SignerPFX = oDatos.Tables(TABLA_CERTIFICADO_PARAMETROS).Rows(0).Item(3)
                    .Password = oDatos.Tables(TABLA_CERTIFICADO_PARAMETROS).Rows(0).Item(4)
                    .CertificadoP12 = oDatos.Tables(TABLA_CERTIFICADO_PARAMETROS).Rows(0).Item(5)
                End With
            End If

        Catch ex As Exception
            Throw ex
        End Try

    End Sub

  


#End Region
End Class
