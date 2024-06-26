@echo off

:: 设置源和目标路径
set srcDir=Data
set clientDir=..\Client\Data
set serverDir=..\Server\GameServer\GameServer\bin\Debug\Data

:: 创建目标文件夹（如果不存在）
if not exist %clientDir% (
    echo 创建客户端目标文件夹 %clientDir%
    mkdir %clientDir%
)

if not exist %serverDir% (
    echo 创建服务器目标文件夹 %serverDir%
    mkdir %serverDir%
)

:: 文件列表
set fileList=CharacterDefine.txt^
 EquipDefine.txt^
 ItemDefine.txt^

:: 复制文件到客户端目录
for %%f in (%fileList%) do (
    echo 复制 %srcDir%\%%f 到 %clientDir%
    copy %srcDir%\%%f %clientDir%
    if errorlevel 1 (
        echo 错误: 无法复制 %%f 到客户端目录
    )
)

:: 复制文件到服务器目录
for %%f in (%fileList%) do (
    echo 复制 %srcDir%\%%f 到 %serverDir%
    copy %srcDir%\%%f %serverDir%
    if errorlevel 1 (
        echo 错误: 无法复制 %%f 到服务器目录
    )
)

:: 暂停以查看结果
pause
