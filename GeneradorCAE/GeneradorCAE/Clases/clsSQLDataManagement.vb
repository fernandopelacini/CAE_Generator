''Imports System.Data.SqlClient
Imports System.Data.OleDb

Public NotInheritable Class clsSQLDataManagement

#Region "Members"
    Private mServer As String
    Private mDatabaseName As String
    Private mUser As String
    Private mPassword As String
    'Private Shared mSQLConnection As SqlConnection
    Private Shared mSQLConnection As OleDbConnection
    'Private mOLEDBConnection As OleDbConnection
    Private mProvider As Providers
    Private mConnectionString As String
    Private oTransaction As OleDbTransaction 'SqlTransaction
    Private mCommand As OleDbCommand 'SqlCommand
    Private mdaDatos As OleDbDataAdapter  ' SqlDataAdapter
    Private mDataSet As DataSet
#End Region

    Public Enum Providers
        SQL = 1
        Oracle = 2
        Other = 3
    End Enum

#Region "Properties"
    Public Property Server() As String
        Get
            Return mServer
        End Get
        Set(ByVal value As String)
            mServer = value
        End Set
    End Property
    Public Property DatabaseName() As String
        Get
            Return mDatabaseName
        End Get
        Set(ByVal value As String)
            mDatabaseName = value
        End Set
    End Property
    Public Property User() As String
        Get
            Return mUser
        End Get
        Set(ByVal value As String)
            mUser = value
        End Set
    End Property
    Public Property Password() As String
        Get
            Return mPassword
        End Get
        Set(ByVal value As String)
            mPassword = value
        End Set
    End Property
    'Public Property SQLConnection() As SqlConnection
    '    Get
    '        Return mSQLConnection
    '    End Get
    '    Set(ByVal value As SqlConnection)
    '        mSQLConnection = value
    '    End Set
    'End Property
    'Public Property OLEDBConnection() As OleDbConnection
    '    Get
    '        Return mOLEDBConnection
    '    End Get
    '    Set(ByVal value As OleDbConnection)
    '        mOLEDBConnection = value
    '    End Set
    'End Property
    Public Property Provider() As Providers
        Get
            Return mProvider
        End Get
        Set(ByVal value As Providers)
            mProvider = value
        End Set
    End Property
    'Public Property ConnectionString() As String
    '    Get
    '        Return mConnectionString
    '    End Get
    '    Set(ByVal value As String)
    '        mConnectionString = value
    '    End Set
    'End Property
    Public Property Command() As OleDbCommand  ' SqlCommand
        Get
            Return mCommand
        End Get
        Set(ByVal value As OleDbCommand) ' SqlCommand)
            mCommand = value
        End Set
    End Property
#End Region

