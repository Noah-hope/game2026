# Dungeon Survival / 地下城生存

Unity 2D 俯视角生存动作小游戏。选择法师或战士，在 **60 秒**内击败不断涌来的敌人、通过升级选择构筑你的英雄、应对逐渐攀升的危险等级，活到最后获得胜利。

---

## 操作方式

| 按键 | 功能 |
|------|------|
| WASD / 方向键 | 移动 |
| 鼠标左键 | 普攻 |
| E | 释放角色技能 |
| ESC | 暂停 |

---

## 角色介绍

### 法师 Mage
- **定位：** 远程 / 范围控制
- **普攻：** 火球 Fireball
- **技能：** Arcane Rain — 在鼠标位置生成紫色法阵，范围内敌人持续受伤害并减速
- **专属升级：** 火球分裂、奥术雨范围/持续/频率增强、冰霜力场减速、法力涌动冷却缩减

### 战士 Warrior
- **定位：** 近战 / 冲刺爆发
- **普攻：** 剑气 Sword Wave
- **技能：** Dash Slash — 向鼠标方向冲刺斩击，击退路径上的敌人，带残影和刀光特效
- **专属升级：** 剑气穿透/增大、冲刺回血/冷却、地裂爆炸、战斗狂怒

---

## 核心功能

- **60 秒生存目标** — 存活满 60 秒即胜利
- **HUD** — 实时显示 Time / Danger Lv / Kills / HP / EXP / 技能冷却
- **难度动态提升** — Danger Lv 0→3 随生存时间自动升级，敌人更强更密
- **精英怪** — 30 秒后出现，体型更大、属性更强、经验双倍
- **远程射手** — 15 秒后出现，保持距离射击玩家
- **角色专属升级** — 每次升级三选一卡牌，Mage/Warrior 各有特色构筑路线
- **升级卡牌** — 横向三选一，按类型 (Common/Mage/Warrior/Rare) 不同颜色边框
- **暂停菜单** — ESC 暂停，支持继续/重新开始/返回主菜单
- **结算评价** — Game Over 或 Victory 后显示生存时间、击杀数、等级、危险等级和本局评价
- **Camera Shake** — 战士冲刺时的震屏反馈

---

## 项目结构

```
Assets/Scripts/
├── Core/          游戏主流程、数据、场景加载
│   ├── GameManager.cs
│   ├── GameData.cs
│   ├── CharacterStats.cs
│   ├── GameUtility.cs
│   └── SceneLoader.cs
├── Player/        玩家控制、血量
│   ├── PlayerController.cs
│   ├── PlayerHealth.cs
│   └── PlayerHealthBar.cs
├── Enemy/         敌人、刷怪、远程怪
│   ├── EnemyController.cs
│   ├── EnemySpawner.cs
│   └── EnemyHealthBar.cs
├── Combat/        投射物、技能区域、特效
│   ├── Projectile.cs
│   ├── ArcaneRainArea.cs
│   ├── CombatEffectFactory.cs
│   ├── DamageTextEffect.cs
│   └── TemporaryEffect.cs
├── UI/            主菜单、角色选择、HUD、结算
│   ├── MainMenuUI.cs
│   ├── CharacterSelectUI.cs
│   └── GameUI.cs
└── Upgrade/       升级选项和升级管理
    ├── UpgradeOption.cs
    └── UpgradeManager.cs
```

---

## 如何运行

1. 用 **Unity Hub** 打开项目根目录
2. 建议使用 Unity **2021.3+** 或更高版本
3. 进入 `Assets/Scenes/MainMenuScene.unity` 点击 Play
4. 或从 Build Settings 设置场景顺序：MainMenuScene → CharacterSelectScene → GameScene

---

## 升级一览

### 通用升级
普攻伤害 +10 · 技能伤害 +5 · 最大生命 +20 · 移速 +10% · 普攻冷却 -10% · 生命恢复 +40 · 技能冷却 -10%

### 法师 Mage 专属
| 升级 | 效果 |
|------|------|
| Fireball Split (Rare) | 一次发射 3 个火球 |
| Arcane Rain Bigger | 奥术雨范围增大 |
| Arcane Rain Longer | 奥术雨持续时间延长 |
| Arcane Rain Faster | 奥术雨伤害频率提高 |
| Frost Field | 奥术雨减速效果增强 |
| Mana Surge | 技能冷却缩减 18% |

### 战士 Warrior 专属
| 升级 | 效果 |
|------|------|
| Sword Wave Pierce (Rare) | 剑气穿透敌人 |
| Sword Wave Bigger | 剑气体型增大 |
| Dash Slash Heal | 冲刺命中回血 |
| Dash Slash Cooldown | 冲刺冷却缩短 |
| Earth Splitter | 冲刺终点爆炸 |
| Battle Frenzy | 冲刺命中后短时间普攻加速 |

---

## 后续优化方向

- 增加音效和背景音乐
- 增加 Boss 战
- 增加更多地图房间
- 增加更多升级流派
- 增加角色动画和粒子特效
- 增加排行榜和存档系统
- 增加更多角色选择
