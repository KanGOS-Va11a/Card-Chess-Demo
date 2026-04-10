using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Cards;
using CardChessDemo.Battle.Equipment;
using CardChessDemo.Battle.Shared;
using RuntimeEquipmentDefinition = CardChessDemo.Battle.Equipment.EquipmentDefinition;

namespace CardChessDemo.Map;

public partial class SystemFeatureLabController : CanvasLayer
{
	private const string TalentBackgroundTexturePath = "res://Assets/Background/94180512_p2_master1200.jpg";
	[Export] public NodePath PlayerPath { get; set; } = new("../Player");

	private Control _panelRoot = null!;
	private Label _hintLabel = null!;
	private Label _statusLabel = null!;
	private TabContainer _tabs = null!;

	private RichTextLabel _statusOverviewText = null!;
	private ItemList _statusEquipmentSlotList = null!;
	private ItemList _statusEquipmentCandidateList = null!;
	private RichTextLabel _statusEquipmentDetailText = null!;
	private Button _statusEquipButton = null!;
	private Button _statusUnequipButton = null!;

	private RichTextLabel _inventoryText = null!;

	private Label _masteryLabel = null!;
	private Control _talentBody = null!;
	private ScrollContainer _talentTreeScroll = null!;
	private Control _talentTreeCanvas = null!;
	private Control _talentDragSurface = null!;
	private Control? _talentBackground;
	private TextureRect? _talentBodyBackground;
	private Node _talentLineLayer = null!;
	private TalentTreeLineCanvas? _talentLineCanvas;
	private Button _cardTreeLabel = null!;
	private Button _roleTreeLabel = null!;
	private ColorRect _talentDetailDim = null!;
	private PanelContainer _talentDetailPanel = null!;
	private Label _talentDetailTitleLabel = null!;
	private RichTextLabel _talentDetail = null!;
	private Button _unlockTalentButton = null!;
	private Button _refundTalentButton = null!;
	private readonly Dictionary<string, Button> _talentButtons = new(StringComparer.Ordinal);
	private readonly HashSet<string> _purchasedTalentIds = new(StringComparer.Ordinal);
	private string _selectedTalentId = string.Empty;
	private bool _isDraggingTalentTree;
	private Vector2 _lastTalentDragPosition = Vector2.Zero;
	private float _talentTreeZoom = 1.0f;
	private Vector2 _talentViewVelocity = Vector2.Zero;
	private float _talentViewHoldTime;
	private Rect2 _talentContentBounds = new Rect2(0, 0, 1, 1);
	private int _pendingTalentViewResetFrames;

	private ItemList _cardCodexList = null!;
	private RichTextLabel _cardCodexDetail = null!;
	private ItemList _enemyCodexList = null!;
	private RichTextLabel _enemyCodexDetail = null!;

	private Label _deckPoolSummaryLabel = null!;
	private Label _deckSummaryLabel = null!;
	private ItemList _availableList = null!;
	private ItemList _deckList = null!;
	private RichTextLabel _deckDetailText = null!;
	private RichTextLabel _deckValidationText = null!;
	private Button _deckAddButton = null!;
	private Button _deckRemoveButton = null!;
	private Button _deckSaveButton = null!;
	private Button _deckResetButton = null!;
	private Button _deckStarterButton = null!;

	private Button _seedInventoryButton = null!;
	private Button _clearInventoryButton = null!;

	private GlobalGameSession? _session;
	private BattleCardLibrary? _cardLibrary;
	private BattleDeckBuildRules? _deckRules;
	private BattleDeckConstructionService? _constructionService;
	private BattleCardTemplate[] _availableTemplates = Array.Empty<BattleCardTemplate>();
	private BattleCardTemplate[] _codexTemplates = Array.Empty<BattleCardTemplate>();
	private RuntimeEquipmentDefinition[] _visibleEquipmentCandidates = Array.Empty<RuntimeEquipmentDefinition>();
	private List<string> _workingDeck = new();
	private int _baseMasteryPoints = 6;
	private string _selectedEquipmentSlotId = EquipmentSlotIds.Weapon;
	private const int TalentTabIndex = 2;

	private static readonly string[] EquipmentSlotOrder = EquipmentSlotIds.All;

#if false

		private readonly EquipmentDefinition[] _equipmentDefinitions =
	{
		new("equip_magnetic_scabbard", "磁锁刀鞘", "weapon", "近战用武器挂件，提供稳定的基础攻击加成。", "攻击 +1"),
		new("equip_arc_pipe", "电弧金属管", "weapon", "荒川早期强化的临时武器，强调攻击与射程。", "攻击 +1 / 射程 +1"),
		new("equip_old_coat", "旧大衣", "armor", "主角初始护具，提供基础生存空间。", "生命 +3 / 减伤 +5%"),
		new("equip_phase_boots", "相位短靴", "armor", "偏机动取向的护具。", "移动 +1"),
		new("equip_red_scarf", "红色方巾", "accessory", "主角身份符号，目前作为占位饰品。", "剧情饰品"),
		new("equip_target_lens", "校准镜片", "accessory", "远程倾向配件，以生命代价换取更稳定的火力。", "射程 +1 / 生命 -2"),
		new("equip_archive_probe", "档案探针", "accessory", "学习流辅助装置，目前作为占位饰品。", "学习辅助"),
		new("equip_parallel_battery", "并联电池组", "armor", "高负载供能组件，目前作为占位装备。", "供能占位"),
		new("equip_forbidden_patch", "禁区补丁", "accessory", "高风险实验型配件，目前作为占位饰品。", "风险占位"),
		new("equip_insulated_cloak", "绝缘披肩", "armor", "后期抗异常护具，目前作为占位装备。", "绝缘占位"),
	};

		private readonly TalentNode[] _talents =
	{
		new("talent_melee_root", "破门本能", TalentTreeGroup.Card, "melee", 1, "近战分支起点，解锁冲撞。", new Vector2(96, 344), Array.Empty<string>(), new[] { "talent_melee_root" }, new[] { "melee" }, new[] { "card_ram" }),
		new("talent_melee_counter", "死斗姿态", TalentTreeGroup.Card, "melee", 1, "贴身反击路线，解锁架势。", new Vector2(0, 252), new[] { "talent_melee_root" }, new[] { "talent_melee_counter" }, unlockedCardIds: new[] { "card_stance" }),
		new("talent_melee_counter_plus", "贴身反击", TalentTreeGroup.Card, "melee", 1, "进一步强化近战反打收益。", new Vector2(0, 138), new[] { "talent_melee_counter" }, new[] { "talent_melee_counter_plus" }),
		new("talent_melee_blow", "压舱重击", TalentTreeGroup.Card, "melee", 1, "破盾与单点击杀路线，解锁重击。", new Vector2(0, 454), new[] { "talent_melee_root" }, new[] { "talent_melee_blow" }, unlockedCardIds: new[] { "card_heavy_blow" }),
		new("talent_melee_blow_plus", "盾裂处决", TalentTreeGroup.Card, "melee", 1, "继续强化对护盾目标的压制能力。", new Vector2(0, 568), new[] { "talent_melee_blow" }, new[] { "talent_melee_blow_plus" }),
		new("talent_melee_core", "劫掠冲动", TalentTreeGroup.Card, "melee", 1, "近战专精节点，正式打开高阶近战牌资格。", new Vector2(174, 470), new[] { "talent_melee_counter", "talent_melee_blow" }, new[] { "talent_melee_core", "melee.specialized" }),

		new("talent_ranged_root", "校准火线", TalentTreeGroup.Card, "ranged", 1, "远程分支起点，解锁快枪。", new Vector2(560, 112), Array.Empty<string>(), new[] { "talent_ranged_root" }, new[] { "ranged" }, new[] { "card_quick_shot" }),
		new("talent_ranged_control", "弹道干预", TalentTreeGroup.Card, "ranged", 1, "击退与压线控制路线，解锁震荡射击。", new Vector2(744, 8), new[] { "talent_ranged_root" }, new[] { "talent_ranged_control" }, unlockedCardIds: new[] { "card_concussion_shot" }),
		new("talent_ranged_control_plus", "制退射线", TalentTreeGroup.Card, "ranged", 1, "强化远程控制收益。", new Vector2(952, 0), new[] { "talent_ranged_control" }, new[] { "talent_ranged_control_plus" }),
		new("talent_ranged_pressure", "制压协议", TalentTreeGroup.Card, "ranged", 1, "准备与压制路线，解锁戒备。", new Vector2(744, 206), new[] { "talent_ranged_root" }, new[] { "talent_ranged_pressure" }, unlockedCardIds: new[] { "card_alert" }),
		new("talent_ranged_signature", "火线改写", TalentTreeGroup.Card, "ranged", 1, "远程招牌节点，解锁拔枪。", new Vector2(954, 246), new[] { "talent_ranged_pressure" }, new[] { "talent_ranged_signature" }, unlockedCardIds: new[] { "draw_revolver" }),
		new("talent_ranged_core", "先手判读", TalentTreeGroup.Card, "ranged", 1, "远程专精节点，打开高阶远程牌资格。", new Vector2(652, 316), new[] { "talent_ranged_control", "talent_ranged_pressure" }, new[] { "talent_ranged_core", "ranged.specialized" }),

		new("talent_flex_root", "荒川同调", TalentTreeGroup.Card, "flex", 1, "创造分支起点，解锁电弧泄露。", new Vector2(560, 492), Array.Empty<string>(), new[] { "talent_flex_root" }, new[] { "flex" }, new[] { "card_arc_leak" }),
		new("talent_flex_field", "现场干预", TalentTreeGroup.Card, "flex", 1, "障碍物互动路线，解锁风化。", new Vector2(744, 618), new[] { "talent_flex_root" }, new[] { "talent_flex_field" }, unlockedCardIds: new[] { "card_weathering" }),
		new("talent_flex_field_plus", "障碍拆解", TalentTreeGroup.Card, "flex", 1, "强化障碍处理与拆除能力。", new Vector2(952, 736), new[] { "talent_flex_field" }, new[] { "talent_flex_field_plus" }),
		new("talent_flex_learning", "战斗记录协议", TalentTreeGroup.Card, "flex", 1, "学习机制入口，解锁学习。", new Vector2(744, 420), new[] { "talent_flex_root" }, new[] { "talent_flex_learning" }, unlockedCardIds: new[] { "card_learning" }),
		new("talent_flex_learning_plus", "系统劫持者", TalentTreeGroup.Card, "flex", 1, "强化学习与异常控制收益。", new Vector2(954, 378), new[] { "talent_flex_learning" }, new[] { "talent_flex_learning_plus" }),
		new("talent_flex_core", "异常演算", TalentTreeGroup.Card, "flex", 1, "创造专精节点，打开高阶创造牌资格。", new Vector2(966, 544), new[] { "talent_flex_field", "talent_flex_learning" }, new[] { "talent_flex_core", "flex.specialized" }),

		new("talent_role_atk", "应激输出", TalentTreeGroup.Role, "role", 1, "基础输出能力，普通攻击 +1。", new Vector2(1440, 1120), Array.Empty<string>(), new[] { "talent_role_atk", "stat.attack_bonus.1" }),
		new("talent_role_hp", "生还者框架", TalentTreeGroup.Role, "role", 1, "基础生存能力，最大生命 +4。", new Vector2(1660, 1120), Array.Empty<string>(), new[] { "talent_role_hp", "stat.max_hp_bonus.4" }),
		new("talent_role_move", "趋前步伐", TalentTreeGroup.Role, "role", 1, "基础机动能力，移动力 +1。", new Vector2(1880, 1120), Array.Empty<string>(), new[] { "talent_role_move", "stat.move_bonus.1" }),
		new("talent_role_defense", "防御校准", TalentTreeGroup.Role, "role", 1, "从生存路线延伸出的减伤节点。", new Vector2(1660, 946), new[] { "talent_role_hp" }, new[] { "talent_role_defense", "stat.defense_reduction_bonus.10" }),
		new("talent_role_guard", "守势支架", TalentTreeGroup.Role, "role", 1, "继续强化防御动作的额外护盾。", new Vector2(1660, 772), new[] { "talent_role_defense" }, new[] { "talent_role_guard", "stat.defense_shield_bonus.2" }),
		new("talent_role_deck", "构筑缓存", TalentTreeGroup.Role, "role", 1, "从机动路线延伸出的构筑承载节点。", new Vector2(1880, 946), new[] { "talent_role_move" }, new[] { "talent_role_deck" }, deckPointBudgetBonus: 2),
		new("talent_role_copies", "牌组编纂", TalentTreeGroup.Role, "role", 1, "强化构筑承载，同名上限 +1。", new Vector2(1880, 772), new[] { "talent_role_deck" }, new[] { "talent_role_copies" }, deckMaxCopiesPerCardBonus: 1),
		new("talent_role_core", "基础动作统合", TalentTreeGroup.Role, "role", 1, "把输出、防御与构筑支撑收束为角色树核心。", new Vector2(1718, 560), new[] { "talent_role_atk", "talent_role_guard", "talent_role_copies" }, new[] { "talent_role_core" }),
	};
		private readonly EnemyCodexEntry[] _enemyCodexEntries =
	{
		new("grunt_debug", "训练敌人", "基础近战测试敌人。", true, "初始开放"),
		new("pirate_brute_elite", "搭船客重压者", "擅长贴身压迫与重击。", false, "在精英战中完成学习后解锁图鉴", "card_pressure_breach"),
		new("alliance_hunter_elite", "联盟猎手标定员", "擅长标记与远程收割。", false, "在精英战中完成学习后解锁图鉴", "card_hunter_mark"),
		new("boss_port_authority", "空港治安主机", "Boss 级压制型敌人。", false, "击败章节 Boss 并完成学习后解锁图鉴", "card_overclock_beam"),
	};

#endif

	private readonly EquipmentDefinition[] _equipmentDefinitions =
	{
		new("equip_magnetic_scabbard", "磁锁刀鞘", "weapon", "近战过渡武器挂件，提供稳定的基础攻击加成。", "攻击 +1"),
		new("equip_arc_pipe", "电弧金属管", "weapon", "荒川早期强化的临时武器，强调攻击与射程。", "攻击 +1 / 射程 +1"),
		new("equip_old_coat", "旧大衣", "armor", "主角初始护具，提供基础生存空间。", "生命 +3 / 减伤 +5%"),
		new("equip_phase_boots", "相位短靴", "armor", "偏机动取向的护具。", "移动 +1"),
		new("equip_red_scarf", "红色方巾", "accessory", "主角身份符号，目前作为占位饰品。", "剧情饰品"),
		new("equip_target_lens", "校准镜片", "accessory", "远程倾向配件，以生命代价换取更稳定的火力。", "射程 +1 / 生命 -2"),
		new("equip_archive_probe", "档案探针", "accessory", "学习流辅助装备，目前作为占位饰品。", "学习辅助"),
		new("equip_parallel_battery", "并联电池组", "armor", "高负载供能组件，目前作为占位装备。", "供能占位"),
		new("equip_forbidden_patch", "禁区补丁", "accessory", "高风险实验型配件，目前作为占位饰品。", "风险占位"),
		new("equip_insulated_cloak", "绝缘披肩", "armor", "后期抗异常护具，目前作为占位装备。", "绝缘占位"),
	};

