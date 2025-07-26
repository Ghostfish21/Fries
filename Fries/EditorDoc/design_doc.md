# Unity Editor Window 系统设计文档

## 1. 需求分析

用户需要一个在Unity Editor中运行的帮助系统，主要功能包括：

*   **搜索系统**: 
    *   **帖子搜索**: 搜索任意帖子的标题。
    *   **文字搜索**: 搜索任意帖子的任意文本内容。
*   **帖子内容展示**: 
    *   每个帖子包含多个页面。
    *   每页支持文本和图片，使用Markdown格式。
    *   支持定义按钮，点击后可执行特定指令（如跳转到第N页）。
*   **指引跳转**: 
    *   点击指引按钮可跳转至特定Scene下的特定GameObject中的特定Component的特定Inspector字段。
    *   点击指引按钮可跳转至Project窗口中的特定文件。
    *  // TODO 暂时 Disable 掉用不到的选项
    *  // TODO 支持保存command结构
    *  // TODO 跳转到具体 Inspector 值的时候需要highlight
*   **Editor Window**: 整个系统完全基于Editor Window实现，不涉及任何in-game UI。

## 2. 系统架构设计
![pic](Assets/Fries/Fries/EditorDoc/a.png)
系统将主要由以下几个模块组成：

1.  **数据模型 (Data Models)**: 定义帖子、页面、按钮、跳转目标等数据结构。
[Button: "Button Text", /InspectorHighlight S1 Fingerprint2 TTEST sm]
2.  **数据管理 (Data Management)**: 负责数据的加载、保存和索引，以便搜索。
3.  **搜索逻辑 (Search Logic)**: 实现标题搜索和全文搜索功能。
4.  **UI 渲染 (UI Rendering)**: 使用Unity Editor GUI或UI Toolkit渲染Editor Window，并解析Markdown内容。
5.  **交互逻辑 (Interaction Logic)**: 处理按钮点击、页面跳转和指引跳转。

## 3. 数据模型 (Data Models)

我们将定义以下C#类来表示系统中的数据：

*   `Post`: 表示一个帖子，包含标题和页面列表。
*   `Page`: 表示帖子中的一个页面，包含Markdown内容和按钮列表。
*   `ButtonInfo`: 表示页面中的一个按钮，包含按钮文本和点击指令。
*   `Command`: 抽象类，表示按钮点击后执行的指令，例如 `GoToPageCommand` 和 `NavigateToUnityObjectCommand`。

## 4. 数据管理

*   **存储**: 帖子数据将以ScriptableObject的形式存储在Unity项目中，方便管理和序列化。
*   **加载**: Editor Window启动时加载所有帖子数据。
*   **索引**: 为搜索功能建立标题和全文索引。

## 5. 搜索逻辑

*   **标题搜索**: 遍历所有帖子，匹配标题。
*   **全文搜索**: 遍历所有帖子的所有页面内容，匹配文本。
*   **搜索结果**: 返回匹配的帖子列表，并可能包含匹配的页面信息。

## 6. UI 渲染

*   **主Editor Window**: 作为系统的入口，包含搜索框、搜索结果列表、帖子内容展示区域。
*   **Markdown 解析**: 需要一个Markdown解析器来将Markdown文本转换为Unity Editor GUI可渲染的元素（例如，粗体、斜体、图片、链接等）。可能需要自定义实现或寻找现有库。
*   **图片显示**: Markdown中的图片路径需要解析并加载对应的Texture2D。
*   **按钮渲染**: 渲染自定义按钮，并绑定点击事件。

## 7. 交互逻辑

*   **页面跳转**: 根据按钮指令切换当前显示的页面。
*   **Unity对象跳转**: 
    *   **Scene/GameObject/Component/Inspector字段**: 使用`UnityEditor.Selection.activeObject`和`UnityEditor.EditorGUIUtility.PingObject`等API来定位和高亮Unity对象。对于Inspector字段，可能需要更复杂的反射或Editor API操作。
    *   **Project文件**: 使用`UnityEditor.AssetDatabase.LoadAssetAtPath`和`UnityEditor.Selection.activeObject`来定位和高亮项目文件。

## 8. 技术选型

*   **UI**: Unity Editor GUI (IMGUI) 或 Unity UI Toolkit (推荐，更现代化，性能更好，但学习曲线较陡峭)。考虑到复杂性和Markdown渲染，可能需要结合使用或选择合适的第三方库。
*   **Markdown解析**: 自行实现或寻找C# Markdown解析库。

## 9. 阶段划分

1.  **数据模型**: 定义所有C#数据结构。
2.  **数据管理**: 实现ScriptableObject的创建、加载和保存。
3.  **搜索**: 实现标题和全文搜索功能。
4.  **Markdown渲染**: 实现Markdown到Editor GUI的渲染。
5.  **按钮和页面跳转**: 实现页面内跳转。
6.  **指引跳转**: 实现Unity对象和项目文件跳转。
7.  **Editor Window集成**: 将所有功能集成到主窗口。
8.  **测试与优化**: 确保系统稳定可用。
9.  **文档**: 提供完整代码和使用说明。



