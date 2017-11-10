Imports System.IO
Imports System.Security
Imports System.Security.Principal
Imports System.Security.Permissions
Imports System.Security.AccessControl

Public Interface IContainer
    Sub Run(ParamArray Parameters() As Object)
    Function Initialize() As Boolean
    Function CanRead(path As String) As Boolean
    Function CanWrite(path As String) As Boolean
    Function Access(path As String, rights As FileSystemRights) As Boolean
    ReadOnly Property Name As String
    ReadOnly Property Author As String
    ReadOnly Property Version As Version
    ReadOnly Property Description As String
    ReadOnly Property Parent As ILoader
    ReadOnly Property BaseDirectory As String
End Interface
<Serializable>
Public MustInherit Class Base
    Inherits MarshalByRefObject
    Implements IContainer, IDisposable
    Private m_parent As ILoader
    Sub New(parent As ILoader)
        Me.m_parent = parent
    End Sub
#Region "Methods"
    Public Overridable Function Initialize() As Boolean Implements IContainer.Initialize
        Return True
    End Function
    Public Overridable Sub Run(ParamArray Parameters() As Object) Implements IContainer.Run
    End Sub
#End Region
#Region "Overrides"
    Public MustOverride ReadOnly Property Name As String Implements IContainer.Name
    Public MustOverride ReadOnly Property Author As String Implements IContainer.Author
    Public MustOverride ReadOnly Property Version As Version Implements IContainer.Version
    Public MustOverride ReadOnly Property Description As String Implements IContainer.Description
#End Region
#Region "Helpers"
    Public Function CanRead(path As String) As Boolean Implements IContainer.CanRead
        Try
            Return Me.CanAccess(path, FileSystemRights.Read)
        Catch
            Return False
        End Try
    End Function
    Public Function CanWrite(path As String) As Boolean Implements IContainer.CanWrite
        Try
            Return Me.CanAccess(path, FileSystemRights.Write)
        Catch
            Return False
        End Try
    End Function
    Public Function Access(path As String, rights As FileSystemRights) As Boolean Implements IContainer.Access
        Try
            Return Me.CanAccess(path, rights)
        Catch
            Return False
        End Try
    End Function
    Public ReadOnly Property Parent As ILoader Implements IContainer.Parent
        Get
            Return Me.m_parent
        End Get
    End Property
    Public ReadOnly Property BaseDirectory As String Implements IContainer.BaseDirectory
        Get
            Return AppDomain.CurrentDomain.RelativeSearchPath
        End Get
    End Property
    Private Function CanAccess(name As String, rights As FileSystemRights) As Boolean
        Dim allow As Boolean = False, deny As Boolean = False
        Dim access As DirectorySecurity = Directory.GetAccessControl(name)
        If (access IsNot Nothing) Then
            Dim authorization As AuthorizationRuleCollection = access.GetAccessRules(True, True, GetType(SecurityIdentifier))
            If (authorization IsNot Nothing) Then
                For Each rule As FileSystemAccessRule In authorization
                    If (rights And rule.FileSystemRights) <> rights Then
                        Continue For
                    ElseIf rule.AccessControlType = AccessControlType.Allow Then
                        allow = True
                    ElseIf rule.AccessControlType = AccessControlType.Deny Then
                        deny = True
                    End If
                Next
                Return allow AndAlso Not deny
            End If
        End If
        Return False
    End Function
#End Region
#Region "IDisposable Support"
    Protected disposedValue As Boolean
    Protected Overridable Sub Dispose(disposing As Boolean)
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