	private readonly TalentNode[] _talents =
	{
		new("talent_melee_root", "破门本能", TalentTreeGroup.Card, "melee", 1, "近战分支起点，解锁冲撞。", new Vector2(96, 344), Array.Empty<string>(), new[] { "talent_melee_root" }, new[] { "melee" }, new[] { "card_ram" }),
		new("talent_melee_counter", "死斗姿态", TalentTreeGroup.Card, "melee", 1, "贴身反击路线，解锁架势。", new Vector2(0, 252), new[] { "talent_melee_root" }, new[] { "talent_melee_counter" }, unlockedCardIds: new[] { "card_stance" }),
		new("talent_melee_counter_plus", "贴身反击", TalentTreeGroup.Card, "melee", 1, "进一步强化近战反打收益。", new Vector2(0, 138), new[] { "talent_melee_counter" }, new[] { "talent_melee_counter_plus" }),
		new("talent_melee_blow", "压舱重击", TalentTreeGroup.Card, "melee", 1, "破盾与单点击杀路线，解锁重击。", new Vector2(0, 454), new[] { "talent_melee_root" }, new[] { "talent_melee_blow" }, unlockedCardIds: new[] { "card_heavy_blow" }),
		new("talent_melee_blow_plus", "盾裂处决", TalentTreeGroup.Card, "melee", 1, "继续强化对护盾目标的压制能力。", new Vector2(0, 568), new[] { "talent_melee_blow" }, new[] { "talent_melee_blow_plus" }),
		new("talent_melee_core", "劫掠冲动", TalentTreeGroup.Card, "melee", 1, "近战专精节点，正式打开高阶近战牌资格。", new Vector2(174, 470), new[] { "talent_melee_counter", "talent_melee_blow" }, new[] { "talent_melee_core", "melee.specialized" }),

		new("talent_ranged_root", "校准火线", TalentTreeGroup.Card, "ranged", 1, "远程分支起点，解锁快枪。", new Vector2(560, 112), Array.Empty<string>(), new[] { "talent_ranged_root" }, new[] { "ranged" }, new[] { "card_quick_shot" }),
		new("talent_ranged_control", "弹道干预", TalentTreeGroup.Card, "ranged", 1, "击退与压线控制路线，解锁震荡射击。", new Vector2(744, 8), new[] { "talent_ranged_root" }, new[] { "talent_ranged_control" }, unlockedCardIds: new[] { "card_concussion_shot" }),
		new("talent_ranged_control_plus", "制退射线", TalentTreeGroup.Card, "ranged", 1, "强化远程控制收益。", new Vector2(952, 0), new[] { "talent_ranged_control" }, new[] { "talent_ranged_control_plus" }),
		new("talent_ranged_pressure", "压制协议", TalentTreeGroup.Card, "ranged", 1, "准备与压制路线，解锁戒备。", new Vector2(744, 206), new[] { "talent_ranged_root" }, new[] { "talent_ranged_pressure" }, unlockedCardIds: new[] { "card_alert" }),
		new("talent_ranged_signature", "火线改写", TalentTreeGroup.Card, "ranged", 1, "远程招牌节点，解锁拔枪。", new Vector2(954, 246), new[] { "talent_ranged_pressure" }, new[] { "talent_ranged_signature" }, unlockedCardIds: new[] { "draw_revolver" }),
		new("talent_ranged_core", "先手判读", TalentTreeGroup.Card, "ranged", 1, "远程专精节点，打开高阶远程牌资格。", new Vector2(652, 316), new[] { "talent_ranged_control", "talent_ranged_pressure" }, new[] { "talent_ranged_core", "ranged.specialized" }),

		new("talent_flex_root", "荒川同调", TalentTreeGroup.Card, "flex", 1, "创造分支起点，解锁电弧泄露。", new Vector2(560, 492), Array.Empty<string>(), new[] { "talent_flex_root" }, new[] { "flex" }, new[] { "card_arc_leak" }),
		new("talent_flex_field", "现场干预", TalentTreeGroup.Card, "flex", 1, "障碍与地形处理路线，解锁风化。", new Vector2(744, 618), new[] { "talent_flex_root" }, new[] { "talent_flex_field" }, unlockedCardIds: new[] { "card_weathering" }),
		new("talent_flex_field_plus", "障碍拆解", TalentTreeGroup.Card, "flex", 1, "强化障碍处理与拆除收益。", new Vector2(952, 736), new[] { "talent_flex_field" }, new[] { "talent_flex_field_plus" }),
		new("talent_flex_learning", "战斗记录协议", TalentTreeGroup.Card, "flex", 1, "学习机制入口，解锁学习。", new Vector2(744, 420), new[] { "talent_flex_root" }, new[] { "talent_flex_learning" }, unlockedCardIds: new[] { "card_learning" }),
		new("talent_flex_learning_plus", "系统劫持者", TalentTreeGroup.Card, "flex", 1, "强化学习与异常控制收益。", new Vector2(954, 378), new[] { "talent_flex_learning" }, new[] { "talent_flex_learning_plus" }),
		new("talent_flex_core", "异常演算", TalentTreeGroup.Card, "flex", 1, "创造专精节点，打开高阶创造牌资格。", new Vector2(966, 544), new[] { "talent_flex_field", "talent_flex_learning" }, new[] { "talent_flex_core", "flex.specialized" }),

		new("talent_role_atk", "应激输出", TalentTreeGroup.Role, "role", 1, "基础输出能力，普通攻击 +1。", new Vector2(1440, 1120), Array.Empty<string>(), new[] { "talent_role_atk", "stat.attack_bonus.1" }),
		new("talent_role_hp", "生还者框架", TalentTreeGroup.Role, "role", 1, "基础生存能力，最大生命 +4。", new Vector2(1660, 1120), Array.Empty<string>(), new[] { "talent_role_hp", "stat.max_hp_bonus.4" }),
		new("talent_role_move", "趋前步伐", TalentTreeGroup.Role, "role", 1, "基础机动能力，移动力 +1。", new Vector2(1880, 1120), Array.Empty<string>(), new[] { "talent_role_move", "stat.move_bonus.1" }),
		new("talent_role_defense", "防御校准", TalentTreeGroup.Role, "role", 1, "从生存路线延伸出的减伤节点。", new Vector2(1660, 946), new[] { "talent_role_hp" }, new[] { "talent_role_defense", "stat.defense_reduction_bonus.10" }),
		new("talent_role_guard", "守势支架", TalentTreeGroup.Role, "role", 1, "继续强化防御动作的额外护盾。", new Vector2(1660, 772), new[] { "talent_role_defense" }, new[] { "talent_role_guard", "stat.defense_shield_bonus.2" }),
		new("talent_role_deck", "构筑缓存", TalentTreeGroup.Role, "role", 1, "从机动路线延伸出的构筑承载节点。", new Vector2(1880, 946), new[] { "talent_role_move" }, new[] { "talent_role_deck" }, deckPointBudgetBonus: 2),
		new("talent_role_copies", "牌组编纂", TalentTreeGroup.Role, "role", 1, "强化构筑承载，同名上限 +1。", new Vector2(1880, 772), new[] { "talent_role_deck" }, new[] { "talent_role_copies" }, deckMaxCopiesPerCardBonus: 1),
		new("talent_role_core", "基础动作统合", TalentTreeGroup.Role, "role", 1, "把输出、防御与构筑支持收束为角色树核心。", new Vector2(1718, 560), new[] { "talent_role_atk", "talent_role_guard", "talent_role_copies" }, new[] { "talent_role_core" }),
	};

	private readonly EnemyCodexEntry[] _enemyCodexEntries =
	{
		new("grunt_debug", "训练敌人", "基础近战测试敌人。", true, "初始开放"),
		new("pirate_brute_elite", "掠船客重压者", "擅长贴身压迫与重击。", false, "在精英战中完成学习后解锁图鉴", "card_pressure_breach"),
		new("alliance_hunter_elite", "联盟猎手标定员", "擅长标记与远程收割。", false, "在精英战中完成学习后解锁图鉴", "card_hunter_mark"),
		new("boss_port_authority", "空港治安主机", "Boss 级压制型敌人。", false, "击败章节 Boss 并完成学习后解锁图鉴", "card_overclock_beam"),
	};

	public override void _Ready()
	{
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		_cardLibrary = GD.Load<BattleCardLibrary>("res://Resources/Battle/Cards/DefaultBattleCardLibrary.tres");
		_deckRules = GD.Load<BattleDeckBuildRules>("res://Resources/Battle/Cards/DefaultBattleDeckBuildRules.tres");
		if (_session != null && _cardLibrary != null && _deckRules != null)
		{
			_session.EnsureDeckBuildInitialized(_cardLibrary);
			_constructionService = new BattleDeckConstructionService(_cardLibrary, _deckRules);
			ApplyDebugCardUnlocks();
		}

		_panelRoot = GetNode<Control>("PanelRoot");
		_hintLabel = GetNode<Label>("HintLabel");
		_statusLabel = GetNode<Label>("StatusLabel");
		_tabs = GetNode<TabContainer>("PanelRoot/Window/Margin/Root/Tabs");
		_statusOverviewText = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/StatusColumn/StatusText");
		_statusEquipmentSlotList = GetNode<ItemList>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/SlotList");
		_statusEquipmentCandidateList = GetNode<ItemList>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/CandidateList");
		_statusEquipmentDetailText = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/EquipmentDetailPanel/EquipmentDetailText");
		_statusEquipButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/ActionRow/EquipButton");
		_statusUnequipButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/ActionRow/UnequipButton");
		_inventoryText = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/InventoryTab/InventoryText");
		_talentBody = GetNode<Control>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body");
		_masteryLabel = GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/MasteryFixedLabel");
		_talentTreeScroll = GetNode<ScrollContainer>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/TalentTreeScroll");
		_talentTreeCanvas = GetNode<Control>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/TalentTreeScroll/TalentTreeCanvas");
		_talentBackground = GetNodeOrNull<Control>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/TalentTreeScroll/TalentTreeCanvas/TalentBackground");
		_talentDragSurface = GetNode<Control>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/TalentTreeScroll/TalentTreeCanvas/TalentDragSurface");
		_talentLineLayer = GetNode("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/TalentTreeScroll/TalentTreeCanvas/TalentLineLayer");
		_talentLineCanvas = _talentLineLayer as TalentTreeLineCanvas;
		_cardTreeLabel = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/TalentTreeScroll/TalentTreeCanvas/CardTreeLabel");
		_roleTreeLabel = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/TalentTreeScroll/TalentTreeCanvas/RoleTreeLabel");
		_talentDetailDim = GetNode<ColorRect>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailDim");
		_talentDetailPanel = GetNode<PanelContainer>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel");
		_talentDetailTitleLabel = GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel/Margin/Content/TitleLabel");
		_talentDetail = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel/Margin/Content/DetailText");
		_unlockTalentButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel/Margin/Content/Footer/UnlockTalentButton");
		_refundTalentButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel/Margin/Content/Footer/RefundTalentButton");
		_cardCodexList = GetNode<ItemList>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/ListColumn/CardList");
		_cardCodexDetail = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/DetailPanel/DetailText");
		_enemyCodexList = GetNode<ItemList>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/ListColumn/EnemyList");
		_enemyCodexDetail = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/DetailPanel/DetailText");
		_deckPoolSummaryLabel = GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Header/PoolSummaryLabel");
		_deckSummaryLabel = GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Header/DeckSummaryLabel");
		_availableList = GetNode<ItemList>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/AvailableColumn/AvailableList");
		_deckList = GetNode<ItemList>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/DeckColumn/DeckList");
		_deckDetailText = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/DetailPanel/DetailText");
		_deckValidationText = GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/ValidationPanel/ValidationText");
		_deckAddButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/ControlColumn/AddButton");
		_deckRemoveButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/ControlColumn/RemoveButton");
		_deckSaveButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Footer/SaveButton");
		_deckResetButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Footer/ResetButton");
		_deckStarterButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Footer/StarterButton");
		_seedInventoryButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/InventoryTab/Footer/SeedInventoryButton");
		_clearInventoryButton = GetNode<Button>("PanelRoot/Window/Margin/Root/Tabs/InventoryTab/Footer/ClearInventoryButton");

		_tabs.SetTabTitle(0, "鑳屽寘");
		_tabs.SetTabTitle(1, "澶╄祴");
		_tabs.SetTabTitle(2, "鍥鹃壌");
		_tabs.SetTabTitle(3, "鏋勭瓚");
		_tabs.SetTabTitle(0, "瑙掕壊");
		_tabs.SetTabTitle(1, "鑳屽寘");
		_tabs.SetTabTitle(2, "澶╄祴");
		_tabs.SetTabTitle(3, "鍥鹃壌");
		_tabs.SetTabTitle(4, "鏋勭瓚");
		_tabs.SetTabTitle(0, "状态");
		_tabs.SetTabTitle(1, "背包");
		_tabs.SetTabTitle(2, "天赋");
		_tabs.SetTabTitle(3, "图鉴");
		_tabs.SetTabTitle(4, "构筑");
		ApplyVisibleUiOverrides();
		ApplyReadableUiTextOverrides();
		_panelRoot.Visible = false;
		_talentDetailPanel.Visible = false;

		_statusEquipmentSlotList.ItemSelected += OnEquipmentSlotSelected;
		_statusEquipmentCandidateList.ItemSelected += OnEquipmentCandidateSelected;
		_statusEquipButton.Pressed += OnEquipButtonPressed;
		_statusUnequipButton.Pressed += OnUnequipButtonPressed;
		_unlockTalentButton.Pressed += OnUnlockTalentPressed;
		_refundTalentButton.Pressed += OnRefundTalentPressed;
		_seedInventoryButton.Pressed += OnSeedInventoryPressed;
		_clearInventoryButton.Pressed += OnClearInventoryPressed;
		_talentDetailDim.GuiInput += OnTalentDetailDimGuiInput;
		_availableList.ItemSelected += OnAvailableSelected;
		_deckList.ItemSelected += OnDeckSelected;
		_cardCodexList.ItemSelected += OnCardCodexSelected;
		_enemyCodexList.ItemSelected += OnEnemyCodexSelected;
		_tabs.TabChanged += OnTabsTabChanged;
		_deckAddButton.Pressed += OnDeckAddPressed;
		_deckRemoveButton.Pressed += OnDeckRemovePressed;
		_deckSaveButton.Pressed += OnDeckSavePressed;
		_deckResetButton.Pressed += OnDeckResetPressed;
		_deckStarterButton.Pressed += OnDeckStarterPressed;
		_cardTreeLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
		_roleTreeLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
		_masteryLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
		_masteryLabel.ZIndex = 200;
		_talentBody.MoveChild(_masteryLabel, _talentBody.GetChildCount() - 1);
		_talentTreeScroll.GuiInput += OnTalentTreeGuiInput;
		_talentTreeCanvas.GuiInput += OnTalentTreeGuiInput;
		_talentDragSurface.GuiInput += OnTalentTreeGuiInput;
		HideTalentTreeScrollBars();
		_talentTreeCanvas.CustomMinimumSize = new Vector2(3200.0f, 2200.0f);
		SyncTalentCanvasOverlaySizes();
		ConfigureTalentBackground();
		ApplyTreeRootStyle(_cardTreeLabel, new Color(0.14f, 0.24f, 0.34f, 0.98f));
		ApplyTreeRootStyle(_roleTreeLabel, new Color(0.24f, 0.18f, 0.12f, 0.98f));

		BuildTalentButtons();
		BuildCodexSource();
		SeedSessionForTesting();
		LoadPurchasedTalentsFromSession();
		RecomputeSessionProgression();
		LoadWorkingDeckFromSession();
		RefreshAll();
	}

	public override void _Process(double delta)
	{
		UpdateStatusHint();
		ApplyReadableStatusHint();
		if (_panelRoot != null && _panelRoot.Visible && _tabs != null && _tabs.CurrentTab == TalentTabIndex)
		{
			_masteryLabel.Text = $"\u5269\u4F59\u4E13\u7CBE\u70B9 {GetAvailablePoints()} | WASD \u79FB\u52A8\u89C6\u56FE";
			if (_pendingTalentViewResetFrames > 0)
			{
				_pendingTalentViewResetFrames -= 1;
				if (_pendingTalentViewResetFrames == 0)
				{
					ResetTalentTreeView();
				}
			}
		}
		UpdateTalentTreeKeyboardPan((float)delta);
	}

	public override void _Input(InputEvent @event)
	{
		if (!IsTalentTabActive() || !_isDraggingTalentTree)
		{
			return;
		}

		if (@event is InputEventMouseMotion mouseMotion)
		{
			Vector2 delta = mouseMotion.GlobalPosition - _lastTalentDragPosition;
			ScrollTalentTreeBy(delta);
			_lastTalentDragPosition = mouseMotion.GlobalPosition;
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event is InputEventMouseButton mouseButton
			&& mouseButton.ButtonIndex == MouseButton.Left
			&& !mouseButton.Pressed)
		{
			_isDraggingTalentTree = false;
			GetViewport().SetInputAsHandled();
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo || keyEvent.Keycode != Key.C)
		{
			return;
		}

		_panelRoot.Visible = !_panelRoot.Visible;
		_hintLabel.Visible = !_panelRoot.Visible;
		SetPlayerInputEnabled(!_panelRoot.Visible);
		if (_panelRoot.Visible)
		{
			RefreshAll();
			ApplyVisibleUiOverrides();
			ApplyReadableUiTextOverrides();
			ScheduleTalentViewReset();
		}

		GetViewport().SetInputAsHandled();
	}

