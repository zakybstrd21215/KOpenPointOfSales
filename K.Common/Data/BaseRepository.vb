﻿
Imports FluentNHibernate.Automapping
Imports FluentNHibernate.Cfg
Imports K.Common.R2.Identity.Entities
Imports NHibernate.Tool.hbm2ddl
Imports NHibernate.Mapping.ByCode
Imports NHibernate.Cfg.MappingSchema
Imports NHibernate.Cfg
Imports NHibernate
Imports System.Reflection

Namespace Data

    Public MustInherit Class BaseRepository

        Private Shared _configuration As Configuration
        Protected Shared SessionFactory As ISessionFactory
        Private Shared _session As ISession
        Private Shared _statelessSession As IStatelessSession

        Public Shared Function ConfigureNHibernate(assembly As String) As Configuration
            _configuration = New Configuration()
            _configuration.AddAssembly(assembly)
            Dim _mapping = GetMapping()
            _configuration.AddDeserializedMapping(_mapping, assembly)
            SchemaMetadataUpdater.QuoteTableAndColumns(_configuration)
            Return _configuration
        End Function

        Public Shared Function ConfigureFluentNHibernate() As FluentConfiguration
            Dim _fluent_configurate = Fluently.Configure()
            With _fluent_configurate
                .Mappings(Function(mapping) mapping.FluentMappings.AddFromAssemblyOf(Of ConfigBase)())
            End With
            '_fluent_configurate.Mappings(Function(c) c.AutoMappings.Add(AutoMap.AssemblyOf(Of ConfigBase)))
            Return _fluent_configurate
        End Function

        Protected Shared Function GetMapping() As HbmMapping
            Dim _mapper As New ModelMapper()
            Dim _p = Assembly.GetAssembly(GetType(ConfigBase)).GetExportedTypes()
            _mapper.AddMappings(_p)
            Return _mapper.CompileMappingForAllExplicitlyAddedEntities()
        End Function

        Public Sub Initialize(assembly As String)
            _configuration = ConfigureNHibernate(assembly)
            SessionFactory = _configuration.BuildSessionFactory()
        End Sub

        Public Sub Initialize()
            Dim _session_factory = ConfigureFluentNHibernate()
            SessionFactory = _session_factory.BuildSessionFactory()
        End Sub

        Public Shared ReadOnly Property Session() As ISession
            Get
                Return TryCast(IIf(_session Is Nothing, SessionFactory.OpenSession(), _session), ISession)
            End Get
        End Property
        
        Public Shared ReadOnly Property StatelessSession() As IStatelessSession
            Get
                Return TryCast(IIf(_statelessSession Is Nothing, SessionFactory.OpenStatelessSession(), _statelessSession), IStatelessSession)
            End Get
        End Property

        Public Function ExecuteICriteria(Of T)() As IList(Of T)
            Using _transaction = Session.BeginTransaction()
                Try
                    Dim _result = Session.CreateCriteria(GetType(T)).List(Of T)()
                    _transaction.Commit()
                    Return _result
                Catch _ex As Exception
                    _transaction.Rollback()
                    Throw New Exception("Error On ExecuteICreteria", _ex)
                End Try
            End Using
        End Function

        Protected Property TableName() As String
        
    End Class
End Namespace