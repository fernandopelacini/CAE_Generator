Module Module1

    Sub Main()
        Try
            GenerarConfiguracionInicial()

            'Paso 0 Ver si no hay un CAEA disponible ANTES que NADA
            Dim oCAE As CAEA
            oCAE = New CAEA

            Dim oAuthentication As AuthenticationRequest

            oCAE.ListarCAEAdisponible(oCAE)
            If oCAE.CAEA = 0 Then


                oAuthentication = New AuthenticationRequest

                'paso1:  Obtener token y sign de authentication y fecha vencimiento
                oAuthentication.ObtenerAuthenticationRequest(WEB_SERVICE_FACTURA_ELECTRONICA, oAuthentication)

                If oAuthentication.Token.Length < 2 Then
                    'Paso1.1: obtener Webserice y certificados de authentication
                    Dim oParametros As CertificadoParametro
                    oParametros = New CertificadoParametro
                    oParametros.ObtenerParametrosCertificado(WEB_SERVICE_FACTURA_ELECTRONICA, oParametros)
                    oAuthentication.GenerarTokenYsignature(WEB_SERVICE_FACTURA_ELECTRONICA, oParametros, oAuthentication)
                    oParametros.Dispose()

                    oAuthentication.InsertarAuthenticationRequest()
                End If

                'paso 3: Obtener caea
                oCAE.GenerarCAEA(oAuthentication)
                'paso 4: actualizar BD con nuevos datos

                oAuthentication.Dispose()
            Else
                Dim dteFechaLimiteSolicitudCAE As DateTime
                dteFechaLimiteSolicitudCAE = oCAE.FechaVigenteHasta.AddDays(1)

                If DateDiff(DateInterval.Day, Today, dteFechaLimiteSolicitudCAE) <= 5 Then
                    oAuthentication = New AuthenticationRequest
                    'paso1:  Obtener token y sign de authentication y fecha vencimiento
                    oAuthentication.ObtenerAuthenticationRequest(WEB_SERVICE_FACTURA_ELECTRONICA, oAuthentication)
                    If oAuthentication.Token.Length < 2 Then
                        'Paso1.1: obtener Webserice y certificados de authentication
                        Dim oParametros As CertificadoParametro
                        oParametros = New CertificadoParametro
                        oParametros.ObtenerParametrosCertificado(WEB_SERVICE_FACTURA_ELECTRONICA, oParametros)
                        oAuthentication.GenerarTokenYsignature(WEB_SERVICE_FACTURA_ELECTRONICA, oParametros, oAuthentication)
                        oParametros.Dispose()

                        'Se cambia para la proxima quincena
                        Dim anio As String
                        Dim mes As String
                        Dim quincena As SByte

                        If Month(oAuthentication.GenerationTime) <= 11 Then
                            anio = Year(oAuthentication.GenerationTime).ToString()
                            If Day(oAuthentication.GenerationTime) <= 15 Then
                                mes = Month(oAuthentication.GenerationTime).ToString()
                                quincena = 2
                            Else
                                mes = CStr(Month(oAuthentication.GenerationTime) + 1)
                                quincena = 1
                            End If
                        Else
                            If Day(oAuthentication.GenerationTime) <= 15 Then
                                anio = Year(oAuthentication.GenerationTime).ToString
                                mes = Month(oAuthentication.GenerationTime).ToString()
                                quincena = 2 '2da de diciembre
                            Else
                                anio = CStr(Year(oAuthentication.GenerationTime) + 1)
                                mes = "1" 'Primera de Enero
                                quincena = 1
                            End If
                        End If
                        If mes <= 9 Then mes = "0" & mes
                        oAuthentication.Periodo = anio & mes
                        oAuthentication.Quincena = quincena.ToString()

                        oAuthentication.InsertarAuthenticationRequest()
                    End If

                    'paso 3: Obtener caea
                    oCAE.GenerarCAEA(oAuthentication)
                    'paso 4: actualizar BD con nuevos datos

                    oAuthentication.Dispose()
                End If

                End If
                oCAE.Dispose()

        Catch ex As Exception
            ErrorLog.Create("GeneradorCAE", ex)
            Console.WriteLine(MENSAJE_ERROR & vbCrLf & ex.Message)
        End Try
        

    End Sub

    Private Sub GenerarConfiguracionInicial()
        Try
            Servidor = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Verificadora").GetValue("Servidor")
            BaseDatos = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Verificadora").GetValue("base")
            UserBD = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Verificadora").GetValue("usuario")
            Password = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Verificadora").GetValue("password")
            Password = Desencriptar(Password)
            conexion = New clsSQLDataManagement(clsSQLDataManagement.Providers.SQL, Servidor, BaseDatos, UserBD, Password)
        Catch ex As Exception
            Throw ex
        End Try

    End Sub

End Module
