# SWMon
SWMon (short for Switch Monitor) connects with a WebRelay over the network, when the relay status changes you can run any application...

- send an email (via bmail or sendemail)
- shutdown all computers at the end of the day (via psshutdown)
- launch a program on every PC in a lab (via psexec)
- change the configuration of a firewall/router/switch (via plink)
- start or stop a Windows service

SWMon can also change the function of DNS Redirector, examples/documentation available at http://jpelectron.com/sample/JPElectron/Switch%20over%20Ethernet/

Also included: (alternate service name, in order to run multiple instances on the same server)
- SWMon2 v1.0.0.1
- SWMon3 v1.0.0.1
- SWMon4 v1.0.0.1
- SWMonDNSR v1.0.0.1 ...for use with DNS Redirector, see FAQ 123.

Tested on Windows XP, Vista, 7, 8, 8.1 and Server 2003, 2008, 2012

This program runs as a service; without any GUI, taskbar, or system tray icon.

Installation:

1) Ensure the Microsoft .NET Framework 4.x is installed
2) Run swmonsetup.msi and follow the wizard
3) Modify C:\SWMON\swmon.ini as indicated (see comments within the file)

Usage:

It is strongly suggested you specify a unique/strong password for webrelay control. If you need to allow anonymous control and still have this program monitor the relay status, set Password=* in swmon.ini to bypass the need to specify a password.
