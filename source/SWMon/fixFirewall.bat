@ECHO OFF
ECHO. 
ECHO. Adding exceptions to Windows Firewall...
ECHO. 
NETSH firewall add allowedprogram c:\swmondnsr\swmondnsrsvc.exe "SWMonDNSR Service" ENABLE
ECHO. Task complete...
ECHO. 
PAUSE
EXIT