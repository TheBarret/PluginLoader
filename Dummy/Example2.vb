Imports Dispatcher
Imports Dispatcher.Runtime
Imports System.Reflection

Public Class Example2
    Inherits Runtime.Base
    Sub New(parent As ILoader)
        MyBase.New(parent)
    End Sub
    Public Overrides Sub Run(ParamArray Parameters() As Object)
        Console.WriteLine("Testing plugin communication...")
        Console.WriteLine(String.Empty)

        Dim comm = Me.Parent.Import(Of CommunicationTest)()

        If (comm IsNot Nothing) Then
            comm.SayHello(Me.Name)
        End If


    End Sub
    Public Overrides ReadOnly Property Name As String
        Get
            Return "Dummy 2"
        End Get
    End Property
    Public Overrides ReadOnly Property Author As String
        Get
            Return "Barret"
        End Get
    End Property
    Public Overrides ReadOnly Property Description As String
        Get
            Return "Dummy Plugin"
        End Get
    End Property
    Public Overrides ReadOnly Property Version As Version
        Get
            Return New Version(1, 0, 0, 0)
        End Get
    End Property
End Class

