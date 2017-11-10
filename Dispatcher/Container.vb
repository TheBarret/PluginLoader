Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Lifetime
Imports System.Security.Permissions
Imports System.Security

<Serializable> <SecurityPermission(SecurityAction.Demand, Infrastructure:=True)>
Public NotInheritable Class Container(Of IType As Class)
    Implements ISponsor, IDisposable
    Private m_refobject As IType
    Sub New(RefObject As IType)
        Try
            If (TypeOf RefObject Is MarshalByRefObject) Then
                Dim lease As Object = Container(Of IType).GetLease(RefObject)
                If TypeOf lease Is ILease Then
                    CType(lease, ILease).Register(Me)
                End If
            End If
        Finally
            Me.RefObject = RefObject
        End Try
    End Sub
    Public Property RefObject As IType
        Get
            If (Not Me.IsDisposed) Then
                Return Me.m_refobject
            End If
            Return Nothing
        End Get
        Set(value As IType)
            Me.m_refobject = value
        End Set
    End Property
    Private Function Type() As Type
        Return Me.RefObject.GetType
    End Function
    Private Function Cast(Of TypeRef As Class)() As TypeRef
        Return TryCast(Me.RefObject, TypeRef)
    End Function
    Public Function IsAssignableFrom(Of TypeRef As Class)() As Boolean
        Return GetType(TypeRef).IsAssignableFrom(Me.Type)
    End Function
    Private Function Renewal(lease As ILease) As TimeSpan Implements ISponsor.Renewal
        If (Not Me.IsDisposed) Then Return LifetimeServices.RenewOnCallTime
        Return TimeSpan.Zero
    End Function
    Private Shared Function GetLease(RefObject As Object) As Object
        Return RemotingServices.GetLifetimeService(CType(RefObject, MarshalByRefObject))
    End Function
#Region "IDisposable"
    Private Property IsDisposed As Boolean
    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
    Protected Sub Dispose(disposing As Boolean)
        If Not Me.IsDisposed Then
            If disposing Then
                If (Me.IsAssignableFrom(Of IDisposable)()) Then
                    Me.Cast(Of IDisposable).Dispose()
                End If
                If (Me.IsAssignableFrom(Of MarshalByRefObject)()) Then
                    Dim lease As Object = Container(Of IType).GetLease(Me.RefObject)
                    If TypeOf lease Is ILease Then
                        CType(lease, ILease).Unregister(Me)
                    End If
                End If
            End If
            Me.RefObject = Nothing
            Me.IsDisposed = True
        End If
    End Sub
#End Region
End Class