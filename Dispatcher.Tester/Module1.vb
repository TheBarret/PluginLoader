Imports System.IO
Imports System.Security
Imports System.Security.Permissions
Imports Dispatcher.Runtime

Module Module1

    Sub Main()

        Dim plugins As String = Path.GetFullPath(".\Plugins\")

        Using host As New Host("sandboxie", plugins)

            If (host.Load) Then
                Try
                    For Each container As Container(Of IContainer) In host.ImportAll(Of IContainer)()
                        Dim plugin As IContainer = container.RefObject
                        Console.WriteLine("+-----------------------------------------------+")
                        Console.WriteLine("Type: {0} {1}", plugin.Name, plugin.Version)
                        Console.WriteLine(String.Empty)

                        If (plugin.Initialize) Then
                            plugin.Run()
                        End If
                    Next

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try
            End If

        End Using

        Console.Read()
    End Sub
End Module