	private void SetPlayerInputEnabled(bool enabled)
	{
		Node? player = PlayerPath.IsEmpty ? null : GetNodeOrNull(PlayerPath);
		if (player == null)
		{
			return;
		}

		player.SetPhysicsProcess(enabled);
		player.SetProcess(enabled);
		player.SetProcessInput(enabled);
		player.SetProcessUnhandledInput(enabled);
	}

	private void UpdateStatusHint()
	{
		{
		Node? readablePlayerNode = PlayerPath.IsEmpty ? null : GetNodeOrNull(PlayerPath);
		Area2D? readableInteractionArea = readablePlayerNode?.GetNodeOrNull<Area2D>("InteractionArea");
		if (readableInteractionArea == null)
		{
			_statusLabel.Text = "\u672A\u627E\u5230\u73A9\u5BB6\u4EA4\u4E92\u8303\u56F4";
			return;
		}

		foreach (Area2D area in readableInteractionArea.GetOverlappingAreas())
		{
			if (area.GetParent() is IInteractable interactable && area.GetParent() is Node ownerNode)
			{
				Player? readablePlayer = readablePlayerNode as Player ?? GetNodeOrNull<Player>(PlayerPath);
				_statusLabel.Text = $"\u53EF\u4EA4\u4E92\u5BF9\u8C61: {ownerNode.Name} \u00B7 {interactable.GetInteractText(readablePlayer!)}";
				return;
			}
		}

		_statusLabel.Text = _panelRoot.Visible
			? "\u7CFB\u7EDF\u9762\u677F\u5DF2\u6253\u5F00\uFF0C\u6309 C \u5173\u95ED"
			: "\u9760\u8FD1\u654C\u4EBA\u540E\u6309 E \u8FDB\u5165\u6218\u6597";
		return;
		}

		Node? playerNode = PlayerPath.IsEmpty ? null : GetNodeOrNull(PlayerPath);
		Area2D? interactionArea = playerNode?.GetNodeOrNull<Area2D>("InteractionArea");
		if (interactionArea == null)
		{
			_statusLabel.Text = "未找到玩家交互范围";
			return;
		}

		foreach (Area2D area in interactionArea.GetOverlappingAreas())
		{
			if (area.GetParent() is IInteractable interactable && area.GetParent() is Node ownerNode)
			{
				Player? player = playerNode as Player ?? GetNodeOrNull<Player>(PlayerPath);
				_statusLabel.Text = $"可交互对象: {ownerNode.Name} · {interactable.GetInteractText(player!)}";
				return;
			}
		}

		_statusLabel.Text = _panelRoot.Visible ? "系统面板已打开，按 C 关闭" : "靠近敌人后按 E 进入战斗";
	}

