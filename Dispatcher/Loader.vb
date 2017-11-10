Imports System.IO
Imports System.Reflection
Imports System.Security.Permissions
Imports Dispatcher.Runtime

<Serializable> <SecurityPermission(SecurityAction.Demand, Infrastructure:=True)>
Public NotInheritable Class Loader
    Inherits MarshalByRefObject
    Implements IDisposable, ILoader
    Public Property Plugins As List(Of Assembly) Implements ILoader.Plugins
    Sub New()
        Me.Plugins = New List(Of Assembly)
    End Sub
    Public Function Initialize(baseDirectory As String, Optional ext As String = "*.dll") As Boolean
        If (Directory.Exists(baseDirectory)) Then
            For Each file As String In Directory.GetFiles(baseDirectory, ext)
                Try
                    Me.Plugins.Add(Assembly.LoadFile(file))
                Catch
                    'Catch ex As BadImageFormatException     : assembly is invalid
                    'Catch ex As FileLoadException           : assembly is Locked (in use)
                    'Catch ex As UnauthorizedAccessException : no access
                End Try
            Next
        End If
        Return Me.Plugins.Any
    End Function
    Public Sub Reset()
        Me.Plugins.Clear()
    End Sub
    Public Function Import(Of T)() As T Implements ILoader.Import
        Return Me.ImportAll(Of T)().FirstOrDefault()
    End Function
    Public Function ImportAll(Of T)() As IEnumerable(Of T) Implements ILoader.ImportAll
        Dim collection As New LinkedList(Of T)
        For Each ctor As ConstructorInfo In Me.GetConstructors(Of T)(New Type() {GetType(ILoader)})
            collection.AddLast(Me.Create(Of T)(ctor, Me))
        Next
        Return collection
    End Function
    Private Function Create(Of T)(ctor As ConstructorInfo, ParamArray Parameters() As Object) As T
        Return CType(ctor.Invoke(Parameters), T)
    End Function
    Private Function GetConstructors(Of T)(ParamArray types() As Type) As IEnumerable(Of ConstructorInfo)
        Dim collection As New LinkedList(Of ConstructorInfo)()
        For Each asm As Assembly In Me.Plugins
            For Each asmType As Type In asm.GetTypes()
                If (asmType.IsClass AndAlso Not asmType.IsAbstract) Then
                    If (asmType.GetInterfaces.Contains(GetType(T))) Then
                        collection.AddLast(asmType.GetConstructor(types))
                    End If
                End If
            Next
        Next
        Return collection
    End Function
#Region "Disposable"
    Private Property DisposedValue As Boolean
    Protected Sub Dispose(disposing As Boolean)
        If Not Me.DisposedValue Then
            If disposing Then
                Me.Reset()
            End If
        End If
        Me.DisposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
