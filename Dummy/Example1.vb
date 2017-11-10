Imports Dispatcher
Imports Dispatcher.Runtime
Imports System.Reflection

Public Interface CommunicationTest
    Sub SayHello(sender As String)
End Interface

Public Class Example1
    Inherits Runtime.Base
    Implements CommunicationTest
    Sub New(parent As ILoader)
        MyBase.New(parent)
    End Sub
    Public Overrides Function Initialize() As Boolean
        Console.Title = "Plugin Demo"
        Return True
    End Function
    Public Overrides Sub Run(ParamArray Parameters() As Object)
        Console.WriteLine("Hello from {0}!", Me.Name)

        '// Show Domain Name
        Console.WriteLine(String.Empty)
        Console.WriteLine("Current domain: {0}", AppDomain.CurrentDomain.FriendlyName)

        '// Test Read/Write
        Console.WriteLine(String.Empty)
        Console.WriteLine("-> Access on .\  = {0} ", Me.CanRead(Me.BaseDirectory))
        Console.WriteLine("-> Access on C:\ = {0} ", Me.CanRead("C:\"))

    End Sub
    Public Sub SayHello(sender As String) Implements CommunicationTest.SayHello
        Console.WriteLine(String.Empty)
        Console.WriteLine("[{0}] Hello from {1}", Me.Name, sender)
    End Sub
    Public Overrides ReadOnly Property Name As String
        Get
            Return "Dummy 1"
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