	private void OnTalentTreeGuiInput(InputEvent @event)
	{
		if (!IsTalentTabActive())
		{
			return;
		}

		if (@event is InputEventMouseButton wheelEvent && wheelEvent.Pressed)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event is InputEventMouseButton mouseButton
			&& mouseButton.ButtonIndex == MouseButton.Left
			&& mouseButton.Pressed)
		{
			_isDraggingTalentTree = true;
			_lastTalentDragPosition = mouseButton.GlobalPosition;
			GetViewport().SetInputAsHandled();
		}
	}

	private void HideTalentTreeScrollBars()
	{
		ScrollBar? horizontal = _talentTreeScroll.GetHScrollBar();
		if (horizontal != null)
		{
			horizontal.Visible = false;
			horizontal.Modulate = new Color(1, 1, 1, 0);
			horizontal.MouseFilter = Control.MouseFilterEnum.Ignore;
			horizontal.CustomMinimumSize = Vector2.Zero;
		}

		ScrollBar? vertical = _talentTreeScroll.GetVScrollBar();
		if (vertical != null)
		{
			vertical.Visible = false;
			vertical.Modulate = new Color(1, 1, 1, 0);
			vertical.MouseFilter = Control.MouseFilterEnum.Ignore;
			vertical.CustomMinimumSize = Vector2.Zero;
		}
	}

	private void HandleTalentTreePointerInput(InputEvent @event)
	{
		if (!IsTalentTabActive())
		{
			_isDraggingTalentTree = false;
			return;
		}

		if (@event is InputEventMouseButton wheelEvent
			&& wheelEvent.Pressed
			&& _talentTreeScroll.GetGlobalRect().HasPoint(wheelEvent.GlobalPosition))
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed)
			{
				if (_talentDetailPanel.Visible
					&& !_talentDetailPanel.GetGlobalRect().HasPoint(mouseButton.GlobalPosition)
					&& !IsPointOverTalentButton(mouseButton.GlobalPosition))
				{
					_selectedTalentId = string.Empty;
					RefreshTalentButtons();
					RefreshTalentDetail();
					GetViewport().SetInputAsHandled();
					return;
				}

				if (!_talentTreeScroll.GetGlobalRect().HasPoint(mouseButton.GlobalPosition))
				{
					return;
				}

				if (_talentDetailPanel.Visible && _talentDetailPanel.GetGlobalRect().HasPoint(mouseButton.GlobalPosition))
				{
					return;
				}

				if (IsPointOverTalentButton(mouseButton.GlobalPosition))
				{
					return;
				}

				_isDraggingTalentTree = true;
				_lastTalentDragPosition = mouseButton.GlobalPosition;
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_isDraggingTalentTree)
			{
				_isDraggingTalentTree = false;
				GetViewport().SetInputAsHandled();
			}

			return;
		}

		if (!_isDraggingTalentTree || @event is not InputEventMouseMotion mouseMotion)
		{
			return;
		}

		Vector2 delta = mouseMotion.GlobalPosition - _lastTalentDragPosition;
		ScrollTalentTreeBy(delta);
		_lastTalentDragPosition = mouseMotion.GlobalPosition;
		GetViewport().SetInputAsHandled();
	}

	private bool IsTalentTabActive()
	{
		return _panelRoot != null
			&& _panelRoot.Visible
			&& _tabs != null
			&& _tabs.CurrentTab == TalentTabIndex;
	}

	private void ScrollTalentTreeBy(Vector2 delta)
	{
		Vector2I limitsX = GetTalentScrollLimits(true);
		Vector2I limitsY = GetTalentScrollLimits(false);
		_talentTreeScroll.ScrollHorizontal = Mathf.Clamp(_talentTreeScroll.ScrollHorizontal - Mathf.RoundToInt(delta.X), limitsX.X, limitsX.Y);
		_talentTreeScroll.ScrollVertical = Mathf.Clamp(_talentTreeScroll.ScrollVertical - Mathf.RoundToInt(delta.Y), limitsY.X, limitsY.Y);
	}

	private bool IsPointOverTalentButton(Vector2 globalPosition)
	{
		return _talentButtons.Values.Any(button => button.Visible && button.GetGlobalRect().HasPoint(globalPosition));
	}

	private void ApplyTalentTreeZoom(float targetZoom, Vector2 mousePosition)
	{
		float clampedZoom = 1.0f;
		if (Mathf.IsEqualApprox(clampedZoom, _talentTreeZoom))
		{
			return;
		}

		float oldZoom = _talentTreeZoom;
		Vector2 logicalMouse = new(
			_talentTreeScroll.ScrollHorizontal + mousePosition.X,
			_talentTreeScroll.ScrollVertical + mousePosition.Y);

		_talentTreeZoom = clampedZoom;
		_talentTreeCanvas.Scale = Vector2.One;
		Vector2I limitsX = GetTalentScrollLimits(true);
		Vector2I limitsY = GetTalentScrollLimits(false);
		_talentTreeScroll.ScrollHorizontal = Mathf.Clamp(Mathf.RoundToInt(logicalMouse.X - mousePosition.X), limitsX.X, limitsX.Y);
		_talentTreeScroll.ScrollVertical = Mathf.Clamp(Mathf.RoundToInt(logicalMouse.Y - mousePosition.Y), limitsY.X, limitsY.Y);
	}

	private void ResetTalentTreeView()
	{
		_talentTreeZoom = 1.0f;
		_talentTreeCanvas.Scale = Vector2.One;
		SyncTalentCanvasOverlaySizes();
		ConfigureTalentBackground();
		_talentViewVelocity = Vector2.Zero;
		_talentViewHoldTime = 0.0f;

		Vector2 focusPoint = (GetControlCenter(_cardTreeLabel) + GetControlCenter(_roleTreeLabel)) * 0.5f;
		float viewportWidth = Mathf.Max(1f, _talentTreeScroll.Size.X);
		float viewportHeight = Mathf.Max(1f, _talentTreeScroll.Size.Y);
		int scrollX = Mathf.RoundToInt(focusPoint.X - viewportWidth * 0.5f);
		int scrollY = Mathf.RoundToInt(focusPoint.Y - viewportHeight * 0.5f);
		Vector2I limitsX = GetTalentScrollLimits(true);
		Vector2I limitsY = GetTalentScrollLimits(false);
		_talentTreeScroll.ScrollHorizontal = Mathf.Clamp(scrollX, limitsX.X, limitsX.Y);
		_talentTreeScroll.ScrollVertical = Mathf.Clamp(scrollY, limitsY.X, limitsY.Y);
	}

	private async void BeginResetTalentTreeViewAfterLayout()
	{
		if (!IsInsideTree())
		{
			return;
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		ResetTalentTreeView();
	}

	private void ScheduleTalentViewReset()
	{
		_pendingTalentViewResetFrames = 2;
		CallDeferred(nameof(BeginResetTalentTreeViewAfterLayout));
	}

	private void OnTabsTabChanged(long tabIndex)
	{
		if (tabIndex == TalentTabIndex && _panelRoot != null && _panelRoot.Visible)
		{
			ScheduleTalentViewReset();
		}
	}

	private void SyncTalentCanvasOverlaySizes()
	{
		Vector2 canvasSize = _talentTreeCanvas.CustomMinimumSize;
		_talentDragSurface.Position = Vector2.Zero;
		_talentDragSurface.Size = canvasSize;
		if (_talentBackground != null)
		{
			_talentBackground.Position = Vector2.Zero;
			_talentBackground.Size = canvasSize;
		}

		if (_talentLineCanvas != null)
		{
			_talentLineCanvas.Position = Vector2.Zero;
			_talentLineCanvas.Size = canvasSize;
		}
	}

	private void RecalculateTalentContentBounds()
	{
		List<Rect2> rects = new();
		foreach (Button button in _talentButtons.Values)
		{
			rects.Add(new Rect2(button.Position, button.Size));
		}

		rects.Add(new Rect2(_cardTreeLabel.Position, _cardTreeLabel.Size));
		rects.Add(new Rect2(_roleTreeLabel.Position, _roleTreeLabel.Size));

		if (rects.Count == 0)
		{
			_talentContentBounds = new Rect2(0, 0, _talentTreeCanvas.CustomMinimumSize.X, _talentTreeCanvas.CustomMinimumSize.Y);
			return;
		}

		float minX = rects.Min(rect => rect.Position.X);
		float minY = rects.Min(rect => rect.Position.Y);
		float maxX = rects.Max(rect => rect.End.X);
		float maxY = rects.Max(rect => rect.End.Y);
		const float padding = 220.0f;
		_talentContentBounds = new Rect2(
			new Vector2(Mathf.Max(0.0f, minX - padding), Mathf.Max(0.0f, minY - padding)),
			new Vector2(
				Mathf.Min(_talentTreeCanvas.CustomMinimumSize.X, maxX + padding) - Mathf.Max(0.0f, minX - padding),
				Mathf.Min(_talentTreeCanvas.CustomMinimumSize.Y, maxY + padding) - Mathf.Max(0.0f, minY - padding)));
	}

	private void UpdateTalentCanvasSizeFromContentBounds()
	{
		float width = Mathf.Max(2600.0f, _talentContentBounds.End.X + 420.0f);
		float height = Mathf.Max(2200.0f, _talentContentBounds.End.Y + 420.0f);
		_talentTreeCanvas.CustomMinimumSize = new Vector2(width, height);
		SyncTalentCanvasOverlaySizes();
	}

	private void ConfigureTalentBackground()
	{
		Texture2D? sourceTexture = GD.Load<Texture2D>(TalentBackgroundTexturePath);
		if (sourceTexture == null)
		{
			return;
		}

		Texture2D backgroundTexture = BuildTalentBackgroundTexture(sourceTexture);
		if (_talentBodyBackground == null)
		{
			_talentBodyBackground = new TextureRect
			{
				Name = "TalentBodyBackgroundRuntime",
				MouseFilter = Control.MouseFilterEnum.Ignore,
				Texture = backgroundTexture,
				Modulate = new Color(1.45f, 1.45f, 1.45f, 1.0f),
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.Scale,
				ZIndex = -100,
			};
			_talentBody.AddChild(_talentBodyBackground);
			_talentBody.MoveChild(_talentBodyBackground, 0);
		}

		_talentBodyBackground.AnchorRight = 1.0f;
		_talentBodyBackground.AnchorBottom = 1.0f;
		_talentBodyBackground.OffsetLeft = 0.0f;
		_talentBodyBackground.OffsetTop = 0.0f;
		_talentBodyBackground.OffsetRight = 0.0f;
		_talentBodyBackground.OffsetBottom = 0.0f;
		_talentBodyBackground.Texture = backgroundTexture;
		_talentBodyBackground.Visible = true;

		TextureRect? textureRect = _talentBackground as TextureRect;
		if (textureRect == null)
		{
			textureRect = new TextureRect
			{
				Name = "TalentBackgroundRuntime",
				MouseFilter = Control.MouseFilterEnum.Ignore,
				ZIndex = -50,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.Scale,
			};
			_talentTreeCanvas.AddChild(textureRect);
			_talentTreeCanvas.MoveChild(textureRect, 0);
			_talentBackground = textureRect;
		}

		textureRect.Texture = backgroundTexture;
		textureRect.Visible = true;
		textureRect.Modulate = new Color(1.35f, 1.35f, 1.35f, 1.0f);
		textureRect.StretchMode = TextureRect.StretchModeEnum.Scale;
		textureRect.Position = Vector2.Zero;
		textureRect.Size = _talentTreeCanvas.CustomMinimumSize;

		StyleBoxFlat backgroundPanel = new()
		{
			BgColor = new Color(0.06f, 0.08f, 0.11f, 0.0f),
			BorderColor = new Color(0.36f, 0.48f, 0.6f, 0.22f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomRight = 6,
			CornerRadiusBottomLeft = 6,
		};
		_talentTreeScroll.AddThemeStyleboxOverride("panel", backgroundPanel);
	}

	private static Texture2D BuildTalentBackgroundTexture(Texture2D sourceTexture)
	{
		Vector2 size = sourceTexture.GetSize();
		if (size.X <= 0.0f || size.Y <= 0.0f)
		{
			return sourceTexture;
		}

		AtlasTexture cropped = new()
		{
			Atlas = sourceTexture,
			Region = new Rect2(
				0.0f,
				size.Y * 0.05f,
				size.X,
				size.Y * 0.72f),
		};
		return cropped;
	}

	private void UpdateTalentTreeKeyboardPan(float delta)
	{
		if (!IsTalentTabActive())
		{
			_talentViewVelocity = Vector2.Zero;
			_talentViewHoldTime = 0.0f;
			return;
		}

		Vector2 input = Vector2.Zero;
		if (Input.IsKeyPressed(Key.A))
		{
			input.X -= 1.0f;
		}

		if (Input.IsKeyPressed(Key.D))
		{
			input.X += 1.0f;
		}

		if (Input.IsKeyPressed(Key.W))
		{
			input.Y -= 1.0f;
		}

		if (Input.IsKeyPressed(Key.S))
		{
			input.Y += 1.0f;
		}

		if (input == Vector2.Zero)
		{
			_talentViewHoldTime = 0.0f;
			_talentViewVelocity = _talentViewVelocity.MoveToward(Vector2.Zero, 2400.0f * delta);
		}
		else
		{
			input = input.Normalized();
			_talentViewHoldTime = Mathf.Min(_talentViewHoldTime + delta, 0.55f);
			float speed = Mathf.Lerp(90.0f, 720.0f, _talentViewHoldTime / 0.55f);
			_talentViewVelocity = _talentViewVelocity.MoveToward(input * speed, 3200.0f * delta);
		}

		if (_talentViewVelocity.LengthSquared() <= 0.01f)
		{
			return;
		}

		AddTalentTreeScroll(_talentViewVelocity * delta);
	}

	private void AddTalentTreeScroll(Vector2 deltaPixels)
	{
		Vector2I limitsX = GetTalentScrollLimits(true);
		Vector2I limitsY = GetTalentScrollLimits(false);
		_talentTreeScroll.ScrollHorizontal = Mathf.Clamp(_talentTreeScroll.ScrollHorizontal + Mathf.RoundToInt(deltaPixels.X), limitsX.X, limitsX.Y);
		_talentTreeScroll.ScrollVertical = Mathf.Clamp(_talentTreeScroll.ScrollVertical + Mathf.RoundToInt(deltaPixels.Y), limitsY.X, limitsY.Y);
	}

	private Vector2I GetTalentScrollLimits(bool horizontal)
	{
		float viewportSize = horizontal ? _talentTreeScroll.Size.X : _talentTreeScroll.Size.Y;
		float canvasSize = horizontal ? _talentTreeCanvas.CustomMinimumSize.X : _talentTreeCanvas.CustomMinimumSize.Y;
		int absoluteMax = Mathf.Max(0, Mathf.RoundToInt(canvasSize - viewportSize));
		return new Vector2I(0, absoluteMax);
	}

	private void ApplyVisibleUiOverrides()
	{
		_hintLabel.Text = "WASD Move  E Interact  C System";
		GetNode<Label>("PanelRoot/Window/Margin/Root/TitleLabel").Text = "System Lab | C Close";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/StatusColumn/Title").Text = "Status";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/Title").Text = "Equipment";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/CandidateTitle").Text = "Items";
		_statusEquipButton.Text = "Equip";
		_statusUnequipButton.Text = "Unequip";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/EquipmentDetailPanel/EquipmentDetailText").Text = "Equipment Detail";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/InventoryTab/InventoryText").Text = "Bag Info";
		_seedInventoryButton.Text = "Seed";
		_clearInventoryButton.Text = "Clear";
		_masteryLabel.Text = "Mastery";
		_cardTreeLabel.Text = "Card Tree";
		_roleTreeLabel.Text = "Role Tree";
		_talentDetailTitleLabel.Text = "Talent Detail";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel/Margin/Content/ActionHint").Text = "Select only. Click blank to close.";
		_unlockTalentButton.Text = "Unlock";
		_refundTalentButton.Text = "Refund";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/ListColumn/Title").Text = "Card Codex";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/ListColumn/Title").Text = "Enemy Codex";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/DetailPanel/DetailText").Text = "Select a card.";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/DetailPanel/DetailText").Text = "Select an enemy.";
		_deckPoolSummaryLabel.Text = "Pool";
		_deckSummaryLabel.Text = "Deck";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/AvailableColumn/AvailableTitle").Text = "Pool Cards";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/DeckColumn/DeckTitle").Text = "Current Deck";
		_deckAddButton.Text = "Add ->";
		_deckRemoveButton.Text = "<- Remove";
		_deckDetailText.Text = "Select a card.";
		_deckValidationText.Text = "Waiting";
		_deckStarterButton.Text = "Starter";
		_deckResetButton.Text = "Reset";
		_deckSaveButton.Text = "Save";
	}

	private void ApplyReadableUiTextOverrides()
	{
		_hintLabel.Text = "WASD \u79FB\u52A8  E \u4EA4\u4E92  C \u83DC\u5355";
		GetNode<Label>("PanelRoot/Window/Margin/Root/TitleLabel").Text = "\u7CFB\u7EDF\u83DC\u5355 \u00B7 \u6309 C \u5173\u95ED";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/StatusColumn/Title").Text = "\u89D2\u8272\u72B6\u6001";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/Title").Text = "\u88C5\u5907\u7BA1\u7406";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/CandidateTitle").Text = "\u53EF\u88C5\u5907\u7269\u54C1";
		_statusEquipButton.Text = "\u88C5\u5907";
		_statusUnequipButton.Text = "\u5378\u4E0B";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/EquipmentDetailPanel/EquipmentDetailText").Text = "\u9009\u62E9\u88C5\u5907\u540E\u5728\u8FD9\u91CC\u67E5\u770B\u8BE6\u60C5";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/InventoryTab/InventoryText").Text = "\u80CC\u5305\u5185\u5BB9";
		_seedInventoryButton.Text = "\u586B\u5145\u6D4B\u8BD5\u7269\u8D44";
		_clearInventoryButton.Text = "\u6E05\u7A7A\u6D4B\u8BD5\u7269\u8D44";
		_masteryLabel.Text = "\u5269\u4F59\u4E13\u7CBE\u70B9";
		_cardTreeLabel.Text = "\u5361\u724C\u6811";
		_roleTreeLabel.Text = "\u89D2\u8272\u80FD\u529B\u6811";
		_talentDetailTitleLabel.Text = "\u5929\u8D4B\u8BE6\u60C5";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel/Margin/Content/ActionHint").Text = "\u70B9\u51FB\u8282\u70B9\u53EA\u4F1A\u9009\u4E2D\u3002\u70B9\u51FB\u7A7A\u767D\u5904\u53EF\u5173\u95ED\u3002";
		_unlockTalentButton.Text = "\u89E3\u9501";
		_refundTalentButton.Text = "\u9000\u70B9";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/ListColumn/Title").Text = "\u5361\u724C\u56FE\u9274";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/ListColumn/Title").Text = "\u654C\u4EBA\u56FE\u9274";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/DetailPanel/DetailText").Text = "\u9009\u62E9\u4E00\u5F20\u5361\u724C\u67E5\u770B\u8BE6\u60C5";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/DetailPanel/DetailText").Text = "\u9009\u62E9\u4E00\u4E2A\u654C\u4EBA\u67E5\u770B\u8BE6\u60C5";
		_deckPoolSummaryLabel.Text = "\u53EF\u7528\u5361\u6C60";
		_deckSummaryLabel.Text = "\u5F53\u524D\u6784\u7B51";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/AvailableColumn/AvailableTitle").Text = "\u53EF\u9009\u5361\u724C";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/DeckColumn/DeckTitle").Text = "\u5F53\u524D\u724C\u7EC4";
		_deckAddButton.Text = "\u52A0\u5165 ->";
		_deckRemoveButton.Text = "<- \u79FB\u9664";
		_deckDetailText.Text = "\u9009\u62E9\u4E00\u5F20\u5361\u724C\u67E5\u770B\u8BE6\u60C5";
		_deckValidationText.Text = "\u7B49\u5F85\u6821\u9A8C";
		_deckStarterButton.Text = "\u9ED8\u8BA4\u724C\u7EC4";
		_deckResetButton.Text = "\u6062\u590D\u4F1A\u8BDD";
		_deckSaveButton.Text = "\u4FDD\u5B58\u6784\u7B51";
		return;

		_hintLabel.Text = "WASD 移动  E 交互  C 系统";
		GetNode<Label>("PanelRoot/Window/Margin/Root/TitleLabel").Text = "系统菜单 · 按 C 关闭";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/StatusColumn/Title").Text = "角色状态";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/Title").Text = "装备管理";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/CandidateTitle").Text = "可装备物品";
		_statusEquipButton.Text = "装备";
		_statusUnequipButton.Text = "卸下";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/StatusTab/Columns/EquipmentColumn/EquipmentDetailPanel/EquipmentDetailText").Text = "选择装备后在这里查看详情";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/InventoryTab/InventoryText").Text = "背包内容";
		_seedInventoryButton.Text = "填充测试物资";
		_clearInventoryButton.Text = "清空测试物资";
		_masteryLabel.Text = "剩余专精点";
		_cardTreeLabel.Text = "卡牌树";
		_roleTreeLabel.Text = "角色能力树";
		_talentDetailTitleLabel.Text = "天赋详情";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/TalentTab/Body/DetailPanel/Margin/Content/ActionHint").Text = "点击节点只会选中。点击空白处可关闭。";
		_unlockTalentButton.Text = "解锁";
		_refundTalentButton.Text = "退点";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/ListColumn/Title").Text = "卡牌图鉴";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/ListColumn/Title").Text = "敌人图鉴";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/CardCodex/Columns/DetailPanel/DetailText").Text = "选择一张卡牌查看详情";
		GetNode<RichTextLabel>("PanelRoot/Window/Margin/Root/Tabs/CodexTab/CodexTabs/EnemyCodex/Columns/DetailPanel/DetailText").Text = "选择一个敌人查看详情";
		_deckPoolSummaryLabel.Text = "可用卡池";
		_deckSummaryLabel.Text = "当前构筑";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/AvailableColumn/AvailableTitle").Text = "可选卡牌";
		GetNode<Label>("PanelRoot/Window/Margin/Root/Tabs/DeckTab/Columns/DeckColumn/DeckTitle").Text = "当前牌组";
		_deckAddButton.Text = "加入 ->";
		_deckRemoveButton.Text = "<- 移除";
		_deckDetailText.Text = "选择一张卡牌查看详情";
		_deckValidationText.Text = "等待校验";
		_deckStarterButton.Text = "默认牌组";
		_deckResetButton.Text = "恢复会话";
		_deckSaveButton.Text = "保存构筑";
	}

	private void ApplyReadableStatusHint()
	{
		{
		Node? readablePlayerNode = PlayerPath.IsEmpty ? null : GetNodeOrNull(PlayerPath);
		Area2D? readableInteractionArea = readablePlayerNode?.GetNodeOrNull<Area2D>("InteractionArea");
		if (readableInteractionArea == null)
		{
			_statusLabel.Text = "\u672A\u627E\u5230\u73A9\u5BB6\u4EA4\u4E92\u8303\u56F4";
			return;
		}

		foreach (Area2D area in readableInteractionArea.GetOverlappingAreas())
		{
			if (area.GetParent() is IInteractable interactable && area.GetParent() is Node ownerNode)
			{
				Player? readablePlayer = readablePlayerNode as Player ?? GetNodeOrNull<Player>(PlayerPath);
				_statusLabel.Text = $"\u53EF\u4EA4\u4E92\u5BF9\u8C61: {ownerNode.Name} \u00B7 {interactable.GetInteractText(readablePlayer!)}";
				return;
			}
		}

		_statusLabel.Text = _panelRoot.Visible
			? "\u7CFB\u7EDF\u9762\u677F\u5DF2\u6253\u5F00\uFF0C\u6309 C \u5173\u95ED"
			: "\u9760\u8FD1\u654C\u4EBA\u540E\u6309 E \u8FDB\u5165\u6218\u6597";
		return;
		}

		Node? playerNode = PlayerPath.IsEmpty ? null : GetNodeOrNull(PlayerPath);
		Area2D? interactionArea = playerNode?.GetNodeOrNull<Area2D>("InteractionArea");
		if (interactionArea == null)
		{
			_statusLabel.Text = "未找到玩家交互范围";
			return;
		}

		foreach (Area2D area in interactionArea.GetOverlappingAreas())
		{
			if (area.GetParent() is IInteractable interactable && area.GetParent() is Node ownerNode)
			{
				Player? player = playerNode as Player ?? GetNodeOrNull<Player>(PlayerPath);
				_statusLabel.Text = $"可交互对象: {ownerNode.Name} · {interactable.GetInteractText(player!)}";
				return;
			}
		}

		_statusLabel.Text = _panelRoot.Visible ? "系统面板已打开，按 C 关闭" : "靠近敌人后按 E 进入战斗";
	}

	private void BuildTalentButtons()
	{
		foreach (TalentNode talent in _talents)
		{
			Button button = new()
			{
				CustomMinimumSize = new Vector2(GetTalentButtonWidth(talent.DisplayName), 38),
				AutowrapMode = TextServer.AutowrapMode.Off,
				FocusMode = Control.FocusModeEnum.None,
			};

			string talentId = talent.Id;
			button.Pressed += () => OnTalentPressed(talentId);
			button.Position = GetDisplayedTalentPosition(talent);
			button.Size = button.CustomMinimumSize;
			button.ZIndex = 10;
			button.MouseFilter = Control.MouseFilterEnum.Stop;
			_talentTreeCanvas.AddChild(button);
			_talentButtons[talentId] = button;
		}

		_cardTreeLabel.Position = new Vector2(840.0f, 640.0f);
		_cardTreeLabel.Size = new Vector2(128.0f, 44.0f);
		_roleTreeLabel.Position = new Vector2(1640.0f, 1688.0f);
		_roleTreeLabel.Size = new Vector2(156.0f, 44.0f);
		RecalculateTalentContentBounds();
		UpdateTalentCanvasSizeFromContentBounds();

		RefreshTalentTreeLines();
	}

	private static Vector2 GetDisplayedTalentPosition(TalentNode talent)
	{
		return talent.Id switch
		{
			"talent_melee_root" => new Vector2(640.0f, 900.0f),
			"talent_melee_counter" => new Vector2(510.0f, 830.0f),
			"talent_melee_counter_plus" => new Vector2(390.0f, 770.0f),
			"talent_melee_blow" => new Vector2(515.0f, 970.0f),
			"talent_melee_blow_plus" => new Vector2(395.0f, 1030.0f),
			"talent_melee_core" => new Vector2(250.0f, 900.0f),

			"talent_ranged_root" => new Vector2(900.0f, 420.0f),
			"talent_ranged_control" => new Vector2(780.0f, 320.0f),
			"talent_ranged_control_plus" => new Vector2(670.0f, 230.0f),
			"talent_ranged_pressure" => new Vector2(1020.0f, 320.0f),
			"talent_ranged_signature" => new Vector2(1130.0f, 230.0f),
			"talent_ranged_core" => new Vector2(900.0f, 180.0f),

			"talent_flex_root" => new Vector2(1160.0f, 900.0f),
			"talent_flex_field" => new Vector2(1280.0f, 970.0f),
			"talent_flex_field_plus" => new Vector2(1400.0f, 1030.0f),
			"talent_flex_learning" => new Vector2(1280.0f, 830.0f),
			"talent_flex_learning_plus" => new Vector2(1400.0f, 770.0f),
			"talent_flex_core" => new Vector2(1520.0f, 900.0f),

			"talent_role_atk" => new Vector2(1420.0f, 1516.0f),
			"talent_role_hp" => new Vector2(1600.0f, 1516.0f),
			"talent_role_move" => new Vector2(1780.0f, 1516.0f),
			"talent_role_defense" => new Vector2(1600.0f, 1372.0f),
			"talent_role_deck" => new Vector2(1780.0f, 1372.0f),
			"talent_role_guard" => new Vector2(1600.0f, 1228.0f),
			"talent_role_copies" => new Vector2(1780.0f, 1228.0f),
			"talent_role_core" => new Vector2(1690.0f, 1084.0f),

			_ => talent.TreePosition,
		};
	}

	private static void ApplyTreeRootStyle(Button button, Color fill)
	{
		StyleBoxFlat style = new()
		{
			BgColor = fill,
			BorderColor = new Color(1.0f, 0.92f, 0.54f, 1.0f),
			BorderWidthLeft = 3,
			BorderWidthTop = 3,
			BorderWidthRight = 3,
			BorderWidthBottom = 3,
			CornerRadiusTopLeft = 24,
			CornerRadiusTopRight = 24,
			CornerRadiusBottomRight = 24,
			CornerRadiusBottomLeft = 24,
			ContentMarginLeft = 10,
			ContentMarginTop = 6,
			ContentMarginRight = 10,
			ContentMarginBottom = 6,
		};
		button.AddThemeStyleboxOverride("normal", style);
		button.AddThemeStyleboxOverride("hover", style);
		button.AddThemeStyleboxOverride("pressed", style);
		button.AddThemeStyleboxOverride("disabled", style);
		button.ZIndex = 12;
	}

	private void RefreshTalentTreeLines()
	{
		if (_talentLineCanvas == null && _talentLineLayer is Node2D legacyLayer)
		{
			foreach (Node child in legacyLayer.GetChildren().ToArray())
			{
				child.QueueFree();
			}
		}

		Vector2 cardRootCenter = GetControlCenter(_cardTreeLabel);
		Vector2 roleRootCenter = GetControlCenter(_roleTreeLabel);
		List<TalentTreeLineCanvas.PolylineData> lines = new();

		foreach (TalentNode talent in _talents)
		{
			if (!_talentButtons.TryGetValue(talent.Id, out Button? targetButton))
			{
				continue;
			}

			Vector2 targetCenter = GetControlCenter(targetButton);
			if (talent.PrerequisiteTalentIds.Length == 0)
			{
				Vector2 rootCenter = talent.Group == TalentTreeGroup.Card ? cardRootCenter : roleRootCenter;
				Color lineColor = _purchasedTalentIds.Contains(talent.Id)
					? new Color(1.0f, 0.86f, 0.28f, 1.0f)
					: new Color(0.56f, 0.60f, 0.66f, 1.0f);
				lines.Add(new TalentTreeLineCanvas.PolylineData(BuildTreeLinePoints(rootCenter, targetCenter), lineColor, 12.0f));
				continue;
			}

			foreach (string prerequisiteId in talent.PrerequisiteTalentIds)
			{
				if (!_talentButtons.TryGetValue(prerequisiteId, out Button? prerequisiteButton))
				{
					continue;
				}

				bool isUnlockedPath = _purchasedTalentIds.Contains(prerequisiteId) && _purchasedTalentIds.Contains(talent.Id);
				Color lineColor = isUnlockedPath
					? new Color(1.0f, 0.86f, 0.28f, 1.0f)
					: new Color(0.56f, 0.60f, 0.66f, 1.0f);
				lines.Add(new TalentTreeLineCanvas.PolylineData(BuildTreeLinePoints(GetControlCenter(prerequisiteButton), targetCenter), lineColor, 12.0f));
			}
		}

		if (_talentLineCanvas != null)
		{
			_talentLineCanvas.SetLines(lines);
			return;
		}

		if (_talentLineLayer is not Node2D fallbackLayer)
		{
			return;
		}

		foreach (TalentTreeLineCanvas.PolylineData line in lines)
		{
			Line2D fallback = new()
			{
				Width = line.Width,
				DefaultColor = line.Color,
				Antialiased = false,
				ZIndex = 1,
			};

			foreach (Vector2 point in line.Points)
			{
				fallback.AddPoint(point);
			}

			fallbackLayer.AddChild(fallback);
		}
	}

	private static Vector2[] BuildTreeLinePoints(Vector2 from, Vector2 to)
	{
		Vector2 delta = to - from;
		float signX = Mathf.Sign(delta.X);
		float signY = Mathf.Sign(delta.Y);
		float diagonal = Mathf.Clamp(Mathf.Min(Mathf.Abs(delta.X), Mathf.Abs(delta.Y)) * 0.35f, 18.0f, 72.0f);
		if (Mathf.Abs(delta.X) >= Mathf.Abs(delta.Y))
		{
			float midX = from.X + delta.X * 0.5f;
			return new[]
			{
				from,
				new Vector2(midX - signX * diagonal, from.Y),
				new Vector2(midX, from.Y + signY * diagonal),
				new Vector2(midX, to.Y - signY * diagonal),
				new Vector2(midX + signX * diagonal, to.Y),
				to,
			};
		}

		float midY = from.Y + delta.Y * 0.5f;
		return new[]
		{
			from,
			new Vector2(from.X, midY - signY * diagonal),
			new Vector2(from.X + signX * diagonal, midY),
			new Vector2(to.X - signX * diagonal, midY),
			new Vector2(to.X, midY + signY * diagonal),
			to,
		};
	}

	private static Vector2 GetControlCenter(Control control)
	{
		return control.Position + control.Size * 0.5f;
	}

	private static float GetTalentButtonWidth(string displayName)
	{
		int charCount = string.IsNullOrWhiteSpace(displayName) ? 4 : displayName.Length;
		return Mathf.Clamp(38.0f + charCount * 16.0f, 116.0f, 196.0f);
	}

	private void BuildCodexSource()
	{
		_codexTemplates = _cardLibrary?.Entries
			.Where(entry => entry != null)
			.OrderBy(entry => entry.CardId, StringComparer.Ordinal)
			.ToArray()
			?? Array.Empty<BattleCardTemplate>();
	}

	private void SeedSessionForTesting()
	{
		if (_session == null)
		{
			return;
		}

		_session.ProgressionState.PlayerLevel = Math.Max(3, _session.ProgressionState.PlayerLevel);
		int levelFloorExperience = _session.RuntimeProgressionRuleSet.GetAccumulatedExperienceForLevel(_session.ProgressionState.PlayerLevel);
		_session.ProgressionState.PlayerExperience = Math.Max(levelFloorExperience, _session.ProgressionState.PlayerExperience);
		_session.PlayerLevel = _session.ProgressionState.PlayerLevel;
		_session.PlayerExperience = _session.ProgressionState.PlayerExperience;
		_session.PlayerAttackDamage = Math.Max(2, _session.PlayerAttackDamage);
		_session.PlayerDefenseDamageReductionPercent = Math.Max(50, _session.PlayerDefenseDamageReductionPercent);
		if (_session.InventoryState.ItemCounts.Count == 0)
		{
			SeedInventoryDefaults();
		}
	}

	private void LoadPurchasedTalentsFromSession()
	{
		if (_session == null)
		{
			return;
		}

		_purchasedTalentIds.Clear();
		foreach (TalentNode talent in _talents)
		{
			if (_session.ProgressionState.TalentIds.Contains(talent.Id, StringComparer.Ordinal))
			{
				_purchasedTalentIds.Add(talent.Id);
			}
		}

		foreach (TalentNode rootTalent in _talents.Where(talent => IsDefaultUnlockedTalent(talent.Id)))
		{
			_purchasedTalentIds.Add(rootTalent.Id);
		}

		_baseMasteryPoints = Math.Max(6, _session.ProgressionState.PlayerMasteryPoints + GetSpentPoints());
	}

	private void LoadWorkingDeckFromSession()
	{
		if (_session == null)
		{
			return;
		}

		_workingDeck = _session.DeckBuildState.CardIds.ToList();
	}

	private void RefreshAll()
	{
		{
		RefreshStatusView();
		RefreshBagView();
		RefreshTalentSummary();
		_masteryLabel.Text = $"\u5269\u4F59\u4E13\u7CBE\u70B9 {GetAvailablePoints()} | WASD \u79FB\u52A8\u89C6\u56FE";
		RefreshTalentButtons();
		RefreshTalentDetail();
		RefreshCodexView();
		RefreshDeckView();
		return;
		}

		RefreshStatusView();
		RefreshBagView();
		RefreshTalentSummary();
		_masteryLabel.Text = $"专精点 {GetAvailablePoints()} | WASD 移动视图";
		RefreshTalentButtons();
		RefreshTalentDetail();
		RefreshCodexView();
		RefreshDeckView();
	}

	private void RefreshStatusView()
	{
		{
		if (_session == null)
		{
			_statusOverviewText.Text = "\u672A\u627E\u5230 GlobalGameSession";
			_statusEquipmentDetailText.Text = "\u65E0\u6CD5\u8BFB\u53D6\u88C5\u5907\u4FE1\u606F\u3002";
			return;
		}

		List<string> readableStatusLines = new()
		{
			$"[b]{_session.PlayerDisplayName}[/b]",
			$"\u7B49\u7EA7: Lv.{_session.PlayerLevel}",
			$"\u7ECF\u9A8C: {_session.GetExperienceProgressWithinLevel()}/{_session.GetExperienceRequiredForNextLevel()}",
			$"\u8DDD\u4E0B\u4E00\u7EA7: {_session.GetExperienceNeededToLevelUp()}",
			$"\u4E13\u7CBE\u70B9: {_session.ProgressionState.PlayerMasteryPoints}",
			string.Empty,
			$"\u751F\u547D: {_session.PlayerCurrentHp}/{_session.GetResolvedPlayerMaxHp()}",
			$"\u79FB\u52A8: {_session.GetResolvedPlayerMovePointsPerTurn()}",
			$"\u653B\u51FB: {_session.GetResolvedPlayerAttackDamage()}",
			$"\u653B\u51FB\u8303\u56F4: {_session.PlayerAttackRange}",
			$"\u9632\u5FA1\u51CF\u4F24: {_session.GetResolvedPlayerDefenseDamageReductionPercent()}%",
			$"\u9632\u5FA1\u62A4\u76FE: {_session.GetResolvedPlayerDefenseShieldGain()}",
			$"\u6784\u7B51\u9884\u7B97\u52A0\u6210: {_session.ProgressionState.DeckPointBudgetBonus}",
			$"\u540C\u540D\u4E0A\u9650\u52A0\u6210: {_session.ProgressionState.DeckMaxCopiesPerCardBonus}",
		};
		_statusOverviewText.Text = string.Join('\n', readableStatusLines);
		RefreshEquipmentSection();
		return;
		}

		if (_session == null)
		{
			_statusOverviewText.Text = "未找到 GlobalGameSession";
			_statusEquipmentDetailText.Text = "无法读取装备信息。";
			return;
		}

		List<string> lines = new()
		{
			$"[b]{_session.PlayerDisplayName}[/b]",
			$"等级: Lv.{_session.PlayerLevel}",
			$"经验: {_session.GetExperienceProgressWithinLevel()}/{_session.GetExperienceRequiredForNextLevel()}",
			$"距下一级: {_session.GetExperienceNeededToLevelUp()}",
			$"专精点: {_session.ProgressionState.PlayerMasteryPoints}",
			string.Empty,
			$"生命: {_session.PlayerCurrentHp}/{_session.GetResolvedPlayerMaxHp()}",
			$"移动: {_session.GetResolvedPlayerMovePointsPerTurn()}",
			$"攻击: {_session.GetResolvedPlayerAttackDamage()}",
			$"攻击范围: {_session.PlayerAttackRange}",
			$"防御减伤: {_session.GetResolvedPlayerDefenseDamageReductionPercent()}%",
			$"防御护盾: {_session.GetResolvedPlayerDefenseShieldGain()}",
			$"构筑预算加成: {_session.ProgressionState.DeckPointBudgetBonus}",
			$"同名上限加成: {_session.ProgressionState.DeckMaxCopiesPerCardBonus}",
		};
		_statusOverviewText.Text = string.Join('\n', lines);
		RefreshEquipmentSection();
	}

	private void RefreshBagView()
	{
		{
		if (_session == null)
		{
			_inventoryText.Text = "\u672A\u627E\u5230 GlobalGameSession";
			return;
		}

		List<string> readableBagLines = new()
		{
			"[b]\u80CC\u5305\u7269\u54C1[/b]",
			string.Empty,
		};

		if (_session.InventoryState.ItemCounts.Count == 0)
		{
			readableBagLines.Add("- (\u7A7A)");
		}
		else
		{
			foreach (Variant key in _session.InventoryState.ItemCounts.Keys)
			{
				string itemId = key.AsString();
				readableBagLines.Add($"- {GetInventoryItemDisplayName(itemId)} x{_session.InventoryState.ItemCounts[key].AsInt32()}");
			}
		}

		readableBagLines.Add(string.Empty);
		readableBagLines.Add("[b]\u989D\u5916\u89E3\u9501\u5361\u724C[/b]");
		readableBagLines.AddRange(_session.ProgressionState.UnlockedCardIds.Length == 0
			? new[] { "- (\u65E0)" }
			: _session.ProgressionState.UnlockedCardIds
				.OrderBy(value => value, StringComparer.Ordinal)
				.Select(value => $"- {GetCardDisplayName(value)}"));
		_inventoryText.Text = string.Join('\n', readableBagLines);
		return;
		}

		if (_session == null)
		{
			_inventoryText.Text = "未找到 GlobalGameSession";
			return;
		}

		List<string> lines = new()
		{
			"[b]背包物品[/b]",
			string.Empty,
		};

		if (_session.InventoryState.ItemCounts.Count == 0)
		{
			lines.Add("- (空)");
		}
		else
		{
			foreach (Variant key in _session.InventoryState.ItemCounts.Keys)
			{
				lines.Add($"- {key.AsString()} x{_session.InventoryState.ItemCounts[key].AsInt32()}");
			}
		}

		lines.Add(string.Empty);
		lines.Add("[b]额外解锁卡牌[/b]");
		lines.AddRange(_session.ProgressionState.UnlockedCardIds.Length == 0
			? new[] { "- (无)" }
			: _session.ProgressionState.UnlockedCardIds.OrderBy(value => value, StringComparer.Ordinal).Select(value => $"- {value}"));
		_inventoryText.Text = string.Join('\n', lines);
	}

	private void RefreshEquipmentSection()
	{
		{
		if (_session == null)
		{
			return;
		}

		_statusEquipmentSlotList.Clear();
		foreach (string slotId in EquipmentSlotOrder)
		{
			string equippedItemId = _session.GetEquippedItemId(slotId);
			string equippedName = GetEquipmentDisplayNameSafe(equippedItemId);
			_statusEquipmentSlotList.AddItem($"{GetEquipmentSlotDisplayNameSafe(slotId)}: {equippedName}");
		}

		int readableSelectedSlotIndex = Array.IndexOf(EquipmentSlotOrder, _selectedEquipmentSlotId);
		if (readableSelectedSlotIndex >= 0)
		{
			_statusEquipmentSlotList.Select(readableSelectedSlotIndex);
		}

		List<RuntimeEquipmentDefinition> readableCandidates = new();
		foreach (Variant key in _session.InventoryState.ItemCounts.Keys)
		{
			string itemId = key.AsString();
			RuntimeEquipmentDefinition? definition = FindEquipmentDefinition(itemId);
			if (definition == null || !string.Equals(definition.SlotId, _selectedEquipmentSlotId, StringComparison.Ordinal))
			{
				continue;
			}

			readableCandidates.Add(definition);
		}

		_visibleEquipmentCandidates = readableCandidates
			.OrderBy(definition => definition.DisplayName, StringComparer.Ordinal)
			.ToArray();

		_statusEquipmentCandidateList.Clear();
		foreach (RuntimeEquipmentDefinition definition in _visibleEquipmentCandidates)
		{
			string suffix = string.Equals(_session.GetEquippedItemId(_selectedEquipmentSlotId), definition.ItemId, StringComparison.Ordinal)
				? " [\u5DF2\u88C5\u5907]"
				: string.Empty;
			_statusEquipmentCandidateList.AddItem($"{definition.DisplayName}{suffix}");
		}

		_statusUnequipButton.Disabled = string.IsNullOrWhiteSpace(_session.GetEquippedItemId(_selectedEquipmentSlotId));
		_statusEquipButton.Disabled = _visibleEquipmentCandidates.Length == 0;

		if (_visibleEquipmentCandidates.Length == 0)
		{
			_statusEquipmentDetailText.Text = $"[b]{GetEquipmentSlotDisplayNameSafe(_selectedEquipmentSlotId)}[/b]\n\u5F53\u524D\u6CA1\u6709\u53EF\u88C5\u5907\u7269\u54C1\u3002";
			return;
		}

		_statusEquipmentCandidateList.Select(0);
		RefreshEquipmentDetail(_visibleEquipmentCandidates[0]);
		return;
		}

		if (_session == null)
		{
			return;
		}

		_statusEquipmentSlotList.Clear();
		foreach (string slotId in EquipmentSlotOrder)
		{
			string equippedItemId = _session.GetEquippedItemId(slotId);
			string equippedName = GetEquipmentDisplayNameSafe(equippedItemId);
			_statusEquipmentSlotList.AddItem($"{GetEquipmentSlotDisplayNameSafe(slotId)}: {equippedName}");
		}

		int selectedSlotIndex = Array.IndexOf(EquipmentSlotOrder, _selectedEquipmentSlotId);
		if (selectedSlotIndex >= 0)
		{
			_statusEquipmentSlotList.Select(selectedSlotIndex);
		}

		List<RuntimeEquipmentDefinition> candidates = new();
		foreach (Variant key in _session.InventoryState.ItemCounts.Keys)
		{
			string itemId = key.AsString();
			RuntimeEquipmentDefinition? definition = FindEquipmentDefinition(itemId);
			if (definition == null || !string.Equals(definition.SlotId, _selectedEquipmentSlotId, StringComparison.Ordinal))
			{
				continue;
			}

			candidates.Add(definition);
		}

		_visibleEquipmentCandidates = candidates
			.OrderBy(definition => definition.DisplayName, StringComparer.Ordinal)
			.ToArray();
		_statusEquipmentCandidateList.Clear();
		foreach (RuntimeEquipmentDefinition definition in _visibleEquipmentCandidates)
		{
			string suffix = string.Equals(_session.GetEquippedItemId(_selectedEquipmentSlotId), definition.ItemId, StringComparison.Ordinal)
				? " [已装备]"
				: string.Empty;
			suffix = string.Equals(_session.GetEquippedItemId(_selectedEquipmentSlotId), definition.ItemId, StringComparison.Ordinal)
				? " [已装备]"
				: string.Empty;
			_statusEquipmentCandidateList.AddItem($"{definition.DisplayName}{suffix}");
		}

		_statusUnequipButton.Disabled = string.IsNullOrWhiteSpace(_session.GetEquippedItemId(_selectedEquipmentSlotId));
		_statusEquipButton.Disabled = _visibleEquipmentCandidates.Length == 0;

		if (_visibleEquipmentCandidates.Length == 0)
		{
			_statusEquipmentDetailText.Text = $"[b]{GetEquipmentSlotDisplayName(_selectedEquipmentSlotId)}[/b]\n当前没有可装备物品。";
			_statusEquipmentDetailText.Text = $"[b]{GetEquipmentSlotDisplayNameSafe(_selectedEquipmentSlotId)}[/b]\n当前没有可装备物品。";
			return;
		}

		_statusEquipmentCandidateList.Select(0);
		RefreshEquipmentDetail(_visibleEquipmentCandidates[0]);
	}

	private void RefreshEquipmentDetail(RuntimeEquipmentDefinition definition)
	{
		{
		if (_session == null)
		{
			return;
		}

		bool readableEquipped = string.Equals(_session.GetEquippedItemId(definition.SlotId), definition.ItemId, StringComparison.Ordinal);
		int readableOwnedCount = _session.InventoryState.ItemCounts.TryGetValue(definition.ItemId, out Variant readableOwnedValue)
			? readableOwnedValue.AsInt32()
			: 0;
		_statusEquipmentDetailText.Text = string.Join('\n', new[]
		{
			$"[b]{definition.DisplayName}[/b]",
			$"\u90E8\u4F4D: {GetEquipmentSlotDisplayNameSafe(definition.SlotId)}",
			$"\u62E5\u6709\u6570\u91CF: {readableOwnedCount}",
			$"\u72B6\u6001: {(readableEquipped ? "\u5DF2\u88C5\u5907" : "\u672A\u88C5\u5907")}",
			$"\u6548\u679C: {definition.BuildModifierSummary()}",
			string.Empty,
			definition.Description,
		});
		return;
		}

		if (_session == null)
		{
			return;
		}

		bool equipped = string.Equals(_session.GetEquippedItemId(definition.SlotId), definition.ItemId, StringComparison.Ordinal);
		int ownedCount = _session.InventoryState.ItemCounts.TryGetValue(definition.ItemId, out Variant ownedValue)
			? ownedValue.AsInt32()
			: 0;
		_statusEquipmentDetailText.Text = string.Join('\n', new[]
		{
			$"[b]{definition.DisplayName}[/b]",
			$"部位: {GetEquipmentSlotDisplayName(definition.SlotId)}",
			$"拥有数量: {ownedCount}",
			$"状态: {(equipped ? "已装备" : "未装备")}",
			$"效果: {definition.BuildModifierSummary()}",
			string.Empty,
			definition.Description,
		});
		_statusEquipmentDetailText.Text = string.Join('\n', new[]
		{
			$"[b]{definition.DisplayName}[/b]",
			$"部位: {GetEquipmentSlotDisplayNameSafe(definition.SlotId)}",
			$"拥有数量: {ownedCount}",
			$"状态: {(equipped ? "已装备" : "未装备")}",
			$"效果: {definition.BuildModifierSummary()}",
			string.Empty,
			definition.Description,
		});
	}

	private void RefreshInventoryView()
	{
		{
		if (_session == null)
		{
			_inventoryText.Text = "\u672A\u627E\u5230 GlobalGameSession";
			return;
		}

		List<string> readableInventoryLines = new()
		{
			"[b]\u80CC\u5305[/b]",
			string.Empty,
			"[b]\u7269\u54C1[/b]",
		};

		if (_session.InventoryState.ItemCounts.Count == 0)
		{
			readableInventoryLines.Add("- (\u7A7A)");
		}
		else
		{
			foreach (Variant key in _session.InventoryState.ItemCounts.Keys)
			{
				string itemId = key.AsString();
				readableInventoryLines.Add($"- {GetInventoryItemDisplayName(itemId)} x{_session.InventoryState.ItemCounts[key].AsInt32()}");
			}
		}

		readableInventoryLines.Add(string.Empty);
		readableInventoryLines.Add("[b]\u989D\u5916\u89E3\u9501\u5361\u724C[/b]");
		readableInventoryLines.AddRange(_session.ProgressionState.UnlockedCardIds.Length == 0
			? new[] { "- (\u65E0)" }
			: _session.ProgressionState.UnlockedCardIds
				.OrderBy(value => value, StringComparer.Ordinal)
				.Select(value => $"- {GetCardDisplayName(value)}"));
		_inventoryText.Text = string.Join('\n', readableInventoryLines);
		return;
		}

		if (_session == null)
		{
			_inventoryText.Text = "未找到 GlobalGameSession";
			return;
		}

		List<string> lines = new()
		{
			"[b]背包[/b]",
			string.Empty,
			"[b]物品[/b]",
		};

		if (_session.InventoryState.ItemCounts.Count == 0)
		{
			lines.Add("- (空)");
		}
		else
		{
			foreach (Variant key in _session.InventoryState.ItemCounts.Keys)
			{
				lines.Add($"- {key.AsString()} x{_session.InventoryState.ItemCounts[key].AsInt32()}");
			}
		}

		lines.Add(string.Empty);
		lines.Add("[b]额外解锁卡牌[/b]");
		lines.AddRange(_session.ProgressionState.UnlockedCardIds.Length == 0
			? new[] { "- (无)" }
			: _session.ProgressionState.UnlockedCardIds.OrderBy(value => value, StringComparer.Ordinal).Select(value => $"- {value}"));
		_inventoryText.Text = string.Join('\n', lines);
	}

	private void RefreshTalentSummary()
	{
		{
		if (_session == null)
		{
			return;
		}

		_masteryLabel.Text = $"\u5269\u4F59\u4E13\u7CBE\u70B9 {GetAvailablePoints()}";
		return;
		}

		if (_session == null)
		{
			return;
		}

		_masteryLabel.Text = $"专精点 {GetAvailablePoints()}";
	}

	private void RefreshTalentButtons()
	{
		foreach (TalentNode talent in _talents)
		{
			if (!_talentButtons.TryGetValue(talent.Id, out Button? button))
			{
				continue;
			}
			bool purchased = _purchasedTalentIds.Contains(talent.Id);
			bool canPurchase = CanPurchase(talent);
			bool canRefund = CanRefund(talent);
			bool selected = string.Equals(_selectedTalentId, talent.Id, StringComparison.Ordinal);
			string status = purchased
				? (canRefund ? "已解锁" : "已解锁")
				: canPurchase ? "可购买" : ArePrerequisitesMet(talent) ? "点数不足" : "前置未满足";
			string prefix = purchased ? "●" : canPurchase ? "◉" : "○";
			string selectedPrefix = selected ? ">> " : string.Empty;
			button.Text = $"{selectedPrefix}{prefix} {talent.DisplayName}\n{status}";
			prefix = purchased ? "●" : canPurchase ? "◆" : "○";
			button.Text = $"{selectedPrefix}{prefix} {talent.DisplayName}";
			button.Modulate = selected
				? (purchased ? new Color(0.94f, 1.0f, 0.96f, 1f) : canPurchase ? new Color(1f, 0.98f, 0.82f, 1f) : new Color(0.82f, 0.84f, 0.9f, 1f))
				: purchased ? new Color(0.72f, 0.95f, 0.8f, 1f) : canPurchase ? new Color(1f, 0.9f, 0.54f, 1f) : new Color(0.48f, 0.48f, 0.48f, 1f);
			ApplyTalentButtonStyle(button, talent, purchased, canPurchase, selected);
		}

		RefreshTalentTreeLines();
	}

	private void ApplyTalentButtonStyle(Button button, TalentNode talent, bool purchased, bool canPurchase, bool selected)
	{
		Color fill = GetTalentFillColor(talent);
		Color border = selected
			? new Color(1.0f, 0.92f, 0.54f, 1.0f)
			: purchased
				? fill.Lightened(0.22f)
				: canPurchase
					? fill.Lightened(0.08f)
					: fill.Darkened(0.34f);

		StyleBoxFlat style = new()
		{
			BgColor = fill,
			BorderColor = border,
			BorderWidthLeft = selected ? 4 : 2,
			BorderWidthTop = selected ? 4 : 2,
			BorderWidthRight = selected ? 4 : 2,
			BorderWidthBottom = selected ? 4 : 2,
			CornerRadiusTopLeft = talent.PrerequisiteTalentIds.Length == 0 ? 24 : talent.Group == TalentTreeGroup.Card ? 18 : 6,
			CornerRadiusTopRight = talent.PrerequisiteTalentIds.Length == 0 ? 24 : talent.Group == TalentTreeGroup.Card ? 18 : 6,
			CornerRadiusBottomRight = talent.PrerequisiteTalentIds.Length == 0 ? 24 : talent.Group == TalentTreeGroup.Card ? 18 : 6,
			CornerRadiusBottomLeft = talent.PrerequisiteTalentIds.Length == 0 ? 24 : talent.Group == TalentTreeGroup.Card ? 18 : 6,
			ContentMarginLeft = 6,
			ContentMarginTop = 4,
			ContentMarginRight = 6,
			ContentMarginBottom = 4,
		};
		button.AddThemeStyleboxOverride("normal", style);
		button.AddThemeStyleboxOverride("hover", style);
		button.AddThemeStyleboxOverride("pressed", style);
		button.AddThemeStyleboxOverride("disabled", style);
		button.AddThemeFontSizeOverride("font_size", 16);
		button.ClipText = true;
	}

	private static Color GetTalentFillColor(TalentNode talent)
	{
		if (talent.Group == TalentTreeGroup.Role)
		{
			return new Color(0.28f, 0.23f, 0.16f, 0.96f);
		}

		return talent.BranchId switch
		{
			"melee" => new Color(0.34f, 0.18f, 0.15f, 0.96f),
			"ranged" => new Color(0.14f, 0.24f, 0.34f, 0.96f),
			"flex" => new Color(0.12f, 0.30f, 0.28f, 0.96f),
			_ => new Color(0.22f, 0.22f, 0.24f, 0.96f),
		};
	}

	private void RefreshTalentDetail()
	{
		{
		if (string.IsNullOrWhiteSpace(_selectedTalentId))
		{
			_talentDetailDim.Visible = false;
			_talentDetailPanel.Visible = false;
			return;
		}

		TalentNode? readableTalent = _talents.FirstOrDefault(item => string.Equals(item.Id, _selectedTalentId, StringComparison.Ordinal));
		if (readableTalent == null)
		{
			_talentDetailDim.Visible = false;
			_talentDetailPanel.Visible = false;
			return;
		}

		bool readablePurchased = _purchasedTalentIds.Contains(readableTalent.Id);
		bool readableCanPurchase = CanPurchase(readableTalent);
		bool readableCanRefund = CanRefund(readableTalent);
		_talentDetailDim.Visible = true;
		_talentDetailPanel.Visible = true;
		_talentDetailTitleLabel.Text = $"\u5929\u8D4B\u8BE6\u60C5 | {readableTalent.DisplayName}";
		_talentDetail.Text = string.Join('\n', new[]
		{
			$"[b]{readableTalent.DisplayName}[/b]",
			readableTalent.Description,
			$"\u82B1\u8D39: {readableTalent.Cost}",
			$"\u524D\u7F6E: {(readableTalent.PrerequisiteTalentIds.Length == 0 ? "\u65E0" : string.Join(", ", readableTalent.PrerequisiteTalentIds))}",
			$"\u5206\u652F: {(readableTalent.GrantedBranchTags.Length == 0 ? "\u65E0" : string.Join(", ", readableTalent.GrantedBranchTags))}",
			$"\u89E3\u9501\u5361\u724C: {(readableTalent.UnlockedCardIds.Length == 0 ? "\u65E0" : string.Join(", ", readableTalent.UnlockedCardIds.Select(GetCardDisplayName)))}",
			$"\u6784\u7B51\u4FEE\u6B63: +{readableTalent.DeckPointBudgetBonus} / \u540C\u540D\u4E0A\u9650 +{readableTalent.DeckMaxCopiesPerCardBonus}",
			string.Empty,
			"\u70B9\u51FB\u8282\u70B9\u53EA\u4F1A\u9009\u4E2D\uFF0C\u4E0D\u4F1A\u76F4\u63A5\u89E3\u9501\u3002",
			"\u8BF7\u4F7F\u7528\u53F3\u4FA7\u6309\u94AE\u786E\u8BA4\u89E3\u9501\u6216\u9000\u70B9\u3002",
		});
		_unlockTalentButton.Disabled = readablePurchased || !readableCanPurchase;
		_unlockTalentButton.Text = readablePurchased ? "\u5DF2\u89E3\u9501" : readableCanPurchase ? $"\u89E3\u9501 ({readableTalent.Cost}\u70B9)" : "\u4E0D\u53EF\u89E3\u9501";
		_refundTalentButton.Disabled = !readablePurchased || !readableCanRefund;
		_refundTalentButton.Text = readableCanRefund ? "\u9000\u70B9" : "\u4E0D\u53EF\u9000\u70B9";
		return;
		}

		TalentNode? talent = _talents.FirstOrDefault(item => string.Equals(item.Id, _selectedTalentId, StringComparison.Ordinal));
		if (talent == null)
		{
			_talentDetailDim.Visible = false;
			_talentDetailPanel.Visible = false;
			_talentDetailTitleLabel.Text = "天赋详情";
			_unlockTalentButton.Disabled = true;
			_unlockTalentButton.Text = "请先选择天赋";
			_refundTalentButton.Disabled = true;
			_refundTalentButton.Text = "未选择节点";
			_talentDetail.Text = "拖动画面查看完整天赋树，点击节点查看详情。";
			return;
		}

		bool purchased = _purchasedTalentIds.Contains(talent.Id);
		bool canPurchase = CanPurchase(talent);
		bool canRefund = CanRefund(talent);
		_talentDetailDim.Visible = true;
		_talentDetailPanel.Visible = true;
		_talentDetailTitleLabel.Text = $"天赋详情 | {talent.DisplayName}";
		_talentDetail.Text = string.Join('\n', new[]
		{
			$"[b]{talent.DisplayName}[/b]",
			talent.Description,
			$"花费: {talent.Cost}",
			$"前置: {(talent.PrerequisiteTalentIds.Length == 0 ? "无" : string.Join(", ", talent.PrerequisiteTalentIds))}",
			$"分支: {(talent.GrantedBranchTags.Length == 0 ? "无" : string.Join(", ", talent.GrantedBranchTags))}",
			$"解锁卡牌: {(talent.UnlockedCardIds.Length == 0 ? "无" : string.Join(", ", talent.UnlockedCardIds))}",
			$"构筑修正: +{talent.DeckPointBudgetBonus} / 同名上限 +{talent.DeckMaxCopiesPerCardBonus}",
			$"附加 TalentIds: {(talent.GrantedTalentIds.Length == 0 ? "无" : string.Join(", ", talent.GrantedTalentIds))}",
			string.Empty,
			"点击节点只会选中，不会直接解锁。",
			"请使用右侧按钮确认解锁或退款。",
		});
		_unlockTalentButton.Disabled = purchased || !canPurchase;
		_unlockTalentButton.Text = purchased ? "已解锁" : canPurchase ? $"解锁 ({talent.Cost}点)" : "不可解锁";
		_refundTalentButton.Disabled = !purchased || !canRefund;
		_refundTalentButton.Text = canRefund ? "退款" : "不可退款";
	}

	private void OnTalentDetailDimGuiInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		if (_talentDetailPanel.GetGlobalRect().HasPoint(mouseButton.GlobalPosition))
		{
			return;
		}

		_selectedTalentId = string.Empty;
		RefreshTalentButtons();
		RefreshTalentDetail();
		GetViewport().SetInputAsHandled();
	}

	private void RefreshCodexView()
	{
		if (_session == null)
		{
			return;
		}

		ProgressionSnapshot progression = _session.BuildProgressionSnapshotModel();
		_cardCodexList.Clear();
		foreach (BattleCardTemplate template in _codexTemplates)
		{
			bool unlocked = template.IsOwned(progression);
			int index = _cardCodexList.AddItem(unlocked ? template.DisplayName : "■■■");
			if (!unlocked)
			{
				_cardCodexList.SetItemCustomFgColor(index, new Color(0.16f, 0.16f, 0.16f, 1.0f));
			}
		}

		_enemyCodexList.Clear();
		foreach (EnemyCodexEntry entry in _enemyCodexEntries)
		{
			bool unlocked = IsEnemyCodexUnlocked(entry, progression);
			int index = _enemyCodexList.AddItem(unlocked ? entry.DisplayName : "■■■");
			if (!unlocked)
			{
				_enemyCodexList.SetItemCustomFgColor(index, new Color(0.16f, 0.16f, 0.16f, 1.0f));
			}
		}

		if (_cardCodexList.ItemCount > 0)
		{
			OnCardCodexSelected(Mathf.Clamp(_cardCodexList.GetSelectedItems().FirstOrDefault(), 0, _cardCodexList.ItemCount - 1));
		}

		if (_enemyCodexList.ItemCount > 0)
		{
			OnEnemyCodexSelected(Mathf.Clamp(_enemyCodexList.GetSelectedItems().FirstOrDefault(), 0, _enemyCodexList.ItemCount - 1));
		}
	}

	private void RefreshDeckView()
	{
		{
		if (_session == null || _constructionService == null || _cardLibrary == null)
		{
			return;
		}

		ProgressionSnapshot readableProgression = _session.BuildProgressionSnapshotModel();
		_availableTemplates = _constructionService.GetAvailableCardPool(readableProgression).ToArray();
		_availableList.Clear();
		foreach (BattleCardTemplate template in _availableTemplates)
		{
			bool overlimit = !template.CanCarryNormally(readableProgression) && template.CanCarryOverlimit(readableProgression);
			string suffix = template.IsLearnedCard ? " [\u5B66\u4E60]" : overlimit ? " [\u8D85\u9650]" : string.Empty;
			_availableList.AddItem($"{template.DisplayName}{suffix}");
		}

		_deckList.Clear();
		foreach (string cardId in _workingDeck)
		{
			BattleCardTemplate? template = _cardLibrary.FindTemplate(cardId);
			_deckList.AddItem(template?.DisplayName ?? cardId);
		}

		DeckBuildSnapshot readableSnapshot = new()
		{
			BuildName = _session.DeckBuildState.BuildName,
			CardIds = _workingDeck.ToArray(),
			RelicIds = _session.DeckBuildState.RelicIds,
		};
		BattleDeckValidationResult readableValidation = _constructionService.ValidateDeck(readableSnapshot, readableProgression);
		_deckPoolSummaryLabel.Text = $"\u53EF\u9009 { _availableTemplates.Length }";
		_deckSummaryLabel.Text = $"\u5F53\u524D {readableValidation.TotalCardCount} \u5F20 / \u5F71\u54CD {readableValidation.TotalBuildPoints}";
		_deckValidationText.Text = BuildReadableDeckValidationText(readableValidation);
		_deckSaveButton.Disabled = !readableValidation.IsValid;
		return;
		}

		if (_session == null || _constructionService == null || _cardLibrary == null)
		{
			return;
		}

		ProgressionSnapshot progression = _session.BuildProgressionSnapshotModel();
		_availableTemplates = _constructionService.GetAvailableCardPool(progression).ToArray();
		_availableList.Clear();
		foreach (BattleCardTemplate template in _availableTemplates)
		{
			bool overlimit = !template.CanCarryNormally(progression) && template.CanCarryOverlimit(progression);
			string suffix = template.IsLearnedCard ? " [学习]" : overlimit ? " [超限]" : string.Empty;
			_availableList.AddItem($"{template.DisplayName}{suffix}");
		}

		_deckList.Clear();
		foreach (string cardId in _workingDeck)
		{
			BattleCardTemplate? template = _cardLibrary.FindTemplate(cardId);
			_deckList.AddItem(template?.DisplayName ?? cardId);
		}

		DeckBuildSnapshot snapshot = new()
		{
			BuildName = _session.DeckBuildState.BuildName,
			CardIds = _workingDeck.ToArray(),
			RelicIds = _session.DeckBuildState.RelicIds,
		};
		BattleDeckValidationResult validation = _constructionService.ValidateDeck(snapshot, progression);
		_deckPoolSummaryLabel.Text = $"可选 { _availableTemplates.Length }";
		_deckSummaryLabel.Text = $"当前 {validation.TotalCardCount} 张 / 影响 {validation.TotalBuildPoints}";
		_deckValidationText.Text = BuildDeckValidationText(validation);
		_deckSaveButton.Disabled = !validation.IsValid;
	}

	private static string BuildDeckValidationText(BattleDeckValidationResult validation)
	{
		return BuildReadableDeckValidationText(validation);

		List<string> lines = new()
		{
			$"最低卡数: {validation.TotalCardCount}/{validation.EffectiveMinDeckSize}",
			$"影响因子: {validation.TotalBuildPoints}/{validation.EffectivePointBudget}",
			$"同名上限: {validation.EffectiveMaxCopiesPerCard}",
			$"超限槽位: {validation.UsedOverlimitCarrySlots}/{validation.EffectiveOverlimitCarrySlots}",
		};

		if (validation.Errors.Count == 0)
		{
			lines.Add("状态: 通过");
		}
		else
		{
			lines.Add("状态: 未通过");
			lines.AddRange(validation.Errors.Select(error => $"- {error}"));
		}

		return string.Join('\n', lines);
	}

	private void OnTalentPressed(string talentId)
	{
		_selectedTalentId = talentId;
		RefreshTalentButtons();
		RefreshTalentDetail();
	}

	private void OnUnlockTalentPressed()
	{
		TalentNode? talent = _talents.FirstOrDefault(item => string.Equals(item.Id, _selectedTalentId, StringComparison.Ordinal));
		if (talent == null || !CanPurchase(talent))
		{
			return;
		}

		_purchasedTalentIds.Add(talent.Id);
		RecomputeSessionProgression();
		LoadWorkingDeckFromSession();
		RefreshAll();
	}

	private void OnRefundTalentPressed()
	{
		TalentNode? talent = _talents.FirstOrDefault(item => string.Equals(item.Id, _selectedTalentId, StringComparison.Ordinal));
		if (talent == null || !CanRefund(talent))
		{
			return;
		}

		_purchasedTalentIds.Remove(talent.Id);
		RecomputeSessionProgression();
		LoadWorkingDeckFromSession();
		RefreshAll();
	}

	private void OnCardCodexSelected(long index)
	{
		{
		if (_session == null || index < 0 || index >= _codexTemplates.Length)
		{
			return;
		}

		BattleCardTemplate selectedTemplate = _codexTemplates[index];
		ProgressionSnapshot cardProgression = _session.BuildProgressionSnapshotModel();
		bool cardUnlocked = selectedTemplate.IsOwned(cardProgression);
		_cardCodexDetail.Text = cardUnlocked
			? string.Join('\n', new[]
			{
				$"[b]{selectedTemplate.DisplayName}[/b]",
				selectedTemplate.Description,
				$"\u8D39\u7528 {selectedTemplate.Cost} / \u5F71\u54CD\u56E0\u5B50 {selectedTemplate.BuildPoints}",
				$"Quick {selectedTemplate.IsQuick} / Exhaust {selectedTemplate.ExhaustsOnPlay}",
				"\u72B6\u6001: \u5DF2\u89E3\u9501",
			})
			: string.Join('\n', new[]
			{
				"[b]\u672A\u89E3\u9501[/b]",
				"[\u9ED1\u8272\u526A\u5F71]",
				$"\u89E3\u9501\u65B9\u5F0F: {BuildReadableCardUnlockHint(selectedTemplate)}",
			});
		return;
		}

		if (_session == null || index < 0 || index >= _codexTemplates.Length)
		{
			return;
		}

		BattleCardTemplate template = _codexTemplates[index];
		ProgressionSnapshot progression = _session.BuildProgressionSnapshotModel();
		bool unlocked = template.IsOwned(progression);
		_cardCodexDetail.Text = unlocked
			? string.Join('\n', new[]
			{
				$"[b]{template.DisplayName}[/b]",
				template.Description,
				$"费用 {template.Cost} / 影响因子 {template.BuildPoints}",
				$"Quick {template.IsQuick} / Exhaust {template.ExhaustsOnPlay}",
				"状态: 已解锁",
			})
			: string.Join('\n', new[]
			{
				"[b]■■■[/b]",
				"[ 黑色剪影 ]",
				$"解锁方式: {BuildCardUnlockHint(template)}",
			});
	}

	private void OnEnemyCodexSelected(long index)
	{
		{
		if (_session == null || index < 0 || index >= _enemyCodexEntries.Length)
		{
			return;
		}

		EnemyCodexEntry selectedEntry = _enemyCodexEntries[index];
		bool enemyUnlocked = IsEnemyCodexUnlocked(selectedEntry, _session.BuildProgressionSnapshotModel());
		_enemyCodexDetail.Text = enemyUnlocked
			? string.Join('\n', new[]
			{
				$"[b]{selectedEntry.DisplayName}[/b]",
				selectedEntry.Description,
				"\u72B6\u6001: \u5DF2\u89E3\u9501",
			})
			: string.Join('\n', new[]
			{
				"[b]\u672A\u89E3\u9501[/b]",
				"[\u9ED1\u8272\u526A\u5F71]",
				$"\u89E3\u9501\u65B9\u5F0F: {selectedEntry.UnlockHint}",
			});
		return;
		}

		if (_session == null || index < 0 || index >= _enemyCodexEntries.Length)
		{
			return;
		}

		EnemyCodexEntry entry = _enemyCodexEntries[index];
		bool unlocked = IsEnemyCodexUnlocked(entry, _session.BuildProgressionSnapshotModel());
		_enemyCodexDetail.Text = unlocked
			? string.Join('\n', new[]
			{
				$"[b]{entry.DisplayName}[/b]",
				entry.Description,
				"状态: 已解锁",
			})
			: string.Join('\n', new[]
			{
				"[b]■■■[/b]",
				"[ 黑色剪影 ]",
				$"解锁方式: {entry.UnlockHint}",
			});
	}
	private bool CanPurchase(TalentNode talent)
	{
		return !_purchasedTalentIds.Contains(talent.Id) && ArePrerequisitesMet(talent) && GetAvailablePoints() >= talent.Cost;
	}

	private bool CanRefund(TalentNode talent)
	{
		if (IsDefaultUnlockedTalent(talent.Id))
		{
			return false;
		}

		return _purchasedTalentIds.Contains(talent.Id)
			&& !_talents.Any(other => _purchasedTalentIds.Contains(other.Id) && other.PrerequisiteTalentIds.Contains(talent.Id, StringComparer.Ordinal));
	}

	private bool ArePrerequisitesMet(TalentNode talent)
	{
		return talent.PrerequisiteTalentIds.All(requiredId => _purchasedTalentIds.Contains(requiredId));
	}

	private static string BuildCardUnlockHint(BattleCardTemplate template)
	{
		return BuildReadableCardUnlockHint(template);

		if (template.IsLearnedCard)
		{
			return "通过学习敌方招牌技能解锁";
		}

		if (template.RequiredTalentIds.Length > 0 || template.RequiredBranchTags.Length > 0)
		{
			return "满足天赋树要求并获得卡牌后解锁";
		}

		if (template.RequiredPlayerLevel > 1)
		{
			return $"角色达到 Lv.{template.RequiredPlayerLevel} 并获得卡牌后解锁";
		}

		return template.UnlockedByDefault ? "初始已解锁" : "通过探索、商店或奖励获得";
	}
	private bool IsEnemyCodexUnlocked(EnemyCodexEntry entry, ProgressionSnapshot progression)
	{
		return entry.UnlockedByDefault
			|| entry.RequiredUnlockedCardIds.All(cardId => progression.UnlockedCardIds.Contains(cardId, StringComparer.Ordinal));
	}

	private int GetSpentPoints()
	{
		return _talents.Where(talent => _purchasedTalentIds.Contains(talent.Id) && !IsDefaultUnlockedTalent(talent.Id)).Sum(talent => talent.Cost);
	}

	private static bool IsDefaultUnlockedTalent(string talentId)
	{
		return string.Equals(talentId, "talent_melee_root", StringComparison.Ordinal)
			|| string.Equals(talentId, "talent_ranged_root", StringComparison.Ordinal)
			|| string.Equals(talentId, "talent_flex_root", StringComparison.Ordinal);
	}

	private int GetAvailablePoints()
	{
		return Math.Max(0, _baseMasteryPoints - GetSpentPoints());
	}

	private void OnGrantPointPressed()
	{
		_baseMasteryPoints += 1;
		RecomputeSessionProgression();
		RefreshAll();
	}

	private void OnRevokePointPressed()
	{
		if (_baseMasteryPoints > GetSpentPoints())
		{
			_baseMasteryPoints -= 1;
			RecomputeSessionProgression();
			RefreshAll();
		}
	}

	private void OnResetTalentsPressed()
	{
		_purchasedTalentIds.Clear();
		_selectedTalentId = string.Empty;
		RecomputeSessionProgression();
		LoadWorkingDeckFromSession();
		RefreshAll();
	}

	private void OnSeedInventoryPressed()
	{
		SeedInventoryDefaults();
		RefreshStatusView();
		RefreshBagView();
	}

	private void OnClearInventoryPressed()
	{
		if (_session == null)
		{
			return;
		}

		_session.InventoryState.ItemCounts.Clear();
		_session.UnequipItem(EquipmentSlotIds.Weapon);
		_session.UnequipItem(EquipmentSlotIds.Armor);
		_session.UnequipItem(EquipmentSlotIds.Accessory);
		RefreshStatusView();
		RefreshBagView();
	}

	private void OnEquipmentSlotSelected(long index)
	{
		if (index < 0 || index >= EquipmentSlotOrder.Length)
		{
			return;
		}

		_selectedEquipmentSlotId = EquipmentSlotOrder[index];
		RefreshEquipmentSection();
	}

	private void OnEquipmentCandidateSelected(long index)
	{
		if (index < 0 || index >= _visibleEquipmentCandidates.Length)
		{
			return;
		}

		RefreshEquipmentDetail(_visibleEquipmentCandidates[index]);
	}

	private void OnEquipButtonPressed()
	{
		if (_session == null || _statusEquipmentCandidateList.GetSelectedItems().Length == 0)
		{
			return;
		}

		int selectedIndex = _statusEquipmentCandidateList.GetSelectedItems()[0];
		if (selectedIndex < 0 || selectedIndex >= _visibleEquipmentCandidates.Length)
		{
			return;
		}

		RuntimeEquipmentDefinition definition = _visibleEquipmentCandidates[selectedIndex];
		if (!_session.TryEquipItem(_selectedEquipmentSlotId, definition.ItemId, out _))
		{
			return;
		}

		RefreshAll();
		_statusEquipmentCandidateList.Select(selectedIndex);
		RefreshEquipmentDetail(definition);
	}

	private void OnUnequipButtonPressed()
	{
		if (_session == null)
		{
			return;
		}

		_session.UnequipItem(_selectedEquipmentSlotId);
		RefreshAll();
	}

	private void OnAvailableSelected(long index)
	{
		if (index < 0 || index >= _availableTemplates.Length)
		{
			return;
		}

		_deckDetailText.Text = BuildDeckDetailText(_availableTemplates[index]);
	}

	private void OnDeckSelected(long index)
	{
		if (index < 0 || index >= _workingDeck.Count || _cardLibrary == null)
		{
			return;
		}

		BattleCardTemplate? template = _cardLibrary.FindTemplate(_workingDeck[(int)index]);
		_deckDetailText.Text = template != null ? BuildDeckDetailText(template) : _workingDeck[(int)index];
	}

	private static string BuildDeckDetailText(BattleCardTemplate template)
	{
		return BuildReadableDeckDetailText(template);

		return string.Join('\n', new[]
		{
			$"[b]{template.DisplayName}[/b]",
			template.Description,
			$"璐圭敤 {template.Cost} / 褰卞搷鍥犲瓙 {template.BuildPoints}",
			$"浼ゅ {template.Damage} / 娌荤枟 {template.HealingAmount} / 鎶界墝 {template.DrawCount} / 鍥炶兘 {template.EnergyGain} / 鎶ょ浘 {template.ShieldGain}",
			$"Quick {template.IsQuick} / Exhaust {template.ExhaustsOnPlay}",
		});
	}

	private void OnDeckAddPressed()
	{
		if (_constructionService == null || _session == null || _availableList.GetSelectedItems().Length == 0)
		{
			return;
		}

		BattleCardTemplate template = _availableTemplates[_availableList.GetSelectedItems()[0]];
		List<string> candidateDeck = new(_workingDeck) { template.CardId };
		BattleDeckValidationResult validation = _constructionService.ValidateDeck(
			new DeckBuildSnapshot { CardIds = candidateDeck.ToArray(), RelicIds = _session.DeckBuildState.RelicIds },
			_session.BuildProgressionSnapshotModel());
		if (!validation.IsValid)
		{
			_deckValidationText.Text = BuildDeckValidationText(validation);
			return;
		}

		_workingDeck = candidateDeck;
		RefreshDeckView();
	}

	private void OnDeckRemovePressed()
	{
		if (_deckList.GetSelectedItems().Length == 0)
		{
			return;
		}

		_workingDeck.RemoveAt(_deckList.GetSelectedItems()[0]);
		RefreshDeckView();
	}

	private void OnDeckSavePressed()
	{
		{
		if (_session == null || _constructionService == null)
		{
			return;
		}

		DeckBuildSnapshot saveSnapshot = new()
		{
			BuildName = _session.DeckBuildState.BuildName,
			CardIds = _workingDeck.ToArray(),
			RelicIds = _session.DeckBuildState.RelicIds,
		};
		BattleDeckValidationResult saveValidation = _constructionService.ValidateDeck(saveSnapshot, _session.BuildProgressionSnapshotModel());
		if (!saveValidation.IsValid)
		{
			_deckValidationText.Text = BuildReadableDeckValidationText(saveValidation);
			return;
		}

		_session.ApplyDeckBuildSnapshot(saveSnapshot.ToDictionary());
		_deckValidationText.Text = BuildReadableDeckValidationText(saveValidation) + "\n\u5DF2\u4FDD\u5B58\u5230 GlobalGameSession";
		return;
		}

		if (_session == null || _constructionService == null)
		{
			return;
		}

		DeckBuildSnapshot snapshot = new()
		{
			BuildName = _session.DeckBuildState.BuildName,
			CardIds = _workingDeck.ToArray(),
			RelicIds = _session.DeckBuildState.RelicIds,
		};
		BattleDeckValidationResult validation = _constructionService.ValidateDeck(snapshot, _session.BuildProgressionSnapshotModel());
		if (!validation.IsValid)
		{
			_deckValidationText.Text = BuildDeckValidationText(validation);
			return;
		}

		_session.ApplyDeckBuildSnapshot(snapshot.ToDictionary());
		_deckValidationText.Text = BuildDeckValidationText(validation) + "\n宸蹭繚瀛樺埌 GlobalGameSession";
	}

	private void OnDeckResetPressed()
	{
		LoadWorkingDeckFromSession();
		RefreshDeckView();
	}

	private void OnDeckStarterPressed()
	{
		if (_cardLibrary == null)
		{
			return;
		}

		_workingDeck = _cardLibrary.BuildStarterDeckCardIds().ToList();
		RefreshDeckView();
	}

	private void SeedInventoryDefaults()
	{
		if (_session == null)
		{
			return;
		}

		Godot.Collections.Dictionary items = _session.InventoryState.ItemCounts;
		items.Clear();
		items["steel_scrap"] = 5;
		items["charged_core"] = 2;
		items["medical_gel"] = 3;
		items["optical_part"] = 1;
		items["equip_magnetic_scabbard"] = 1;
		items["equip_arc_pipe"] = 1;
		items["equip_old_coat"] = 1;
		items["equip_phase_boots"] = 1;
		items["equip_red_scarf"] = 1;
		items["equip_target_lens"] = 1;
		items["equip_archive_probe"] = 1;
		items["equip_parallel_battery"] = 1;
		items["equip_forbidden_patch"] = 1;
		items["equip_insulated_cloak"] = 1;
	}

	private void RecomputeSessionProgression()
	{
		if (_session == null)
		{
			return;
		}

		List<string> talentIds = new();
		List<string> branchTags = new();
		List<string> unlockedCardIds = new();
		int budgetBonus = 0;
		int copiesBonus = 0;

		foreach (TalentNode talent in _talents.Where(talent => _purchasedTalentIds.Contains(talent.Id)))
		{
			talentIds.Add(talent.Id);
			talentIds.AddRange(talent.GrantedTalentIds);
			branchTags.AddRange(talent.GrantedBranchTags);
			unlockedCardIds.AddRange(talent.UnlockedCardIds);
			budgetBonus += talent.DeckPointBudgetBonus;
			copiesBonus += talent.DeckMaxCopiesPerCardBonus;
		}

		_session.ProgressionState.PlayerLevel = Math.Max(3, _session.ProgressionState.PlayerLevel);
		int levelFloorExperience = _session.RuntimeProgressionRuleSet.GetAccumulatedExperienceForLevel(_session.ProgressionState.PlayerLevel);
		_session.ProgressionState.PlayerExperience = Math.Max(levelFloorExperience, _session.ProgressionState.PlayerExperience);
		_session.ProgressionState.PlayerMasteryPoints = GetAvailablePoints();
		_session.ProgressionState.TalentIds = talentIds.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray();
		_session.ProgressionState.TalentBranchTags = branchTags.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray();
		_session.ProgressionState.UnlockedCardIds = unlockedCardIds.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray();
		_session.ProgressionState.DeckPointBudgetBonus = budgetBonus;
		_session.ProgressionState.DeckMaxCopiesPerCardBonus = copiesBonus;

		_session.PlayerLevel = _session.ProgressionState.PlayerLevel;
		_session.PlayerExperience = _session.ProgressionState.PlayerExperience;
		_session.PlayerMasteryPoints = _session.ProgressionState.PlayerMasteryPoints;
		_session.TalentIds = _session.ProgressionState.TalentIds;
		_session.TalentBranchTags = _session.ProgressionState.TalentBranchTags;
		_session.UnlockedCardIds = _session.ProgressionState.UnlockedCardIds;
		_session.DeckPointBudgetBonus = budgetBonus;
		_session.DeckMaxCopiesPerCardBonus = copiesBonus;
		ApplyDebugCardUnlocks();
	}

	private void ApplyDebugCardUnlocks()
	{
		if (_session == null || _cardLibrary == null)
		{
			return;
		}

		string[] debugUnlockedCardIds = _cardLibrary.Entries
			.Where(template => template != null && !template.IsLearnedCard)
			.Select(template => template.CardId)
			.Where(cardId => !string.IsNullOrWhiteSpace(cardId))
			.Distinct(StringComparer.Ordinal)
			.ToArray();
		_session.ProgressionState.UnlockedCardIds = _session.ProgressionState.UnlockedCardIds
			.Concat(debugUnlockedCardIds)
			.Where(cardId => !string.IsNullOrWhiteSpace(cardId))
			.Distinct(StringComparer.Ordinal)
			.ToArray();
		_session.ProgressionState.TalentBranchTags = _session.ProgressionState.TalentBranchTags
			.Concat(new[] { "melee", "ranged", "flex" })
			.Where(tag => !string.IsNullOrWhiteSpace(tag))
			.Distinct(StringComparer.Ordinal)
			.ToArray();
		_session.UnlockedCardIds = _session.ProgressionState.UnlockedCardIds;
		_session.TalentBranchTags = _session.ProgressionState.TalentBranchTags;
	}

	private RuntimeEquipmentDefinition? FindEquipmentDefinition(string itemId)
	{
		return _session?.FindEquipmentDefinition(itemId);
	}

	private string GetEquipmentDisplayNameSafe(string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return "\u672A\u88C5\u5907";
		}

		return FindEquipmentDefinition(itemId)?.DisplayName ?? itemId;

		if (string.IsNullOrWhiteSpace(itemId))
		{
			return "未装备";
		}

		return FindEquipmentDefinition(itemId)?.DisplayName ?? itemId;
	}

	private static string GetEquipmentSlotDisplayNameSafe(string slotId)
	{
		if (string.Equals(slotId, EquipmentSlotIds.Weapon, StringComparison.Ordinal))
		{
			return "\u6B66\u5668";
		}

		if (string.Equals(slotId, EquipmentSlotIds.Armor, StringComparison.Ordinal))
		{
			return "\u62A4\u7532";
		}

		if (string.Equals(slotId, EquipmentSlotIds.Accessory, StringComparison.Ordinal))
		{
			return "\u914D\u4EF6";
		}

		return slotId;

		if (string.Equals(slotId, EquipmentSlotIds.Weapon, StringComparison.Ordinal))
		{
			return "武器";
		}

		if (string.Equals(slotId, EquipmentSlotIds.Armor, StringComparison.Ordinal))
		{
			return "护甲";
		}

		if (string.Equals(slotId, EquipmentSlotIds.Accessory, StringComparison.Ordinal))
		{
			return "配件";
		}

		return slotId;
	}

	private string GetEquipmentDisplayName(string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return "\u672A\u88C5\u5907";
		}

		return FindEquipmentDefinition(itemId)?.DisplayName ?? itemId;

		if (string.IsNullOrWhiteSpace(itemId))
		{
			return "未装备";
		}

		return FindEquipmentDefinition(itemId)?.DisplayName ?? itemId;
	}
	private static string GetEquipmentSlotDisplayName(string slotId)
	{
		return GetEquipmentSlotDisplayNameSafe(slotId);

		return slotId switch
		{
			EquipmentSlotIds.Weapon => "姝﹀櫒",
			EquipmentSlotIds.Armor => "鎶ょ敳",
			EquipmentSlotIds.Accessory => "楗板搧",
			_ => slotId,
		};
	}

	private string GetInventoryItemDisplayName(string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return string.Empty;
		}

		if (FindEquipmentDefinition(itemId) is RuntimeEquipmentDefinition definition)
		{
			return definition.DisplayName;
		}

		return itemId switch
		{
			"steel_scrap" => "\u94A2\u94C1\u788E\u7247",
			"charged_core" => "\u5145\u80FD\u6838\u5FC3",
			"medical_gel" => "\u533B\u7597\u51DD\u80F6",
			"optical_part" => "\u5149\u5B66\u96F6\u4EF6",
			_ => itemId,
		};
	}

	private string GetCardDisplayName(string cardId)
	{
		if (string.IsNullOrWhiteSpace(cardId))
		{
			return string.Empty;
		}

		return _cardLibrary?.FindTemplate(cardId)?.DisplayName ?? cardId;
	}

	private static string BuildReadableDeckValidationText(BattleDeckValidationResult validation)
	{
		List<string> lines = new()
		{
			$"\u6700\u4F4E\u5361\u6570: {validation.TotalCardCount}/{validation.EffectiveMinDeckSize}",
			$"\u5F71\u54CD\u56E0\u5B50: {validation.TotalBuildPoints}/{validation.EffectivePointBudget}",
			$"\u540C\u540D\u4E0A\u9650: {validation.EffectiveMaxCopiesPerCard}",
			$"\u8D85\u9650\u69FD\u4F4D: {validation.UsedOverlimitCarrySlots}/{validation.EffectiveOverlimitCarrySlots}",
		};

		if (validation.Errors.Count == 0)
		{
			lines.Add("\u72B6\u6001: \u901A\u8FC7");
		}
		else
		{
			lines.Add("\u72B6\u6001: \u672A\u901A\u8FC7");
			lines.AddRange(validation.Errors.Select(error => $"- {error}"));
		}

		return string.Join('\n', lines);
	}

	private static string BuildReadableDeckDetailText(BattleCardTemplate template)
	{
		return string.Join('\n', new[]
		{
			$"[b]{template.DisplayName}[/b]",
			template.Description,
			$"\u8D39\u7528 {template.Cost} / \u5F71\u54CD\u56E0\u5B50 {template.BuildPoints}",
			$"\u4F24\u5BB3 {template.Damage} / \u6CBB\u7597 {template.HealingAmount} / \u62BD\u724C {template.DrawCount} / \u56DE\u80FD {template.EnergyGain} / \u62A4\u76FE {template.ShieldGain}",
			$"Quick {template.IsQuick} / Exhaust {template.ExhaustsOnPlay}",
		});
	}

	private static string BuildReadableCardUnlockHint(BattleCardTemplate template)
	{
		if (template.IsLearnedCard)
		{
			return "\u901A\u8FC7\u5B66\u4E60\u654C\u65B9\u7279\u6B8A\u884C\u52A8\u89E3\u9501";
		}

		if (template.RequiredTalentIds.Length > 0 || template.RequiredBranchTags.Length > 0)
		{
			return "\u6EE1\u8DB3\u5929\u8D4B\u6811\u8981\u6C42\u5E76\u83B7\u5F97\u5361\u724C\u540E\u89E3\u9501";
		}

		if (template.RequiredPlayerLevel > 1)
		{
			return $"\u89D2\u8272\u8FBE\u5230 Lv.{template.RequiredPlayerLevel} \u5E76\u83B7\u5F97\u5361\u724C\u540E\u89E3\u9501";
		}

		return template.UnlockedByDefault
			? "\u521D\u59CB\u5DF2\u89E3\u9501"
			: "\u901A\u8FC7\u63A2\u7D22\u3001\u5546\u5E97\u6216\u5956\u52B1\u83B7\u5F97";
	}

	private enum TalentTreeGroup
	{
		Card = 0,
		Role = 1,
	}

	private sealed class TalentNode
	{
		public TalentNode(string id, string displayName, TalentTreeGroup group, string branchId, int cost, string description, Vector2 treePosition, string[] prerequisiteTalentIds, string[] grantedTalentIds, string[]? grantedBranchTags = null, string[]? unlockedCardIds = null, int deckPointBudgetBonus = 0, int deckMaxCopiesPerCardBonus = 0)
		{
			Id = id;
			DisplayName = displayName;
			Group = group;
			BranchId = branchId;
			Cost = cost;
			Description = description;
			TreePosition = treePosition;
			PrerequisiteTalentIds = prerequisiteTalentIds ?? Array.Empty<string>();
			GrantedTalentIds = grantedTalentIds ?? Array.Empty<string>();
			GrantedBranchTags = grantedBranchTags ?? Array.Empty<string>();
			UnlockedCardIds = unlockedCardIds ?? Array.Empty<string>();
			DeckPointBudgetBonus = deckPointBudgetBonus;
			DeckMaxCopiesPerCardBonus = deckMaxCopiesPerCardBonus;
		}

		public string Id { get; }
		public string DisplayName { get; }
		public TalentTreeGroup Group { get; }
		public string BranchId { get; }
		public int Cost { get; }
		public string Description { get; }
		public Vector2 TreePosition { get; }
		public string[] PrerequisiteTalentIds { get; }
		public string[] GrantedTalentIds { get; }
		public string[] GrantedBranchTags { get; }
		public string[] UnlockedCardIds { get; }
		public int DeckPointBudgetBonus { get; }
		public int DeckMaxCopiesPerCardBonus { get; }
	}

	private sealed class EnemyCodexEntry
	{
		public EnemyCodexEntry(string id, string displayName, string description, bool unlockedByDefault, string unlockHint, params string[] requiredUnlockedCardIds)
		{
			Id = id;
			DisplayName = displayName;
			Description = description;
			UnlockedByDefault = unlockedByDefault;
			UnlockHint = unlockHint;
			RequiredUnlockedCardIds = requiredUnlockedCardIds ?? Array.Empty<string>();
		}

		public string Id { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public bool UnlockedByDefault { get; }
		public string UnlockHint { get; }
		public string[] RequiredUnlockedCardIds { get; }
	}

	private sealed class EquipmentDefinition
	{
		public EquipmentDefinition(string itemId, string displayName, string slotId, string description, string bonusSummary)
		{
			ItemId = itemId;
			DisplayName = displayName;
			SlotId = slotId;
			Description = description;
			BonusSummary = bonusSummary;
		}

		public string ItemId { get; }
		public string DisplayName { get; }
		public string SlotId { get; }
		public string Description { get; }
		public string BonusSummary { get; }
	}
}