#Region "Methods"
    ''' <summary>
    ''' Initializes the class, indicating the provider use to connect to database
    ''' </summary>
    ''' <param name="intProvider"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal intProvider As Providers, ByVal strServer As String, ByVal strDatabase As String, ByVal strUser As String, ByVal strPassword As String)
        mProvider = intProvider
        mServer = strServer
        mUser = strUser
        mPassword = strPassword

        mDatabaseName = strDatabase 'My.Settings.DBMaster

        Select Case Provider
            Case Providers.SQL
                'Cadena SQL
                'SQLNCLI
                'SQLOLEDB
                mConnectionString = "Provider=SQLOLEDB;Data Source=" & Server & ";Initial Catalog=" & DatabaseName & " ;User ID=" & User & " ;Password=" & Password & ";Encrypt=False"
                mSQLConnection = New OleDbConnection() 'SqlConnection
                mSQLConnection.ConnectionString = mConnectionString
            Case Providers.Oracle
                'Cadena Oracle
                mConnectionString = "Data Source=TORCL;Integrated Security=SSPI;"
                'TODO
                'Iniciar conexion ORACLE
                'Asignar esa conexion al connectionString
            Case Providers.Other
                'Cadena OLEDB
                mConnectionString = "TODO"
                'TODO
                'Iniciar otras conexiones
                'Asignar esa conexion al connectionString
        End Select

        InitializeCommand()
    End Sub

    ''' <summary>
    ''' Initializes the command
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitializeCommand()
        mCommand = New OleDbCommand ' SqlCommand
        mCommand.CommandType = CommandType.Text
        mCommand.CommandTimeout = 480 ' My.Settings.TimeOut
        mDataSet = New DataSet
    End Sub

    ''' <summary>
    ''' Close the connection to the database
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CloseConnection(Optional ByVal bteConnection As Providers = Providers.SQL) As Boolean
        'Dim sqlerr As OleDbError

        Try
            Select Case bteConnection
                Case Providers.SQL
                    If mSQLConnection.State.Equals(ConnectionState.Open) Then
                        mSQLConnection.Close()
                        Return True
                    End If
                Case Providers.Oracle
                    'TODO
                Case Providers.Other
                    'TODO
            End Select
        Catch
            '      Dim strErrMsg As New System.Text.StringBuilder()
            '      Dim oleErr As OleDbError
            '      For Each oleErr In ex.Errors
            '          strErrMsg.Append(oleErr.NativeError.ToString() & vbNewLine & _
            '          oleErr.Message.ToString & vbNewLine)
            '      Next
            '      MessageBox.Show(strErrMsg.ToString, "Data Error", _
            'MessageBoxButtons.OK, MessageBoxIcon.Error)
            Throw

            'Catch
            '    Throw
            '    'MessageBox.Show(ex.Message.ToString())

        End Try

        Return False
    End Function
    ''' <summary>
    ''' Opens the connection to the database
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function OpenConnection() As Boolean
        If Not mSQLConnection Is Nothing Then
            If mSQLConnection.State.Equals(ConnectionState.Closed) Then
                'Throw New ApplicationException("Database conecction is already opened.")
                Return False
            End If
        End If
        Try
            If mSQLConnection Is Nothing Then
                mSQLConnection.ConnectionString = mConnectionString
                mSQLConnection.Open()
                InitializeCommand()
            End If
        Catch ex As OleDbException
            ErrorLog.Create("GeneradorCAE", ex)
            Console.WriteLine("Database connection could not be opened." & vbCrLf & ex.Source)
            Return False
        Catch ex As ApplicationException
            Console.WriteLine(ex.Message & vbCrLf & ex.Source)
            Return False
        End Try
        Return True
    End Function
    ''' <summary>
    ''' Adds the query to the Command.CommandText property
    ''' </summary>
    ''' <param name="strQuery">Query to be executed</param>
    ''' <remarks></remarks>
    Private Sub PrepareCommand(ByVal strQuery As String)
        mCommand.CommandType = CommandType.Text
        mCommand.CommandText = strQuery
        mCommand.Connection = mSQLConnection
        If mSQLConnection.State.Equals(ConnectionState.Closed) Then mSQLConnection.Open()
    End Sub
    ''' <summary>
    '''Executes a query and returns the results in a datareader
    ''' Used for SELECT queries
    ''' </summary>
    ''' <param name="strQuery">Query to be executed</param>
    ''' <returns>The query resutls.</returns>
    Public Function ExecuteQuery(ByVal strQuery As String) As OleDbDataReader  ' SqlDataReader
        Dim oReader As OleDbDataReader ' SqlDataReader
        Try
            PrepareCommand(strQuery)
            oReader = mCommand.ExecuteReader(CommandBehavior.CloseConnection)
            'If oTransaction Is Nothing Then mSQLConnection.Close()
            'Return Me.Command.ExecuteReader()
            Return oReader
        Catch
            Throw
            ' MsgBox("Error while executing the query: " & vbNewLine & ex.Message, MsgBoxStyle.Critical)
        End Try
        Return Nothing
    End Function
    ''' <summary>
    ''' Executes an scalar
    ''' </summary>
    ''' <param name="strQuery">Query to be executed</param>
    ''' <returns>Scalar result</returns>
    Public Function ExecuteScalar(ByVal strQuery As String) As Integer
        Dim intEscalar As Integer = 0
        Try
            PrepareCommand(strQuery)
            If mCommand.ExecuteScalar() Is DBNull.Value Then
                intEscalar = 0
            Else
                intEscalar = CInt(mCommand.ExecuteScalar().ToString)
            End If

        Catch
            Throw
            'MsgBox("Error while executing the query: " & vbNewLine & ex.Message, MsgBoxStyle.Critical)
        Finally
            If oTransaction Is Nothing Then mSQLConnection.Close()
        End Try

        Return intEscalar
    End Function
    ''' <summary>
    ''' Executes a non query command.
    ''' Used for DELETE, INSERT or UPDATE operations
    ''' </summary>
    Public Sub ExecuteNonQuery(ByVal strQuery As String)
        Try
            PrepareCommand(strQuery)
            mCommand.ExecuteNonQuery()
        Catch
            Throw

        Finally
            If oTransaction Is Nothing Then mSQLConnection.Close()
        End Try

    End Sub

    ''' <summary>
    ''' Test connection to SQL Server
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function TestConnection() As Boolean
        Try
            If mSQLConnection.State.Equals(ConnectionState.Closed) Then
                mSQLConnection.Open()
                mSQLConnection.Close()
            End If

        Catch ex As OleDbException ' SqlException   'Exception
            Console.WriteLine("Test connection failed - ")
            Return False
        End Try
        Return True
    End Function

    ''' <summary>
    ''' Begins a transaction
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub BeginTransaction()
        If mSQLConnection.State.Equals(ConnectionState.Closed) Then mSQLConnection.Open()
        Try
            oTransaction = mSQLConnection.BeginTransaction()
            mCommand.Transaction = oTransaction
        Catch  'SqlClient.SqlException
            mCommand.Transaction.Rollback()
            oTransaction.Dispose()
            mSQLConnection.Close()
            Throw
            'MsgBox("Error: " & ex.Message, MsgBoxStyle.Exclamation, "GPM")
        End Try

    End Sub
    ''' <summary>
    ''' Commits a transaction
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CommitTransaction() As Boolean
        Try
            mCommand.Transaction.Commit()
            oTransaction.Dispose()
        Catch  ' SqlException
            Throw
            'MsgBox("Error: " & ex.Message, MsgBoxStyle.Exclamation, "GPM")
            Return False
        Finally
            mSQLConnection.Close()
        End Try

        Return True
    End Function
    ''' <summary>
    ''' Rollbacks a transaction
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RollbackTransaction()
        Try
            mCommand.Transaction.Rollback()
            oTransaction.Dispose()
        Catch
            mSQLConnection.Close()
            Throw
        End Try


    End Sub
    ''' <summary>
    ''' Destroy the object
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Dispose()
        If mSQLConnection.State.Equals(ConnectionState.Open) Then mSQLConnection.Close()
        MyBase.Finalize()
    End Sub
  
    ''' <summary>
    ''' Prepares the basic commands for the dataset
    ''' </summary>
    ''' <param name="strQuery"></param>
    ''' <remarks></remarks>
    Private Sub PrepareDataset(ByVal strQuery As String)

        If Me.mdaDatos Is Nothing Then mdaDatos = New OleDbDataAdapter(strQuery, mSQLConnection) ' SqlDataAdapter(strQuery, mSQLConnection)
        mdaDatos.SelectCommand = Command
        mDataSet.Clear()
    End Sub

    ''' <summary>
    ''' Sets the command to execute stored procedures
    ''' </summary>
    ''' <param name="strQuery">Query to be executed</param>
    ''' <remarks></remarks>
    Private Sub PrepareCommandForSP(ByVal strQuery As String, ByVal oParametros As OleDbParameterCollection)

        mCommand.CommandText = strQuery
        mCommand.Connection = mSQLConnection
        mCommand.CommandType = CommandType.StoredProcedure


        'If Not oParametros Is Nothing Then
        '    Dim i As Integer = 0
        '    For i = 0 To oParametros.Count - 1
        '        Me.Command.Parameters.AddWithValue(oParametros.Item(i).ParameterName, oParametros(i).Value)
        '    Next
        'End If

        If mSQLConnection.State.Equals(ConnectionState.Closed) Then mSQLConnection.Open()
    End Sub

    ''' <summary>
    ''' Sets the command to execute stored procedures
    ''' </summary>
    ''' <param name="strQuery">Nombre del store procedure a ejecutar</param>
    ''' <param name="strParametroNombre">nombre del parametro o variable del SP</param>
    ''' <param name="strParametroValor">Valor que se asigna al parametro del SP </param> 
    ''' <remarks></remarks>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")> Private Sub PrepareCommandForSP(ByVal strQuery As String, ByVal strParametroValor As String, ByVal strParametroNombre As String)
        mCommand.CommandText = strQuery
        mCommand.Connection = mSQLConnection
        mCommand.CommandType = CommandType.StoredProcedure
        mCommand.Parameters.AddWithValue(strParametroNombre, strParametroValor)
        If mSQLConnection.State.Equals(ConnectionState.Closed) Then mSQLConnection.Open()
    End Sub

    ''' <summary>
    ''' Sets the command to execute stored procedures
    ''' </summary>
    ''' <param name="strQuery">Query to be executed</param>
    ''' <param name="strParametroNombre">Paremetro unico</param>
    ''' <param name="intParametroValor"></param>
    ''' <remarks></remarks>
    Private Sub PrepareCommandForSP(ByVal strQuery As String, ByVal intParametroValor As Integer, ByVal strParametroNombre As String)
        mCommand.CommandText = strQuery
        mCommand.Connection = mSQLConnection
        mCommand.CommandType = CommandType.StoredProcedure
        mCommand.Parameters.AddWithValue(strParametroNombre, intParametroValor)
        If mSQLConnection.State.Equals(ConnectionState.Closed) Then mSQLConnection.Open()
    End Sub
    ''' <summary>
    ''' Executes a query a returns the results in a dataset.
    ''' </summary>
    ''' <param name="strQuery">Query to be executed or SP name</param>
    ''' <param name="strTableName">Table name, this will be used to fill the dataset
    ''' through the adapter and using the table name</param>
    ''' <param name="blnStoredProcedure">Indica si se va a ejecutar un stored procedure o una query normal</param>
    ''' <param name="oParametros">Colleccion de parametros del SP</param>
    ''' <returns>Dataset</returns>
    ''' <remarks></remarks>
    Public Function ExecuteDataset(ByVal strQuery As String, _
                                   ByVal strTableName As String, _
                                   Optional ByVal blnStoredProcedure As Boolean = False, _
                                   Optional ByVal oParametros As OleDbParameterCollection = Nothing, _
                                   Optional ByVal oDataset As dsDatos = Nothing) As DataSet
        Try
            If blnStoredProcedure Then
                PrepareCommandForSP(strQuery, oParametros)
            Else
                PrepareCommand(strQuery)
            End If
            PrepareDataset(strQuery)
            If oDataset Is Nothing Then

                Me.mdaDatos.Fill(mDataSet, strTableName)
                mCommand.Parameters.Clear()
                Return mDataSet
            Else
                Me.mdaDatos.Fill(oDataset, strTableName)
                mCommand.Parameters.Clear()
                Return oDataset
            End If

        Catch ox As OleDbException
            Throw
            'MsgBox("Error while executing the query: " & vbNewLine & ox.Message, MsgBoxStyle.Critical)
        Catch
            Throw
            'MsgBox("Error while executing the query: " & vbNewLine & ex.Message, MsgBoxStyle.Critical)
        End Try
        Return Nothing
    End Function
    ''' <summary>
    ''' Executes a query a returns the results in a dataset.
    ''' </summary>
    ''' <param name="strQuery">Query to be executed or SP name</param>
    ''' <param name="strTableName">Table name, this will be used to fill the dataset
    ''' through the adapter and using the table name</param>
    ''' <param name="strParametro">Nombre del parametro en el SP</param>
    ''' <param name="strValor">VAlor a asignar a la variable</param>
    ''' <param name="blnStoredProcedure">Indica si se va a ejecutar un stored procedure o una query normal</param>
    '''  Colleccion de parametros del SP
    ''' <returns>Dataset</returns>
    ''' <remarks></remarks>
    Public Function ExecuteDataset(ByVal strQuery As String, _
                                   ByVal strTableName As String, _
                                   ByVal strParametro As String, _
                                   ByVal strValor As String, _
                                   Optional ByVal blnStoredProcedure As Boolean = False, _
                                   Optional ByVal oDataset As dsDatos = Nothing) As DataSet
        Try

            PrepareCommandForSP(strQuery, strValor, strParametro)

            PrepareDataset(strQuery)

            If oDataset Is Nothing Then
                Dim oDatos As DataSet
                oDatos = New DataSet
                Me.mdaDatos.Fill(oDatos, strTableName)
                mCommand.Parameters.Clear()
                Return oDatos
            Else
                Me.mdaDatos.Fill(oDataset, strTableName)
                mCommand.Parameters.Clear()
                Return oDataset
            End If

           


        Catch ox As OleDbException
            'MsgBox("Error while executing the query: " & vbNewLine & ox.Message, MsgBoxStyle.Critical)
            Throw
        Catch
            Throw 'MsgBox("Error while executing the query: " & vbNewLine & ex.Message, MsgBoxStyle.Critical)
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Ejecuta un SP con un solo parametro
    ''' </summary>
    ''' <param name="strProcedureName">Nombre del procedimiento almacenado a ejecutar</param>
    ''' <remarks></remarks>
    Public Sub ExecuteSPNonQuery(ByVal strProcedureName As String, ByVal strParametroNombre As String, ByVal strValor As String)
        Try
            PrepareCommandForSP(strProcedureName, strValor, strParametroNombre)
            mCommand.ExecuteNonQuery()
        Catch
            Throw

        Finally
            If oTransaction Is Nothing Then mSQLConnection.Close()
        End Try

    End Sub
    ''' <summary>
    ''' Ejecuta un SP con un solo parametro
    ''' </summary>
    ''' <param name="strProcedureName">Nombre del SP a ejecutar</param>
    ''' <param name="strParametroNombre">Nombre del parametro del SP</param>
    ''' <param name="intValor">Valor a asignar a la variable</param>
    ''' <remarks></remarks>
    Public Sub ExecuteSPNonQuery(ByVal strProcedureName As String, ByVal strParametroNombre As String, ByVal intValor As Integer)
        Try
            PrepareCommandForSP(strProcedureName, intValor, strParametroNombre)
            mCommand.ExecuteNonQuery()
        Catch
            Throw

        Finally
            If oTransaction Is Nothing Then mSQLConnection.Close()
        End Try

    End Sub
    ''' <summary>
    ''' Ejecuta un SP con varios parametros
    ''' </summary>
    ''' <param name="strProcedureName">Nombre del procedimiento almacenado a ejecutar</param>
    ''' <remarks></remarks>
    Public Sub ExecuteSPNonQuery(ByVal strProcedureName As String, Optional ByVal oParametros As OleDbParameterCollection = Nothing)
        Try
            PrepareCommandForSP(strProcedureName, oParametros)
            mCommand.ExecuteNonQuery()
        Catch
            Throw

        Finally
            If oTransaction Is Nothing Then mSQLConnection.Close()
        End Try

    End Sub
  

    ''' <summary>
    '''Executes a SP and returns the results in a datareader
    ''' Used for SELECT queries
    ''' </summary>
    ''' <param name="strStoredProcedure">Nombre del stored procedure a ejecutar</param>
    ''' <param name="strParametroNombre">Nombre  del parametro del SP</param>
    ''' <param name="strValor">Valor que se asigna a la varible del SP</param>
    ''' <returns>The query resutls.</returns>
    Public Function ExecuteReaderStoredProcedure(ByVal strStoredProcedure As String, ByVal strParametroNombre As String, ByVal strValor As String) As OleDbDataReader
        Dim oReader As OleDbDataReader
        Try
            PrepareCommandForSP(strStoredProcedure, strValor, strParametroNombre)
            oReader = mCommand.ExecuteReader(CommandBehavior.CloseConnection)
            Return oReader
        Catch
            Throw
        End Try
        Return Nothing
    End Function

    ''' <summary>
    '''Executes a SP and returns the results in a datareader
    ''' Used for SELECT queries
    ''' </summary>
    ''' <param name="strStoredProcedure">Nombre del stored procedure a ejecutar</param>
    ''' <param name="oParametros">Colecion de parametros del SP</param>
    ''' <returns>The query resutls.</returns>
    Public Function ExecuteReaderStoredProcedure(ByVal strStoredProcedure As String, ByVal oParametros As OleDbParameterCollection) As OleDbDataReader
        Dim oReader As OleDbDataReader
        Try
            PrepareCommandForSP(strStoredProcedure, oParametros)
            oReader = mCommand.ExecuteReader(CommandBehavior.CloseConnection)
            Return oReader
        Catch
            Throw
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Ejecuta un Scalar de un SP con un parametro
    ''' </summary>
    ''' <param name="strProcedureName">Nombre del SP a ejecutar</param>
    ''' <param name="strParametroNombre">Nombre del parametro del SP</param>
    ''' <param name="strValor">Valor para ese parametro</param>
    ''' <remarks></remarks>
    Public Function ExecuteSPScalar(ByVal strProcedureName As String, ByVal strParametroNombre As String, ByVal strValor As String) As Integer
        Dim intEscalar As Integer = 0

        Try
            PrepareCommandForSP(strProcedureName, strValor, strParametroNombre)
            If mCommand.ExecuteScalar() Is DBNull.Value Then
                intEscalar = 0
            Else
                intEscalar = CInt(mCommand.ExecuteScalar().ToString)
            End If
        Catch
            Throw

        Finally
            If oTransaction Is Nothing Then mSQLConnection.Close()
        End Try
        Return intEscalar
    End Function

    ''' <summary>
    ''' Ejecuta un Scalar de un SP con mas de un parametro 
    ''' </summary>
    ''' <param name="strProcedureName">Nombre del SP a ejecutar</param>
    ''' <param name="oParametros">Collecion de parametros con sus valores</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExecuteSPScalar(ByVal strProcedureName As String, ByVal oParametros As OleDb.OleDbParameterCollection) As Integer
        Dim intEscalar As Integer = 0

        Try
            PrepareCommandForSP(strProcedureName, oParametros)
            If mCommand.ExecuteScalar() Is DBNull.Value Then
                intEscalar = 0
            Else
                intEscalar = CInt(mCommand.ExecuteScalar().ToString)
            End If
        Catch
            Throw

        Finally
            If oTransaction Is Nothing Then mSQLConnection.Close()
        End Try
        Return intEscalar
    End Function
#End Region
End Class
