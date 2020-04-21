@ECHO OFF
ECHO Switching to use Green INI...
DEL c:\dnsredir\dnsredir.ini
COPY c:\dnsredir\dnsredir-green.ini c:\dnsredir\dnsredir.ini
DEL c:\dnsredir\dnsredir-green.ini
EXIT