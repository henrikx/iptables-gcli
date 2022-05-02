# iptables-gcli
`iptables-gcli` is a portable `iptables` wrapper which aims to make it easy to add, edit and reorder iptables rules by adding a graphical user-interface to `iptables`. 

To save your rules, please use `iptables-save` and `iptables-persistent`. This is by design, as the application is only an editor.

# How it works
`iptables-gcli` works by parsing the `iptables -S` command output and using it to create an editable list of rules. The rules are displayed as arguments to `iptables`-commands. The user can then freely edit the rules and the order they are applied. When the time comes to apply the changes, `iptables-gcli` will flush the table and then quickly apply the rules again in the displayed order.

# Notes
- This software is currently in alpha and is **NOT READY FOR PRODUCTION USE!**
- `iptables` [does not have a programmatic way of managing it](https://stackoverflow.com/questions/109553/how-can-i-programmatically-manage-iptables-rules-on-the-fly). As a result, the application solely uses `iptables` commands to interact with it. This approach has a few drawbacks, such as the fact that while applying rules, the `iptables` rules may be inactive for a little period of time while the rules are flushed (under 0.5 seconds). 

# Building
Due to the nature of .NET Core, even though this is a Linux program it may still be built on Windows machines.
- `git clone https://github.com/henrikx/iptables-gcli.git`
- `cd iptables-gcli`
- `dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true,PublishTrimmed=true --self-contained`
- Binaries will be placed in `iptables-gcli\iptables-gcli\bin\Release\netcoreapp3.1\linux-x64\publish`

# Screenshots
![image](https://user-images.githubusercontent.com/10342989/147159688-0e403738-bd30-4685-9069-449075e3f43e.png)
![image](https://user-images.githubusercontent.com/10342989/147159712-1a485563-1c93-4e94-b672-bcc3e331ea5d.png)
