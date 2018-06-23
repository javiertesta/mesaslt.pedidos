using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Pedidos")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Mesas L.T. S.R.L.")]
[assembly: AssemblyProduct("Sistema de gestión de pedidos")]
[assembly: AssemblyCopyright("Copyright © 2015 Javier Testa")]
[assembly: AssemblyTrademark("Mesas L.T.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c475bd66-66d0-4a51-a2b9-2b1f38243b8c")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]

[assembly: AssemblyFileVersion("2.1.0.3")]
// 2.0.0.0 -> 05/07/2016 03:41 [Rediseñada la interfaz gráfica. Adicionado el tipo de artículo "Fuera de lista".]
// 2.0.0.1 -> 06/07/2016 00:33 [Bug: Una vista no escribía correctamente. Al no estar presente AntiForgeryToken, el sistema mostraba un error.]
// 2.0.0.2 -> 06/07/2016 00:33 [Bug: Como ahora el sistema muestra SeguimientoGlobal.CantidadPendiente, el servidor mostraba un error al listar pedidos dados de baja.]
// 2.0.0.3 -> 06/07/2016 01:38 [Adicionada una portada para el controlador "Seguimiento".]
// 2.0.0.4 -> 06/07/2016 01:54 [Actualizada la gráfica del controlador "Seguimiento".]
// 2.0.0.5 -> 06/07/2016 04:25 [Actualizada la última etapa del controlador "Seguimiento".]
// 2.0.0.6 -> 06/07/2016 06:48 [Corregido pequeño bug de visualización en la vista Pedidos\Index.]
// 2.0.0.7 -> 06/07/2016 11:53 [Actualizada la gráfica del controlador "Informes".]
// 2.1.0.0 -> 06/06/2018 10:40 [Listado de demorados. Pequeñas mejoras.]
// 2.1.0.1 -> 06/06/2018 10:40 [El sistema ya no traba el cambio de seguimiento de pedidos dados de baja.]
// 2.1.0.2 -> 09/06/2018 00:32 [Corregido error al listar el historial de un pedido, en donde los ítems históricos no tienen SeguimientoGlobal asociado.]
// 2.1.0.3 -> 23/06/2018 02:28 [Corregido bug al listar el historial de un único pedido que no tiene seguimiento definido (por ser backup de otro)]
