@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion

title 飞书云文档 MCP 配置向导

echo.
echo ============================================================
echo          飞书云文档 MCP 一键配置向导
echo ============================================================
echo.
echo  本工具将引导你为 Cursor 配置飞书云文档访问能力。
echo  完成后，Cursor 中的 AI 助手就能直接读取你的飞书文档。
echo.
echo  整个过程大约需要 5-10 分钟。
echo.
echo ============================================================
echo.
pause

:: ============================================================
:: 步骤 0: 检测并安装 Node.js
:: ============================================================
echo.
echo [步骤 0/7] 检测 Node.js 环境...
echo.

set "NODE_DIR=%USERPROFILE%\nodejs"

where node >nul 2>&1
if !errorlevel! equ 0 (
    for /f "tokens=*" %%i in ('node -v') do set NODE_VER=%%i
    echo  已检测到 Node.js !NODE_VER!，跳过安装。
    for /f "tokens=*" %%i in ('where npx') do (
        set "NPX_CMD=%%i"
        goto :got_npx_system
    )
)

:got_npx_system
if defined NPX_CMD goto :node_ready

if exist "!NODE_DIR!\node.exe" (
    echo  已检测到本地 Node.js 安装。
    set "NPX_CMD=!NODE_DIR!\npx.cmd"
    set "PATH=!NODE_DIR!;!PATH!"
    goto :node_ready
)

echo  未检测到 Node.js，正在自动下载安装...
echo  （下载约 35MB，请稍候）
echo.

set "NODE_URL=https://nodejs.org/dist/v22.14.0/node-v22.14.0-win-x64.zip"
set "NODE_ZIP=%TEMP%\node.zip"

powershell -Command "Invoke-WebRequest -Uri '!NODE_URL!' -OutFile '!NODE_ZIP!' -UseBasicParsing"
if !errorlevel! neq 0 (
    echo.
    echo  [错误] Node.js 下载失败，请检查网络连接后重试。
    echo  你也可以手动安装：https://nodejs.org/
    pause
    exit /b 1
)

echo  下载完成，正在解压...
powershell -Command "Expand-Archive -Path '!NODE_ZIP!' -DestinationPath '%TEMP%\nodejs_tmp' -Force"

if not exist "!NODE_DIR!" mkdir "!NODE_DIR!"
xcopy "%TEMP%\nodejs_tmp\node-v22.14.0-win-x64\*" "!NODE_DIR!\" /E /Y /Q >nul
rmdir /S /Q "%TEMP%\nodejs_tmp" >nul 2>&1
del "!NODE_ZIP!" >nul 2>&1

powershell -Command "$p=[System.Environment]::GetEnvironmentVariable('Path','User'); if($p -notlike '*!NODE_DIR!*'){[System.Environment]::SetEnvironmentVariable('Path',\"$p;!NODE_DIR!\",'User')}"
set "PATH=!NODE_DIR!;!PATH!"
set "NPX_CMD=!NODE_DIR!\npx.cmd"

echo  Node.js 安装完成！

:node_ready

:: 获取 node 所在目录（用于 env PATH）
for %%F in ("!NPX_CMD!") do set "NODE_BIN_DIR=%%~dpF"
if "!NODE_BIN_DIR:~-1!"=="\" set "NODE_BIN_DIR=!NODE_BIN_DIR:~0,-1!"

echo.
echo ============================================================
echo.

:: ============================================================
:: 步骤 1: 引导创建飞书应用
:: ============================================================
echo [步骤 1/7] 创建飞书自建应用
echo.
echo  请在浏览器中完成以下操作：
echo.
echo  1. 链接将自动打开 - 用你的飞书账号登录
echo  2. 点击「创建企业自建应用」
echo  3. 应用名称随意填写（如"Cursor助手"）
echo  4. 创建完成后进入应用页面
echo.
start "" "https://open.feishu.cn/app"
echo  完成创建后，按任意键继续...
pause >nul

echo.
echo ============================================================
echo.

:: ============================================================
:: 步骤 2: 输入 App ID 和 App Secret
:: ============================================================
echo [步骤 2/7] 输入应用凭证
echo.
echo  在你刚创建的应用页面，左侧菜单点「凭证与基础信息」：
echo  - 复制 App ID（格式如 cli_xxxxxxxx）
echo  - 点击 App Secret 旁的复制按钮
echo.

:input_credentials
set "APP_ID="
set /p APP_ID="  请粘贴 App ID: "
if "!APP_ID!"=="" (
    echo  App ID 不能为空，请重新输入。
    goto :input_credentials
)

set "APP_SECRET="
set /p APP_SECRET="  请粘贴 App Secret: "
if "!APP_SECRET!"=="" (
    echo  App Secret 不能为空，请重新输入。
    goto :input_credentials
)

echo.
echo  正在验证凭证...

powershell -Command "$r=Invoke-RestMethod -Uri 'https://open.feishu.cn/open-apis/auth/v3/app_access_token/internal' -Method POST -Body ('{\"app_id\":\"!APP_ID!\",\"app_secret\":\"!APP_SECRET!\"}') -ContentType 'application/json'; exit $r.code"
if !errorlevel! neq 0 (
    echo.
    echo  [错误] 凭证验证失败！请确认 App ID 和 App Secret 是否正确。
    echo  提示：请直接用复制按钮复制，不要手动输入。
    echo.
    goto :input_credentials
)

echo  凭证验证通过！
echo.
echo ============================================================
echo.

