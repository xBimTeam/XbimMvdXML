@rem Only two files are needed to run the plugin
@md ..\..\XbimWindowsUI\Output\Debug\Plugins\XbimPlugin.MvdXML\ > nul 2> nul
copy bin\Debug\XbimPlugin.MvdXML.exe ..\..\XbimWindowsUI\Output\Debug\Plugins\XbimPlugin.MvdXML\
copy bin\Debug\Xbim.MvdXml.dll ..\..\XbimWindowsUI\Output\Debug\Plugins\XbimPlugin.MvdXML\
@md ..\..\XbimWindowsUI\Output\Release\Plugins\XbimPlugin.MvdXML\ > nul 2> nul
copy bin\Release\XbimPlugin.MvdXML.exe ..\..\XbimWindowsUI\Output\Release\Plugins\XbimPlugin.MvdXML\
copy bin\Release\Xbim.MvdXml.dll ..\..\XbimWindowsUI\Output\Release\Plugins\XbimPlugin.MvdXML\
@pause