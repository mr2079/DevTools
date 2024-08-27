# Developer Tools
Simple project to create a cli app for guid, token, dns and ...
## Setup
After clone the repository, open ```cmd``` or ```powershell``` in ```DevTools.Cli``` project

run below command to create nupkg:
```
dotnet pack
```
after nupkg created in ```.\nupkg```, you must install it as a global tool with dotnet cli:
```
dotnet tool install DevTools.Cli --global --add-source .\nupkg\
```
and then you can access cli app globaly with ```dev-tools``` in command line

examples:
```
PS C:\> dev-tools guid

710edf46-5da8-4a35-8b7e-fff9e78662bc
```

```
PS C:\> dev-tools guid --c 3

f5f5b2fb-e6e7-4fad-ab88-0f23b1644ace
77d1236f-ab77-42d3-a1cc-b51dbd2831f9
9ce3855e-3b6b-4deb-87a3-b79d8ff076bf
```

```
PS C:\> dev-tools token 24

wYwZzzD9ep6cshl7EWbwdUZ2
```

```
PS C:\> dev-tools token 16 --c 3

PMqzzFvEjemibpGN
vg1Nyvyx41PDe8i5
rcU1wWetY4HlxL1a
```

```
PS C:\> dev-tools dns-change 1.1.1.1 1.0.0.1

dns addresses have been changed to 1.1.1.1, 1.0.0.1
```

```
PS C:\> dev-tools dns-reset

dns addresses have been removed
```
