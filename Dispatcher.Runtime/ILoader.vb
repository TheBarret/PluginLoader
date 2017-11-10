Imports System.Reflection

Public Interface ILoader
    Function Import(Of T)() As T
    Function ImportAll(Of T)() As IEnumerable(Of T)
    Property Plugins As List(Of Assembly)
End Interface