Imports System.IO
Imports System.Security
Imports System.Security.Permissions
Imports System.Security.Policy
Imports System.Security.AccessControl

<SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain:=True, Infrastructure:=True)>
Public NotInheritable Class Host
    Implements IDisposable
    Protected Property Name As String
    Protected Property Loader As Loader
    Protected Property Domain As AppDomain
    Protected Property Setup As AppDomainSetup
    Protected Property BaseDirectory As String
    Protected Property Container As Container(Of Loader)
    Sub New(Name As String, baseDirectory As String)
        Me.Name = Name
        Me.BaseDirectory = Path.GetFullPath(baseDirectory)
    End Sub
    Public Function Load() As Boolean
        Me.Unload()
        Me.Create(Host.MinimalPermissionSet(Me.BaseDirectory))
        Return Me.Loader.Initialize(Me.BaseDirectory)
    End Function
    Public Function Load(perms As PermissionSet) As Boolean
        Me.Unload()
        Me.Create(perms)
        Return Me.Loader.Initialize(Me.BaseDirectory)
    End Function
    Public Sub Unload()
        If (Me.Domain IsNot Nothing) Then
            Try
                Me.Container.Dispose()
                AppDomain.Unload(Me.Domain)
            Finally
                Me.Loader = Nothing
                Me.Domain = Nothing
            End Try
        End If
    End Sub
    Public Sub Create(perms As PermissionSet)
        Me.Setup = Host.CreateSetup(Me.BaseDirectory)
        Me.Domain = Host.CreateDomain(Me.Name, Me.Setup, perms)
        Me.Loader = Host.CreateLoader(Me.Domain)
        Me.Container = New Container(Of Loader)(Me.Loader)
    End Sub
    Public Function Import(Of T As Class)() As Container(Of T)
        Dim instance As T = Me.Loader.Import(Of T)()
        If instance IsNot Nothing Then Return New Container(Of T)(instance)
        Return Nothing
    End Function
    Public Function ImportAll(Of T As Class)() As IEnumerable(Of Container(Of T))
        Dim collection As New LinkedList(Of Container(Of T))()
        For Each instance As T In Me.Loader.ImportAll(Of T)()
            collection.AddLast(New Container(Of T)(instance))
        Next
        Return collection
    End Function
    Public Shared Function CreateLoader(domain As AppDomain) As Loader
        Return CType(domain.CreateInstanceAndUnwrap(GetType(Loader).Assembly.FullName, GetType(Loader).FullName), Loader)
    End Function
    Public Shared Function CreateDomain(name As String, setup As AppDomainSetup, perms As PermissionSet) As AppDomain
        Return AppDomain.CreateDomain(name, Nothing, setup, perms, {Host.GetName(Of Object)(), Host.GetName(Of Host)()})
    End Function
    Public Shared Function CreateSetup(BaseDirectory As String) As AppDomainSetup
        If (Directory.Exists(BaseDirectory)) Then
            Return New AppDomainSetup() With {.ApplicationBase = Path.GetDirectoryName(GetType(Host).Assembly.Location),
                                              .PrivateBinPath = BaseDirectory, .DisallowBindingRedirects = False,
                                              .CachePath = BaseDirectory, .DisallowApplicationBaseProbing = False, .DisallowCodeDownload = True}
        End If
        Throw New IOException("directory does not exist")
    End Function
    Public Shared Function GetName(Of T)() As StrongName
        Return GetType(T).Assembly.Evidence.GetHostEvidence(Of StrongName)()
    End Function
    Public Shared Function MinimalPermissionSet(BaseDirectory As String) As PermissionSet
        Dim perm As New PermissionSet(PermissionState.None)

        '// Permission setup for managed code and type exporting
        perm.AddPermission(New SecurityPermission(SecurityPermissionFlag.Execution))
        perm.AddPermission(New SecurityPermission(SecurityPermissionFlag.Infrastructure))
        perm.AddPermission(New SecurityPermission(SecurityPermissionFlag.RemotingConfiguration))

        '// Allows for change in console or form settings applications
        perm.AddPermission(New UIPermission(UIPermissionWindow.SafeTopLevelWindows))

        '// Allows for I/O on the filesystem base folder and view ACL
        perm.AddPermission(New FileIOPermission(FileIOPermissionAccess.AllAccess, BaseDirectory))
        perm.AddPermission(New FileIOPermission(FileIOPermissionAccess.AllAccess, AccessControlActions.View, BaseDirectory))

        Return perm
    End Function
#Region "IDisposable Support"
    Protected disposedValue As Boolean
    Protected Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Me.Unload()
            End If
        End If
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class