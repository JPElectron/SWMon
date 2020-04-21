@ECHO OFF
ECHO Switching to use Red INI...
COPY c:\dnsredir\dnsredir.ini c:\dnsredir\dnsredir-green.ini
DEL c:\dnsredir\dnsredir.ini
COPY c:\dnsredir\dnsredir-red.ini c:\dnsredir\dnsredir.ini
EXIT