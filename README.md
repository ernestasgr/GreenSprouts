Green sprouts

Kad veiktų Queue ir aplamai pasileistų programos:
1. Atsisiųsti Docker dekstop (jeigu neturite) https://www.docker.com/products/docker-desktop/
2. Atsidaryti terminalą arba PowerShell
3. Paleisti šitą komandą: docker run -d --hostname rmq --name rabbit-server -p 8080:15672 -p 5672:5672 rabbitmq:3-management
4. donezo

Environment variables kurios reikia prisideti prie savo PC: 
REDACTED

appsettings.json (I connection string irasome reiksmes)
1. "Default": "Server=tcp:pvp.database.windows.net,1433;Initial Catalog=pvp;Persist Security Info=False;User           ID=sqladmin;Password=si7NnYir7gCK8sb;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
2. "VM": "do8MmUot8hVL9dn"

"Default" - Prisijungimas prie duombazes
"VM" - Prisijungimas prie Azure Virtual Machine.

WSL instaliacija:
1. Instaliuoti WSL, ijungiam powershell su admin teisem ir rasom wsl --install -d Ubuntu
2. Irasom i powershell/terminal komanda: Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform
3. Irasom yes, kad restartuotu PC.

Prisijungti per SSH su WSL instaliavus:
1. irasom i cmd: wsl (kad prie installinto wsl pereitume)
2. irasai tada: ssh pvpvmadmin@20.166.77.219
3. turetu paprasyti passwordo, kuris yra: do8MmUot8hVL9dn (jeigu nepraso tai servas isjungtas, reikia pajungti projekta, kad VM isijungtu)
