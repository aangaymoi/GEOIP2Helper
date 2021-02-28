::https://dev.maxmind.com/geoip/geoip2/geolite2/
::http://geolite.maxmind.com/download/geoip/database/GeoLite2-City.tar.gz

echo off

::MAKE SURE NO MORE ZIP PACKAGE EXISTS
del /f /q *.gz
del /f /q *.tar

"X:\06. Tools\wget\wget.exe" https://geolite.maxmind.com/download/geoip/database/GeoLite2-Country.tar.gz

IF %ERRORLEVEL% NEQ 0 (
	goto @ERROR_HANDLING
)

"X:\06. Tools\wget\wget.exe" https://geolite.maxmind.com/download/geoip/database/GeoLite2-City.tar.gz

IF %ERRORLEVEL% NEQ 0 (
	goto @ERROR_HANDLING
)

"X:\06. Tools\wget\wget.exe" https://geolite.maxmind.com/download/geoip/database/GeoLite2-ASN.tar.gz

IF %ERRORLEVEL% NEQ 0 (
	goto @ERROR_HANDLING
)

REM ::BAK
for /f "tokens=1 delims=" %%a in ('dir /b /s *.mmdb') do (
	move /y "%%a" "%%a.bak"	
)

::Extract
"x:\06. Tools\Maldives\7za.exe" e GeoLite2-Country.tar.gz
"x:\06. Tools\Maldives\7za.exe" e GeoLite2-City.tar.gz
"x:\06. Tools\Maldives\7za.exe" e GeoLite2-ASN.tar.gz

::Extract
"x:\06. Tools\Maldives\7za.exe" e GeoLite2-Country.tar -y
IF %ERRORLEVEL% NEQ 0 (
	goto @ERROR_HANDLING
)

"x:\06. Tools\Maldives\7za.exe" e GeoLite2-City.tar -y
IF %ERRORLEVEL% NEQ 0 (
	goto @ERROR_HANDLING
)

"x:\06. Tools\Maldives\7za.exe" e GeoLite2-ASN.tar -y
IF %ERRORLEVEL% NEQ 0 (
	goto @ERROR_HANDLING
)

for /f "tokens=1 delims=" %%a in ('dir /b /s *.mmdb') do (
	move /y "%%a" "."	
)

::DEL ZIP PACKAGE
del /f /q *.tar.gz
del /f /q *.tar

::DEL OLD VERSION
del /f /q "*.bak"

::FOR /D %%p IN (".\*.*") DO rmdir "%%p" /s /q

:ERROR_HANDLING

@pause