# Quest 3S 披风飞行系统

## 场景与运行

- 主游戏场景：`Assets/SkyQuest/SkyQuest3S.unity`。现有 `SkyQuestLocomotion` 在 VR 运行时接入披风飞行；桌面场景仍使用原来的键盘移动。
- 独立练习场：`Assets/SkyQuest/CapeFlightTest.unity`。包含起飞平台、浮空平台、空中光源、地面补能、规划挑战、终点、检查点与跌落恢复地板。它在 Build Settings 中默认关闭，测试 Quest 时临时启用它并关闭主场景，测试结束后恢复主场景。
- 如需重建练习场，在 Unity 菜单执行 `SKY > Build cape flight test level`。此命令会重新生成并保存该练习场。
- 使用项目现有 Unity 6000.3.20f1、OpenXR、Meta OpenXR、XR Interaction Toolkit 及 Input System。无需外部美术资源。

## Quest 控制

| 操作 | 输入 |
| --- | --- |
| 起飞 / 空中助推 | 右手 A；每次按下只触发一次，冷却 0.6 秒 |
| 移动 / 空中转向 | 左摇杆；默认按角色朝向计算 |
| 快速转向 | 右摇杆左右 |
| 安全减速 | 按下右摇杆 |
| 第一/第三视角 | 右手 B |
| 先祖物品装备 | 右手握把 |
| 舒适传送模式 | 左手握把 + 右手 B |

输入映射位于 `Assets/SkyQuest/Resources/CapeFlightActions.inputactions`。在 Unity 的 Input Actions 编辑器中修改 `Cape Flight` 的 Boost、Move、Snap Turn、Safe Stop 绑定。手柄断开时不会继续触发新的助推。训练场终点按右摇杆可重开；掉落时会回到最近检查点。

## 组件分工与可调参数

- `SkyQuestLocomotion` 仍是唯一调用 `CharacterController.Move` 的组件；它负责 HMD 跟踪尺寸、碰撞及现有角色外观。
- `CapeFlightController` 读取动作并计算速度和飞行状态；Inspector 可调整起飞、助推、速度上限、滑翔重力、阻力、冷却与舒适模式。默认按身体方向转向，勾选 `headRelativeSteering` 可改为头部方向。
- `CapeEnergySystem` 独立管理 5 格能量、每次成功助推消耗 1 格、恢复、上限及变化事件。滑翔不按时间消耗能量。
- `CapeRechargeReceiver` 将同时覆盖的区域按最高补能速率处理。`CapeRechargeZone` 默认空中光源 2 格/秒、地面区域 1 格/秒；`CapeInstantRechargePickup` 默认一次补 2 格。主场景现有风流和能量球也能为披风补能。
- `CapeEnergyUI` 在左手腕附近显示五格能量与部分填充；`CapeFlightFeedback` 提供轻量音效和手柄震动。面板不会固定在视野中央。
- 主场景的飞行、能量、手腕 UI、反馈及补能组件已直接挂在 `Sky Quest 3S XR Origin` 上，可在 Inspector 调整；运行时 `SkyQuestLocomotion.Start` 负责初始化。若重建主场景，执行 `SKY > Install cape flight in Quest scene` 可重新安装并保存这些组件。练习场 XR Origin 在运行时自动接入。
- 主场景中原有的 `XR Origin (XR Rig)` 已停用，避免与当前角色重复注册 XR 交互器；它仍保留在场景里供查看。

## 已做验证与真机检查

Unity 菜单 `SKY > Validate cape flight system` 会重新执行编辑器检查，并将每项结果写到 `Assets/SkyQuest/CapeFlightValidationReport.txt`。目前通过了动作资源导入、初始能量、起飞扣费、冷却不扣费、墙体不误判落地、稳定落地、零能量拒绝助推、重叠区域取最高补能速率、离开区域停止补能和上限钳制。练习场已在 Unity Play 模式中打开，修正了丢失脚本与提示牌显示，控制台无脚本编译错误。

主场景已在 Unity 编辑器 Play 模式中运行；停用重复 XR Origin 后控制台显示 0 个错误、3 个警告。编辑器没有连接头显，镜头跟踪与控制器输入仍需真机验证。仍需在 Quest 3S 真机依次检查：按 A 与长按 A、冷却内重复按 A、空中转向与十秒滑翔、断开/重连手柄、撞墙、三类补能、终点和恢复流程、坐姿/站姿、手腕面板位置及舒适度。以上真机项目没有标记为已通过。编辑器中的 `StopSubsystems without an initialized manager` 警告来自未连接头显的 XR 初始化；`Area.001` 面光源警告属于原场景灯光配置，需在打包前单独复查。
