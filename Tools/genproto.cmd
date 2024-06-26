@echo off
REM 切换到指定的目录
cd /d D:\MyUnityProject\MyMMORPG\Tools\protoc-27.2-win64\bin

REM 编译 message.proto 文件为 C# 代码
REM --proto_path 指定 .proto 文件的目录
REM --csharp_out 指定生成的 C# 代码的输出目录
protoc --proto_path=D:\MyUnityProject\MyMMORPG\Src\Lib\proto --csharp_out=D:\MyUnityProject\MyMMORPG\Src\Lib\Protocol D:\MyUnityProject\MyMMORPG\Src\Lib\proto\message.proto

REM 检查上一个命令是否成功
if %errorlevel% neq 0 (
    echo 编译 message.proto 失败
    pause
    exit /b %errorlevel%
)

echo 编译 message.proto 成功

REM 重命名生成的文件
cd /d D:\MyUnityProject\MyMMORPG\Src\Lib\Protocol
rename Message.cs message.cs

REM 暂停以查看输出结果
pause
