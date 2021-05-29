# SWMon

SWMon (short for Switch Monitor) connects with a WebRelay over the network, when the relay status changes you can run any application...

- send an email (via bmail or sendemail)
- shutdown all computers at the end of the day (via psshutdown)
- launch a program on every PC in a lab (via psexec)
- change the configuration of a firewall/router/switch (via plink)
- start or stop a Windows service

SWMon can also change the function of DNS Redirector, examples/documentation available at https://drive.google.com/drive/folders/1xfFpZ02QotR5JqbDMXnsMBRVBL3rh_Cl?usp=sharing

Also included: (alternate service name, in order to run multiple instances on the same server)
- SWMon2 v1.0.0.1
- SWMon3 v1.0.0.1
- SWMon4 v1.0.0.1
- SWMonDNSR v1.0.0.1 ...for use with DNS Redirector, see FAQ 123.

Tested on Windows XP, Vista, 7, 8, 8.1 and Server 2003, 2008, 2012

This program runs as a service; without any GUI, taskbar, or system tray icon.

## Installation

1) Ensure the Microsoft .NET Framework 4.x is installed
2) Run swmonsetup.msi and follow the wizard
3) Modify C:\SWMON\swmon.ini as indicated (see comments within the file)

## Usage

It is strongly suggested you specify a unique/strong password for webrelay control. If you need to allow anonymous control and still have this program monitor the relay status, set Password=* in swmon.ini to bypass the need to specify a password.

## Related Links

WebRelay from http://www.controlbyweb.com/webrelay

## License

GPL does not allow you to link GPL-licensed components with other proprietary software (unless you publish as GPL too).

GPL does not allow you to modify the GPL code and make the changes proprietary, so you cannot use GPL code in your non-GPL projects.

If you wish to integrate this software into your commercial software package, or you are a corporate entity with more than 10 employees, then you should obtain a per-instance license, or a site-wide license, from http://jpelectron.com/buy

For all other use cases please consider: <a href='https://ko-fi.com/C0C54S4JF' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi2.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

[End of Line]
