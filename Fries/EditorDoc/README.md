# Unity Editor Help System

这是一个为Unity Editor设计的帮助系统，它允许您创建带有Markdown格式内容的多页面帖子，并支持搜索、页面跳转以及直接导航到Unity项目中的特定对象或文件。

## 功能特性

*   **分级搜索**: 支持按帖子标题或帖子内容进行搜索。
*   **多页面帖子**: 每个帖子可以包含多个页面，方便组织长篇内容。
*   **Markdown支持**: 帖子内容支持Markdown格式，包括粗体、斜体和图片。
*   **交互式按钮**: 可以在帖子页面中添加按钮，实现页面内跳转、导航到Unity对象、导航到项目文件、打开Editor窗口或执行菜单项等功能。
*   **Editor Window**: 整个系统完全集成在Unity Editor中，通过自定义窗口进行操作。

## 安装与使用

1.  **将项目文件导入Unity**: 将 `Assets/Editor/HelpSystem` 文件夹及其内容复制到您的Unity项目的 `Assets/Editor/` 目录下。

2.  **创建HelpSystemData**: 
    *   在Unity Editor中，右键点击Project窗口，选择 `Create -> Help System -> Help System Data`。这将创建一个名为 `HelpSystemData.asset` 的ScriptableObject文件。这个文件将存储您的所有帮助帖子数据。
    *   如果您在打开帮助系统窗口时看到警告信息提示 `HelpSystemData.asset` 未找到，系统会自动为您创建一个。

3.  **打开帮助系统窗口**: 
    *   在Unity Editor菜单栏中，选择 `Window -> Help System`。这将打开帮助系统的主窗口。

4.  **创建和管理帖子**: 
    *   在 `HelpSystemData.asset` 文件的Inspector窗口中，您会看到一个“Add New Post”按钮。点击它可以创建一个新的帖子。
    *   每个帖子都是一个独立的ScriptableObject，您可以在Inspector中编辑其标题和页面内容。
    *   **添加页面**: 在帖子Inspector中，点击“Add Page”按钮可以为当前帖子添加新页面。
    *   **编辑页面内容**: 每个页面都有一个文本区域用于输入Markdown内容。支持以下Markdown语法：
        *   **粗体**: `**文本**` 或 `__文本__`
        *   *斜体*: `*文本*` 或 `_文本_`
        *   **图片**: `![alt text](Assets/Path/To/Your/Image.png)` (请确保图片路径正确，且图片文件存在于Unity项目中)
    *   **添加按钮**: 在每个页面下，您可以点击“Add Button”来添加交互按钮。
        *   **Button Text**: 按钮上显示的文本。
        *   **Command Type**: 选择按钮点击后执行的命令类型：
            *   `GoToPageCommand`: 跳转到当前帖子中的指定页面索引（从0开始）。
            *   `NavigateToUnityObjectCommand`: 导航并高亮Hierarchy或Project窗口中的Unity对象（例如GameObject, Component, Asset）。
            *   `NavigateToProjectFileCommand`: 导航并高亮Project窗口中的特定文件（通过Asset Path）。
            *   `NavigateToSceneObjectCommand`: 导航并高亮特定场景中的GameObject及其组件或字段。需要提供场景名称、GameObject路径、组件类型和字段名称。
            *   `OpenWindowCommand`: 打开指定的Unity Editor窗口（通过窗口的完整类型名称，例如 `UnityEditor.ConsoleWindow`）。
            *   `ExecuteMenuItemCommand`: 执行Unity Editor菜单栏中的某个菜单项（通过菜单项的完整路径，例如 `Window/General/Console`）。

5.  **使用搜索功能**: 
    *   在帮助系统窗口顶部的搜索框中输入关键词。
    *   勾选“Title Only”可以只搜索帖子标题，否则会搜索所有帖子的标题和内容。
    *   点击“Search”按钮或按下回车键执行搜索。
    *   搜索结果会显示在搜索框下方，点击帖子标题即可查看其内容。

6.  **导航和交互**: 
    *   在帖子内容区域，您可以使用“Previous Page”和“Next Page”按钮在页面之间切换。
    *   点击页面中的自定义按钮，将执行您在Inspector中配置的相应命令。

## 示例

假设您有一个名为 `MyGameObject` 的GameObject在场景中，并且它上面有一个 `MyComponent` 组件，您想创建一个按钮点击后导航到这个组件的某个字段。您可以这样配置 `NavigateToSceneObjectCommand`：

*   **Scene Name**: `MyScene` (您的场景名称)
*   **GameObject Path**: `MyGameObject` (如果MyGameObject是根对象，或者 `Parent/MyGameObject` 如果它在Parent下)
*   **Component Type**: `MyComponent` (组件的完整类型名称)
*   **Field Name**: `myFieldName` (组件中您想高亮的字段名称)

## 注意事项

*   Markdown渲染器目前只支持粗体、斜体和图片。图片必须是Unity项目中的Asset，并提供正确的Asset路径。
*   `NavigateToSceneObjectCommand` 中的字段高亮功能需要更复杂的Editor脚本，目前仅能选择到组件，无法直接高亮Inspector中的特定字段。
*   请确保您在创建 `NavigateToUnityObjectCommand` 和 `NavigateToProjectFileCommand` 时，引用的对象或路径是有效的。

## 贡献

如果您有任何改进建议或发现Bug，欢迎提出！


