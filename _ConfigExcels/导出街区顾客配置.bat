@echo off
setlocal
cd /d "%~dp0"
node export_shop_behavior_config.js
if errorlevel 1 (
    echo.
    echo 导出失败，请检查上方提示后重新运行。
) else (
    echo.
    echo 顾客行为配置已更新。
)
pause