:: ============================================================
:: 步骤 3: 添加权限
:: ============================================================
echo [步骤 3/7] 添加应用权限
echo.
echo  现在需要为应用添加云文档读取权限。
echo  链接将自动打开，请在页面中：
echo.
echo  1. 搜索 drive:drive:readonly 并开通
echo  2. 搜索 docx:document:readonly 并开通
echo  3. 如果有「批量开通」按钮，点击它
echo.
start "" "https://open.feishu.cn/app/!APP_ID!/auth"
echo  添加权限后，按任意键继续...
pause >nul

echo.
echo ============================================================
echo.

:: ============================================================
:: 步骤 4: 开启网页应用能力
:: ============================================================
echo [步骤 4/7] 开启网页应用能力
echo.
echo  在应用页面完成以下操作：
echo.
echo  1. 左侧菜单点击「添加应用能力」
echo  2. 找到「网页应用」，点击「添加」
echo  3. 桌面端主页填写：http://localhost:3000
echo  4. 移动端主页填写：http://localhost:3000
echo  5. 点击保存
echo.
echo  完成后，按任意键继续...
pause >nul

echo.
echo ============================================================
echo.

:: ============================================================
:: 步骤 5: 添加回调地址并发布
:: ============================================================
echo [步骤 5/7] 配置安全设置并发布应用
echo.
echo  【A. 添加回调地址】
echo.
echo  链接将自动打开，请在「重定向URL」中添加：
echo.
echo     http://localhost:3000/callback
echo.
echo  然后点击保存。
echo.
start "" "https://open.feishu.cn/app/!APP_ID!/safe"
echo  完成后按任意键继续...
pause >nul
echo.
echo  【B. 发布应用】
echo.
echo  1. 左侧菜单「应用发布」 -^> 「版本管理与发布」
echo  2. 点击「创建版本」
echo  3. 版本号填 1.0.0，说明随意
echo  4. 提交后审批通过（如果你是管理员可直接通过）
echo.
echo  发布完成后，按任意键继续...
pause >nul

echo.
echo ============================================================
echo.

:: ============================================================
:: 步骤 6: OAuth 登录
:: ============================================================
echo [步骤 6/7] 飞书账号授权登录
echo.
echo  即将弹出浏览器进行飞书授权登录。
echo  请在浏览器页面中点击「授权」按钮。
echo.
echo  正在启动...
echo.

:try_login
"!NPX_CMD!" -y @larksuiteoapi/lark-mcp login -a !APP_ID! -s !APP_SECRET!
if !errorlevel! neq 0 goto :login_failed
goto :login_success

:login_failed
echo.
echo  [错误] 登录失败！可能原因：
echo  - 未正确配置回调地址 http://localhost:3000/callback
echo  - 未开启网页应用能力
echo  - 未发布应用
echo  - 端口 3000 被占用
echo.
set /p RETRY="  是否重试？(Y/n): "
if /i "!RETRY!"=="n" (
    echo  请排查问题后重新运行本脚本。
    pause
    exit /b 1
)
goto :try_login

:login_success
echo.
echo  授权登录成功！
echo.
echo ============================================================
echo.

:: ============================================================
:: 步骤 7: 生成 Cursor MCP 配置
:: ============================================================
echo [步骤 7/7] 生成 Cursor MCP 配置文件
echo.

set "PROJECT_DIR=%CD%"
echo  检测到当前目录：!PROJECT_DIR!
echo.

set "CONFIRM_DIR=Y"
set /p CONFIRM_DIR="  MCP 配置将写入此目录的 .cursor\mcp.json，确认？(Y/n): "
if /i "!CONFIRM_DIR!"=="n" (
    set /p PROJECT_DIR="  请输入 Cursor 项目目录完整路径: "
)

if not exist "!PROJECT_DIR!\.cursor" mkdir "!PROJECT_DIR!\.cursor"

:: 检查是否已有 mcp.json
if exist "!PROJECT_DIR!\.cursor\mcp.json" (
    echo.
    echo  [注意] 已存在 .cursor\mcp.json，将备份为 mcp.json.bak
    copy "!PROJECT_DIR!\.cursor\mcp.json" "!PROJECT_DIR!\.cursor\mcp.json.bak" >nul
)

:: 用 PowerShell 生成 JSON（避免 batch echo 转义问题）
powershell -Command ^
  "$json = @{mcpServers=@{'lark-mcp'=@{command='!NPX_CMD!';args=@('-y','@larksuiteoapi/lark-mcp','mcp','-a','!APP_ID!','-s','!APP_SECRET!','--oauth','--token-mode','user_access_token','-t','preset.doc.default,drive.v1.file.list');env=@{PATH='!NODE_BIN_DIR!;C:\Windows\system32;C:\Windows'}}}}; $json | ConvertTo-Json -Depth 5 | Set-Content '!PROJECT_DIR!\.cursor\mcp.json' -Encoding UTF8"

echo.
echo  配置文件已生成：!PROJECT_DIR!\.cursor\mcp.json
echo.

:: ============================================================
:: 完成
:: ============================================================
echo ============================================================
echo.
echo  恭喜！飞书云文档 MCP 配置完成！
echo.
echo  最后一步：
echo.
echo  1. 打开 Cursor
echo  2. 按 Ctrl+Shift+P
echo  3. 输入 Reload Window 并回车
echo  4. 在 Settings -^> MCP 中确认 lark-mcp 状态为绿色
echo.
echo  然后你就可以让 AI 助手直接读取你的飞书文档了！
echo  例如：「请读取我飞书里的 xxx 文档」
echo.
echo ============================================================
echo.
pause
