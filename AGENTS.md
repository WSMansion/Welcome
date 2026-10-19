# WelCome 项目约定

Unity 工程位于仓库的 WelCome/ 目录。

## 项目资料

### 飞书云文档

- 资料目录 token：A0QXfEMWzl1Oxwde2oacChprnpf
- 目录链接：https://my.feishu.cn/drive/folder/A0QXfEMWzl1Oxwde2oacChprnpf
- 已有文档：
  - Cursor-开发日志（token：URkPdqMzbodYxox5YuKchT69n1g）
  - Cursor-代码架构（token：RC8WdN5lDoJs33xezBSc0i96nBg）
  - Cursor-当前状态与规划（token：FUuGdb0DWo0ENQxEWbucVYF2n1b）
- 更新文档时先创建新版本并移入资料目录，确认新版本无误后再删除旧版。

## Unity C# 开发约定

### 代码分层

- Core/：基础框架，如 Singleton、EventBus、GameManager、GameState、GameEvents。
- Models/：纯数据类和枚举（如 Enums、ProductInstance、CustomerInstance），不引用 Systems。
- Systems/：游戏逻辑，只引用 Core 和 Models。
- Data/：配置加载（ConfigManager），只引用 Core。
- UI/：界面面板和工具类，可引用其他层。
- 枚举和数据模型不要定义在 System 文件中，统一放在 Models。

### UI 与生命周期

- 所有界面都必须是预制体，放在 Assets/Prefabs/UI/。禁止用代码临时搭建界面层级。
- 运行时只实例化预制体并绑定引用；每次显示时由 OnShow() 刷新数据。重复列表项也使用预制体模板，代码只填数据。
- EventSystem 应放在场景 Hierarchy 中，不要在运行时创建。
- 背景 Image、文字等非交互元素设置 raycastTarget = false。
- UI 颜色直接使用 UIHelper.AccentGold 等公共字段；RectTransform 使用 rect.Set(xMin, yMin, xMax, yMax) 扩展方法。

### Singleton 与事件

- 单场景游戏默认不使用 DontDestroyOnLoad；按需重写 protected virtual bool Persistent => false;。
- 必须处理 Domain Reload，在 RuntimeInitializeOnLoadMethod 中重置静态字段。
- EventBus 的 Subscribe 应防止重复注册；Publish 应捕获单个订阅者的异常，避免中断整条事件链。
- 事件结构体统一放在 Core/GameEvents.cs。

### 命名空间、状态与配置

- EventType 位于 WelCome.Models，使用时引入 using WelCome.Models。
- Luban 生成的表访问器带 Tb 前缀，例如 Tables.TbXxx。
- ChangeState 不要递归调用自身；需要延后一帧时使用协程。UI 回调执行后检查当前状态是否仍匹配。
- ConfigManager.LoadAllConfigs 必须有 try-catch；LoadJson 遇到缺失文件返回空 JSONObject。
- 系统初始化时检查 ConfigManager.IsLoaded 后再访问 Tables。

### 资源与对象命名

- 可搜索的资源使用稳定、直观的业务名称；同一个业务对象不要在资源名中混用不同称呼。
- 同类资源变体采用业务名加两位编号，例如 Customer_00、Customer_01，从 00 开始。
- Prefab 文件名与根 GameObject 名保持一致，方便在 Project 窗口和 Hierarchy 中定位。
- 同一业务域的可搜索资源使用统一前缀；顾客相关资源统一使用 `Customer` 前缀，例如 `Customer_00`、`CustomerSpeechBubble`、`CustomerTextThought`。
- 正式图片只放在 Assets/Art 对应业务目录。商品图标放在 Assets/Art/OpenShop/Products，文件名与配方表 icon 字段一致，由图标库引用原图，不要再复制进 Resources。

### 自检

- 修改代码后检查 Unity Editor.log 中新增的编译和运行时错误，并静态检查编译问题、命名空间冲突及缺失 using。
- 不要把阻断性问题留给用户进入 Unity 后发现。
