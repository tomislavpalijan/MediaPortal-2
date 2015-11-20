
@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" RestorePackages.targets /target:RestoreBuildPackages

set PathToBuildReport=.\..\Packages\BuildReport.1.0.0
xcopy /I /Y %PathToBuildReport%\_BuildReport_Files .\_BuildReport_Files

set CONFIGURATION=Debug

set xml=Build_Report_%CONFIGURATION%_Components.xml
set html=Build_Report_%CONFIGURATION%_Components.html

set logger=/l:XmlFileLogger,"%PathToBuildReport%\MSBuild.ExtensionPack.Loggers.dll";logfile=%xml%
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /m Build.proj %logger% /property:BuildClient=true;BuildServer=true;BuildServiceMonitor=true;BuildSetup=false;Configuration=%CONFIGURATION%
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /m Build.proj %logger% /property:OneStepOnly=true;ClearBinDirectory=false;BuildClient=false;BuildServer=false;BuildServiceMonitor=false;BuildSetup=true;Configuration=%CONFIGURATION%

%PathToBuildReport%\msxsl %xml% _BuildReport_Files\BuildReport.xslt -o %html%
