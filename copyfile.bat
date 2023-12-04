@echo off
echo Copying LC_API.dll...
copy E:\VisualStudio\projects\LC-API\bin\Release\netstandard2.1\LC_API.dll E:\VisualStudio\projects\LC-API\publish\

echo Zipping the publish folder...
cd E:\VisualStudio\projects\LC-API\publish\
powershell Compress-Archive -Path * -DestinationPath ..\publish.zip

echo Task completed.
