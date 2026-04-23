using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using CardChessDemo.Battle.Actions;
using CardChessDemo.Battle.AI;
using CardChessDemo.Battle.Arakawa;
using CardChessDemo.Battle.Board;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Cards;
using CardChessDemo.Battle.Data;
using CardChessDemo.Battle.Enemies;
using CardChessDemo.Battle.Encounters;
using CardChessDemo.Battle.Equipment;
using CardChessDemo.Battle.Presentation;
using CardChessDemo.Battle.Rooms;
using CardChessDemo.Battle.Shared;
using CardChessDemo.Battle.State;
using CardChessDemo.Battle.Turn;
using CardChessDemo.Battle.UI;
using CardChessDemo.Battle.Visual;
using CardChessDemo.Audio;

namespace CardChessDemo.Battle;

public partial class BattleSceneController : Node2D
{
	private const double PlayerActionResolveBufferSeconds = 0.24d;
	private const string DrawRevolverCardId = "draw_revolver";
	private const string ArcLeakCardId = "card_arc_leak";
	private const string RamCardId = "card_ram";
	private const string AimCardId = "card_aim";
	private const string SnipeCardId = "card_snipe";
	private const string AlertCardId = "card_alert";
	private const string RollCallCardId = "card_roll_call";
	private const string LearningCardId = "card_learning";
	private const string PunchCardId = "card_punch";
	private const string GuardCardId = "card_guard";
	private const string RollCardId = "card_roll";
	private const string CalmCardId = "card_calm";
	private const string ChargeUpCardId = "card_charge_up";
	private const string BootCardId = "card_boot";
	private const string AlertGuardCardId = "card_alert_guard";
	private const string PlunderCardId = "card_plunder";
	private const string StructuralBoostCardId = "card_structural_boost";
	private const string TacticalShiftCardId = "card_tactical_shift";
	private const string VaultCardId = "card_vault";
	private const string OptimizeCardId = "card_optimize";
	private const string ContemplateCardId = "card_contemplate";
	private const string RepairCardId = "card_repair";
	private const string FieldPatchPlusCardId = "card_field_patch_plus";
	private const string MagneticHuntCardId = "card_magnetic_hunt";
	private const string MagneticHuntSkillId = "magnetic_hunt";
	private const string CaptainBashSkillId = "captain_bash";
	private const string FlameGridSkillId = "flame_grid";
	private const string CallCrewSkillId = "call_crew";
	private const string BossCallCrewUsedFlagId = "boss_call_crew_used";
	private const string StanceCardId = "card_stance";
	private const string HeavyBlowCardId = "card_heavy_blow";
	private const string ConcussionShotCardId = "card_concussion_shot";
	private const string WeatheringCardId = "card_weathering";
	private const string DrawnRevolverWeaponItemId = "drawn_revolver";
	private const string PressureBreachSkillId = "pressure_breach";
	private const string EmergencyRepairSkillId = "emergency_repair";
	private const int DrawnRevolverBasicAttackCharges = 6;
	private const string BattleBackgroundTexturePath = "res://Assets/Background/94180512_p2_master1200.jpg";
	private const string BattleReturnTransitionOverlayScenePath = "res://Scene/Transitions/BattleReturnTransitionOverlay.tscn";
	private const string MainMenuScenePath = "res://Scene/UI/MainMenu.tscn";
	private const string BossRustCaptainEncounterId = "boss_rust_captain";
	private static readonly ArakawaAbilityDefinition BuildWallAbility = new("build_wall", "\u9020\u7269", 1);
	private static readonly ArakawaAbilityDefinition EnhanceCardAbility = new("enhance_card", "\u5F3A\u5316\u5361\u724C", 1);
	private static readonly ArakawaAbilityDefinition EnhanceWeaponAbility = new("enhance_weapon", "\u5F3A\u5316\u6B66\u5668", 1);
	private static readonly IReadOnlyDictionary<string, BattleCardEnhancementDefinition> PrototypeCardEnhancements =
		new Dictionary<string, BattleCardEnhancementDefinition>(StringComparer.Ordinal)
		{
			["cross_slash"] = new BattleCardEnhancementDefinition("+", "婵犵數濮烽弫鍛婃叏閻戣棄鏋侀柛娑橈攻閸欏繘鏌ｉ幋锝嗩棄闁哄绶氶弻娑樷槈濮楀牊鏁鹃梺鍛婄懃缁绘﹢寮婚敐澶婄闁挎繂妫Λ鍕⒑閸濆嫷鍎庣紒鑸靛哺瀵鈽夊Ο閿嬵潔濠殿喗顨呴悧濠囧极妤ｅ啯鈷戦柛娑橈功閹冲啰绱掔紒姗堣€跨€殿喖顭烽弫鎰緞婵犲嫷鍚呴梻浣瑰缁诲倸螞椤撶倣娑㈠礋椤栨稈鎷洪梺鍛婄箓鐎氱兘宕曟惔锝囩＜闁兼悂娼ч崫铏光偓娈垮枛椤兘骞冮姀銈呯閻忓繑鐗楃€氫粙姊虹拠鏌ュ弰婵炰匠鍕彾濠电姴浼ｉ敐澶樻晩闁告挆鍜冪床闂備浇顕栭崹搴ㄥ礃閿濆棗鐦辩紓鍌氬€风欢锟犲闯椤曗偓瀹曞綊骞庨挊澶岊唹闂侀潧绻掓慨顓炍ｉ崼銉︾厪闊洦娲栧暩濡炪倖鎸搁幖顐﹀煘閹达附鍊烽柛娆忣樈濡偟绱撴担铏瑰笡閻㈩垪鈧磭鏆︽繝闈涱儏缁犵粯銇勯弮鍥嗘帡骞忓ú顏呯厸濠㈣泛鑻禒锕€顭块悷鐗堫棦閽樻繈鏌ㄩ弬娆炬綗濞存粍绮撻弻鐔衡偓娑欘焽缁犳牗銇勯妷锝呯仼闁宠鍨块弫宥夊礋椤掍焦鐦撻柣搴ゎ潐濞叉粓宕伴弽顓溾偓浣糕槈濮楀棙鍍垫俊鎻掓湰閻楁洟寮查鍫熲拻闁稿本鐟чˇ锕傛煙鐠囇呯瘈闁靛棗鍟村畷濂稿Ψ閵壯嶇幢闂備胶绮濠氬储瑜忕划鍫熷緞閹邦厾鍘电紓鍌欓檷閸ㄥ綊寮搁悢鍏肩厓闂佸灝顑呴悘瀵糕偓娈垮枛閻栧ジ骞冨▎鎾崇骇闁规惌鍘介ˉ瀣繆閻愵亜鈧呮媼閿濆洨涓嶉柟鎹愵嚙缁犳牗淇婇妶鍌氫壕濡炪値鍋呯换鍫ャ€侀弴銏狀潊闁绘鍋ｉ崑鏍⒑鐠囨彃顒㈡い鏃€鐗犲畷浼村冀椤撴稈鍋撻敃鍌涘€婚柦妯侯槹閻庮剟姊洪悷鏉库挃缂侇噮鍨跺畷鎴﹀箛閻楀牏鍘剧紒鐐緲瀹曨剚鏅堕鈧弻宥堫檨闁告挻鐩畷鎴﹀Χ閸滀焦缍庡┑鐐叉▕娴滄繈藟閸喓绠鹃柟杈剧秮閸濇椽鎳栭弽顓熲拻濞达絼璀﹂悞鐐叏濮楀牏鐣遍柍璇茬Ч瀵挳鎮╅悽鐢靛姽闂備礁婀遍崕銈夈€冮崱娑樺惞闁告劦鍠楅悡鍐煕濠靛棗顏╅柡鍡欏枛閺屻劌鈽夊▎鎴犵厜濠殿喖锕ㄥ▍锝囨閹烘嚦鐔煎传閸曟垶娲熷娲川婵犲啫闉嶉梺鑽ゅ暱閺呮盯鎮惧畡鎵虫斀閻庯綆鈧厞鍥ㄧ厱闁靛鍨哄▍鍥煥濞戞瑧绠為柟顔筋殜閻涱噣宕归鐓庮潛闂備礁缍婇弨閬嶅垂閸ф鍋傛い鎺戝閺佸棝鏌涚仦缁㈡當濞存粎鍋撶换娑㈠醇濠靛牅铏庨梺鍝勵儐缁嬫帡濡甸崟顖ｆ晣闁绘ɑ褰冮獮瀣倵濞堝灝娅橀柛瀣躬閻涱喖螣閼测晝锛滃┑鈽嗗灥濡椼劌效濡ゅ懏鈷掑ù锝堟娴滃綊鏌嶅畡鎵ⅵ鐎规洘绮岄埢搴ㄥ箻瀹曞洤濮︽俊鐐€栫敮鎺楀磹閸涘﹦顩烽弶鍫涘妽閸欏繐鈹戦悩鎻掓殲闁靛洦绻冩穱濠囶敃閵忕姵娈梺瀹犳椤︽澘顭囪箛娑樜╅柟鎵虫櫅閳ь剚绻傞～蹇涙惞閸︻厾鐓撻梺鍓茬厛閸ｎ噣宕濇径瀣瘈闁汇垽娼ф禒婊堟煥閺囨ê鈧繈骞冮幆褏鏆嬮柟浣冩珪閺傗偓闂佽鍑界紞鍡樼閻愪警鏁婇柟鐑樺焾濞撳鏌曢崼婵囶棡妞ゃ儱妫涢幃顔尖枎閹惧鍘?2", damageDelta: 2),
			["quick_cut"] = new BattleCardEnhancementDefinition("+", "婵犵數濮烽弫鍛婃叏閻戣棄鏋侀柛娑橈攻閸欏繘鏌ｉ幋锝嗩棄闁哄绶氶弻娑樷槈濮楀牊鏁鹃梺鍛婄懃缁绘﹢寮婚敐澶婄闁挎繂妫Λ鍕⒑閸濆嫷鍎庣紒鑸靛哺瀵鈽夊Ο閿嬵潔濠殿喗顨呴悧濠囧极妤ｅ啯鈷戦柛娑橈功閹冲啰绱掔紒姗堣€跨€殿喖顭烽弫鎰緞婵犲嫷鍚呴梻浣瑰缁诲倸螞椤撶倣娑㈠礋椤栨稈鎷洪梺鍛婄箓鐎氱兘宕曟惔锝囩＜闁兼悂娼ч崫铏光偓娈垮枛椤兘骞冮姀銈呯閻忓繑鐗楃€氫粙姊虹拠鏌ュ弰婵炰匠鍕彾濠电姴浼ｉ敐澶樻晩闁告挆鍜冪床闂備浇顕栭崹搴ㄥ礃閿濆棗鐦辩紓鍌氬€风欢锟犲闯椤曗偓瀹曞綊骞庨挊澶岊唹闂侀潧绻掓慨顓炍ｉ崼銉︾厪闊洦娲栧暩濡炪倖鎸搁幖顐﹀煘閹达附鍊烽柛娆忣樈濡偟绱撴担铏瑰笡閻㈩垪鈧磭鏆︽繝闈涱儏缁犵粯銇勯弮鍥嗘帡骞忓ú顏呯厸濠㈣泛鑻禒锕€顭块悷鐗堫棦閽樻繈鏌ㄩ弬娆炬綗濞存粍绮撻弻鐔衡偓娑欘焽缁犳牗銇勯妷锝呯仼闁宠鍨块弫宥夊礋椤掍焦鐦撻柣搴ゎ潐濞叉粓宕伴弽顓溾偓浣糕槈濮楀棙鍍垫俊鎻掓湰閻楁洟寮查鍫熲拻闁稿本鐟чˇ锕傛煙鐠囇呯瘈闁靛棗鍟村畷濂稿Ψ閵壯嶇幢闂備胶绮濠氬储瑜忕划鍫熷緞閹邦厾鍘电紓鍌欓檷閸ㄥ綊寮搁悢鍏肩厓闂佸灝顑呴悘瀵糕偓娈垮枛閻栧ジ骞冨▎鎾崇骇闁规惌鍘介ˉ瀣繆閻愵亜鈧呮媼閿濆洨涓嶉柟鎹愵嚙缁犳牗淇婇妶鍌氫壕濡炪値鍋呯换鍫ャ€侀弴銏狀潊闁绘鍋ｉ崑鏍⒑鐠囨彃顒㈡い鏃€鐗犲畷浼村冀椤撴稈鍋撻敃鍌涘€婚柦妯侯槹閻庮剟姊洪悷鏉库挃缂侇噮鍨跺畷鎴﹀箛閻楀牏鍘剧紒鐐緲瀹曨剚鏅堕鈧弻宥堫檨闁告挻鐩畷鎴﹀Χ閸滀焦缍庡┑鐐叉▕娴滄繈藟閸喓绠鹃柟杈剧秮閸濇椽鎳栭弽顓熲拻濞达絼璀﹂悞鐐叏濮楀牏鐣遍柍璇茬Ч瀵挳鎮╅悽鐢靛姽闂備礁婀遍崕銈夈€冮崱娑樺惞闁告劦鍠楅悡鍐煕濠靛棗顏╅柡鍡欏枛閺屻劌鈽夊▎鎴犵厜濠殿喖锕ㄥ▍锝囨閹烘嚦鐔煎传閸曟垶娲熷娲川婵犲啫闉嶉梺鑽ゅ暱閺呮盯鎮惧畡鎵虫斀閻庯綆鈧厞鍥ㄧ厱闁靛鍨哄▍鍥煥濞戞瑧绠為柟顔筋殜閻涱噣宕归鐓庮潛闂備礁缍婇弨閬嶅垂閸ф鍋傛い鎺戝閺佸棝鏌涚仦缁㈡當濞存粎鍋撶换娑㈠醇濠靛牅铏庨梺鍝勵儐缁嬫帡濡甸崟顖ｆ晣闁绘ɑ褰冮獮瀣倵濞堝灝娅橀柛瀣躬閻涱喖螣閼测晝锛滃┑鈽嗗灥濡椼劌效濡ゅ懏鈷掑ù锝堟娴滃綊鏌嶅畡鎵ⅵ鐎规洘绮岄埢搴ㄥ箻瀹曞洤濮︽俊鐐€栫敮鎺楀磹閸涘﹦顩烽弶鍫涘妽閸欏繐鈹戦悩鎻掓殲闁靛洦绻冩穱濠囶敃閵忕姵娈梺瀹犳椤︽澘顭囪箛娑樜╅柟鎵虫櫅閳ь剚绻傞～蹇涙惞閸︻厾鐓撻梺鍓茬厛閸ｎ噣宕濇径瀣瘈闁汇垽娼ф禒婊堟煥閺囨ê鈧繈骞冮幆褏鏆嬮柟浣冩珪閺傗偓闂佽鍑界紞鍡樼閻愪警鏁婇柟鐑樺焾濞撳鏌曢崼婵囶棡妞ゃ儱妫涢幃顔尖枎閹惧鍘?1", damageDelta: 1),
			["line_shot"] = new BattleCardEnhancementDefinition("+", "婵犵數濮烽弫鍛婃叏閻戣棄鏋侀柛娑橈攻閸欏繘鏌ｉ幋锝嗩棄闁哄绶氶弻娑樷槈濮楀牊鏁鹃梺鍛婄懃缁绘﹢寮婚敐澶婄闁挎繂妫Λ鍕⒑閸濆嫷鍎庣紒鑸靛哺瀵鈽夊Ο閿嬵潔濠殿喗顨呴悧濠囧极妤ｅ啯鈷戦柛娑橈功閹冲啰绱掔紒姗堣€跨€殿喖顭烽弫鎰緞婵犲嫷鍚呴梻浣瑰缁诲倸螞椤撶倣娑㈠礋椤栨稈鎷洪梺鍛婄箓鐎氱兘宕曟惔锝囩＜闁兼悂娼ч崫铏光偓娈垮枛椤兘骞冮姀銈呯閻忓繑鐗楃€氫粙姊虹拠鏌ュ弰婵炰匠鍕彾濠电姴浼ｉ敐澶樻晩闁告挆鍜冪床闂備浇顕栭崹搴ㄥ礃閿濆棗鐦辩紓鍌氬€风欢锟犲闯椤曗偓瀹曞綊骞庨挊澶岊唹闂侀潧绻掓慨顓炍ｉ崼銉︾厪闊洦娲栧暩濡炪倖鎸搁幖顐﹀煘閹达附鍊烽柛娆忣樈濡偟绱撴担铏瑰笡閻㈩垪鈧磭鏆︽繝闈涱儏缁犵粯銇勯弮鍥嗘帡骞忓ú顏呯厸濠㈣泛鑻禒锕€顭块悷鐗堫棦閽樻繈鏌ㄩ弬娆炬綗濞存粍绮撻弻鐔衡偓娑欘焽缁犳牗銇勯妷锝呯仼闁宠鍨块弫宥夊礋椤掍焦鐦撻柣搴ゎ潐濞叉粓宕伴弽顓溾偓浣糕槈濮楀棙鍍垫俊鎻掓湰閻楁洟寮查鍫熲拻闁稿本鐟чˇ锕傛煙鐠囇呯瘈闁靛棗鍟村畷濂稿Ψ閵壯嶇幢闂備胶绮濠氬储瑜忕划鍫熷緞閹邦厾鍘电紓鍌欓檷閸ㄥ綊寮搁悢鍏肩厓闂佸灝顑呴悘瀵糕偓娈垮枛閻栧ジ骞冨▎鎾崇骇闁规惌鍘介ˉ瀣繆閻愵亜鈧呮媼閿濆洨涓嶉柟鎹愵嚙缁犳牗淇婇妶鍌氫壕濡炪値鍋呯换鍫ャ€侀弴銏狀潊闁绘鍋ｉ崑鏍⒑鐠囨彃顒㈡い鏃€鐗犲畷浼村冀椤撴稈鍋撻敃鍌涘€婚柦妯侯槹閻庮剟姊洪悷鏉库挃缂侇噮鍨跺畷鎴﹀箛閻楀牏鍘剧紒鐐緲瀹曨剚鏅堕鈧弻宥堫檨闁告挻鐩畷鎴﹀Χ閸滀焦缍庡┑鐐叉▕娴滄繈藟閸喓绠鹃柟杈剧秮閸濇椽鎳栭弽顓熲拻濞达絼璀﹂悞鐐叏濮楀牏鐣遍柍璇茬Ч瀵挳鎮╅悽鐢靛姽闂備礁婀遍崕銈夈€冮崱娑樺惞闁告劦鍠楅悡鍐煕濠靛棗顏╅柡鍡欏枛閺屻劌鈽夊▎鎴犵厜濠殿喖锕ㄥ▍锝囨閹烘嚦鐔煎传閸曟垶娲熷娲川婵犲啫闉嶉梺鑽ゅ暱閺呮盯鎮惧畡鎵虫斀閻庯綆鈧厞鍥ㄧ厱闁靛鍨哄▍鍥煥濞戞瑧绠為柟顔筋殜閻涱噣宕归鐓庮潛闂備礁缍婇弨閬嶅垂閸ф鍋傛い鎺戝閺佸棝鏌涚仦缁㈡當濞存粎鍋撶换娑㈠醇濠靛牅铏庨梺鍝勵儐缁嬫帡濡甸崟顖ｆ晣闁绘ɑ褰冮獮瀣倵濞堝灝娅橀柛瀣躬閻涱喖螣閼测晝锛滃┑鈽嗗灥濡椼劌效濡ゅ懏鈷掑ù锝堟娴滃綊鏌嶅畡鎵ⅵ鐎规洘绮岄埢搴ㄥ箻瀹曞洤濮︽俊鐐€栫敮鎺楀磹閸涘﹦顩烽弶鍫涘妽閸欏繐鈹戦悩鎻掓殲闁靛洦绻冩穱濠囶敃閵忕姵娈梺瀹犳椤︽澘顭囪箛娑樜╅柟鎵虫櫅閳ь剚绻傞～蹇涙惞閸︻厾鐓撻梺鍓茬厛閸ｎ噣宕濇径瀣瘈闁汇垽娼ф禒婊堟煥閺囨ê鈧繈骞冮幆褏鏆嬮柟浣冩珪閺傗偓闂佽鍑界紞鍡樼閻愪警鏁婇柟鐑樺焾濞撳鏌曢崼婵囶棡妞ゃ儱妫涢幃顔尖枎閹惧鍘?2", damageDelta: 2),
			["heavy_shot"] = new BattleCardEnhancementDefinition("+", "婵犵數濮烽弫鍛婃叏閻戣棄鏋侀柛娑橈攻閸欏繘鏌ｉ幋锝嗩棄闁哄绶氶弻娑樷槈濮楀牊鏁鹃梺鍛婄懃缁绘﹢寮婚敐澶婄闁挎繂妫Λ鍕⒑閸濆嫷鍎庣紒鑸靛哺瀵鈽夊Ο閿嬵潔濠殿喗顨呴悧濠囧极妤ｅ啯鈷戦柛娑橈功閹冲啰绱掔紒姗堣€跨€殿喖顭烽弫鎰緞婵犲嫷鍚呴梻浣瑰缁诲倸螞椤撶倣娑㈠礋椤栨稈鎷洪梺鍛婄箓鐎氱兘宕曟惔锝囩＜闁兼悂娼ч崫铏光偓娈垮枛椤兘骞冮姀銈呯閻忓繑鐗楃€氫粙姊虹拠鏌ュ弰婵炰匠鍕彾濠电姴浼ｉ敐澶樻晩闁告挆鍜冪床闂備浇顕栭崹搴ㄥ礃閿濆棗鐦辩紓鍌氬€风欢锟犲闯椤曗偓瀹曞綊骞庨挊澶岊唹闂侀潧绻掓慨顓炍ｉ崼銉︾厪闊洦娲栧暩濡炪倖鎸搁幖顐﹀煘閹达附鍊烽柛娆忣樈濡偟绱撴担铏瑰笡閻㈩垪鈧磭鏆︽繝闈涱儏缁犵粯銇勯弮鍥嗘帡骞忓ú顏呯厸濠㈣泛鑻禒锕€顭块悷鐗堫棦閽樻繈鏌ㄩ弬娆炬綗濞存粍绮撻弻鐔衡偓娑欘焽缁犳牗銇勯妷锝呯仼闁宠鍨块弫宥夊礋椤掍焦鐦撻柣搴ゎ潐濞叉粓宕伴弽顓溾偓浣糕槈濮楀棙鍍垫俊鎻掓湰閻楁洟寮查鍫熲拻闁稿本鐟чˇ锕傛煙鐠囇呯瘈闁靛棗鍟村畷濂稿Ψ閵壯嶇幢闂備胶绮濠氬储瑜忕划鍫熷緞閹邦厾鍘电紓鍌欓檷閸ㄥ綊寮搁悢鍏肩厓闂佸灝顑呴悘瀵糕偓娈垮枛閻栧ジ骞冨▎鎾崇骇闁规惌鍘介ˉ瀣繆閻愵亜鈧呮媼閿濆洨涓嶉柟鎹愵嚙缁犳牗淇婇妶鍌氫壕濡炪値鍋呯换鍫ャ€侀弴銏狀潊闁绘鍋ｉ崑鏍⒑鐠囨彃顒㈡い鏃€鐗犲畷浼村冀椤撴稈鍋撻敃鍌涘€婚柦妯侯槹閻庮剟姊洪悷鏉库挃缂侇噮鍨跺畷鎴﹀箛閻楀牏鍘剧紒鐐緲瀹曨剚鏅堕鈧弻宥堫檨闁告挻鐩畷鎴﹀Χ閸滀焦缍庡┑鐐叉▕娴滄繈藟閸喓绠鹃柟杈剧秮閸濇椽鎳栭弽顓熲拻濞达絼璀﹂悞鐐叏濮楀牏鐣遍柍璇茬Ч瀵挳鎮╅悽鐢靛姽闂備礁婀遍崕銈夈€冮崱娑樺惞闁告劦鍠楅悡鍐煕濠靛棗顏╅柡鍡欏枛閺屻劌鈽夊▎鎴犵厜濠殿喖锕ㄥ▍锝囨閹烘嚦鐔煎传閸曟垶娲熷娲川婵犲啫闉嶉梺鑽ゅ暱閺呮盯鎮惧畡鎵虫斀閻庯綆鈧厞鍥ㄧ厱闁靛鍨哄▍鍥煥濞戞瑧绠為柟顔筋殜閻涱噣宕归鐓庮潛闂備礁缍婇弨閬嶅垂閸ф鍋傛い鎺戝閺佸棝鏌涚仦缁㈡當濞存粎鍋撶换娑㈠醇濠靛牅铏庨梺鍝勵儐缁嬫帡濡甸崟顖ｆ晣闁绘ɑ褰冮獮瀣倵濞堝灝娅橀柛瀣躬閻涱喖螣閼测晝锛滃┑鈽嗗灥濡椼劌效濡ゅ懏鈷掑ù锝堟娴滃綊鏌嶅畡鎵ⅵ鐎规洘绮岄埢搴ㄥ箻瀹曞洤濮︽俊鐐€栫敮鎺楀磹閸涘﹦顩烽弶鍫涘妽閸欏繐鈹戦悩鎻掓殲闁靛洦绻冩穱濠囶敃閵忕姵娈梺瀹犳椤︽澘顭囪箛娑樜╅柟鎵虫櫅閳ь剚绻傞～蹇涙惞閸︻厾鐓撻梺鍓茬厛閸ｎ噣宕濇径瀣瘈闁汇垽娼ф禒婊堟煥閺囨ê鈧繈骞冮幆褏鏆嬮柟浣冩珪閺傗偓闂佽鍑界紞鍡樼閻愪警鏁婇柟鐑樺焾濞撳鏌曢崼婵囶棡妞ゃ儱妫涢幃顔尖枎閹惧鍘?2", damageDelta: 2),
			["battle_read"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棙鐨戠紒杈ㄦ崌瀹曟帒顫濋钘変壕闁绘垼濮ら崵鍕煠缁嬭法浠涢柛娆忕箻濮婃椽宕归鍛壋缂備胶濞€缁犳牠寮诲鍫闂佸憡鎸鹃崰鏍ь嚕婵犳艾鐏崇€规洖娲﹀▓鏇㈡⒑闁偛鑻晶瀵糕偓瑙勬磻閸楀啿顕ｆ禒瀣垫晣闁绘灏欓妶锕傛⒒娴ｈ櫣銆婇柛鎾寸箞瀹曟瑩鎮烽幏鏃撶稻閵堬綁宕橀埞鐐闂備線娼荤€靛矂宕㈤挊澶嗘瀺闁哄洨鍋愰弨鑺ャ亜閺冨倸浜鹃柡鍡忔櫊閺岀喖鐛崹顔句患闂佸疇妫勯ˇ鍨叏閳ь剟鏌ｅΟ鍨毢闁绘稈鏅滄穱濠囨倷椤忓嫧鍋撻弽顓熷亱婵°倕鎳庨弰銉╂煕閹伴潧鏋涢柛鎴犲█閺屾稑鈽夐崡鐐茬闂佸搫鍟悧濠囧磻閹扮増鈷掗柛顐ゅ枔閳笺儲绻涘畝濠侀偗婵﹨娅ｇ槐鎺戭潨閸℃鏆︽繝鐢靛仜瀵墎鎹㈤崱娑樜﹂柟閭﹀幗婵挳鏌涢幘鍐茬骇閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝椤ㄥ﹤鐣烽崨濠勭瘈闁搞儯鍔庨崢閬嶆煟鎼搭垳绉甸柛瀣噽娴滄悂顢橀姀锛勫帗闁荤喐鐟ョ€氼剟鎮橀幘顔界厱闁靛ň鏅濋悾娲煙椤旂晫鎳囬柟绋匡攻瀵板嫭寰勬繝鍌滀喊闂傚倸鍊搁崐椋庣矆娓氣偓楠炴牠顢曢敂钘夊壒婵犮垼娉涢惉鑲╃矆婵犲伅褰掓晲婢跺棗浜炬繛鎴炴皑閻ｉ箖姊绘担绋款棌闁稿鎳庣叅闁哄稁鍘介崐宄扳攽閻樻彃顏柛鐘冲姍閹﹢鎮欓幓鎺嗘寖闂佺粯绻傞悥濂稿蓟閿熺姴鐐婇柕澶堝劤娴犻箖姊洪幎鑺ユ暠闁搞劌鐏濋～蹇旂節濮橆剟鍞堕梺缁樻閵嗏偓濠殿喖娲娲偡閺夋寧鍊梺鍛婃⒐閻熲晠鐛崘銊庢棃鍩€椤掑嫸缍栨繝闈涱儛閺佸啴鏌曡箛鏇炐ラ柕鍫櫍濮婄粯鎷呴搹鐟扮婵炴挻纰嶉〃濠傜暦閹惰姤鏅查柛娑卞灡濞堥箖姊洪崜鎻掍簼婵炶绠撳顐﹀幢濞戞瑧鍘撻悷婊勭矒瀹曟粌鈽夊顒€鐏婇梺鍓插亖閸庨亶鏌嬮崶銊х瘈闁靛骏绱曠粻鎾剁磽瀹ュ拑韬€殿噮鍋婇獮鍥级閸ф鏁规繝鐢靛█濞佳囨偋閸曨垰纾婚柨婵嗩槹閳锋帒霉閿濆洤鍔嬮柛銈傚亾闂備礁顓介弶鍨瀷缂傚倸鍊归幐濠氬Χ閿濆绀冮柕濞у啯鎲?1", drawCountDelta: 1),
			["meditate"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棙鐨戠紒杈ㄦ崌瀹曟帒顫濋钘変壕闁绘垼濮ら崵鍕煠缁嬭法浠涢柛娆忕箻濮婃椽宕归鍛壋缂備胶濞€缁犳牠寮诲鍫闂佸憡鎸鹃崰鏍ь嚕婵犳艾鐏崇€规洖娲﹀▓鏇㈡⒑闁偛鑻晶瀵糕偓瑙勬磻閸楀啿顕ｆ禒瀣垫晣闁绘灏欓妶锕傛⒒娴ｈ櫣銆婇柛鎾寸箞瀹曟瑩鎮烽幏鏃撶稻閵堬綁宕橀埞鐐闂備線娼荤€靛矂宕㈤挊澶嗘瀺闁哄洨鍋愰弨鑺ャ亜閺冨倸浜鹃柡鍡忔櫊閺岀喖鐛崹顔句患闂佸疇妫勯ˇ鍨叏閳ь剟鏌ｅΟ鍨毢闁绘稈鏅滄穱濠囨倷椤忓嫧鍋撻弽顓熷亱婵°倕鎳庨弰銉╂煕閹伴潧鏋涢柛鎴犲█閺屾稑鈽夐崡鐐茬闂佸搫鍟悧濠囧磻閹扮増鈷掗柛顐ゅ枔閳笺儲绻涘畝濠侀偗婵﹨娅ｇ槐鎺戭潨閸℃鏆︽繝鐢靛仜瀵墎鎹㈤崱娑樜﹂柟閭﹀幗婵挳鏌涢幘鍐茬骇閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝椤ㄥ﹤鐣烽崨濠勭瘈闁搞儯鍔庨崢閬嶆煟鎼搭垳绉甸柛瀣噽娴滄悂顢橀姀锛勫帗闁荤喐鐟ョ€氼剟鎮橀幘顔界厱闁靛ň鏅濋悾娲煙椤旂晫鎳囬柟绋匡攻瀵板嫭寰勬繝鍌滀喊闂傚倸鍊搁崐椋庣矆娓氣偓楠炴牠顢曢敂钘夊壒婵犮垼娉涢惉鑲╃矆婵犲伅褰掓晲婢跺棗浜炬繛鎴炴皑閻ｉ箖姊绘担绋款棌闁稿鎳庣叅闁哄稁鍘介崐宄扳攽閻樻彃顏柛鐘冲姍閹﹢鎮欓幓鎺嗘寖闂佺粯绻傞悥濂稿蓟閿熺姴鐐婇柕澶堝劤娴犻箖姊洪幎鑺ユ暠闁搞劌鐏濋～蹇旂節濮橆剟鍞堕梺缁樻閵嗏偓濠殿喖娲娲偡閺夋寧鍊梺鍛婃⒐閻熲晠鐛崘銊庢棃鍩€椤掑嫸缍栨繝闈涱儛閺佸啴鏌曡箛鏇炐ラ柕鍫櫍濮婄粯鎷呴搹鐟扮婵炴挻纰嶉〃濠傜暦閹惰姤鏅查柛娑卞灡濞堥箖姊洪崜鎻掍簼婵炶绠撳顐﹀幢濞戞瑧鍘撻悷婊勭矒瀹曟粌鈽夊顒€鐏婇梺鍓插亖閸庨亶鏌嬮崶銊х瘈闁靛骏绱曠粻鎾剁磽瀹ュ拑韬€殿噮鍋婇獮鍥级閸ф鏁规繝鐢靛█濞佳囨偋閸曨垰纾婚柨婵嗩槹閳锋帒霉閿濆洤鍔嬮柛銈傚亾闂備礁顓介弶鍨瀷缂傚倸鍊归幐濠氬Χ閿濆绀冮柕濞у啯鎲?1", drawCountDelta: 1),
			["surge"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棗鍝烘慨濠傤煼瀹曟帒鈻庨幒鎴濆腐婵＄偑鍊ら崢濂告偋韫囨稑绠栨い鏇楀亾妞ゃ垺娲熼弫鍐焵椤掑倻鐭嗗鑸靛姇缁犺绻涢敐搴″濠德ゅ亹缁辨帡鎮╅棃娑欑亪闂佸搫鐬奸崰鏍ь潖閼姐倐鍋撻棃娑橆棌婵″樊鍠栭埞鎴﹀煡閸℃ぞ绨奸梺鎸庢磸閸ㄤ粙鐛崘顓滀汗闁圭儤鍨归悾鎶芥⒑閸︻厼鍔嬮柛銊ф暬閳ユ牗寰勬繛鐐杸闂佺粯鍔樼亸娆愭櫠閿斿墽纾奸柣妯虹－婢ь亝銇勯銏㈢閻撱倖銇勮箛鎾村婵☆偄鍟埞鎴︽倷閺夋垹浠稿┑顔角滈崝鎴﹀箖閸ф绀嬫い鏍ㄧ▓閹疯櫣绱撴担鍓插剰閻忓繐鎳樺銊х磼濡偐顔曢梺鑲┾拡閸撴瑩寮告惔銊︾厽闁挎繂鎳庡Σ缁樸亜椤愶絿鐭掗柛鈹惧亾濡炪倖甯掔€氼剟寮伴妷鈺傜厓鐟滄粓宕滃璺何﹂柛鏇ㄥ灠缁犳娊鏌涢埄鍐︿沪濠㈣娲樻穱濠囨倷椤忓嫧鍋撻妶鍡欘洸婵犲﹤鐗嗛悿顕€鎮楀☉娅偐鎹㈤崱娑欑厱妞ゆ劧绲剧粈鈧Δ鐘靛亼閸ㄧ儤绌辨繝鍥ч柛灞剧煯婢规洘淇婇悙顏勨偓銈夊磻閸曨個娲敇閻戝棙缍庡┑鐐叉▕娴滄粎绮昏ぐ鎺撶厽闁归偊鍘肩徊缁樸亜椤掆偓閻楀繒妲愰幘璇茬＜婵ɑ鐦烽姀銈嗙叆闁哄洢鍔嬮柇顖炴煟濞戝崬娅嶇€规洘锕㈡俊鍛婃償閿濆懏鐏堥梺鍦劜缁绘繃淇婇崼鏇炵倞闁冲搫鍋嗗鎾绘⒒閸屾艾鈧兘鎳楅崼鏇椻偓锕傚醇閵夈儱鐝樺銈嗗笒鐎氥劑鍩€椤掆偓閸熸潙鐣烽崡鐐╂婵☆垱妞垮Λ鐔兼⒒娓氣偓閳ь剛鍋涢懟顖涙櫠鐎涙ɑ鍙忓┑鐘插鐢盯鏌熷畡鐗堝殗闁圭厧缍婇幃鐑藉箥椤曞懎浠归梻鍌氬€风粈渚€骞夐垾鎰佹綎闁荤喐鍣村ú顏勫唨妞ゆ挆鍕珨闂備焦瀵х换鍌炈囬鐐寸厑闁搞儺鍓氶悡娑㈡煕閵夘垳鍒板ù婊呭亾缁绘繈濮€閿濆懐鍘梺鍛婃⒐閻楃姴鐣烽幇鐗堝€婚柤鎭掑劤閸樹粙姊洪悷閭﹀殶闁稿孩鍔欓幃鐐寸鐎ｎ偆鍘卞┑鈽嗗灠閸氬寮抽浣瑰弿濠电姴鍟妵婵堚偓瑙勬磸閸斿秶鎹㈠┑瀣闁靛瀵屽鏃堟⒒閸屾瑧鍔嶉悗绗涘厾楦跨疀濞戞锛熼梻鍌氱墛缁嬫捇寮抽敃鍌涚厵闁绘鐗婄欢鑼磼閻樺磭鈯曢柕鍥у楠炴鎹勯悜妯尖偓濠氭倵?1", energyGainDelta: 1),
			["draw_spark"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棙鐨戠紒杈ㄦ崌瀹曟帒顫濋钘変壕闁绘垼濮ら崵鍕煠缁嬭法浠涢柛娆忕箻濮婃椽宕归鍛壋缂備胶濞€缁犳牠寮诲鍫闂佸憡鎸鹃崰鏍ь嚕婵犳艾鐏崇€规洖娲﹀▓鏇㈡⒑闁偛鑻晶瀵糕偓瑙勬磻閸楀啿顕ｆ禒瀣垫晣闁绘灏欓妶锕傛⒒娴ｈ櫣銆婇柛鎾寸箞瀹曟瑩鎮烽幏鏃撶稻閵堬綁宕橀埞鐐闂備線娼荤€靛矂宕㈤挊澶嗘瀺闁哄洨鍋愰弨鑺ャ亜閺冨倸浜鹃柡鍡忔櫊閺岀喖鐛崹顔句患闂佸疇妫勯ˇ鍨叏閳ь剟鏌ｅΟ鍨毢闁绘稈鏅滄穱濠囨倷椤忓嫧鍋撻弽顓熷亱婵°倕鎳庨弰銉╂煕閹伴潧鏋涢柛鎴犲█閺屾稑鈽夐崡鐐茬闂佸搫鍟悧濠囧磻閹扮増鈷掗柛顐ゅ枔閳笺儲绻涘畝濠侀偗婵﹨娅ｇ槐鎺戭潨閸℃鏆︽繝鐢靛仜瀵墎鎹㈤崱娑樜﹂柟閭﹀幗婵挳鏌涢幘鍐茬骇閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝椤ㄥ﹤鐣烽崨濠勭瘈闁搞儯鍔庨崢閬嶆煟鎼搭垳绉甸柛瀣噽娴滄悂顢橀姀锛勫帗闁荤喐鐟ョ€氼剟鎮橀幘顔界厱闁靛ň鏅濋悾娲煙椤旂晫鎳囬柟绋匡攻瀵板嫭寰勬繝鍌滀喊闂傚倸鍊搁崐椋庣矆娓氣偓楠炴牠顢曢敂钘夊壒婵犮垼娉涢惉鑲╃矆婵犲伅褰掓晲婢跺棗浜炬繛鎴炴皑閻ｉ箖姊绘担绋款棌闁稿鎳庣叅闁哄稁鍘介崐宄扳攽閻樻彃顏柛鐘冲姍閹﹢鎮欓幓鎺嗘寖闂佺粯绻傞悥濂稿蓟閿熺姴鐐婇柕澶堝劤娴犻箖姊洪幎鑺ユ暠闁搞劌鐏濋～蹇旂節濮橆剟鍞堕梺缁樻閵嗏偓濠殿喖娲娲偡閺夋寧鍊梺鍛婃⒐閻熲晠鐛崘銊庢棃鍩€椤掑嫸缍栨繝闈涱儛閺佸啴鏌曡箛鏇炐ラ柕鍫櫍濮婄粯鎷呴搹鐟扮婵炴挻纰嶉〃濠傜暦閹惰姤鏅查柛娑卞灡濞堥箖姊洪崜鎻掍簼婵炶绠撳顐﹀幢濞戞瑧鍘撻悷婊勭矒瀹曟粌鈽夊顒€鐏婇梺鍓插亖閸庨亶鏌嬮崶銊х瘈闁靛骏绱曠粻鎾剁磽瀹ュ拑韬€殿噮鍋婇獮鍥级閸ф鏁规繝鐢靛█濞佳囨偋閸曨垰纾婚柨婵嗩槹閳锋帒霉閿濆洤鍔嬮柛銈傚亾闂備礁顓介弶鍨瀷缂傚倸鍊归幐濠氬Χ閿濆绀冮柕濞у啯鎲?1", drawCountDelta: 1),
			["quick_plan"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棙鐨戠紒杈ㄦ崌瀹曟帒顫濋钘変壕闁绘垼濮ら崵鍕煠缁嬭法浠涢柛娆忕箻濮婃椽宕归鍛壋缂備胶濞€缁犳牠寮诲鍫闂佸憡鎸鹃崰鏍ь嚕婵犳艾鐏崇€规洖娲﹀▓鏇㈡⒑闁偛鑻晶瀵糕偓瑙勬磻閸楀啿顕ｆ禒瀣垫晣闁绘灏欓妶锕傛⒒娴ｈ櫣銆婇柛鎾寸箞瀹曟瑩鎮烽幏鏃撶稻閵堬綁宕橀埞鐐闂備線娼荤€靛矂宕㈤挊澶嗘瀺闁哄洨鍋愰弨鑺ャ亜閺冨倸浜鹃柡鍡忔櫊閺岀喖鐛崹顔句患闂佸疇妫勯ˇ鍨叏閳ь剟鏌ｅΟ鍨毢闁绘稈鏅滄穱濠囨倷椤忓嫧鍋撻弽顓熷亱婵°倕鎳庨弰銉╂煕閹伴潧鏋涢柛鎴犲█閺屾稑鈽夐崡鐐茬闂佸搫鍟悧濠囧磻閹扮増鈷掗柛顐ゅ枔閳笺儲绻涘畝濠侀偗婵﹨娅ｇ槐鎺戭潨閸℃鏆︽繝鐢靛仜瀵墎鎹㈤崱娑樜﹂柟閭﹀幗婵挳鏌涢幘鍐茬骇閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝椤ㄥ﹤鐣烽崨濠勭瘈闁搞儯鍔庨崢閬嶆煟鎼搭垳绉甸柛瀣噽娴滄悂顢橀姀锛勫帗闁荤喐鐟ョ€氼剟鎮橀幘顔界厱闁靛ň鏅濋悾娲煙椤旂晫鎳囬柟绋匡攻瀵板嫭寰勬繝鍌滀喊闂傚倸鍊搁崐椋庣矆娓氣偓楠炴牠顢曢敂钘夊壒婵犮垼娉涢惉鑲╃矆婵犲伅褰掓晲婢跺棗浜炬繛鎴炴皑閻ｉ箖姊绘担绋款棌闁稿鎳庣叅闁哄稁鍘介崐宄扳攽閻樻彃顏柛鐘冲姍閹﹢鎮欓幓鎺嗘寖闂佺粯绻傞悥濂稿蓟閿熺姴鐐婇柕澶堝劤娴犻箖姊洪幎鑺ユ暠闁搞劌鐏濋～蹇旂節濮橆剟鍞堕梺缁樻閵嗏偓濠殿喖娲娲偡閺夋寧鍊梺鍛婃⒐閻熲晠鐛崘銊庢棃鍩€椤掑嫸缍栨繝闈涱儛閺佸啴鏌曡箛鏇炐ラ柕鍫櫍濮婄粯鎷呴搹鐟扮婵炴挻纰嶉〃濠傜暦閹惰姤鏅查柛娑卞灡濞堥箖姊洪崜鎻掍簼婵炶绠撳顐﹀幢濞戞瑧鍘撻悷婊勭矒瀹曟粌鈽夊顒€鐏婇梺鍓插亖閸庨亶鏌嬮崶銊х瘈闁靛骏绱曠粻鎾剁磽瀹ュ拑韬€殿噮鍋婇獮鍥级閸ф鏁规繝鐢靛█濞佳囨偋閸曨垰纾婚柨婵嗩槹閳锋帒霉閿濆洤鍔嬮柛銈傚亾闂備礁顓介弶鍨瀷缂傚倸鍊归幐濠氬Χ閿濆绀冮柕濞у啯鎲?1", drawCountDelta: 1),
			["burning_edge"] = new BattleCardEnhancementDefinition("+", "婵犵數濮烽弫鍛婃叏閻戣棄鏋侀柛娑橈攻閸欏繘鏌ｉ幋锝嗩棄闁哄绶氶弻娑樷槈濮楀牊鏁鹃梺鍛婄懃缁绘﹢寮婚敐澶婄闁挎繂妫Λ鍕⒑閸濆嫷鍎庣紒鑸靛哺瀵鈽夊Ο閿嬵潔濠殿喗顨呴悧濠囧极妤ｅ啯鈷戦柛娑橈功閹冲啰绱掔紒姗堣€跨€殿喖顭烽弫鎰緞婵犲嫷鍚呴梻浣瑰缁诲倸螞椤撶倣娑㈠礋椤栨稈鎷洪梺鍛婄箓鐎氱兘宕曟惔锝囩＜闁兼悂娼ч崫铏光偓娈垮枛椤兘骞冮姀銈呯閻忓繑鐗楃€氫粙姊虹拠鏌ュ弰婵炰匠鍕彾濠电姴浼ｉ敐澶樻晩闁告挆鍜冪床闂備浇顕栭崹搴ㄥ礃閿濆棗鐦辩紓鍌氬€风欢锟犲闯椤曗偓瀹曞綊骞庨挊澶岊唹闂侀潧绻掓慨顓炍ｉ崼銉︾厪闊洦娲栧暩濡炪倖鎸搁幖顐﹀煘閹达附鍊烽柛娆忣樈濡偟绱撴担铏瑰笡閻㈩垪鈧磭鏆︽繝闈涱儏缁犵粯銇勯弮鍥嗘帡骞忓ú顏呯厸濠㈣泛鑻禒锕€顭块悷鐗堫棦閽樻繈鏌ㄩ弬娆炬綗濞存粍绮撻弻鐔衡偓娑欘焽缁犳牗銇勯妷锝呯仼闁宠鍨块弫宥夊礋椤掍焦鐦撻柣搴ゎ潐濞叉粓宕伴弽顓溾偓浣糕槈濮楀棙鍍垫俊鎻掓湰閻楁洟寮查鍫熲拻闁稿本鐟чˇ锕傛煙鐠囇呯瘈闁靛棗鍟村畷濂稿Ψ閵壯嶇幢闂備胶绮濠氬储瑜忕划鍫熷緞閹邦厾鍘电紓鍌欓檷閸ㄥ綊寮搁悢鍏肩厓闂佸灝顑呴悘瀵糕偓娈垮枛閻栧ジ骞冨▎鎾崇骇闁规惌鍘介ˉ瀣繆閻愵亜鈧呮媼閿濆洨涓嶉柟鎹愵嚙缁犳牗淇婇妶鍌氫壕濡炪値鍋呯换鍫ャ€侀弴銏狀潊闁绘鍋ｉ崑鏍⒑鐠囨彃顒㈡い鏃€鐗犲畷浼村冀椤撴稈鍋撻敃鍌涘€婚柦妯侯槹閻庮剟姊洪悷鏉库挃缂侇噮鍨跺畷鎴﹀箛閻楀牏鍘剧紒鐐緲瀹曨剚鏅堕鈧弻宥堫檨闁告挻鐩畷鎴﹀Χ閸滀焦缍庡┑鐐叉▕娴滄繈藟閸喓绠鹃柟杈剧秮閸濇椽鎳栭弽顓熲拻濞达絼璀﹂悞鐐叏濮楀牏鐣遍柍璇茬Ч瀵挳鎮╅悽鐢靛姽闂備礁婀遍崕銈夈€冮崱娑樺惞闁告劦鍠楅悡鍐煕濠靛棗顏╅柡鍡欏枛閺屻劌鈽夊▎鎴犵厜濠殿喖锕ㄥ▍锝囨閹烘嚦鐔煎传閸曟垶娲熷娲川婵犲啫闉嶉梺鑽ゅ暱閺呮盯鎮惧畡鎵虫斀閻庯綆鈧厞鍥ㄧ厱闁靛鍨哄▍鍥煥濞戞瑧绠為柟顔筋殜閻涱噣宕归鐓庮潛闂備礁缍婇弨閬嶅垂閸ф鍋傛い鎺戝閺佸棝鏌涚仦缁㈡當濞存粎鍋撶换娑㈠醇濠靛牅铏庨梺鍝勵儐缁嬫帡濡甸崟顖ｆ晣闁绘ɑ褰冮獮瀣倵濞堝灝娅橀柛瀣躬閻涱喖螣閼测晝锛滃┑鈽嗗灥濡椼劌效濡ゅ懏鈷掑ù锝堟娴滃綊鏌嶅畡鎵ⅵ鐎规洘绮岄埢搴ㄥ箻瀹曞洤濮︽俊鐐€栫敮鎺楀磹閸涘﹦顩烽弶鍫涘妽閸欏繐鈹戦悩鎻掓殲闁靛洦绻冩穱濠囶敃閵忕姵娈梺瀹犳椤︽澘顭囪箛娑樜╅柟鎵虫櫅閳ь剚绻傞～蹇涙惞閸︻厾鐓撻梺鍓茬厛閸ｎ噣宕濇径瀣瘈闁汇垽娼ф禒婊堟煥閺囨ê鈧繈骞冮幆褏鏆嬮柟浣冩珪閺傗偓闂佽鍑界紞鍡樼閻愪警鏁婇柟鐑樺焾濞撳鏌曢崼婵囶棡妞ゃ儱妫涢幃顔尖枎閹惧鍘?2", damageDelta: 2),
			["hook_shot"] = new BattleCardEnhancementDefinition("+", "婵犵數濮烽弫鍛婃叏閻戣棄鏋侀柛娑橈攻閸欏繘鏌ｉ幋锝嗩棄闁哄绶氶弻娑樷槈濮楀牊鏁鹃梺鍛婄懃缁绘﹢寮婚敐澶婄闁挎繂妫Λ鍕⒑閸濆嫷鍎庣紒鑸靛哺瀵鈽夊Ο閿嬵潔濠殿喗顨呴悧濠囧极妤ｅ啯鈷戦柛娑橈功閹冲啰绱掔紒姗堣€跨€殿喖顭烽弫鎰緞婵犲嫷鍚呴梻浣瑰缁诲倸螞椤撶倣娑㈠礋椤栨稈鎷洪梺鍛婄箓鐎氱兘宕曟惔锝囩＜闁兼悂娼ч崫铏光偓娈垮枛椤兘骞冮姀銈呯閻忓繑鐗楃€氫粙姊虹拠鏌ュ弰婵炰匠鍕彾濠电姴浼ｉ敐澶樻晩闁告挆鍜冪床闂備浇顕栭崹搴ㄥ礃閿濆棗鐦辩紓鍌氬€风欢锟犲闯椤曗偓瀹曞綊骞庨挊澶岊唹闂侀潧绻掓慨顓炍ｉ崼銉︾厪闊洦娲栧暩濡炪倖鎸搁幖顐﹀煘閹达附鍊烽柛娆忣樈濡偟绱撴担铏瑰笡閻㈩垪鈧磭鏆︽繝闈涱儏缁犵粯銇勯弮鍥嗘帡骞忓ú顏呯厸濠㈣泛鑻禒锕€顭块悷鐗堫棦閽樻繈鏌ㄩ弬娆炬綗濞存粍绮撻弻鐔衡偓娑欘焽缁犳牗銇勯妷锝呯仼闁宠鍨块弫宥夊礋椤掍焦鐦撻柣搴ゎ潐濞叉粓宕伴弽顓溾偓浣糕槈濮楀棙鍍垫俊鎻掓湰閻楁洟寮查鍫熲拻闁稿本鐟чˇ锕傛煙鐠囇呯瘈闁靛棗鍟村畷濂稿Ψ閵壯嶇幢闂備胶绮濠氬储瑜忕划鍫熷緞閹邦厾鍘电紓鍌欓檷閸ㄥ綊寮搁悢鍏肩厓闂佸灝顑呴悘瀵糕偓娈垮枛閻栧ジ骞冨▎鎾崇骇闁规惌鍘介ˉ瀣繆閻愵亜鈧呮媼閿濆洨涓嶉柟鎹愵嚙缁犳牗淇婇妶鍌氫壕濡炪値鍋呯换鍫ャ€侀弴銏狀潊闁绘鍋ｉ崑鏍⒑鐠囨彃顒㈡い鏃€鐗犲畷浼村冀椤撴稈鍋撻敃鍌涘€婚柦妯侯槹閻庮剟姊洪悷鏉库挃缂侇噮鍨跺畷鎴﹀箛閻楀牏鍘剧紒鐐緲瀹曨剚鏅堕鈧弻宥堫檨闁告挻鐩畷鎴﹀Χ閸滀焦缍庡┑鐐叉▕娴滄繈藟閸喓绠鹃柟杈剧秮閸濇椽鎳栭弽顓熲拻濞达絼璀﹂悞鐐叏濮楀牏鐣遍柍璇茬Ч瀵挳鎮╅悽鐢靛姽闂備礁婀遍崕銈夈€冮崱娑樺惞闁告劦鍠楅悡鍐煕濠靛棗顏╅柡鍡欏枛閺屻劌鈽夊▎鎴犵厜濠殿喖锕ㄥ▍锝囨閹烘嚦鐔煎传閸曟垶娲熷娲川婵犲啫闉嶉梺鑽ゅ暱閺呮盯鎮惧畡鎵虫斀閻庯綆鈧厞鍥ㄧ厱闁靛鍨哄▍鍥煥濞戞瑧绠為柟顔筋殜閻涱噣宕归鐓庮潛闂備礁缍婇弨閬嶅垂閸ф鍋傛い鎺戝閺佸棝鏌涚仦缁㈡當濞存粎鍋撶换娑㈠醇濠靛牅铏庨梺鍝勵儐缁嬫帡濡甸崟顖ｆ晣闁绘ɑ褰冮獮瀣倵濞堝灝娅橀柛瀣躬閻涱喖螣閼测晝锛滃┑鈽嗗灥濡椼劌效濡ゅ懏鈷掑ù锝堟娴滃綊鏌嶅畡鎵ⅵ鐎规洘绮岄埢搴ㄥ箻瀹曞洤濮︽俊鐐€栫敮鎺楀磹閸涘﹦顩烽弶鍫涘妽閸欏繐鈹戦悩鎻掓殲闁靛洦绻冩穱濠囶敃閵忕姵娈梺瀹犳椤︽澘顭囪箛娑樜╅柟鎵虫櫅閳ь剚绻傞～蹇涙惞閸︻厾鐓撻梺鍓茬厛閸ｎ噣宕濇径瀣瘈闁汇垽娼ф禒婊堟煥閺囨ê鈧繈骞冮幆褏鏆嬮柟浣冩珪閺傗偓闂佽鍑界紞鍡樼閻愪警鏁婇柟鐑樺焾濞撳鏌曢崼婵囶棡妞ゃ儱妫涢幃顔尖枎閹惧鍘?2", damageDelta: 2),
			["deep_focus"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棙鐨戠紒杈ㄦ崌瀹曟帒顫濋钘変壕闁绘垼濮ら崵鍕煠缁嬭法浠涢柛娆忕箻濮婃椽宕归鍛壋缂備胶濞€缁犳牠寮诲鍫闂佸憡鎸鹃崰鏍ь嚕婵犳艾鐏崇€规洖娲﹀▓鏇㈡⒑闁偛鑻晶瀵糕偓瑙勬磻閸楀啿顕ｆ禒瀣垫晣闁绘灏欓妶锕傛⒒娴ｈ櫣銆婇柛鎾寸箞瀹曟瑩鎮烽幏鏃撶稻閵堬綁宕橀埞鐐闂備線娼荤€靛矂宕㈤挊澶嗘瀺闁哄洨鍋愰弨鑺ャ亜閺冨倸浜鹃柡鍡忔櫊閺岀喖鐛崹顔句患闂佸疇妫勯ˇ鍨叏閳ь剟鏌ｅΟ鍨毢闁绘稈鏅滄穱濠囨倷椤忓嫧鍋撻弽顓熷亱婵°倕鎳庨弰銉╂煕閹伴潧鏋涢柛鎴犲█閺屾稑鈽夐崡鐐茬闂佸搫鍟悧濠囧磻閹扮増鈷掗柛顐ゅ枔閳笺儲绻涘畝濠侀偗婵﹨娅ｇ槐鎺戭潨閸℃鏆︽繝鐢靛仜瀵墎鎹㈤崱娑樜﹂柟閭﹀幗婵挳鏌涢幘鍐茬骇閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝椤ㄥ﹤鐣烽崨濠勭瘈闁搞儯鍔庨崢閬嶆煟鎼搭垳绉甸柛瀣噽娴滄悂顢橀姀锛勫帗闁荤喐鐟ョ€氼剟鎮橀幘顔界厱闁靛ň鏅濋悾娲煙椤旂晫鎳囬柟绋匡攻瀵板嫭寰勬繝鍌滀喊闂傚倸鍊搁崐椋庣矆娓氣偓楠炴牠顢曢敂钘夊壒婵犮垼娉涢惉鑲╃矆婵犲伅褰掓晲婢跺棗浜炬繛鎴炴皑閻ｉ箖姊绘担绋款棌闁稿鎳庣叅闁哄稁鍘介崐宄扳攽閻樻彃顏柛鐘冲姍閹﹢鎮欓幓鎺嗘寖闂佺粯绻傞悥濂稿蓟閿熺姴鐐婇柕澶堝劤娴犻箖姊洪幎鑺ユ暠闁搞劌鐏濋～蹇旂節濮橆剟鍞堕梺缁樻閵嗏偓濠殿喖娲娲偡閺夋寧鍊梺鍛婃⒐閻熲晠鐛崘銊庢棃鍩€椤掑嫸缍栨繝闈涱儛閺佸啴鏌曡箛鏇炐ラ柕鍫櫍濮婄粯鎷呴搹鐟扮婵炴挻纰嶉〃濠傜暦閹惰姤鏅查柛娑卞灡濞堥箖姊洪崜鎻掍簼婵炶绠撳顐﹀幢濞戞瑧鍘撻悷婊勭矒瀹曟粌鈽夊顒€鐏婇梺鍓插亖閸庨亶鏌嬮崶銊х瘈闁靛骏绱曠粻鎾剁磽瀹ュ拑韬€殿噮鍋婇獮鍥级閸ф鏁规繝鐢靛█濞佳囨偋閸曨垰纾婚柨婵嗩槹閳锋帒霉閿濆洤鍔嬮柛銈傚亾闂備礁顓介弶鍨瀷缂傚倸鍊归幐濠氬Χ閿濆绀冮柕濞у啯鎲?1", drawCountDelta: 1),
			["spark_charge"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棗鍝烘慨濠傤煼瀹曟帒鈻庨幒鎴濆腐婵＄偑鍊ら崢濂告偋韫囨稑绠栨い鏇楀亾妞ゃ垺娲熼弫鍐焵椤掑倻鐭嗗鑸靛姇缁犺绻涢敐搴″濠德ゅ亹缁辨帡鎮╅棃娑欑亪闂佸搫鐬奸崰鏍ь潖閼姐倐鍋撻棃娑橆棌婵″樊鍠栭埞鎴﹀煡閸℃ぞ绨奸梺鎸庢磸閸ㄤ粙鐛崘顓滀汗闁圭儤鍨归悾鎶芥⒑閸︻厼鍔嬮柛銊ф暬閳ユ牗寰勬繛鐐杸闂佺粯鍔樼亸娆愭櫠閿斿墽纾奸柣妯虹－婢ь亝銇勯銏㈢閻撱倖銇勮箛鎾村婵☆偄鍟埞鎴︽倷閺夋垹浠稿┑顔角滈崝鎴﹀箖閸ф绀嬫い鏍ㄧ▓閹疯櫣绱撴担鍓插剰閻忓繐鎳樺銊х磼濡偐顔曢梺鑲┾拡閸撴瑩寮告惔銊︾厽闁挎繂鎳庡Σ缁樸亜椤愶絿鐭掗柛鈹惧亾濡炪倖甯掔€氼剟寮伴妷鈺傜厓鐟滄粓宕滃璺何﹂柛鏇ㄥ灠缁犳娊鏌涢埄鍐︿沪濠㈣娲樻穱濠囨倷椤忓嫧鍋撻妶鍡欘洸婵犲﹤鐗嗛悿顕€鎮楀☉娅偐鎹㈤崱娑欑厱妞ゆ劧绲剧粈鈧Δ鐘靛亼閸ㄧ儤绌辨繝鍥ч柛灞剧煯婢规洘淇婇悙顏勨偓銈夊磻閸曨個娲敇閻戝棙缍庡┑鐐叉▕娴滄粎绮昏ぐ鎺撶厽闁归偊鍘肩徊缁樸亜椤掆偓閻楀繒妲愰幘璇茬＜婵ɑ鐦烽姀銈嗙叆闁哄洢鍔嬮柇顖炴煟濞戝崬娅嶇€规洘锕㈡俊鍛婃償閿濆懏鐏堥梺鍦劜缁绘繃淇婇崼鏇炵倞闁冲搫鍋嗗鎾绘⒒閸屾艾鈧兘鎳楅崼鏇椻偓锕傚醇閵夈儱鐝樺銈嗗笒鐎氥劑鍩€椤掆偓閸熸潙鐣烽崡鐐╂婵☆垱妞垮Λ鐔兼⒒娓氣偓閳ь剛鍋涢懟顖涙櫠鐎涙ɑ鍙忓┑鐘插鐢盯鏌熷畡鐗堝殗闁圭厧缍婇幃鐑藉箥椤曞懎浠归梻鍌氬€风粈渚€骞夐垾鎰佹綎闁荤喐鍣村ú顏勫唨妞ゆ挆鍕珨闂備焦瀵х换鍌炈囬鐐寸厑闁搞儺鍓氶悡娑㈡煕閵夘垳鍒板ù婊呭亾缁绘繈濮€閿濆懐鍘梺鍛婃⒐閻楃姴鐣烽幇鐗堝€婚柤鎭掑劤閸樹粙姊洪悷閭﹀殶闁稿孩鍔欓幃鐐寸鐎ｎ偆鍘卞┑鈽嗗灠閸氬寮抽浣瑰弿濠电姴鍟妵婵堚偓瑙勬磸閸斿秶鎹㈠┑瀣闁靛瀵屽鏃堟⒒閸屾瑧鍔嶉悗绗涘厾楦跨疀濞戞锛熼梻鍌氱墛缁嬫捇寮抽敃鍌涚厵闁绘鐗婄欢鑼磼閻樺磭鈯曢柕鍥у楠炴鎹勯悜妯尖偓濠氭倵?1", energyGainDelta: 1),
			["burst_drive"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹垾鐔煎箒瀹ュ棗鍝烘慨濠傤煼瀹曟帒鈻庨幒鎴濆腐婵＄偑鍊ら崢濂告偋韫囨稑绠栨い鏇楀亾妞ゃ垺娲熼弫鍐焵椤掑倻鐭嗗鑸靛姇缁犺绻涢敐搴″濠德ゅ亹缁辨帡鎮╅棃娑欑亪闂佸搫鐬奸崰鏍ь潖閼姐倐鍋撻棃娑橆棌婵″樊鍠栭埞鎴﹀煡閸℃ぞ绨奸梺鎸庢磸閸ㄤ粙鐛崘顓滀汗闁圭儤鍨归悾鎶芥⒑閸︻厼鍔嬮柛銊ф暬閳ユ牗寰勬繛鐐杸闂佺粯鍔樼亸娆愭櫠閿斿墽纾奸柣妯虹－婢ь亝銇勯銏㈢閻撱倖銇勮箛鎾村婵☆偄鍟埞鎴︽倷閺夋垹浠稿┑顔角滈崝鎴﹀箖閸ф绀嬫い鏍ㄧ▓閹疯櫣绱撴担鍓插剰閻忓繐鎳樺銊х磼濡偐顔曢梺鑲┾拡閸撴瑩寮告惔銊︾厽闁挎繂鎳庡Σ缁樸亜椤愶絿鐭掗柛鈹惧亾濡炪倖甯掔€氼剟寮伴妷鈺傜厓鐟滄粓宕滃璺何﹂柛鏇ㄥ灠缁犳娊鏌涢埄鍐︿沪濠㈣娲樻穱濠囨倷椤忓嫧鍋撻妶鍡欘洸婵犲﹤鐗嗛悿顕€鎮楀☉娅偐鎹㈤崱娑欑厱妞ゆ劧绲剧粈鈧Δ鐘靛亼閸ㄧ儤绌辨繝鍥ч柛灞剧煯婢规洘淇婇悙顏勨偓銈夊磻閸曨個娲敇閻戝棙缍庡┑鐐叉▕娴滄粎绮昏ぐ鎺撶厽闁归偊鍘肩徊缁樸亜椤掆偓閻楀繒妲愰幘璇茬＜婵ɑ鐦烽姀銈嗙叆闁哄洢鍔嬮柇顖炴煟濞戝崬娅嶇€规洘锕㈡俊鍛婃償閿濆懏鐏堥梺鍦劜缁绘繃淇婇崼鏇炵倞闁冲搫鍋嗗鎾绘⒒閸屾艾鈧兘鎳楅崼鏇椻偓锕傚醇閵夈儱鐝樺銈嗗笒鐎氥劑鍩€椤掆偓閸熸潙鐣烽崡鐐╂婵☆垱妞垮Λ鐔兼⒒娓氣偓閳ь剛鍋涢懟顖涙櫠鐎涙ɑ鍙忓┑鐘插鐢盯鏌熷畡鐗堝殗闁圭厧缍婇幃鐑藉箥椤曞懎浠归梻鍌氬€风粈渚€骞夐垾鎰佹綎闁荤喐鍣村ú顏勫唨妞ゆ挆鍕珨闂備焦瀵х换鍌炈囬鐐寸厑闁搞儺鍓氶悡娑㈡煕閵夘垳鍒板ù婊呭亾缁绘繈濮€閿濆懐鍘梺鍛婃⒐閻楃姴鐣烽幇鐗堝€婚柤鎭掑劤閸樹粙姊洪悷閭﹀殶闁稿孩鍔欓幃鐐寸鐎ｎ偆鍘卞┑鈽嗗灠閸氬寮抽浣瑰弿濠电姴鍟妵婵堚偓瑙勬磸閸斿秶鎹㈠┑瀣闁靛瀵屽鏃堟⒒閸屾瑧鍔嶉悗绗涘厾楦跨疀濞戞锛熼梻鍌氱墛缁嬫捇寮抽敃鍌涚厵闁绘鐗婄欢鑼磼閻樺磭鈯曢柕鍥у楠炴鎹勯悜妯尖偓濠氭倵?1", energyGainDelta: 1),
			["guard_up"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹檧鏋庨柟瀛樼箖閻忓酣姊婚崒姘偓鎼佸磹妞嬪孩顐芥慨妯挎硾閻掑灚銇勯幒鎴濃偓鍛婄濠婂牊鐓犳繛鑼额嚙閻忥妇鈧娲忛崹浠嬬嵁閺嶃劍濯撮柛锔诲幖楠炴姊绘担渚敯婵炲拑缍佸畷鎴濃槈椤兘鍋撻崨鎼晢闁告洦鍓涢崢闈浳旈悩闈涗粶闁诲繑绻堥幃姗€鍩￠崨顔惧帗闁荤喐鐟ョ€氼剟鎮橀埡鍛厓鐟滄粓宕滈妸褏绀婇柛鈩冾焽椤╂煡鏌ｉ幇顓犲闁搞儺鍓欑痪褎绻涢崱娆忎壕閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝閻熝勭閹间焦顥堟繛鎴炲笚閺傗偓婵＄偑鍊栭悧顓犲緤娴犲宓侀柡宥庡幖绾偓闂備緡鍓欑粔鐢告偂閺囩喍绻嗛柕鍫濇噹閺嗘瑥霉濠婂啰绉洪柡宀嬬節閸┾偓妞ゆ帊鑳堕々鐑芥倵閿濆骸浜為柛妯圭矙濮婇缚銇愰幒鎴滃枈闂佸憡鎸婚惄顖炵嵁濡ゅ懏鍋愰柣銏㈡暩閿涙繈鎮楅獮鍨姎婵☆偅顨婇幃姗€骞橀鐣屽幐闁诲繒鍋涙晶浠嬪煡婢跺浜滄い鎰剁悼缁犵偞銇勯姀鈽嗘畷鐎垫澘瀚换婵嬪磼濮樺吋顥嶅┑鐘殿暜缁辨洟宕戦幋锕€纾归柕鍫濐槸閸屻劑鏌ｉ幘宕囩槏婵炲樊浜滃洿婵犮垼娉涢敃锕傚储濞差亝鐓欓柛蹇氬亹閺嗘﹢鏌涢弬璺ㄧ伇缂侇喖顭烽弫鎰板幢濞嗘垹妲囬梻浣稿閻撳牓宕戦崟顒佸弿闁割偆鍠嗘禍婊堟煏婢舵ê鐏ｇ紒鈾€鍋撴繝娈垮枛閿曘儱顪冩禒瀣疇闁跨喓濮村洿闂佸憡渚楅崰姘跺焵椤掑嫮鐣烘慨濠冩そ閹筹繝濡堕崨顔锯偓顓㈡⒑缁嬪灝顒㈠┑鐐诧躬瀹曟椽鍩€椤掍降浜滈柟鍝勭Ф椤︼箓鏌涢妶搴″闂囧绻濇繝鍌涘櫣濞寸姾椴搁〃銉╂倷閺夋垹浼屾繝娈垮枓閸嬫捇姊洪棃娑氬闁瑰啿閰ｉ幊鎾诲垂椤旇鏂€闂佺粯鍔栧娆撴倶閿曞倹鐓熼柣鏃€绻傚ú銈夊磼閵婏负浜滈煫鍥ㄦ尵婢ф盯鏌嶉柨瀣伌闁哄本鐩獮姗€寮堕幋鐘点偡濠电偛鐡ㄧ划宥囨崲閸曨厽顫曢柟鐑樻⒐鐎氭岸鏌熺紒妯哄潑闁稿鎹囧畷绋课旈埀顒勫及閵夆晜鐓冪憸婊堝礈濮樿泛桅闁告洦鍨扮粻鎶芥煕閳╁啨浠﹀瑙勬礋濮婄粯鎷呮笟顖涙暞濡炪倖娲樼划搴∥ｉ幇鏉垮嵆闁绘ɑ褰冮悘濠囨煟鎼搭垳绉靛ù婊冪埣瀹曘儳鈧綆鍠楅悡鏇㈡煃閳轰礁鏆欏┑顔兼搐闇夋繝濠傚缁犵偞鎱ㄦ繝鍐┿仢鐎规洏鍔嶇换婵嬪磼濮樺吋缍傞梻鍌欑閹碱偊宕愭搴㈩偨婵ê澧庡畵?2", shieldGainDelta: 2),
			["brace"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹檧鏋庨柟瀛樼箖閻忓酣姊婚崒姘偓鎼佸磹妞嬪孩顐芥慨妯挎硾閻掑灚銇勯幒鎴濃偓鍛婄濠婂牊鐓犳繛鑼额嚙閻忥妇鈧娲忛崹浠嬬嵁閺嶃劍濯撮柛锔诲幖楠炴姊绘担渚敯婵炲拑缍佸畷鎴濃槈椤兘鍋撻崨鎼晢闁告洦鍓涢崢闈浳旈悩闈涗粶闁诲繑绻堥幃姗€鍩￠崨顔惧帗闁荤喐鐟ョ€氼剟鎮橀埡鍛厓鐟滄粓宕滈妸褏绀婇柛鈩冾焽椤╂煡鏌ｉ幇顓犲闁搞儺鍓欑痪褎绻涢崱娆忎壕閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝閻熝勭閹间焦顥堟繛鎴炲笚閺傗偓婵＄偑鍊栭悧顓犲緤娴犲宓侀柡宥庡幖绾偓闂備緡鍓欑粔鐢告偂閺囩喍绻嗛柕鍫濇噹閺嗘瑥霉濠婂啰绉洪柡宀嬬節閸┾偓妞ゆ帊鑳堕々鐑芥倵閿濆骸浜為柛妯圭矙濮婇缚銇愰幒鎴滃枈闂佸憡鎸婚惄顖炵嵁濡ゅ懏鍋愰柣銏㈡暩閿涙繈鎮楅獮鍨姎婵☆偅顨婇幃姗€骞橀鐣屽幐闁诲繒鍋涙晶浠嬪煡婢跺浜滄い鎰剁悼缁犵偞銇勯姀鈽嗘畷鐎垫澘瀚换婵嬪磼濮樺吋顥嶅┑鐘殿暜缁辨洟宕戦幋锕€纾归柕鍫濐槸閸屻劑鏌ｉ幘宕囩槏婵炲樊浜滃洿婵犮垼娉涢敃锕傚储濞差亝鐓欓柛蹇氬亹閺嗘﹢鏌涢弬璺ㄧ伇缂侇喖顭烽弫鎰板幢濞嗘垹妲囬梻浣稿閻撳牓宕戦崟顒佸弿闁割偆鍠嗘禍婊堟煏婢舵ê鐏ｇ紒鈾€鍋撴繝娈垮枛閿曘儱顪冩禒瀣疇闁跨喓濮村洿闂佸憡渚楅崰姘跺焵椤掑嫮鐣烘慨濠冩そ閹筹繝濡堕崨顔锯偓顓㈡⒑缁嬪灝顒㈠┑鐐诧躬瀹曟椽鍩€椤掍降浜滈柟鍝勭Ф椤︼箓鏌涢妶搴″闂囧绻濇繝鍌涘櫣濞寸姾椴搁〃銉╂倷閺夋垹浼屾繝娈垮枓閸嬫捇姊洪棃娑氬闁瑰啿閰ｉ幊鎾诲垂椤旇鏂€闂佺粯鍔栧娆撴倶閿曞倹鐓熼柣鏃€绻傚ú銈夊磼閵婏负浜滈煫鍥ㄦ尵婢ф盯鏌嶉柨瀣伌闁哄本鐩獮姗€寮堕幋鐘点偡濠电偛鐡ㄧ划宥囨崲閸曨厽顫曢柟鐑樻⒐鐎氭岸鏌熺紒妯哄潑闁稿鎹囧畷绋课旈埀顒勫及閵夆晜鐓冪憸婊堝礈濮樿泛桅闁告洦鍨扮粻鎶芥煕閳╁啨浠﹀瑙勬礋濮婄粯鎷呮笟顖涙暞濡炪倖娲樼划搴∥ｉ幇鏉垮嵆闁绘ɑ褰冮悘濠囨煟鎼搭垳绉靛ù婊冪埣瀹曘儳鈧綆鍠楅悡鏇㈡煃閳轰礁鏆欏┑顔兼搐闇夋繝濠傚缁犵偞鎱ㄦ繝鍐┿仢鐎规洏鍔嶇换婵嬪磼濮樺吋缍傞梻鍌欑閹碱偊宕愭搴㈩偨婵ê澧庡畵?3", shieldGainDelta: 3),
			["quick_guard"] = new BattleCardEnhancementDefinition("+", "闂傚倸鍊搁崐鎼佸磹閹间礁纾归柟闂寸绾惧綊鏌熼梻瀵割槮缁炬儳缍婇弻鐔兼⒒鐎靛壊妲紒鐐劤缂嶅﹪寮婚悢鍏尖拻閻庨潧澹婂Σ顔剧磼閻愵剙鍔ょ紓宥咃躬瀵鎮㈤崗灏栨嫽闁诲酣娼ф竟濠偽ｉ鍓х＜闁绘劦鍓欓崝銈囩磽瀹ュ拑韬€殿喖顭烽幃銏ゅ礂鐏忔牗瀚介梺璇查叄濞佳勭珶婵犲伣锝夘敊閸撗咃紲闂佺粯鍔﹂崜娆撳礉閵堝洨纾界€广儱鎷戦煬顒傗偓娈垮枛椤兘寮幇顓炵窞濠电姴瀚烽崥鍛存⒒娴ｇ懓顕滅紒璇插€块獮澶娾槈閵忕姷顔掔紓鍌欑劍椤洭宕㈤柆宥嗏拺闂傚牊绋撴晶鏇㈡煙閸愭煡鍙勬い銏℃椤㈡﹢濮€閿涘嫬骞愰梺璇茬箳閸嬫稒鏅堕挊澹濊櫣鈧稒菧娴滄粓鏌曡箛濠傚⒉缂佲偓鐎ｎ喗鐓涘ù锝囨嚀婵秶鈧娲栧畷顒勫煝鎼粹檧鏋庨柟瀛樼箖閻忓酣姊婚崒姘偓鎼佸磹妞嬪孩顐芥慨妯挎硾閻掑灚銇勯幒鎴濃偓鍛婄濠婂牊鐓犳繛鑼额嚙閻忥妇鈧娲忛崹浠嬬嵁閺嶃劍濯撮柛锔诲幖楠炴姊绘担渚敯婵炲拑缍佸畷鎴濃槈椤兘鍋撻崨鎼晢闁告洦鍓涢崢闈浳旈悩闈涗粶闁诲繑绻堥幃姗€鍩￠崨顔惧帗闁荤喐鐟ョ€氼剟鎮橀埡鍛厓鐟滄粓宕滈妸褏绀婇柛鈩冾焽椤╂煡鏌ｉ幇顓犲闁搞儺鍓欑痪褎绻涢崱娆忎壕閻庨潧鐭傚娲濞戞艾顣哄┑鈽嗗亝閻熝勭閹间焦顥堟繛鎴炲笚閺傗偓婵＄偑鍊栭悧顓犲緤娴犲宓侀柡宥庡幖绾偓闂備緡鍓欑粔鐢告偂閺囩喍绻嗛柕鍫濇噹閺嗘瑥霉濠婂啰绉洪柡宀嬬節閸┾偓妞ゆ帊鑳堕々鐑芥倵閿濆骸浜為柛妯圭矙濮婇缚銇愰幒鎴滃枈闂佸憡鎸婚惄顖炵嵁濡ゅ懏鍋愰柣銏㈡暩閿涙繈鎮楅獮鍨姎婵☆偅顨婇幃姗€骞橀鐣屽幐闁诲繒鍋涙晶浠嬪煡婢跺浜滄い鎰剁悼缁犵偞銇勯姀鈽嗘畷鐎垫澘瀚换婵嬪磼濮樺吋顥嶅┑鐘殿暜缁辨洟宕戦幋锕€纾归柕鍫濐槸閸屻劑鏌ｉ幘宕囩槏婵炲樊浜滃洿婵犮垼娉涢敃锕傚储濞差亝鐓欓柛蹇氬亹閺嗘﹢鏌涢弬璺ㄧ伇缂侇喖顭烽弫鎰板幢濞嗘垹妲囬梻浣稿閻撳牓宕戦崟顒佸弿闁割偆鍠嗘禍婊堟煏婢舵ê鐏ｇ紒鈾€鍋撴繝娈垮枛閿曘儱顪冩禒瀣疇闁跨喓濮村洿闂佸憡渚楅崰姘跺焵椤掑嫮鐣烘慨濠冩そ閹筹繝濡堕崨顔锯偓顓㈡⒑缁嬪灝顒㈠┑鐐诧躬瀹曟椽鍩€椤掍降浜滈柟鍝勭Ф椤︼箓鏌涢妶搴″闂囧绻濇繝鍌涘櫣濞寸姾椴搁〃銉╂倷閺夋垹浼屾繝娈垮枓閸嬫捇姊洪棃娑氬闁瑰啿閰ｉ幊鎾诲垂椤旇鏂€闂佺粯鍔栧娆撴倶閿曞倹鐓熼柣鏃€绻傚ú銈夊磼閵婏负浜滈煫鍥ㄦ尵婢ф盯鏌嶉柨瀣伌闁哄本鐩獮姗€寮堕幋鐘点偡濠电偛鐡ㄧ划宥囨崲閸曨厽顫曢柟鐑樻⒐鐎氭岸鏌熺紒妯哄潑闁稿鎹囧畷绋课旈埀顒勫及閵夆晜鐓冪憸婊堝礈濮樿泛桅闁告洦鍨扮粻鎶芥煕閳╁啨浠﹀瑙勬礋濮婄粯鎷呮笟顖涙暞濡炪倖娲樼划搴∥ｉ幇鏉垮嵆闁绘ɑ褰冮悘濠囨煟鎼搭垳绉靛ù婊冪埣瀹曘儳鈧綆鍠楅悡鏇㈡煃閳轰礁鏆欏┑顔兼搐闇夋繝濠傚缁犵偞鎱ㄦ繝鍐┿仢鐎规洏鍔嶇换婵嬪磼濮樺吋缍傞梻鍌欑閹碱偊宕愭搴㈩偨婵ê澧庡畵?2", shieldGainDelta: 2),
		};
	[Export] public PackedScene? ForcedBattleRoomScene { get; set; }
	[Export] public PackedScene[] BattleRoomScenes { get; set; } = Array.Empty<PackedScene>();
	[Export] public BattleRoomPoolDefinition? BattleRoomPools { get; set; }
	[Export] public BattlePrefabLibrary? BattlePrefabLibrary { get; set; }
	[Export] public BattleEnemyLibrary? BattleEnemyLibrary { get; set; }
	[Export] public BattleEncounterLibrary? EncounterLibrary { get; set; }
	[Export] public BattleCardLibrary? BattleCardLibrary { get; set; }
	[Export] public BattleDeckBuildRules? BattleDeckBuildRules { get; set; }
	[Export] public string EncounterId { get; set; } = string.Empty;
	[Export] public string[] EncounterEnemyTypeIds { get; set; } = { "grunt" };
	[Export] public string EncounterEnemyDefinitionId { get; set; } = "battle_enemy";
	[Export] public string EncounterPreferredRoomPoolId { get; set; } = string.Empty;
	[Export] public int RandomSeed { get; set; } = 1337;
	[Export] public float CameraZoom { get; set; } = 1.0f;
	[Export] public int CameraTopMarginPixels { get; set; } = 8;
	[Export] public int CameraBottomMarginPixels { get; set; } = 52;
	[Export(PropertyHint.Range, "4,64,1")] public int CameraEdgePanMarginPixels { get; set; } = 22;
	[Export(PropertyHint.Range, "16,320,1")] public int CameraResetDurationMs { get; set; } = 180;
	[Export(PropertyHint.Range, "20,480,1")] public float CameraPanPixelsPerSecond { get; set; } = 160.0f;
	[Export(PropertyHint.Range, "0.1,1.0,0.05")] public float CameraMinBoardVisibleRatio { get; set; } = 0.8f;
	[Export(PropertyHint.Range, "1,4,1")] public float CameraFocusZoomMultiplier { get; set; } = 2.0f;
	[Export(PropertyHint.Range, "0.02,1.2,0.01")] public float CameraFocusHoldSeconds { get; set; } = 1.8f;
	[Export(PropertyHint.Range, "0.05,3.0,0.05")] public float AttackFocusHoldSeconds { get; set; } = 2.0f;
	[Export(PropertyHint.Range, "0.05,3.0,0.05")] public float ArakawaBuildFocusHoldSeconds { get; set; } = 0.8f;
	[Export] public bool ShowBattleFloorLayer { get; set; } = true;
	[Export] public int PlayerHandSize { get; set; } = 7;
	[Export] public int PlayerEnergyPerTurn { get; set; } = 3;

	public BoardState? BoardState { get; private set; }
	public BoardObjectRegistry? Registry { get; private set; }
	public BoardQueryService? QueryService { get; private set; }
	public BoardPathfinder? Pathfinder { get; private set; }
	public BoardTargetingService? TargetingService { get; private set; }
	public TurnActionState? TurnState { get; private set; }
	public BattleRoomTemplate? CurrentRoom { get; private set; }
	public GlobalGameSession? GlobalSession { get; private set; }
	public BattleObjectStateManager? StateManager { get; private set; }

	private RandomNumberGenerator _rng = new();
	private BattlePieceViewManager? _pieceViewManager;
	private BattleFloatingTextLayer? _floatingTextLayer;
	private BattleActionService? _actionService;
	private EnemyTurnResolver? _enemyTurnResolver;
	private BattleHudController? _hud;
	private BattleDeckState? _playerDeck;
	private Control? _battleFailOverlay;
	private ColorRect? _battleFailFlash;
	private Label? _battleFailLabel;
	private bool _battleFailureSequenceStarted;
	private bool _battleVictorySequenceStarted;
	private bool _battleResultCommitted;
	private bool _playerCounterStanceActive;
	private int _playerCounterStanceExpiresOnTurnIndex = -1;
	private int _playerCounterStanceDamage = 6;
	private bool _isArakawaWheelOpen;
	private ArakawaAbilityMode _arakawaAbilityMode = ArakawaAbilityMode.None;
	private BattleRequest? _activeBattleRequest;
	private bool _retreatPending;
	private int _retreatTurnIndex = -1;
	private int _retreatStartHp = -1;
	private bool _isPlayerMoveResolving;
	private Camera2D? _battleCamera;
	private Sprite2D? _battleBackground;
	private Rect2 _cameraPanBounds = new();
	private Vector2 _cameraRestPosition = Vector2.Zero;
	private Vector2 _cameraPanVelocity = Vector2.Zero;
	private float _cameraPanHoldTime;
	private Tween? _cameraResetTween;
	private Tween? _cameraCinematicTween;
	private bool _isCameraCinematicBusy;
	private readonly List<string> _currentTurnActionLogEntries = new();
	private readonly List<string> _previousTurnActionLogEntries = new();
	private readonly List<PendingDelayedCardEffect> _pendingDelayedCardEffects = new();
	private readonly Dictionary<string, EnemyLearnState> _enemyLearnStates = new(StringComparer.Ordinal);
	private readonly HashSet<string> _pendingLearnedCardIds = new(StringComparer.Ordinal);
	private readonly List<string> _defeatedEnemyDefinitionIds = new();
	private int _initialEnemyUnitCount;
	private int _currentTurnActionLogTurnIndex = 1;
	private int _previousTurnActionLogTurnIndex;

	private enum PendingDelayedCardEffectKind
	{
		AlertStrike = 0,
		ContemplateEnergy = 1,
	}

	private sealed class PendingDelayedCardEffect
	{
		public PendingDelayedCardEffectKind Kind { get; init; }
		public string SourceObjectId { get; init; } = string.Empty;
		public int TriggerTurnIndex { get; init; }
		public int Radius { get; init; }
		public int Damage { get; init; }
		public int Energy { get; init; }
		public int RequiredHp { get; init; }
	}

	private sealed class EnemyLearnRewardProfile
	{
		public string NormalCardId { get; init; } = string.Empty;
		public string SignatureCardId { get; init; } = string.Empty;
	}

	private sealed class EnemyLearnState
	{
		public bool SignatureAvailable { get; set; }
		public int ResetOnTurnIndex { get; set; } = -1;
	}

	public override void _Ready()
	{
		_rng.Seed = (ulong)Math.Max(RandomSeed, 1);

		GlobalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		ApplyPendingBattleRequest();
		ApplyPendingEncounterId();
		BattlePrefabLibrary ??= GD.Load<BattlePrefabLibrary>("res://Resources/Battle/Presentation/DefaultBattlePrefabLibrary.tres");
		BattleEnemyLibrary ??= GD.Load<BattleEnemyLibrary>("res://Resources/Battle/Enemies/DefaultBattleEnemyLibrary.tres");
		EncounterLibrary ??= GD.Load<BattleEncounterLibrary>("res://Resources/Battle/Encounters/DebugBattleEncounterLibrary.tres");
		BattleCardLibrary ??= GD.Load<BattleCardLibrary>("res://Resources/Battle/Cards/DefaultBattleCardLibrary.tres");
		BattleDeckBuildRules ??= GD.Load<BattleDeckBuildRules>("res://Resources/Battle/Cards/DefaultBattleDeckBuildRules.tres");
		GlobalSession?.EnsureDeckBuildInitialized(BattleCardLibrary);
		ResolveEncounterConfiguration();

		BoardState = new BoardState();
		Registry = new BoardObjectRegistry();
		QueryService = new BoardQueryService(BoardState, Registry);
		TurnState = new TurnActionState();
		TurnState.StartNewTurn(1);
		_currentTurnActionLogTurnIndex = TurnState.TurnIndex;
		IReadOnlyList<BattleCardDefinition> prototypeDeck = BuildAvailableCardCatalog();
		BattleDeckRuntimeInit? deckRuntimeInit = BuildDeckRuntimeInit(prototypeDeck);
		IReadOnlyList<BattleCardDefinition> battleDeckSource = ResolveBattleDeckSource(prototypeDeck, deckRuntimeInit);
		_playerDeck = new BattleDeckState(
			battleDeckSource,
			(ulong)Math.Max(RandomSeed, 1),
			PlayerHandSize,
			PlayerEnergyPerTurn,
			deckRuntimeInit);
		SyncEnergyRechargeTrackingState();
		_playerDeck.DrawCards(ResolveOpeningDrawCount(deckRuntimeInit, _playerDeck.HandSize));
		_playerDeck.StartPlayerTurn();

		CurrentRoom = InstantiateSelectedRoom();
		Node2D roomContainer = GetNode<Node2D>("RoomContainer");
		roomContainer.AddChild(CurrentRoom);
		roomContainer.MoveChild(CurrentRoom, 0);
		_battleBackground = EnsureBattleBackground(roomContainer);
		AttachBattleBackgroundToRoom();
		ConfigureBattleBackground(roomContainer);
		ConfigureBattleFloorLayerVisibility();
		GameAudio.Instance?.PlayBattleMusic();

		RoomLayoutDefinition layout = CurrentRoom.BuildLayoutDefinition(EncounterEnemyDefinitionId, BattleEnemyLibrary);
		BoardInitializer initializer = new(BoardState, Registry);
		initializer.InitializeFromLayout(layout);
		ApplyInitialPlayerCombatState();
		Pathfinder = new BoardPathfinder(CurrentRoom.Topology, QueryService);
		TargetingService = new BoardTargetingService(CurrentRoom.Topology, Registry, QueryService);

		if (GlobalSession == null || BattlePrefabLibrary == null)
		{
			throw new InvalidOperationException("BattleSceneController: GlobalSession or BattlePrefabLibrary is missing.");
		}

		StateManager = new BattleObjectStateManager(Registry, BattlePrefabLibrary, GlobalSession, BattleEnemyLibrary);
		StateManager.Initialize();
		StateManager.SyncAllFromRegistry();
		_initialEnemyUnitCount = Registry.AllObjects.Count(boardObject =>
			boardObject.ObjectType == BoardObjectType.Unit
			&& boardObject.Faction == BoardObjectFaction.Enemy);
		_defeatedEnemyDefinitionIds.Clear();

		Node pieceRoot = GetNode<Node>("RoomContainer/PieceRoot");
		Node killFxRoot = EnsureRoomLayerRoot("KillFxRoot", false);
		_pieceViewManager = new BattlePieceViewManager(
			pieceRoot,
			killFxRoot,
			BattlePrefabLibrary,
			BattleEnemyLibrary);
		_pieceViewManager.Rebuild(Registry, StateManager, CurrentRoom);
		_floatingTextLayer = GetNodeOrNull<BattleFloatingTextLayer>("RoomContainer/FloatingTextLayer");
		_actionService = new BattleActionService(BoardState, Registry, QueryService, Pathfinder, StateManager, _pieceViewManager, CurrentRoom, GlobalSession, _floatingTextLayer);
		_actionService.ActionLogged += OnBattleActionLogged;
		_actionService.EnemyDefeated += OnEnemyDefeated;
		_enemyTurnResolver = new EnemyTurnResolver(
			Registry,
			StateManager,
			Pathfinder,
			QueryService,
			TargetingService,
			_actionService,
			new EnemyAiRegistry(),
			CurrentRoom,
			this,
			OnEnemyAttackResolved,
			OnActiveEnemyTurnChanged,
			ExecuteEnemySpecialDecisionAsync);

		BattleBoardOverlay? overlay = GetNodeOrNull<BattleBoardOverlay>("RoomContainer/BoardOverlay");
		if (overlay != null)
		{
			overlay.ZIndex = 20;
			overlay.Bind(CurrentRoom);
		}

		_hud = GetNodeOrNull<BattleHudController>("BattleHud");
		if (_hud != null)
		{
			_hud.Bind(TurnState);
			_hud.AttackRequested += OnAttackRequested;
			_hud.DefendRequested += OnDefendRequested;
			_hud.RetreatRequested += OnRetreatRequested;
			_hud.ArakawaWheelRequested += OnArakawaWheelRequested;
			_hud.ArakawaAbilityRequested += OnArakawaAbilityRequested;
			_hud.ArakawaCancelRequested += OnArakawaCancelRequested;
			_hud.MeditateRequested += OnMeditateRequested;
			_hud.CardRequested += OnCardRequested;
			_hud.EndTurnRequested += OnEndTurnRequested;
			_hud.SetActionLogState(_currentTurnActionLogTurnIndex, _currentTurnActionLogEntries, _previousTurnActionLogTurnIndex, _previousTurnActionLogEntries);
		}

		_battleFailOverlay = GetNodeOrNull<Control>("BattleFailOverlay");
		_battleFailFlash = GetNodeOrNull<ColorRect>("BattleFailOverlay/Flash");
		_battleFailLabel = GetNodeOrNull<Label>("BattleFailOverlay/DefeatLabel");

		GlobalSession.PlayerRuntimeChanged += OnPlayerRuntimeChanged;
		GlobalSession.ArakawaRuntimeChanged += OnArakawaRuntimeChanged;
		ConfigureCameraForBattle();

		GD.Print($"BattleSceneController: layout={layout.LayoutId}, size={layout.BoardSize}, objects={Registry.Count}");
	}

	private Sprite2D? EnsureBattleBackground(Node2D roomContainer)
	{
		Sprite2D? existing = roomContainer.GetNodeOrNull<Sprite2D>("BattleBackground");
		if (existing != null)
		{
			return existing;
		}

		Texture2D? texture = GD.Load<Texture2D>(BattleBackgroundTexturePath);
		if (texture == null)
		{
			GD.Print($"BattleSceneController: failed to load background texture at {BattleBackgroundTexturePath}.");
			return null;
		}

		Sprite2D background = new()
		{
			Name = "BattleBackground",
			Texture = texture,
			Centered = true,
			ZIndex = 0,
		};
		roomContainer.AddChild(background);
		GD.Print($"BattleSceneController: battle background created at runtime from {BattleBackgroundTexturePath}.");
		return background;
	}

	private Node EnsureRoomLayerRoot(string nodeName, bool ySortEnabled)
	{
		Node2D roomContainer = GetNode<Node2D>("RoomContainer");
		if (roomContainer.GetNodeOrNull<Node>(nodeName) is Node existing)
		{
			return existing;
		}

		Node2D created = new()
		{
			Name = nodeName,
			YSortEnabled = ySortEnabled,
		};
		roomContainer.AddChild(created);
		GD.Print($"BattleSceneController: created missing room layer root '{nodeName}' at runtime.");
		return created;
	}

	private void ConfigureBattleBackground(Node2D roomContainer)
	{
		if (_battleBackground == null || CurrentRoom == null || _battleBackground.Texture == null)
		{
			GD.Print("BattleSceneController: battle background missing or texture unresolved.");
			return;
		}

		Vector2 boardPixelSize = new(
			CurrentRoom.BoardSize.X * CurrentRoom.CellSizePixels,
			CurrentRoom.BoardSize.Y * CurrentRoom.CellSizePixels);
		Vector2 textureSize = _battleBackground.Texture.GetSize();
		if (textureSize.X <= 0.0f || textureSize.Y <= 0.0f)
		{
			return;
		}

		const float horizontalPadding = 128.0f;
		const float verticalPadding = 128.0f;
		float scale = Math.Max(
			(boardPixelSize.X + horizontalPadding) / textureSize.X,
			(boardPixelSize.Y + verticalPadding) / textureSize.Y);

		_battleBackground.Scale = new Vector2(scale, scale);
		_battleBackground.Position = boardPixelSize * 0.5f;
		GD.Print(
			$"BattleSceneController: background configured parent={_battleBackground.GetParent()?.Name}, " +
			$"pos={_battleBackground.Position}, scale={_battleBackground.Scale}, " +
			$"size={textureSize}, visible={_battleBackground.Visible}, z={_battleBackground.ZIndex}");
	}

	private void AttachBattleBackgroundToRoom()
	{
		if (_battleBackground == null || CurrentRoom == null)
		{
			GD.Print("BattleSceneController: battle background node or current room missing before attach.");
			return;
		}

		_battleBackground.Reparent(CurrentRoom, false);

		int insertionIndex = 0;
		if (CurrentRoom.GetNodeOrNull<Node>("FloorLayer") is Node floorLayer)
		{
			insertionIndex = floorLayer.GetIndex();
		}

		CurrentRoom.MoveChild(_battleBackground, insertionIndex);
		GD.Print(
			$"BattleSceneController: background attached to room={CurrentRoom.Name}, " +
			$"childIndex={_battleBackground.GetIndex()}, insertionIndex={insertionIndex}");
	}

	private void ConfigureBattleFloorLayerVisibility()
	{
		if (CurrentRoom?.GetNodeOrNull<TileMapLayer>("FloorLayer") is not TileMapLayer floorLayer)
		{
			GD.Print("BattleSceneController: floor layer not found when configuring floor visibility.");
			return;
		}

		floorLayer.Visible = ShowBattleFloorLayer;
		GD.Print($"BattleSceneController: floor layer visible={floorLayer.Visible}");
	}

	public override void _ExitTree()
	{
		if (GlobalSession != null)
		{
			GlobalSession.PlayerRuntimeChanged -= OnPlayerRuntimeChanged;
			GlobalSession.ArakawaRuntimeChanged -= OnArakawaRuntimeChanged;
		}

		if (_hud != null)
		{
			_hud.AttackRequested -= OnAttackRequested;
			_hud.DefendRequested -= OnDefendRequested;
			_hud.RetreatRequested -= OnRetreatRequested;
			_hud.ArakawaWheelRequested -= OnArakawaWheelRequested;
			_hud.ArakawaAbilityRequested -= OnArakawaAbilityRequested;
			_hud.ArakawaCancelRequested -= OnArakawaCancelRequested;
			_hud.MeditateRequested -= OnMeditateRequested;
			_hud.CardRequested -= OnCardRequested;
			_hud.EndTurnRequested -= OnEndTurnRequested;
		}

		if (_actionService != null)
		{
			_actionService.ActionLogged -= OnBattleActionLogged;
			_actionService.EnemyDefeated -= OnEnemyDefeated;
		}
	}

	public override void _Process(double delta)
	{
		if (Registry == null || CurrentRoom == null || StateManager == null)
		{
			return;
		}

		StateManager.SyncAllFromRegistry();
		if (!_battleFailureSequenceStarted && GlobalSession?.PlayerCurrentHp <= 0)
		{
			StartBattleFailureSequence();
		}
		else if (TryResolveVictory())
		{
			return;
		}

		bool hasHoveredCell = CurrentRoom.TryScreenToCell(GetGlobalMousePosition(), out Vector2I hoveredCell);
		Vector2 hoverScreenPosition = GetViewport().GetMousePosition();
		_hud?.SetHoveredUnitState(hasHoveredCell ? GetHoveredObjectState(hoveredCell) : null, hoverScreenPosition);
		if (_playerDeck != null)
		{
			SyncEnergyRechargeTrackingState();
			_hud?.SetCardState(
				_playerDeck.CurrentEnergy,
				_playerDeck.MaxEnergyPerTurn,
				TurnState?.EnergyRechargeTurnProgress ?? 0,
				TurnState?.EnergyRechargeTurnInterval ?? 3,
				_playerDeck.Hand,
				TurnState?.SelectedCardInstanceId ?? string.Empty,
				_playerDeck.DrawPileCards,
				_playerDeck.DiscardPileCards,
				_playerDeck.ExhaustPileCards);
		}
		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		if (_hud != null && GlobalSession != null)
		{
			bool canUseArakawa = CanUseArakawaThisTurn();
			if (!canUseArakawa)
			{
				_isArakawaWheelOpen = false;
				if (_arakawaAbilityMode != ArakawaAbilityMode.None)
				{
					CancelArakawaAbilityMode();
				}
			}

			_hud.SetArakawaState(
				GlobalSession.ArakawaCurrentEnergy,
				GlobalSession.ArakawaMaxEnergy,
				canUseArakawa,
				_isArakawaWheelOpen,
				GetCurrentArakawaAbilityId());
			_hud.SetPlayerStatusState(
				playerState?.CurrentHp ?? 0,
				playerState?.MaxHp ?? 0,
				playerState?.CurrentShield ?? 0);
			_hud.SetRetreatActionState(playerState != null && IsPlayerStandingOnEscapeCell(playerState.Cell) && IsRetreatFeatureAvailable());
			_hud.SetActionLogState(_currentTurnActionLogTurnIndex, _currentTurnActionLogEntries, _previousTurnActionLogTurnIndex, _previousTurnActionLogEntries);
		}

		UpdateBattleCameraPan(delta);

		BattleBoardOverlay? overlay = GetNodeOrNull<BattleBoardOverlay>("RoomContainer/BoardOverlay");
		if (overlay == null)
		{
			return;
		}

		overlay.SetEscapeCells(IsRetreatFeatureAvailable()
			? CurrentRoom.GetEscapeCells()
			: Array.Empty<Vector2I>());
		overlay.SetTelegraphCells(BuildActiveTelegraphCells());
		if (BoardState != null)
		{
			overlay.SetArcTerrainCells(BoardState.EnumerateCells()
				.Where(cell => string.Equals(cell.TerrainId, BattleActionService.ArcTerrainId, StringComparison.Ordinal))
				.Select(cell => cell.Cell));
			overlay.SetFireTerrainCells(BoardState.EnumerateCells()
				.Where(cell => string.Equals(cell.TerrainId, BattleActionService.FireTerrainId, StringComparison.Ordinal))
				.Select(cell => cell.Cell));
		}

		if (_isPlayerMoveResolving)
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			overlay.SetAttackTargetCells(Array.Empty<Vector2I>());
			overlay.SetSupportTargetCells(Array.Empty<Vector2I>());
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		if (_battleFailureSequenceStarted)
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			overlay.SetAttackTargetCells(Array.Empty<Vector2I>());
			overlay.SetSupportTargetCells(Array.Empty<Vector2I>());
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		if (playerState == null)
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			overlay.SetAttackTargetCells(Array.Empty<Vector2I>());
			overlay.SetSupportTargetCells(Array.Empty<Vector2I>());
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		if (_arakawaAbilityMode == ArakawaAbilityMode.BuildWall)
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			overlay.SetAttackTargetCells(Array.Empty<Vector2I>());
			overlay.SetSupportTargetCells(BuildArakawaWallTargetCells(), playerState.Cell);
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		if (TurnState?.IsCardTargeting == true)
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			BattleCardDefinition? selectedDefinition = GetSelectedCardDefinition();
			if (selectedDefinition?.TargetingMode == BattleCardTargetingMode.FriendlyUnit
				|| selectedDefinition?.TargetingMode == BattleCardTargetingMode.Cell)
			{
				overlay.SetAttackTargetCells(Array.Empty<Vector2I>());
				overlay.SetSupportTargetCells(BuildSelectedCardTargetCells(playerState.ObjectId), playerState.Cell);
			}
			else
			{
				overlay.SetAttackTargetCells(BuildSelectedCardTargetCells(playerState.ObjectId), playerState.Cell);
				overlay.SetSupportTargetCells(Array.Empty<Vector2I>());
			}
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		if (TurnState?.IsAttackTargeting == true)
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			overlay.SetAttackTargetCells(BuildAttackTargetCells(playerState.ObjectId, playerState.Cell, playerState.AttackRange), playerState.Cell);
			overlay.SetSupportTargetCells(Array.Empty<Vector2I>());
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		if (!CanPlayerMoveThisTurn())
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			overlay.SetAttackTargetCells(Array.Empty<Vector2I>());
			overlay.SetSupportTargetCells(Array.Empty<Vector2I>());
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		List<Vector2I> reachableCells = BuildReachableCells(playerState.ObjectId, playerState.Cell, playerState.MovePointsPerTurn);
		overlay.SetReachableCells(reachableCells, playerState.Cell);
		overlay.SetAttackTargetCells(Array.Empty<Vector2I>());
		overlay.SetSupportTargetCells(Array.Empty<Vector2I>());

		if (hasHoveredCell && reachableCells.Contains(hoveredCell))
		{
			overlay.SetPreviewPath(BuildPreviewPath(playerState.ObjectId, playerState.Cell, hoveredCell, playerState.MovePointsPerTurn));
		}
		else
		{
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_battleFailureSequenceStarted)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && GlobalSession != null)
		{
			if (keyEvent.Keycode == Key.Pageup)
			{
				GlobalSession.ApplyMovePointDelta(1);
				return;
			}

			if (keyEvent.Keycode == Key.Pagedown)
			{
				GlobalSession.ApplyMovePointDelta(-1);
				return;
			}

			if (keyEvent.Keycode == Key.T || keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.KpEnter)
			{
				EndPlayerTurn();
				return;
			}

			if (keyEvent.Keycode == Key.Y)
			{
				TryResetBattleCamera();
				return;
			}
		}

		if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		if (CurrentRoom == null || StateManager == null)
		{
			return;
		}

		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		if (playerState == null)
		{
			return;
		}

		if (!CurrentRoom.TryScreenToCell(GetGlobalMousePosition(), out Vector2I targetCell))
		{
			if (TurnState?.IsCardTargeting == true || TurnState?.IsAttackTargeting == true)
			{
				TurnState.CancelTargeting();
			}

			return;
		}

		if (_arakawaAbilityMode == ArakawaAbilityMode.BuildWall)
		{
			TryExecuteArakawaBuildWall(targetCell);
			return;
		}

		if (TurnState?.IsCardTargeting == true && _playerDeck != null)
		{
			if (!_playerDeck.TryGetHandCard(TurnState.SelectedCardInstanceId, out BattleCardInstance? selectedCard) || selectedCard == null)
			{
				TurnState.CancelTargeting();
				return;
			}

			if (!selectedCard.Definition.RequiresTarget)
			{
				TurnState.CancelTargeting();
				return;
			}

			BoardObject? cardTarget = GetCardTargetAtCell(playerState.ObjectId, targetCell, selectedCard.Definition);
			if (selectedCard.Definition.TargetingMode != BattleCardTargetingMode.Cell && cardTarget == null)
			{
				TurnState.CancelTargeting();
				return;
			}

			TryPlayCard(playerState.ObjectId, selectedCard.InstanceId, cardTarget?.ObjectId, targetCell, out _);
			return;
		}

		if (TurnState?.IsAttackTargeting == true)
		{
			BoardObject? attackTarget = GetAttackableObjectAtCell(playerState.ObjectId, targetCell);
			if (attackTarget == null)
			{
				TurnState.CancelTargeting();
				return;
			}

			TryAttackObject(playerState.ObjectId, attackTarget.ObjectId, out _);
			return;
		}

		if (!CanPlayerMoveThisTurn())
		{
			return;
		}

		if (!BuildReachableCells(playerState.ObjectId, playerState.Cell, playerState.MovePointsPerTurn).Contains(targetCell))
		{
			return;
		}

		_ = TryMoveObjectAsync(playerState.ObjectId, targetCell);
	}

	public bool TryMoveObject(string objectId, Vector2I targetCell, out string failureReason)
	{
		failureReason = "BoardQueryService has not been initialized.";

		if (_actionService == null || StateManager == null)
		{
			return false;
		}

		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		bool isPlayerObject = playerState?.ObjectId == objectId;
		if (isPlayerObject && !CanPlayerMoveThisTurn())
		{
			failureReason = TurnState?.HasEndedTurn == true
				? "The current turn has already ended."
				: "The player has already moved this turn.";
			return false;
		}

		bool moved = _actionService.TryMoveObject(objectId, targetCell, out failureReason);
		if (moved)
		{
			if (isPlayerObject)
			{
				TurnState?.MarkMoved();
			}
		}

		return moved;
	}

	public async System.Threading.Tasks.Task<bool> TryMoveObjectAsync(string objectId, Vector2I targetCell)
	{
		if (_actionService == null || StateManager == null)
		{
			return false;
		}

		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		bool isPlayerObject = playerState?.ObjectId == objectId;
		if (isPlayerObject && !CanPlayerMoveThisTurn())
		{
			return false;
		}

		if (isPlayerObject)
		{
			_isPlayerMoveResolving = true;
		}

		try
		{
			bool moved = await _actionService.TryMoveObjectAsync(objectId, targetCell);
			if (moved && isPlayerObject)
			{
				TurnState?.MarkMoved();
			}

			return moved;
		}
		finally
		{
			if (isPlayerObject)
			{
				_isPlayerMoveResolving = false;
			}
		}
	}

	private void OnPlayerRuntimeChanged()
	{
		StateManager?.SyncPlayerFromSession();
	}

	private void OnBattleActionLogged(string line)
	{
		AppendBattleActionLog(line);
	}

	private void AppendBattleActionLog(string line)
	{
		if (string.IsNullOrWhiteSpace(line))
		{
			return;
		}

		_currentTurnActionLogEntries.Add(line);
	}

	private void AdvanceBattleActionLogTurn(int nextTurnIndex)
	{
		_previousTurnActionLogEntries.Clear();
		_previousTurnActionLogEntries.AddRange(_currentTurnActionLogEntries);
		_previousTurnActionLogTurnIndex = _currentTurnActionLogTurnIndex;
		_currentTurnActionLogEntries.Clear();
		_currentTurnActionLogTurnIndex = nextTurnIndex;
	}

	private string ResolveObjectDisplayName(string objectId)
	{
		return StateManager?.Get(objectId)?.DisplayName ?? objectId;
	}

	private ProgressionSnapshot GetPlayerProgressionSnapshot()
	{
		return GlobalSession?.BuildProgressionSnapshotModel() ?? new ProgressionSnapshot();
	}

	private int GetPlayerTalentScalarBonus(string prefix)
	{
		return GetPlayerProgressionSnapshot().GetTalentScalarBonus(prefix);
	}

	private static int SumImpactAmount(DamageApplicationResult result, params CombatImpactType[] impactTypes)
	{
		if (impactTypes == null || impactTypes.Length == 0)
		{
			return 0;
		}

		return result.Impacts
			.Where(impact => impactTypes.Contains(impact.ImpactType))
			.Sum(impact => impact.Amount);
	}

	private void OnArakawaRuntimeChanged()
	{
	}

	private DefenseActionDefinition BuildPlayerDefenseActionDefinition()
	{
		int reductionPercent = GlobalSession?.GetResolvedPlayerDefenseDamageReductionPercent() ?? 50;
		int shieldGain = GlobalSession?.GetResolvedPlayerDefenseShieldGain() ?? 0;
		return new DefenseActionDefinition(reductionPercent, shieldGain);
	}

	private void ApplyInitialPlayerCombatState()
	{
		if (Registry == null || GlobalSession == null)
		{
			return;
		}

		var resolvedStats = GlobalSession.ResolvePlayerStats();
		int maxHp = Math.Max(1, resolvedStats.MaxHp);
		int currentHp = Mathf.Clamp(GlobalSession.PlayerCurrentHp, 0, maxHp);
		int startingShield = Math.Max(0, resolvedStats.DefenseShieldGain);

		foreach (BoardObject boardObject in Registry.AllObjects.Where(boardObject =>
			boardObject.ObjectType == BoardObjectType.Unit
			&& boardObject.Faction == BoardObjectFaction.Player))
		{
			boardObject.SyncCombatStats(maxHp, currentHp, startingShield, startingShield);
		}
	}

	private void OnEndTurnRequested()
	{
		if (_battleFailureSequenceStarted)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		EndPlayerTurn();
	}

	private void OnAttackRequested()
	{
		if (_battleFailureSequenceStarted || TurnState == null)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		if (TurnState.IsAttackTargeting)
		{
			GameAudio.Instance?.PlayUiCancel();
			TurnState.CancelTargeting();
			return;
		}

		if (!TurnState.CanEnterAttackTargeting)
		{
			return;
		}

		GameAudio.Instance?.PlayUiConfirm();
		TurnState.EnterAttackTargeting();
	}

	private void OnCardRequested(string cardInstanceId)
	{
		if (_battleFailureSequenceStarted || TurnState == null || _playerDeck == null || StateManager == null)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		if (_arakawaAbilityMode == ArakawaAbilityMode.EnhanceCard)
		{
			TryExecuteArakawaEnhanceCard(cardInstanceId);
			return;
		}

		if (!_playerDeck.TryGetHandCard(cardInstanceId, out BattleCardInstance? cardInstance) || cardInstance == null)
		{
			return;
		}

		if (TurnState.IsCardTargeting && string.Equals(TurnState.SelectedCardInstanceId, cardInstanceId, StringComparison.Ordinal))
		{
			if (cardInstance.Definition.RequiresTarget)
			{
				return;
			}

			BattleObjectState? selectedPlayerState = StateManager.GetPrimaryPlayerState();
			if (selectedPlayerState == null)
			{
				return;
			}

			TryPlayCard(selectedPlayerState.ObjectId, cardInstanceId, null, null, out _);
			return;
		}

		if (!_playerDeck.CanPlay(cardInstanceId, out _))
		{
			return;
		}

		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		if (playerState == null)
		{
			return;
		}

		GameAudio.Instance?.PlayCardSelect();
		TurnState.EnterCardTargeting(cardInstanceId);
	}

	private void OnMeditateRequested()
	{
		if (_battleFailureSequenceStarted || _playerDeck == null || TurnState == null)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		if (!TurnState.CanSelectCard)
		{
			return;
		}

		_playerDeck.DiscardHand();
		_playerDeck.DrawToHandSize();
		if (StateManager?.GetPrimaryPlayerState() is BattleObjectState playerState)
		{
			AppendBattleActionLog($"{playerState.DisplayName}->{playerState.DisplayName} \u8C03\u606F");
		}
		TurnState.MarkActed();
		ResolveTurnPostPhase();
	}

	private async void OnDefendRequested()
	{
		if (_battleFailureSequenceStarted || TurnState == null || StateManager == null || _actionService == null)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		if (!TurnState.CanSelectCard)
		{
			return;
		}

		if (TurnState.IsAttackTargeting || TurnState.IsCardTargeting)
		{
			TurnState.CancelTargeting();
		}

		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		if (playerState == null)
		{
			return;
		}

		await _actionService.ApplyDefenseActionAsync(playerState.ObjectId, BuildPlayerDefenseActionDefinition(), TurnState.TurnIndex);
		int defenseShieldGain = GlobalSession?.GetResolvedPlayerDefenseShieldGain() ?? 0;
		AppendBattleActionLog(defenseShieldGain > 0
			? $"{playerState.DisplayName}->{playerState.DisplayName} \u9632\u5FA1+{defenseShieldGain}"
			: $"{playerState.DisplayName}->{playerState.DisplayName} \u9632\u5FA1");
		TurnState.MarkActed();
		ResolveTurnPostPhase();
	}

	private void OnRetreatRequested()
	{
		if (_battleFailureSequenceStarted || TurnState == null || GlobalSession == null)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		if (!CanAttemptRetreatThisTurn())
		{
			return;
		}

		if (TurnState.IsAttackTargeting || TurnState.IsCardTargeting)
		{
			TurnState.CancelTargeting();
		}

		CancelArakawaAbilityMode();
		_isArakawaWheelOpen = false;
		_retreatPending = false;
		_retreatTurnIndex = -1;
		_retreatStartHp = -1;
		if (StateManager?.GetPrimaryPlayerState() is BattleObjectState playerState)
		{
			AppendBattleActionLog($"{playerState.DisplayName}->{playerState.DisplayName} \u9003\u8DD1");
		}
		CommitBattleResult(BattleOutcome.Retreat);
	}

	private void OnArakawaWheelRequested()
	{
		if (!CanUseArakawaThisTurn())
		{
			_isArakawaWheelOpen = false;
			CancelArakawaAbilityMode();
			return;
		}

		if (_arakawaAbilityMode != ArakawaAbilityMode.None)
		{
			GameAudio.Instance?.PlayUiCancel();
			CancelArakawaAbilityMode();
			_isArakawaWheelOpen = false;
			return;
		}

		_isArakawaWheelOpen = !_isArakawaWheelOpen;
		if (_isArakawaWheelOpen)
		{
			GameAudio.Instance?.PlayUiConfirm();
		}
		else
		{
			GameAudio.Instance?.PlayUiCancel();
		}
	}

	private void OnArakawaAbilityRequested(string abilityId)
	{
		if (!CanUseArakawaThisTurn())
		{
			_isArakawaWheelOpen = false;
			return;
		}

		_isArakawaWheelOpen = false;
		switch (abilityId)
		{
			case "build_wall":
				BeginArakawaAbilityMode(ArakawaAbilityMode.BuildWall);
				break;

			case "enhance_card":
				BeginArakawaAbilityMode(ArakawaAbilityMode.EnhanceCard);
				break;

			case "enhance_weapon":
				TryExecuteArakawaEnhanceWeapon();
				break;
		}
	}

	private void OnArakawaCancelRequested()
	{
		_isArakawaWheelOpen = false;
		GameAudio.Instance?.PlayUiCancel();
		CancelArakawaAbilityMode();
	}

	private bool CanPlayerMoveThisTurn()
	{
		return TurnState?.CanMove != false;
	}

	private bool CanUseArakawaThisTurn()
	{
		return !_battleFailureSequenceStarted
			&& TurnState?.IsPlayerTurn == true
			&& GlobalSession != null
			&& GlobalSession.ArakawaCurrentEnergy > 0;
	}

	private bool CanAttemptRetreatThisTurn()
	{
		if (TurnState?.CanRetreat != true || GlobalSession == null || StateManager?.GetPrimaryPlayerState() is not BattleObjectState playerState)
		{
			return false;
		}

		if (!IsPlayerStandingOnEscapeCell(playerState.Cell))
		{
			return false;
		}

		return IsRetreatFeatureAvailable();
	}

	private bool IsRetreatFeatureAvailable()
	{
		if (GlobalSession == null)
		{
			return false;
		}

		if (_activeBattleRequest != null
			&& _activeBattleRequest.RuntimeModifiers.TryGetValue("allow_retreat", out Variant allowRetreatVariant)
			&& allowRetreatVariant.VariantType == Variant.Type.Bool)
		{
			return allowRetreatVariant.AsBool();
		}

		return true;
	}

	private bool IsPlayerStandingOnEscapeCell(Vector2I playerCell)
	{
		return CurrentRoom != null && CurrentRoom.GetEscapeCells().Contains(playerCell);
	}

	private void BeginArakawaAbilityMode(ArakawaAbilityMode abilityMode)
	{
		if (TurnState?.IsAttackTargeting == true || TurnState?.IsCardTargeting == true)
		{
			TurnState.CancelTargeting();
		}

		_arakawaAbilityMode = abilityMode;
	}

	private void CancelArakawaAbilityMode()
	{
		_arakawaAbilityMode = ArakawaAbilityMode.None;
	}

	private string GetCurrentArakawaAbilityId()
	{
		return _arakawaAbilityMode switch
		{
			ArakawaAbilityMode.BuildWall => BuildWallAbility.AbilityId,
			ArakawaAbilityMode.EnhanceCard => EnhanceCardAbility.AbilityId,
			_ => string.Empty,
		};
	}

	private void EndPlayerTurn()
	{
		if (_battleFailureSequenceStarted || TurnState == null)
		{
			return;
		}

		if (_isPlayerMoveResolving)
		{
			return;
		}

		if (!TurnState.IsPlayerTurn && !TurnState.IsAttackTargeting)
		{
			return;
		}

		TurnState.MarkEndedTurn();
		ResolveTurnPostPhase();
	}

	private BattleObjectState? GetHoveredObjectState(Vector2I hoveredCell)
	{
		if (QueryService == null || StateManager == null)
		{
			return null;
		}

		foreach (BoardObject boardObject in QueryService.GetObjectsAtCell(hoveredCell))
		{
			return StateManager.Get(boardObject.ObjectId);
		}

		return null;
	}

	private BoardObject? GetCardTargetAtCell(string sourceObjectId, Vector2I targetCell, BattleCardDefinition cardDefinition)
	{
		if (Registry == null || !Registry.TryGet(sourceObjectId, out BoardObject? sourceObject) || sourceObject == null)
		{
			return null;
		}

		return cardDefinition.TargetingMode switch
		{
			BattleCardTargetingMode.EnemyUnit => GetAttackableObjectAtCell(sourceObjectId, targetCell) is BoardObject enemyTarget
				? GetManhattanTarget(sourceObject, enemyTarget, cardDefinition.Range)
				: null,
			BattleCardTargetingMode.StraightLineEnemy => GetAttackableObjectAtCell(sourceObjectId, targetCell) is BoardObject lineEnemyTarget
				? GetStraightLineTarget(sourceObjectId, lineEnemyTarget, cardDefinition.Range)
				: null,
			BattleCardTargetingMode.FriendlyUnit => GetFriendlyUnitAtCell(sourceObjectId, targetCell) is BoardObject friendlyTarget
				? GetManhattanTarget(sourceObject, friendlyTarget, cardDefinition.Range)
				: null,
			BattleCardTargetingMode.Cell => CurrentRoom != null && CurrentRoom.Topology.IsInsideBoard(targetCell)
				&& GetManhattanDistance(sourceObject.Cell, targetCell) <= cardDefinition.Range
				? sourceObject
				: null,
			_ => null,
		};
	}

	private BoardObject? GetAttackableObjectAtCell(string sourceObjectId, Vector2I targetCell)
	{
		if (_actionService == null)
		{
			return null;
		}

		return _actionService.GetAttackableObjectAtCell(sourceObjectId, targetCell);
	}

	private BoardObject? GetFriendlyUnitAtCell(string sourceObjectId, Vector2I targetCell)
	{
		if (Registry == null || !Registry.TryGet(sourceObjectId, out BoardObject? sourceObject) || sourceObject == null || QueryService == null)
		{
			return null;
		}

		foreach (BoardObject boardObject in QueryService.GetObjectsAtCell(targetCell))
		{
			if (boardObject.ObjectType == BoardObjectType.Unit && boardObject.Faction == sourceObject.Faction)
			{
				return boardObject;
			}
		}

		return null;
	}

	private bool TryPlayCard(string attackerId, string cardInstanceId, string? targetId, Vector2I? targetCell, out string failureReason)
	{
		failureReason = string.Empty;

		if (Registry == null || BoardState == null || StateManager == null || _pieceViewManager == null || TurnState == null || CurrentRoom == null || _playerDeck == null)
		{
			failureReason = "Battle systems are not initialized.";
			return false;
		}

		if (!Registry.TryGet(attackerId, out BoardObject? attacker) || attacker == null)
		{
			failureReason = $"Attacker {attackerId} was not found.";
			return false;
		}

		if (!_playerDeck.TryGetHandCard(cardInstanceId, out BattleCardInstance? cardInstance) || cardInstance == null)
		{
			failureReason = $"Card {cardInstanceId} was not found in hand.";
			return false;
		}

		if (!_playerDeck.CanPlay(cardInstanceId, out failureReason))
		{
			return false;
		}

		if (!TryResolveCardTarget(attackerId, targetId, targetCell, cardInstance.Definition, out BoardObject? targetObject, out failureReason))
		{
			return false;
		}

		if (!_playerDeck.CommitPlayedCard(cardInstanceId, out BattleCardInstance? committedCard, out failureReason))
		{
			return false;
		}

		_hud?.PlayCardUseEffect(cardInstance);
		double cardUseAudioDuration = cardInstance.Definition.CardId == LearningCardId
			? GameAudio.Instance?.PlayLearningCardUse() ?? 0.0d
			: GameAudio.Instance?.PlayCardUse() ?? 0.0d;
		_actionService?.RegisterExternalPresentationDuration(cardUseAudioDuration);
		_pieceViewManager.PlayAction(attackerId);

		if (!TryResolveSpecialCardEffect(attackerId, cardInstance, targetObject, targetCell, out failureReason))
		{
			_playerDeck.RollbackPlayedCard(committedCard);
			return false;
		}

		if (targetObject != null && cardInstance.Definition.Damage > 0)
		{
			if (_actionService == null)
			{
				failureReason = "Battle action service is not initialized.";
				return false;
			}

			Vector2 knockbackDirection = Vector2.Zero;
			if (Registry != null && Registry.TryGet(attackerId, out BoardObject? attackerObject) && attackerObject != null)
			{
				knockbackDirection = new Vector2(
					targetObject.Cell.X - attackerObject.Cell.X,
					targetObject.Cell.Y - attackerObject.Cell.Y);
			}

			DamageApplicationResult damageResult = _actionService.ApplyDamageToTarget(
				targetObject.ObjectId,
				cardInstance.Definition.Damage,
				knockbackDirection,
				out bool wasDestroyed,
				out string damageFailureReason,
				allowKillKnockback: true);
			if (!string.IsNullOrWhiteSpace(damageFailureReason))
			{
				_playerDeck.RollbackPlayedCard(committedCard);
				failureReason = damageFailureReason;
				return false;
			}

			int damageAmount = SumImpactAmount(damageResult, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
			if (damageAmount > 0)
			{
				if (wasDestroyed && targetObject.ObjectType == BoardObjectType.Unit && targetObject.Faction == BoardObjectFaction.Enemy)
				{
					Vector2I attackerCell = Registry != null && Registry.TryGet(attackerId, out BoardObject? attackerObjectForFocus) && attackerObjectForFocus != null
						? attackerObjectForFocus.Cell
						: targetObject.Cell;
					TriggerBattleCameraFocusForCells(attackerCell, targetObject.Cell);
				}
				AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} \u653B\u51FB{damageAmount}");
			}
		}

		if (cardInstance.Definition.EnergyGain > 0)
		{
			_playerDeck.GainEnergy(cardInstance.Definition.EnergyGain);
		}

		if (cardInstance.Definition.ShieldGain > 0)
		{
			if (_actionService == null)
			{
				failureReason = "Battle action service is not initialized.";
				return false;
			}

			DamageApplicationResult shieldResult = _actionService.ApplyShieldGainToTarget(attackerId, cardInstance.Definition.ShieldGain, out string shieldFailureReason);
			if (!string.IsNullOrWhiteSpace(shieldFailureReason))
			{
				_playerDeck.RollbackPlayedCard(committedCard);
				failureReason = shieldFailureReason;
				return false;
			}

			int shieldGain = SumImpactAmount(shieldResult, CombatImpactType.ShieldGain);
			if (shieldGain > 0)
			{
				AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(attackerId)} \u62A4\u76FE+{shieldGain}");
			}
		}

		if (cardInstance.Definition.HealingAmount > 0)
		{
			if (_actionService == null)
			{
				failureReason = "Battle action service is not initialized.";
				return false;
			}

			string healingTargetId = targetObject?.ObjectId ?? attackerId;
			DamageApplicationResult healingResult = _actionService.ApplyHealingToTarget(healingTargetId, cardInstance.Definition.HealingAmount, out string healingFailureReason);
			if (!string.IsNullOrWhiteSpace(healingFailureReason))
			{
				_playerDeck.RollbackPlayedCard(committedCard);
				failureReason = healingFailureReason;
				return false;
			}

			int healAmount = SumImpactAmount(healingResult, CombatImpactType.HealthHeal);
			if (healAmount > 0)
			{
				AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(healingTargetId)} \u6CBB\u7597{healAmount}");
			}
		}

		if (cardInstance.Definition.DrawCount > 0)
		{
			_playerDeck.DrawCards(cardInstance.Definition.DrawCount);
		}

		StateManager.SyncAllFromRegistry();
		_pieceViewManager.Sync(Registry, StateManager, CurrentRoom);
		TurnState.MarkActed(!cardInstance.Definition.IsQuick);

		if (!cardInstance.Definition.IsQuick)
		{
			ResolveTurnPostPhase();
		}

		return true;
	}

	private bool TryAttackObject(string attackerId, string targetId, out string failureReason)
	{
		failureReason = string.Empty;

		if (_actionService == null || TurnState == null)
		{
			failureReason = "Battle systems are not initialized.";
			return false;
		}

		Vector2I attackerCell = Vector2I.Zero;
		Vector2I targetCell = Vector2I.Zero;
		BoardObjectFaction targetFaction = BoardObjectFaction.World;
		BoardObjectType targetType = BoardObjectType.Obstacle;
		if (Registry != null)
		{
			if (Registry.TryGet(attackerId, out BoardObject? attackerObject) && attackerObject != null)
			{
				attackerCell = attackerObject.Cell;
			}

			if (Registry.TryGet(targetId, out BoardObject? targetObject) && targetObject != null)
			{
				targetCell = targetObject.Cell;
				targetFaction = targetObject.Faction;
				targetType = targetObject.ObjectType;
			}
		}

		if (!_actionService.TryAttackObject(attackerId, targetId, out bool wasDestroyed, out failureReason, allowKillKnockback: true))
		{
			return false;
		}

		ConsumePlayerWeaponAttackCharge(attackerId);

		if (wasDestroyed && targetType == BoardObjectType.Unit && targetFaction == BoardObjectFaction.Enemy)
		{
			TriggerBattleCameraFocusForCells(attackerCell, targetCell);
		}

		TurnState.MarkActed();
		ResolveTurnPostPhase();
		return true;
	}

	private async void ResolveTurnPostPhase()
	{
		if (TurnState == null)
		{
			return;
		}

		if (TurnState.Phase != TurnPhase.TurnPost)
		{
			return;
		}

		if (TurnState.HasActed)
		{
			double resolveDelay = Math.Max(
				PlayerActionResolveBufferSeconds,
				_actionService?.LastImpactPresentationDurationSeconds ?? 0.0d);
			await ToSignal(GetTree().CreateTimer(resolveDelay), SceneTreeTimer.SignalName.Timeout);
			if (_battleFailureSequenceStarted || TurnState.Phase != TurnPhase.TurnPost)
			{
				return;
			}
		}

		_playerDeck?.EndPlayerTurn();
		StateManager?.SyncAllFromRegistry();
		_actionService?.ResolveTurnEnd(BoardObjectFaction.Player, TurnState.TurnIndex);
		if (_actionService?.IsPlayerDefeated == true)
		{
			StartBattleFailureSequence();
			return;
		}
		TurnState.BeginEnemyTurn();
		_actionService?.ResolveTurnStart(BoardObjectFaction.Enemy, TurnState.TurnIndex);
		if (_enemyTurnResolver != null)
		{
			await _enemyTurnResolver.ResolveTurnAsync();
		}

		if (_actionService?.IsPlayerDefeated == true)
		{
			StartBattleFailureSequence();
			return;
		}

		if (TryResolveRetreatSuccess())
		{
			return;
		}

		_actionService?.ResolveTurnEnd(BoardObjectFaction.Enemy, TurnState.TurnIndex);
		TurnState.AdvanceToNextTurn();
		AdvanceBattleActionLogTurn(TurnState.TurnIndex);
		if (_playerCounterStanceActive && TurnState.TurnIndex >= _playerCounterStanceExpiresOnTurnIndex)
		{
			_playerCounterStanceActive = false;
			_playerCounterStanceExpiresOnTurnIndex = -1;
		}
		if (_playerDeck != null)
		{
			SyncEnergyRechargeTrackingState();
			if (_playerDeck.CurrentEnergy < _playerDeck.MaxEnergyPerTurn && TurnState.AdvanceEnergyRechargeProgress())
			{
				_playerDeck.GainEnergy(_playerDeck.EnergyRegenAmount);
				SyncEnergyRechargeTrackingState();
			}
		}

		_playerDeck?.StartPlayerTurn();
		_actionService?.ResolveTurnStart(BoardObjectFaction.Player, TurnState.TurnIndex);
		ResolvePendingDelayedCardEffectsForTurnStart(TurnState.TurnIndex);
		TryResolveVictory();
	}

	private bool TryResolveVictory()
	{
		if (_battleResultCommitted || _battleFailureSequenceStarted || _battleVictorySequenceStarted || Registry == null)
		{
			return false;
		}

		bool hasRemainingEnemy = Registry.AllObjects.Any(boardObject =>
			boardObject.ObjectType == BoardObjectType.Unit
			&& boardObject.Faction == BoardObjectFaction.Enemy);
		if (hasRemainingEnemy)
		{
			return false;
		}

		StartBattleVictorySequence();
		return true;
	}

	private async void StartBattleVictorySequence()
	{
		if (_battleVictorySequenceStarted || _battleResultCommitted)
		{
			return;
		}

		_battleVictorySequenceStarted = true;
		double resolveDelay = Math.Max(
			PlayerActionResolveBufferSeconds,
			_actionService?.LastImpactPresentationDurationSeconds ?? 0.0d);
		if (resolveDelay > 0.0d)
		{
			await ToSignal(GetTree().CreateTimer(resolveDelay), SceneTreeTimer.SignalName.Timeout);
		}

		if (_battleResultCommitted || _battleFailureSequenceStarted)
		{
			return;
		}

		CommitBattleResult(BattleOutcome.Victory);
	}

	private bool TryResolveRetreatSuccess()
	{
		if (!_retreatPending || TurnState == null || GlobalSession == null)
		{
			return false;
		}

		bool isSameTurnRetreatWindow = _retreatTurnIndex == TurnState.TurnIndex;
		bool hpWasPreserved = _retreatStartHp >= 0 && GlobalSession.PlayerCurrentHp >= _retreatStartHp;
		_retreatPending = false;
		_retreatTurnIndex = -1;
		_retreatStartHp = -1;

		if (!isSameTurnRetreatWindow || !hpWasPreserved)
		{
			return false;
		}

		CommitBattleResult(BattleOutcome.Retreat);
		return true;
	}

	private void OnEnemyAttackResolved(string attackerId, string targetId, int attackRange)
	{
		if (!_playerCounterStanceActive || _actionService == null || StateManager == null || Registry == null)
		{
			return;
		}

		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		if (playerState == null || playerState.CurrentHp <= 0 || !string.Equals(playerState.ObjectId, targetId, StringComparison.Ordinal) || attackRange > 1)
		{
			return;
		}

		if (!Registry.TryGet(attackerId, out BoardObject? attackerObject) || attackerObject == null)
		{
			return;
		}

		DamageApplicationResult result = _actionService.ApplyDamageToTarget(
			attackerId,
			_playerCounterStanceDamage,
			ResolveDirectionVector(playerState.ObjectId, attackerId),
			out _,
			out string failureReason,
			allowKillKnockback: true);
		if (!string.IsNullOrWhiteSpace(failureReason))
		{
			return;
		}

		int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (damageAmount > 0)
		{
			AppendBattleActionLog($"{playerState.DisplayName}->{ResolveObjectDisplayName(attackerId)} \u53CD\u51FB{damageAmount}");
		}
	}

	private void OnActiveEnemyTurnChanged(string enemyObjectId)
	{
		_pieceViewManager?.SetActiveTurnObject(enemyObjectId);
		if (string.IsNullOrWhiteSpace(enemyObjectId) || Registry == null || StateManager == null || TurnState == null)
		{
			return;
		}

		if (!Registry.TryGet(enemyObjectId, out BoardObject? enemyObject) || enemyObject == null)
		{
			return;
		}

		if (enemyObject.DefinitionId == "pirate_brute_elite"
			&& StateManager.Get(enemyObjectId) is BattleObjectState bruteState
			&& bruteState.MaxHp > 0
			&& bruteState.CurrentHp * 2 <= bruteState.MaxHp)
		{
			ActivateEnemySignatureLearnState(enemyObjectId, TurnState.TurnIndex + 1);
		}

		if (enemyObject.DefinitionId == "scrap_medic_elite" && HasSupportHealerTarget(enemyObjectId))
		{
			ActivateEnemySignatureLearnState(enemyObjectId, TurnState.TurnIndex + 1);
		}
	}

	private Vector2I[] BuildActiveTelegraphCells()
	{
		if (StateManager == null)
		{
			return Array.Empty<Vector2I>();
		}

		return StateManager.AllStates
			.Where(state => state.IsTelegraphing)
			.SelectMany(state => state.PendingSpecialCells)
			.Distinct()
			.OrderBy(cell => cell.Y)
			.ThenBy(cell => cell.X)
			.ToArray();
	}

	private async Task ExecuteEnemySpecialDecisionAsync(string enemyId, EnemyAiDecision decision)
	{
		if (StateManager == null || Registry == null || QueryService == null || _pieceViewManager == null || _actionService == null)
		{
			return;
		}

		if (StateManager.Get(enemyId) is not BattleObjectState enemyState)
		{
			return;
		}

		switch (decision.DecisionType)
		{
			case EnemyAiDecisionType.Telegraph:
				enemyState.PendingSpecialSkillId = decision.SpecialSkillId;
				enemyState.PendingSpecialTargetObjectId = decision.TargetObjectId;
				enemyState.PendingSpecialTargetCell = decision.SpecialTargetCell;
				enemyState.PendingSpecialCells = decision.SpecialCells ?? Array.Empty<Vector2I>();
				AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)} \u9884\u544A\u4E86\u7279\u6B8A\u884C\u52A8");
				ShowBossTelegraphShout(enemyId, decision.SpecialSkillId);
				if (TurnState != null)
				{
					ActivateEnemySignatureLearnState(enemyId, TurnState.TurnIndex + 1);
				}
				break;

			case EnemyAiDecisionType.Special:
				await ExecuteEnemySpecialSkillAsync(enemyId, enemyState, decision);
				enemyState.ClearPendingSpecial();
				break;
		}
	}

	private void ShowBossTelegraphShout(string enemyId, string skillId)
	{
		if (_floatingTextLayer == null || Registry == null || CurrentRoom == null)
		{
			return;
		}

		if (!Registry.TryGet(enemyId, out BoardObject? enemyObject) || enemyObject == null)
		{
			return;
		}

		string shoutText = skillId switch
		{
			MagneticHuntSkillId => "\u8FC7\u6765\uff01",
			CaptainBashSkillId => "\u5316\u4E3A\u5C18\u57C3\uff01",
			FlameGridSkillId => "\u70C8\u706B\u71CE\u539F\uff01",
			CallCrewSkillId => "\u5168\u5458\u96C6\u7ED3\uff01",
			_ => string.Empty,
		};

		if (string.IsNullOrWhiteSpace(shoutText))
		{
			return;
		}

		Vector2 localPosition = CurrentRoom.CellToLocalCenter(enemyObject.Cell) + new Vector2(0.0f, -26.0f);
		_floatingTextLayer.ShowText(
			$"{enemyId}:telegraph",
			localPosition,
			shoutText,
			new Color(1.0f, 0.94f, 0.58f, 1.0f));
	}

	private async Task ExecuteEnemySpecialSkillAsync(string enemyId, BattleObjectState enemyState, EnemyAiDecision decision)
	{
		switch (decision.SpecialSkillId)
		{
			case PressureBreachSkillId:
				await ExecutePressureBreachAsync(enemyId, enemyState, decision);
				enemyState.SetSpecialSkillCooldown(PressureBreachSkillId, 3);
				break;

			case EmergencyRepairSkillId:
				await ExecuteEmergencyRepairAsync(enemyId, enemyState, decision);
				enemyState.SetSpecialSkillCooldown(EmergencyRepairSkillId, 3);
				break;

			case MagneticHuntSkillId:
				await ExecuteBossMagneticHuntAsync(enemyId, enemyState, decision);
				enemyState.SetSpecialSkillCooldown(MagneticHuntSkillId, 4);
				break;

			case CaptainBashSkillId:
				await ExecuteBossCaptainBashAsync(enemyId, enemyState, decision);
				enemyState.SetSpecialSkillCooldown(CaptainBashSkillId, 3);
				break;

			case "flame_grid":
				await ExecuteBossFlameGridAsync(enemyId, enemyState, decision);
				enemyState.SetSpecialSkillCooldown("flame_grid", 5);
				break;

			case "call_crew":
				await ExecuteBossCallCrewAsync(enemyId, enemyState, decision);
				enemyState.SetSpecialSkillCooldown("call_crew", 99);
				break;
		}
	}

	private async Task ExecutePressureBreachAsync(string enemyId, BattleObjectState enemyState, EnemyAiDecision decision)
	{
		Vector2 direction = ResolveDirectionVector(enemyId, decision.TargetObjectId);
		_pieceViewManager.PlayAction(enemyId);
		if (direction != Vector2.Zero)
		{
			_pieceViewManager.PlayMotionOffset(enemyId, direction.Normalized() * 6.0f, 0.10d, 0.18d);
		}

		await ToSignal(GetTree().CreateTimer(0.10d), SceneTreeTimer.SignalName.Timeout);

		bool dealtDamage = false;
		foreach (Vector2I cell in decision.SpecialCells)
		{
			foreach (BoardObject boardObject in QueryService!.GetObjectsAtCell(cell))
			{
				if (boardObject.ObjectId == enemyId)
				{
					continue;
				}

				if (boardObject.Faction == BoardObjectFaction.Enemy)
				{
					continue;
				}

				DamageApplicationResult result = _actionService!.ApplyDamageToTarget(
					boardObject.ObjectId,
					7,
					direction,
					out _,
					out string failureReason,
					allowKillKnockback: false);
				if (!string.IsNullOrWhiteSpace(failureReason))
				{
					continue;
				}

				int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
				if (damageAmount > 0)
				{
					dealtDamage = true;
					AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)}->{ResolveObjectDisplayName(boardObject.ObjectId)} \u9AD8\u538B\u7A81\u8FDB{damageAmount}");
				}
			}
		}

		if (dealtDamage)
		{
			DamageApplicationResult shieldResult = _actionService!.ApplyShieldGainToTarget(enemyId, 2, out _);
			int shieldAmount = SumImpactAmount(shieldResult, CombatImpactType.ShieldGain);
			if (shieldAmount > 0)
			{
				AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)}->{ResolveObjectDisplayName(enemyId)} \u62A4\u76FE+{shieldAmount}");
			}
		}

		StateManager!.SyncAllFromRegistry();
		_pieceViewManager.Sync(Registry!, StateManager!, CurrentRoom!);
	}

	private async Task ExecuteEmergencyRepairAsync(string enemyId, BattleObjectState enemyState, EnemyAiDecision decision)
	{
		_pieceViewManager!.PlayAction(enemyId);
		await ToSignal(GetTree().CreateTimer(0.08d), SceneTreeTimer.SignalName.Timeout);

		if (Registry == null || StateManager == null)
		{
			return;
		}

		if (!Registry.TryGet(enemyId, out BoardObject? healerObject) || healerObject == null)
		{
			return;
		}

		List<string> healedSegments = new();
		foreach (BoardObject boardObject in Registry.AllObjects
			.Where(boardObject => boardObject.ObjectId != enemyId)
			.Where(boardObject => boardObject.Faction == healerObject.Faction)
			.Where(boardObject => boardObject.Faction == BoardObjectFaction.Enemy)
			.Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit || boardObject.ObjectType == BoardObjectType.Obstacle)
			.Where(boardObject => GetManhattanDistance(healerObject.Cell, boardObject.Cell) <= 2))
		{
			DamageApplicationResult healingResult = _actionService!.ApplyHealingToTarget(boardObject.ObjectId, 3, out string healFailureReason);
			if (!string.IsNullOrWhiteSpace(healFailureReason))
			{
				continue;
			}

			int healAmount = SumImpactAmount(healingResult, CombatImpactType.HealthHeal);
			if (healAmount > 0)
			{
				healedSegments.Add($"{ResolveObjectDisplayName(boardObject.ObjectId)} \u6CBB\u7597{healAmount}");
			}
		}

		if (healedSegments.Count > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)} \u8303\u56F4\u4FEE\u590D: {string.Join(" / ", healedSegments)}");
		}

		StateManager!.SyncAllFromRegistry();
		_pieceViewManager.Sync(Registry!, StateManager!, CurrentRoom!);
	}

	private async Task ExecuteBossMagneticHuntAsync(string enemyId, BattleObjectState enemyState, EnemyAiDecision decision)
	{
		if (_actionService == null || Registry == null || QueryService == null || CurrentRoom == null)
		{
			return;
		}

		_pieceViewManager!.PlayAction(enemyId);
		await ToSignal(GetTree().CreateTimer(0.08d), SceneTreeTimer.SignalName.Timeout);
		ExecuteBossMagneticHunt(enemyId, decision.SpecialCells ?? Array.Empty<Vector2I>());
		StateManager!.SyncAllFromRegistry();
		_pieceViewManager.Sync(Registry!, StateManager!, CurrentRoom!);
	}

	private void ExecuteBossMagneticHunt(string enemyId, IReadOnlyList<Vector2I> lineCells)
	{
		if (_actionService == null || Registry == null || QueryService == null || CurrentRoom == null)
		{
			return;
		}

		if (!Registry.TryGet(enemyId, out BoardObject? enemyObject) || enemyObject == null)
		{
			return;
		}

		BoardObject? hitTarget = null;
		foreach (Vector2I cell in lineCells)
		{
			foreach (BoardObject boardObject in QueryService.GetObjectsAtCell(cell))
			{
				if (boardObject.ObjectId == enemyId || boardObject.Faction == BoardObjectFaction.Enemy)
				{
					continue;
				}

				if (boardObject.ObjectType == BoardObjectType.Obstacle || boardObject.BlocksLineOfSight)
				{
					return;
				}

				if (boardObject.ObjectType == BoardObjectType.Unit)
				{
					hitTarget = boardObject;
					break;
				}
			}

			if (hitTarget != null)
			{
				break;
			}
		}

		if (hitTarget == null)
		{
			return;
		}

		Vector2 knockDirection = ResolveDirectionVector(enemyId, hitTarget.ObjectId);
		DamageApplicationResult result = _actionService.ApplyDamageToTarget(
			hitTarget.ObjectId,
			4,
			knockDirection,
			out _,
			out string failureReason,
			allowKillKnockback: false);
		if (!string.IsNullOrWhiteSpace(failureReason))
		{
			return;
		}

		int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (damageAmount > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)}->{ResolveObjectDisplayName(hitTarget.ObjectId)} \u78C1\u7D22\u6355\u730E{damageAmount}");
		}

		if (hitTarget.IsDestroyed || StateManager?.Get(hitTarget.ObjectId) is not BattleObjectState targetState || !targetState.IsPlayer)
		{
			return;
		}

		if (lineCells.Count == 0)
		{
			return;
		}

		Vector2I pullDirection = lineCells[0] - enemyObject.Cell;
		if (!CurrentRoom.Topology.TryNormalizeCardinalDirection(pullDirection, out Vector2I normalizedDirection))
		{
			return;
		}

		Vector2I pullCell = enemyObject.Cell + normalizedDirection;
		if (!CurrentRoom.Topology.IsInsideBoard(pullCell) || !_actionService.TryMoveObject(hitTarget.ObjectId, pullCell, out _, ignoreTerrainEffects: true))
		{
			return;
		}

		StateManager?.SyncAllFromRegistry();
		_pieceViewManager?.Sync(Registry, StateManager!, CurrentRoom);
		_pieceViewManager?.PlayMove(hitTarget.ObjectId);
	}

	private async Task ExecuteBossCaptainBashAsync(string enemyId, BattleObjectState enemyState, EnemyAiDecision decision)
	{
		if (_actionService == null || Registry == null || QueryService == null)
		{
			return;
		}

		_pieceViewManager!.PlayAction(enemyId);
		await ToSignal(GetTree().CreateTimer(0.10d), SceneTreeTimer.SignalName.Timeout);
		if (CurrentRoom != null)
		{
			await _pieceViewManager.PlayTileCrashEffectAsync(decision.SpecialCells ?? Array.Empty<Vector2I>(), CurrentRoom);
		}

		HashSet<string> hitTargets = new(StringComparer.Ordinal);
		foreach (Vector2I cell in decision.SpecialCells ?? Array.Empty<Vector2I>())
		{
			foreach (BoardObject boardObject in QueryService.GetObjectsAtCell(cell))
			{
				if (boardObject.ObjectId == enemyId || !hitTargets.Add(boardObject.ObjectId))
				{
					continue;
				}

				Vector2 knockbackDirection = ResolveDirectionVector(enemyId, boardObject.ObjectId);
				DamageApplicationResult result = _actionService.ApplyDamageToTarget(
					boardObject.ObjectId,
					6,
					knockbackDirection,
					out _,
					out string failureReason,
					allowKillKnockback: false);
				if (!string.IsNullOrWhiteSpace(failureReason))
				{
					continue;
				}

				int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
				if (damageAmount > 0)
				{
					AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)}->{ResolveObjectDisplayName(boardObject.ObjectId)} \u8239\u957F\u91CD\u7838{damageAmount}");
				}
			}
		}

		StateManager!.SyncAllFromRegistry();
		_pieceViewManager.Sync(Registry!, StateManager!, CurrentRoom!);
	}

	private async Task ExecuteBossFlameGridAsync(string enemyId, BattleObjectState enemyState, EnemyAiDecision decision)
	{
		if (_actionService == null)
		{
			return;
		}

		_pieceViewManager!.PlayAction(enemyId);
		await ToSignal(GetTree().CreateTimer(0.08d), SceneTreeTimer.SignalName.Timeout);
		if (CurrentRoom != null)
		{
			await _pieceViewManager.PlayTileCrashEffectAsync(decision.SpecialCells ?? Array.Empty<Vector2I>(), CurrentRoom);
		}
		if (_actionService.TryCreateFireTerrain(decision.SpecialCells ?? Array.Empty<Vector2I>(), out _))
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)} \u6539\u9020\u4E86\u6218\u573A\u5730\u5F62");
		}

		StateManager!.SyncAllFromRegistry();
		_pieceViewManager.Sync(Registry!, StateManager!, CurrentRoom!);
	}

	private async Task ExecuteBossCallCrewAsync(string enemyId, BattleObjectState enemyState, EnemyAiDecision decision)
	{
		if (_actionService == null)
		{
			return;
		}

		_pieceViewManager!.PlayAction(enemyId);
		await ToSignal(GetTree().CreateTimer(0.08d), SceneTreeTimer.SignalName.Timeout);

		int spawnIndex = 0;
		foreach (Vector2I cell in (decision.SpecialCells ?? Array.Empty<Vector2I>()).Distinct())
		{
			BoardObjectSpawnDefinition spawn = new()
			{
				ObjectId = $"boss_crew_{Guid.NewGuid():N}"[..18],
				DefinitionId = "pirate_blocker",
				AiId = "melee_basic",
				ObjectType = BoardObjectType.Unit,
				Cell = cell,
				Faction = BoardObjectFaction.Enemy,
				Tags = new[] { "enemy", "pirate_blocker", "boss_summon" },
				MaxHp = 7,
				CurrentHp = 7,
				MaxShield = 1,
				CurrentShield = 1,
				BlocksMovement = true,
				BlocksLineOfSight = true,
				StackableWithUnit = false,
				InitialFacing = Vector2I.Left,
			};

			if (_actionService.TrySpawnBoardObject(spawn, out _, out _))
			{
				spawnIndex++;
			}
		}

		if (spawnIndex > 0)
		{
			enemyState.SetRuntimeFlag(BossCallCrewUsedFlagId);
			AppendBattleActionLog($"{ResolveObjectDisplayName(enemyId)} \u53EB\u6765\u4E86\u63F4\u519B");
		}

		StateManager!.SyncAllFromRegistry();
		_pieceViewManager.Sync(Registry!, StateManager!, CurrentRoom!);
	}

	private EnemyLearnRewardProfile? ResolveEnemyLearnRewardProfile(string definitionId)
	{
		BattleEnemyDefinition? enemyDefinition = BattleEnemyLibrary?.FindEntry(definitionId);
		if (enemyDefinition == null)
		{
			return null;
		}

		if (string.IsNullOrWhiteSpace(enemyDefinition.NormalLearnCardId)
			&& string.IsNullOrWhiteSpace(enemyDefinition.SignatureLearnCardId))
		{
			return null;
		}

		return new EnemyLearnRewardProfile
		{
			NormalCardId = enemyDefinition.NormalLearnCardId,
			SignatureCardId = enemyDefinition.SignatureLearnCardId,
		};
	}

	private bool IsSignatureLearnStateAvailable(BoardObject targetObject)
	{
		BattleEnemyDefinition? enemyDefinition = BattleEnemyLibrary?.FindEntry(targetObject.DefinitionId);
		if (_enemyLearnStates.TryGetValue(targetObject.ObjectId, out EnemyLearnState? state) && state.SignatureAvailable)
		{
			return true;
		}

		if (enemyDefinition?.SignatureLearnRequiresRuntimeFlag == true)
		{
			return false;
		}

		if (StateManager?.Get(targetObject.ObjectId) is not BattleObjectState targetState)
		{
			return false;
		}

		return enemyDefinition?.SignatureLearnAvailableAtHalfHpOrBelow == true
			&& targetState.MaxHp > 0
			&& targetState.CurrentHp * 2 <= targetState.MaxHp;
	}

	private void ActivateEnemySignatureLearnState(string enemyObjectId, int resetOnTurnIndex = -1)
	{
		if (string.IsNullOrWhiteSpace(enemyObjectId))
		{
			return;
		}

		if (!_enemyLearnStates.TryGetValue(enemyObjectId, out EnemyLearnState? state))
		{
			state = new EnemyLearnState();
			_enemyLearnStates[enemyObjectId] = state;
		}

		state.SignatureAvailable = true;
		state.ResetOnTurnIndex = resetOnTurnIndex;
	}

	private void ResetEnemyLearnStatesForTurnStart(int currentTurnIndex)
	{
		foreach (KeyValuePair<string, EnemyLearnState> entry in _enemyLearnStates.ToArray())
		{
			if (entry.Value.ResetOnTurnIndex != currentTurnIndex)
			{
				continue;
			}

			entry.Value.SignatureAvailable = false;
			entry.Value.ResetOnTurnIndex = -1;
		}
	}

	private Vector2 ResolveDirectionVector(string attackerId, string targetId)
	{
		Vector2I directionCell = ResolveDirectionCell(attackerId, targetId);
		return new Vector2(directionCell.X, directionCell.Y);
	}

	private Vector2I ResolveDirectionCell(string attackerId, string targetId)
	{
		if (Registry == null
			|| !Registry.TryGet(attackerId, out BoardObject? attackerObject)
			|| attackerObject == null
			|| !Registry.TryGet(targetId, out BoardObject? targetObject)
			|| targetObject == null)
		{
			return Vector2I.Right;
		}

		Vector2I direction = new(
			Math.Sign(targetObject.Cell.X - attackerObject.Cell.X),
			Math.Sign(targetObject.Cell.Y - attackerObject.Cell.Y));
		if (CurrentRoom != null && CurrentRoom.Topology.TryNormalizeCardinalDirection(direction, out Vector2I normalizedDirection))
		{
			return normalizedDirection;
		}

		return direction == Vector2I.Zero ? Vector2I.Right : direction;
	}

	private void ApplyPendingBattleRequest()
	{
		if (GlobalSession == null)
		{
			return;
		}

		_activeBattleRequest = GlobalSession.ConsumePendingBattleRequest();
		_activeBattleRequest?.ApplyToSession(GlobalSession);
		if (_activeBattleRequest == null)
		{
			return;
		}

		if (!string.IsNullOrWhiteSpace(_activeBattleRequest.EncounterId))
		{
			EncounterId = _activeBattleRequest.EncounterId;
		}

		if (_activeBattleRequest.RandomSeed > 0)
		{
			RandomSeed = _activeBattleRequest.RandomSeed;
			_rng.Seed = (ulong)_activeBattleRequest.RandomSeed;
		}
	}

	private void ApplyPendingEncounterId()
	{
		if (GlobalSession == null)
		{
			return;
		}

		string encounterId = GlobalSession.ConsumePendingBattleEncounterId();
		if (!string.IsNullOrWhiteSpace(encounterId))
		{
			EncounterId = encounterId;
		}
	}

	private void StartBattleFailureSequence()
	{
		if (_battleFailureSequenceStarted || GlobalSession == null)
		{
			return;
		}

		_battleFailureSequenceStarted = true;
		GameAudio.Instance?.StopMusic(0.18f);
		BattleObjectState? playerState = StateManager?.GetPrimaryPlayerState();
		if (playerState != null)
		{
			_pieceViewManager?.PlayDefeat(playerState.ObjectId);
		}

		if (_battleFailOverlay == null || _battleFailFlash == null || _battleFailLabel == null)
		{
			CommitBattleResult(BattleOutcome.Defeat);
			return;
		}

		_battleFailOverlay.Visible = true;
		_battleFailOverlay.Modulate = Colors.White;
		_battleFailFlash.Color = new Color(0.45f, 0.04f, 0.06f, 0.0f);
		_battleFailLabel.Visible = true;
		_battleFailLabel.Modulate = new Color(1.0f, 0.92f, 0.92f, 0.0f);
		_battleFailLabel.Scale = new Vector2(0.9f, 0.9f);

		Tween tween = CreateTween();
		tween.SetParallel();
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(_battleFailFlash, "color", new Color(0.45f, 0.04f, 0.06f, 0.82f), 0.18f);
		tween.TweenProperty(_battleFailLabel, "modulate:a", 1.0f, 0.16f).SetDelay(0.08f);
		tween.TweenProperty(_battleFailLabel, "scale", Vector2.One, 0.22f).SetDelay(0.08f);
		tween.Finished += () => CommitBattleResult(BattleOutcome.Defeat);
	}

	private void CommitBattleResult(BattleOutcome outcome)
	{
		if (_battleResultCommitted || GlobalSession == null)
		{
			return;
		}

		_battleResultCommitted = true;
		int remainingEnemyCount = Registry?.AllObjects.Count(boardObject =>
			boardObject.ObjectType == BoardObjectType.Unit
			&& boardObject.Faction == BoardObjectFaction.Enemy) ?? 0;
		Godot.Collections.Dictionary runtimeFlags = new()
		{
			["room_layout_id"] = CurrentRoom?.LayoutId ?? string.Empty,
			["initial_enemy_count"] = _initialEnemyUnitCount,
			["defeated_enemy_count"] = _defeatedEnemyDefinitionIds.Count,
			["remaining_enemy_count"] = remainingEnemyCount,
			["all_enemies_defeated"] = outcome == BattleOutcome.Victory && remainingEnemyCount == 0,
		};
		Godot.Collections.Array<string> defeatedEnemyDefinitionIds = new();
		foreach (string definitionId in _defeatedEnemyDefinitionIds)
		{
			defeatedEnemyDefinitionIds.Add(definitionId);
		}

		runtimeFlags["defeated_enemy_definition_ids"] = defeatedEnemyDefinitionIds;
		Godot.Collections.Array<string> encounteredEnemyDefinitionIds = new();
		HashSet<string> encounteredDefinitionIdSet = new(StringComparer.Ordinal);
		if (Registry != null)
		{
			foreach (BoardObject boardObject in Registry.AllObjects)
			{
				if (boardObject.ObjectType != BoardObjectType.Unit
					|| boardObject.Faction != BoardObjectFaction.Enemy
					|| string.IsNullOrWhiteSpace(boardObject.DefinitionId)
					|| !encounteredDefinitionIdSet.Add(boardObject.DefinitionId))
				{
					continue;
				}

				encounteredEnemyDefinitionIds.Add(boardObject.DefinitionId);
			}
		}

		foreach (string definitionId in _defeatedEnemyDefinitionIds.OrderBy(value => value, StringComparer.Ordinal))
		{
			if (string.IsNullOrWhiteSpace(definitionId) || !encounteredDefinitionIdSet.Add(definitionId))
			{
				continue;
			}

			encounteredEnemyDefinitionIds.Add(definitionId);
		}

		runtimeFlags["encountered_enemy_definition_ids"] = encounteredEnemyDefinitionIds;
		if (_pendingLearnedCardIds.Count > 0)
		{
			Godot.Collections.Array<string> learnedCardIds = new();
			foreach (string cardId in _pendingLearnedCardIds.OrderBy(value => value, StringComparer.Ordinal))
			{
				learnedCardIds.Add(cardId);
			}

			runtimeFlags["learned_card_ids"] = learnedCardIds;
		}
		GlobalSession.CompleteBattle(BattleResult.FromSession(
			GlobalSession,
			outcome,
			_activeBattleRequest?.RequestId ?? string.Empty,
			EncounterId,
			outcome == BattleOutcome.Victory ? EncounterId : string.Empty,
			runtimeFlags));
		ReturnToPendingMapSceneIfAny();
	}

	private void OnEnemyDefeated(string enemyObjectId, string definitionId)
	{
		if (string.IsNullOrWhiteSpace(definitionId))
		{
			return;
		}

		_defeatedEnemyDefinitionIds.Add(definitionId);
	}

	private BattleDeckRuntimeInit? BuildDeckRuntimeInit(IReadOnlyList<BattleCardDefinition> prototypeDeck)
	{
		if (_activeBattleRequest == null)
		{
			return null;
		}

		Dictionary<string, BattleCardDefinition> definitionMap = prototypeDeck
			.GroupBy(definition => definition.CardId, StringComparer.Ordinal)
			.ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

		BattleCardDefinition[] buildCards = ResolveCardDefinitionsFromSnapshot(_activeBattleRequest.DeckBuildSnapshot, "card_ids", definitionMap);
		if (BattleCardLibrary != null && BattleDeckBuildRules != null)
		{
			DeckBuildSnapshot deckBuildSnapshot = DeckBuildSnapshot.FromDictionary(_activeBattleRequest.DeckBuildSnapshot);
			ProgressionSnapshot progressionSnapshot = ProgressionSnapshot.FromDictionary(_activeBattleRequest.ProgressionSnapshot);
			BattleDeckConstructionService constructionService = new(BattleCardLibrary, BattleDeckBuildRules);
			BattleDeckValidationResult validationResult;
			BattleCardDefinition[] resolvedBuildCards = constructionService.BuildRuntimeDefinitions(deckBuildSnapshot, progressionSnapshot, out validationResult);
			if (validationResult.IsValid && resolvedBuildCards.Length > 0)
			{
				buildCards = resolvedBuildCards;
				definitionMap = resolvedBuildCards
					.GroupBy(definition => definition.CardId, StringComparer.Ordinal)
					.ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
			}
		else if (!validationResult.IsValid)
		{
			GD.PushWarning($"BattleSceneController: deck build snapshot failed validation. {string.Join(" | ", validationResult.Errors)}");
		}
	}
		BattleCardDefinition[] startingHandCards = ResolveCardDefinitionsFromSnapshot(_activeBattleRequest.DeckRuntimeInitOverrides, "starting_hand_card_ids", definitionMap);
		BattleCardDefinition[] startingDrawPileCards = ResolveCardDefinitionsFromSnapshot(_activeBattleRequest.DeckRuntimeInitOverrides, "starting_draw_pile_card_ids", definitionMap);
		BattleCardDefinition[] startingDiscardPileCards = ResolveCardDefinitionsFromSnapshot(_activeBattleRequest.DeckRuntimeInitOverrides, "starting_discard_pile_card_ids", definitionMap);
		BattleCardDefinition[] startingExhaustPileCards = ResolveCardDefinitionsFromSnapshot(_activeBattleRequest.DeckRuntimeInitOverrides, "starting_exhaust_pile_card_ids", definitionMap);
		int handSizeOverride = ReadIntOverride(_activeBattleRequest.DeckRuntimeInitOverrides, "hand_size_override");
		int maxEnergyOverride = ReadIntOverride(_activeBattleRequest.DeckRuntimeInitOverrides, "max_energy_override");
		int initialEnergy = ReadIntOverride(_activeBattleRequest.DeckRuntimeInitOverrides, "initial_energy");
		int openingDrawCount = ReadIntOverride(_activeBattleRequest.DeckRuntimeInitOverrides, "opening_draw_count");

		if (buildCards.Length == 0
			&& startingHandCards.Length == 0
			&& startingDrawPileCards.Length == 0
			&& startingDiscardPileCards.Length == 0
			&& startingExhaustPileCards.Length == 0
			&& handSizeOverride < 0
			&& maxEnergyOverride < 0
			&& initialEnergy < 0
			&& openingDrawCount < 0)
		{
			return null;
		}

		return new BattleDeckRuntimeInit
		{
			BuildCards = buildCards,
			StartingHandCards = startingHandCards,
			StartingDrawPileCards = startingDrawPileCards,
			StartingDiscardPileCards = startingDiscardPileCards,
			StartingExhaustPileCards = startingExhaustPileCards,
			HandSizeOverride = handSizeOverride,
			MaxEnergyOverride = maxEnergyOverride,
			InitialEnergy = initialEnergy,
			OpeningDrawCount = openingDrawCount,
		};
	}

	private IReadOnlyList<BattleCardDefinition> ResolveBattleDeckSource(
		IReadOnlyList<BattleCardDefinition> prototypeDeck,
		BattleDeckRuntimeInit? runtimeInit)
	{
		if (runtimeInit != null && runtimeInit.BuildCards.Length > 0)
		{
			return runtimeInit.BuildCards;
		}

		if (_activeBattleRequest != null
			&& _activeBattleRequest.DeckBuildSnapshot.TryGetValue("card_ids", out Variant cardIdsVariant)
			&& cardIdsVariant.Obj is Godot.Collections.Array)
		{
			return Array.Empty<BattleCardDefinition>();
		}

		return prototypeDeck;
	}

	private IReadOnlyList<BattleCardDefinition> BuildAvailableCardCatalog()
	{
		if (BattleCardLibrary != null && BattleCardLibrary.AllEntries.Count > 0)
		{
			List<BattleCardDefinition> definitions = BattleCardLibrary.AllEntries
				.Where(template => template != null)
				.Select(template => template.BuildRuntimeDefinition())
				.ToList();

			if (!definitions.Any(definition => string.Equals(definition.CardId, DrawRevolverCardId, StringComparison.Ordinal)))
			{
				definitions.Insert(1, new BattleCardDefinition(
					DrawRevolverCardId,
					"\u62D4\u67AA",
					"\u672C\u573A\u6218\u6597\u4E34\u65F6\u5207\u6362\u4E3A\u5DE6\u8F6E\uff1A\u666E\u901A\u653B\u51FB\u5C04\u7A0B\u6539\u4E3A 2\uff0c\u4F24\u5BB3\u6539\u4E3A 4\uff0c\u53EF\u653B\u51FB 6 \u6B21",
					2,
					BattleCardCategory.Skill,
					BattleCardTargetingMode.None));
			}

			if (!definitions.Any(definition => string.Equals(definition.CardId, ArcLeakCardId, StringComparison.Ordinal)))
			{
				definitions.Insert(2, new BattleCardDefinition(
					ArcLeakCardId,
					"\u7535\u5F27\u6CC4\u9732",
					"\u5BF9 3 \u683C\u5185\u76EE\u6807\u683C\u53CA\u5176\u76F8\u90BB\u683C\u751F\u6210\u7535\u5F27\u5730\u5F62",
					1,
					BattleCardCategory.Skill,
					BattleCardTargetingMode.Cell,
					range: 3));
			}

			if (!definitions.Any(definition => string.Equals(definition.CardId, RamCardId, StringComparison.Ordinal)))
			{
				definitions.Insert(3, new BattleCardDefinition(
					RamCardId,
					"\u51B2\u649E",
					"\u76F4\u7EBF 2 \u683C\u5185\u51B2\u5230\u76EE\u6807\u9762\u524D\uff0c\u9020\u6210 3 \u4F24\u5BB3\uff1B\u82E5\u76EE\u6807\u88AB\u963B\u6321\uff0C\u518D\u8FFD\u52A0 4 \u70B9\u649E\u51FB\u4F24\u5BB3",
					1,
					BattleCardCategory.Attack,
					BattleCardTargetingMode.StraightLineEnemy,
					range: 2));
			}

			if (!definitions.Any(definition => string.Equals(definition.CardId, AimCardId, StringComparison.Ordinal)))
			{
				definitions.Add(new BattleCardDefinition(
					AimCardId,
					"\u7784\u51C6",
					"\u5C04\u7A0B 4\uff1b\u6807\u8BB0\u76EE\u6807\uff0C\u4E0B\u4E00\u6B21\u5C04\u51FB\u989D\u5916 +1 \u4F24\u5BB3",
					0,
					BattleCardCategory.Skill,
					BattleCardTargetingMode.StraightLineEnemy,
					range: 4));
			}

			if (!definitions.Any(definition => string.Equals(definition.CardId, SnipeCardId, StringComparison.Ordinal)))
			{
				definitions.Add(new BattleCardDefinition(
					SnipeCardId,
					"\u72D9\u51FB",
					"\u5C04\u7A0B 4\uff1B\u5BF9\u5355\u4F53\u654C\u4EBA\u9020\u6210 7 \u4F24\u5BB3",
					1,
					BattleCardCategory.Attack,
					BattleCardTargetingMode.StraightLineEnemy,
					range: 4,
					damage: 7,
					exhaustsOnPlay: true));
			}

			if (!definitions.Any(definition => string.Equals(definition.CardId, AlertCardId, StringComparison.Ordinal)))
			{
				definitions.Add(new BattleCardDefinition(
					AlertCardId,
					"\u6212\u5907",
					"\u4E0B\u56DE\u5408\u5F00\u59CB\u65F6\uff0C\u5BF9\u8303\u56F4 2 \u683C\u5185\u5168\u90E8\u654C\u4EBA\u9020\u6210 6 \u4F24\u5BB3",
					1,
					BattleCardCategory.Skill,
					BattleCardTargetingMode.None));
			}

			if (!definitions.Any(definition => string.Equals(definition.CardId, RollCallCardId, StringComparison.Ordinal)))
			{
				definitions.Add(new BattleCardDefinition(
					RollCallCardId,
					"\u70B9\u540D",
					"\u5BF9\u5DF2\u6807\u8BB0\u76EE\u6807\u9020\u6210 3 \u4F24\u5BB3\uff1B\u51FB\u6740\u540E\u518D\u5BF9\u5C04\u7A0B 2 \u5185\u53E6\u4E00\u540D\u6700\u4F4E\u8840\u91CF\u654C\u4EBA\u9020\u6210 3 \u4F24\u5BB3",
					1,
					BattleCardCategory.Attack,
					BattleCardTargetingMode.None));
			}

			return definitions.ToArray();
		}

		return Array.Empty<BattleCardDefinition>();
	}

	private static int ResolveOpeningDrawCount(BattleDeckRuntimeInit? runtimeInit, int defaultCount)
	{
		if (runtimeInit == null)
		{
			return defaultCount;
		}

		if (runtimeInit.OpeningDrawCount >= 0)
		{
			return runtimeInit.OpeningDrawCount;
		}

		return runtimeInit.HasExplicitStartingHand ? 0 : defaultCount;
	}

	private static BattleCardDefinition[] ResolveCardDefinitionsFromSnapshot(
		Godot.Collections.Dictionary snapshot,
		string key,
		IReadOnlyDictionary<string, BattleCardDefinition> definitionMap)
	{
		if (!snapshot.TryGetValue(key, out Variant value) || value.Obj is not Godot.Collections.Array rawArray)
		{
			return Array.Empty<BattleCardDefinition>();
		}

		List<BattleCardDefinition> resolved = new();
		foreach (Variant item in rawArray)
		{
			string cardId = item.AsString();
			if (!string.IsNullOrWhiteSpace(cardId) && definitionMap.TryGetValue(cardId, out BattleCardDefinition? definition))
			{
				resolved.Add(definition);
			}
		}

		return resolved.ToArray();
	}

	private static BattleCardDefinition[] BuildRemainingDeckCards(IReadOnlyList<BattleCardDefinition> buildCards, IReadOnlyList<BattleCardDefinition> startingHandCards)
	{
		List<BattleCardDefinition> remaining = buildCards.ToList();
		foreach (BattleCardDefinition startingCard in startingHandCards)
		{
			int index = remaining.FindIndex(definition => string.Equals(definition.CardId, startingCard.CardId, StringComparison.Ordinal));
			if (index >= 0)
			{
				remaining.RemoveAt(index);
			}
		}

		return remaining.ToArray();
	}

	private static int ReadIntOverride(Godot.Collections.Dictionary snapshot, string key)
	{
		return snapshot.TryGetValue(key, out Variant value) ? value.AsInt32() : -1;
	}

	private async void ReturnToPendingMapSceneIfAny()
	{
		if (GlobalSession == null)
		{
			return;
		}

		MapResumeContext? resumeContext = GlobalSession.PeekPendingBattleReturnContext();
		BattleResult? resultSummary = GlobalSession.PeekLastBattleResult();

		if (resultSummary is BattleResult visibleResult
			&& (resultSummary.DidPlayerWin || resultSummary.DidPlayerRetreat || resultSummary.DidPlayerFail))
		{
			await ShowBattleResultOverlayAsync(visibleResult);
		}

		if (resultSummary?.DidPlayerFail == true)
		{
			if (GlobalSession.TryLoadPrimarySave(out string checkpointScenePath, out _))
			{
				await PlayBattleReturnOverlayAsync();
				Error rollbackResult = GetTree().ChangeSceneToFile(checkpointScenePath);
				if (rollbackResult != Error.Ok)
				{
					GD.PushError($"BattleSceneController: defeat rollback failed, error={rollbackResult}, path='{checkpointScenePath}'");
				}
				return;
			}

			GlobalSession.ClearPendingBattleReturnContext();
			await PlayBattleReturnOverlayAsync();
			Error menuResult = GetTree().ChangeSceneToFile(MainMenuScenePath);
			if (menuResult != Error.Ok)
			{
				GD.PushError($"BattleSceneController: defeat return to menu failed, error={menuResult}");
			}
			return;
		}

		if (resultSummary?.DidPlayerWin == true
			&& string.Equals(resultSummary.EncounterId, BossRustCaptainEncounterId, StringComparison.Ordinal))
		{
			GlobalSession.ClearPendingBattleReturnContext();
			await ShowEndOfDemoOverlayAsync();
			await PlayBattleReturnOverlayAsync();
			Error menuResult = GetTree().ChangeSceneToFile(MainMenuScenePath);
			if (menuResult != Error.Ok)
			{
				GD.PushError($"BattleSceneController: demo ending return to menu failed, error={menuResult}");
			}
			return;
		}

		if (resumeContext == null || string.IsNullOrWhiteSpace(resumeContext.ScenePath))
		{
			return;
		}

		await PlayBattleReturnOverlayAsync();

		Error result = GetTree().ChangeSceneToFile(resumeContext.ScenePath);
		if (result != Error.Ok)
		{
			GD.PushError($"BattleSceneController: return to map failed, error={result}");
		}
	}

	private async Task PlayBattleReturnOverlayAsync()
	{
		if (GD.Load<PackedScene>(BattleReturnTransitionOverlayScenePath) is PackedScene overlayScene
			&& overlayScene.Instantiate() is CardChessDemo.Map.BattleReturnTransitionOverlay overlay)
		{
			GameAudio.Instance?.StopMusic(0.20f);
			GetTree().Root.AddChild(overlay);
			await overlay.PlayAsync();
			overlay.QueueFree();
		}
	}

	private async Task ShowBattleResultOverlayAsync(BattleResult result)
	{
		CanvasLayer overlayLayer = new() { Layer = 120 };
		ColorRect dim = new()
		{
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			Color = new Color(0.04f, 0.05f, 0.08f, 0.84f),
			MouseFilter = Control.MouseFilterEnum.Stop,
		};
		PanelContainer panel = new()
		{
			CustomMinimumSize = new Vector2(250.0f, 132.0f),
			Position = new Vector2(35.0f, 24.0f),
			MouseFilter = Control.MouseFilterEnum.Stop,
		};
		VBoxContainer content = new();
		Label titleLabel = new()
		{
			Text = BuildBattleResultTitleText(result),
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		RichTextLabel detailLabel = new()
		{
			FitContent = true,
			ScrollActive = false,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			Text = BuildBattleResultSummaryText(result),
			CustomMinimumSize = new Vector2(220.0f, 80.0f),
		};
		Label continueLabel = new()
		{
			Text = "\u6309 E / Enter \u7EE7\u7EED",
			HorizontalAlignment = HorizontalAlignment.Right,
		};

		content.AddChild(titleLabel);
		content.AddChild(detailLabel);
		content.AddChild(continueLabel);
		panel.AddChild(content);
		overlayLayer.AddChild(dim);
		overlayLayer.AddChild(panel);
		GetTree().Root.AddChild(overlayLayer);

		while (GodotObject.IsInstanceValid(overlayLayer))
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (Input.IsActionJustPressed("interact") || Input.IsActionJustPressed("ui_accept"))
			{
				break;
			}
		}

		if (GodotObject.IsInstanceValid(overlayLayer))
		{
			overlayLayer.QueueFree();
		}
	}

	private static string BuildBattleResultTitleText(BattleResult result)
	{
		if (result.DidPlayerRetreat)
		{
			return "\u64A4\u79BB\u7ED3\u7B97";
		}

		if (result.DidPlayerFail)
		{
			return "\u6218\u8D25\u7ED3\u7B97";
		}

		return "\u6218\u6597\u7ED3\u7B97";
	}

	private static string BuildBattleResultSummaryText(BattleResult result)
	{
		List<string> lines = new();
		ProgressionDelta progressionDelta = result.GetProgressionDeltaModel();
		if (progressionDelta.ExperienceDelta > 0)
		{
			lines.Add($"\u7ECF\u9A8C +{progressionDelta.ExperienceDelta}");
		}

		Dictionary<string, int> defeatedEnemyCounts = new(StringComparer.Ordinal);
		foreach (Godot.Collections.Dictionary rewardEntryRaw in result.RewardEntries)
		{
			BattleRewardEntry rewardEntry = BattleRewardEntry.FromDictionary(rewardEntryRaw);
			if (rewardEntry.RewardType == "learned_card")
			{
				lines.Add($"\u5361\u724C\u89E3\u9501\uFF1A{rewardEntry.RewardId}");
				continue;
			}

			if (rewardEntry.RewardType == "enemy_defeat_exp")
			{
				if (!string.IsNullOrWhiteSpace(rewardEntry.RewardId))
				{
					defeatedEnemyCounts[rewardEntry.RewardId] = defeatedEnemyCounts.TryGetValue(rewardEntry.RewardId, out int count)
						? count + 1
						: 1;
				}
				continue;
			}

			if (rewardEntry.RewardType == "encounter_clear_bonus")
			{
				lines.Add($"\u5168\u706D\u5956\u52B1\uFF1A\u7ECF\u9A8C +{rewardEntry.Amount}");
				continue;
			}

			lines.Add($"{rewardEntry.RewardType}: {rewardEntry.RewardId} x{rewardEntry.Amount}");
		}

		foreach ((string enemyDefinitionId, int count) in defeatedEnemyCounts.OrderBy(entry => ResolveBattleResultEnemyDisplayName(entry.Key), StringComparer.Ordinal))
		{
			string enemyDisplayName = ResolveBattleResultEnemyDisplayName(enemyDefinitionId);
			lines.Add(count > 1
				? $"\u51FB\u8D25 {enemyDisplayName} x{count}"
				: $"\u51FB\u8D25 {enemyDisplayName}");
		}

		return lines.Count == 0 ? "\u65E0\u5956\u52B1" : string.Join("\n", lines);
	}

	private async Task ShowEndOfDemoOverlayAsync()
	{
		CanvasLayer overlayLayer = new() { Layer = 130 };
		ColorRect dim = new()
		{
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			Color = new Color(0.03f, 0.04f, 0.06f, 0.90f),
			MouseFilter = Control.MouseFilterEnum.Stop,
		};
		PanelContainer panel = new()
		{
			CustomMinimumSize = new Vector2(250.0f, 118.0f),
			Position = new Vector2(35.0f, 32.0f),
			MouseFilter = Control.MouseFilterEnum.Stop,
		};
		VBoxContainer content = new();
		Label titleLabel = new()
		{
			Text = "\u6E38\u620F\u5230\u6B64\u7ED3\u675F",
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		titleLabel.AddThemeFontSizeOverride("font_size", 16);
		RichTextLabel detailLabel = new()
		{
			FitContent = true,
			ScrollActive = false,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			Text = "\u4F60\u5DF2\u51FB\u8D25\u9523\u8239\u957F\u3002\n\u672C\u6B21 Demo \u5185\u5BB9\u5C31\u5230\u8FD9\u91CC\uff0c\u611F\u8C22\u6E38\u73A9\u3002",
			CustomMinimumSize = new Vector2(220.0f, 62.0f),
		};
		Label continueLabel = new()
		{
			Text = "\u6309 E / Enter \u8FD4\u56DE\u4E3B\u83DC\u5355",
			HorizontalAlignment = HorizontalAlignment.Right,
		};

		content.AddChild(titleLabel);
		content.AddChild(detailLabel);
		content.AddChild(continueLabel);
		panel.AddChild(content);
		overlayLayer.AddChild(dim);
		overlayLayer.AddChild(panel);
		GetTree().Root.AddChild(overlayLayer);

		while (GodotObject.IsInstanceValid(overlayLayer))
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (Input.IsActionJustPressed("interact") || Input.IsActionJustPressed("ui_accept"))
			{
				break;
			}
		}

		if (GodotObject.IsInstanceValid(overlayLayer))
		{
			overlayLayer.QueueFree();
		}
	}

	private static string ResolveBattleResultEnemyDisplayName(string enemyDefinitionId)
	{
		if (string.IsNullOrWhiteSpace(enemyDefinitionId))
		{
			return "\u654C\u4EBA";
		}

		BattleEnemyLibrary? enemyLibrary = GD.Load<BattleEnemyLibrary>("res://Resources/Battle/Enemies/DefaultBattleEnemyLibrary.tres");
		string displayName = enemyLibrary?.FindEntry(enemyDefinitionId)?.DisplayName ?? string.Empty;
		return string.IsNullOrWhiteSpace(displayName) ? enemyDefinitionId : displayName;
	}
	private void ConfigureCameraForBattle()
	{
		if (CurrentRoom == null)
		{
			return;
		}

		_battleCamera = GetNodeOrNull<Camera2D>("Camera2D");
		if (_battleCamera == null)
		{
			return;
		}

		Vector2 viewportSize = GetViewportRect().Size;
		float boardWidth = CurrentRoom.BoardSize.X * CurrentRoom.CellSizePixels;
		float boardHeight = CurrentRoom.BoardSize.Y * CurrentRoom.CellSizePixels;
		Vector2 boardOrigin = CurrentRoom.ToGlobal(Vector2.Zero);
		float topMargin = Mathf.Max(0.0f, CameraTopMarginPixels);
		float bottomMargin = Mathf.Max(0.0f, CameraBottomMarginPixels);
		float usableViewportHeight = Mathf.Max(1.0f, viewportSize.Y - topMargin - bottomMargin);
		float centeredTopInset = Mathf.Max(0.0f, (usableViewportHeight - boardHeight) * 0.5f);
		float targetBoardTop = topMargin + centeredTopInset;
		float currentBoardTop = boardOrigin.Y;
		float cameraYOffset = currentBoardTop - targetBoardTop;
		Vector2 boardCenter = boardOrigin + new Vector2(boardWidth * 0.5f, boardHeight * 0.5f);
		_cameraRestPosition = boardCenter + new Vector2(0.0f, cameraYOffset);

		_battleCamera.Enabled = true;
		_battleCamera.Zoom = new Vector2(CameraZoom, CameraZoom);
		_battleCamera.Position = _cameraRestPosition;
		_cameraPanBounds = BuildBattleCameraPanBounds(boardOrigin, boardWidth, boardHeight, _battleCamera.Zoom);
	}

	private Rect2 BuildBattleCameraPanBounds(Vector2 boardOrigin, float boardWidth, float boardHeight, Vector2 cameraZoom)
	{
		Vector2 viewportWorldSize = GetViewportRect().Size * cameraZoom;
		float minVisibleRatio = Mathf.Clamp(CameraMinBoardVisibleRatio, 0.1f, 1.0f);
		float minVisibleBoardWidth = boardWidth * minVisibleRatio;
		float minVisibleBoardHeight = boardHeight * minVisibleRatio;

		float minCenterX = boardOrigin.X + minVisibleBoardWidth - viewportWorldSize.X * 0.5f;
		float maxCenterX = boardOrigin.X + boardWidth - minVisibleBoardWidth + viewportWorldSize.X * 0.5f;
		float minCenterY = boardOrigin.Y + minVisibleBoardHeight - viewportWorldSize.Y * 0.5f;
		float maxCenterY = boardOrigin.Y + boardHeight - minVisibleBoardHeight + viewportWorldSize.Y * 0.5f;

		if (minCenterX > maxCenterX)
		{
			float centerX = boardOrigin.X + boardWidth * 0.5f;
			minCenterX = centerX;
			maxCenterX = centerX;
		}

		if (minCenterY > maxCenterY)
		{
			float centerY = boardOrigin.Y + boardHeight * 0.5f;
			minCenterY = centerY;
			maxCenterY = centerY;
		}

		return new Rect2(
			new Vector2(minCenterX, minCenterY),
			new Vector2(maxCenterX - minCenterX, maxCenterY - minCenterY));
	}

	private void UpdateBattleCameraPan(double delta)
	{
		if (_battleCamera == null || _isCameraCinematicBusy)
		{
			_cameraPanVelocity = Vector2.Zero;
			_cameraPanHoldTime = 0.0f;
			return;
		}

		float horizontalFactor = 0.0f;
		float verticalFactor = 0.0f;

		if (Input.IsKeyPressed(Key.A))
		{
			horizontalFactor -= 1.0f;
		}

		if (Input.IsKeyPressed(Key.D))
		{
			horizontalFactor += 1.0f;
		}

		if (Input.IsKeyPressed(Key.W))
		{
			verticalFactor -= 1.0f;
		}

		if (Input.IsKeyPressed(Key.S))
		{
			verticalFactor += 1.0f;
		}

		if (Mathf.IsZeroApprox(horizontalFactor) && Mathf.IsZeroApprox(verticalFactor))
		{
			_cameraPanHoldTime = 0.0f;
			_cameraPanVelocity = _cameraPanVelocity.MoveToward(Vector2.Zero, 640.0f * (float)delta);
		}
		else
		{
			_cameraResetTween?.Kill();
			Vector2 deltaMove = new(horizontalFactor, verticalFactor);
			if (deltaMove.LengthSquared() > 1.0f)
			{
				deltaMove = deltaMove.Normalized();
			}

			_cameraPanHoldTime = Mathf.Min(_cameraPanHoldTime + (float)delta, 0.45f);
			float rampedSpeed = Mathf.Lerp(48.0f, CameraPanPixelsPerSecond, _cameraPanHoldTime / 0.45f);
			_cameraPanVelocity = _cameraPanVelocity.MoveToward(deltaMove * rampedSpeed, 960.0f * (float)delta);
		}

		if (_cameraPanVelocity.LengthSquared() <= 0.01f)
		{
			return;
		}

		Vector2 nextPosition = _battleCamera.Position + _cameraPanVelocity * (float)delta;
		_battleCamera.Position = ClampBattleCameraPosition(nextPosition);
	}

	private void TryResetBattleCamera()
	{
		if (_battleCamera == null || _isCameraCinematicBusy)
		{
			return;
		}

		_cameraPanVelocity = Vector2.Zero;
		_cameraPanHoldTime = 0.0f;
		_cameraResetTween?.Kill();
		_cameraResetTween = CreateTween();
		_cameraResetTween.SetEase(Tween.EaseType.Out);
		_cameraResetTween.SetTrans(Tween.TransitionType.Cubic);
		_cameraResetTween.TweenProperty(
			_battleCamera,
			"position",
			ClampBattleCameraPosition(_cameraRestPosition),
			Math.Max(0.06d, CameraResetDurationMs / 1000.0d));
	}

	private Vector2 ClampBattleCameraPosition(Vector2 targetPosition)
	{
		if (_cameraPanBounds.Size == Vector2.Zero)
		{
			return targetPosition;
		}

		return new Vector2(
			Mathf.Clamp(targetPosition.X, _cameraPanBounds.Position.X, _cameraPanBounds.End.X),
			Mathf.Clamp(targetPosition.Y, _cameraPanBounds.Position.Y, _cameraPanBounds.End.Y));
	}

	private void TriggerBattleCameraFocusForCell(Vector2I cell)
	{
		if (CurrentRoom == null)
		{
			return;
		}

		double holdDuration = Math.Max(ArakawaBuildFocusHoldSeconds, BattleActionService.UtilityPresentationDurationSeconds);
		_ = PlayBattleCameraFocusAsync(GetBattleWorldPositionForCell(cell), holdDuration);
	}

	private void TriggerBattleCameraFocusForCells(Vector2I firstCell, Vector2I secondCell)
	{
		Vector2 focusPosition = (GetBattleWorldPositionForCell(firstCell) + GetBattleWorldPositionForCell(secondCell)) * 0.5f;
		double holdDuration = Math.Max(
			AttackFocusHoldSeconds,
			Math.Max(
				BattleActionService.AttackPresentationDurationSeconds,
				_actionService?.LastImpactPresentationDurationSeconds ?? 0.0d));
		_ = PlayBattleCameraFocusAsync(focusPosition, holdDuration);
	}

	private void TriggerBattleCameraFocusForObjects(string firstObjectId, string secondObjectId)
	{
		if (Registry == null
			|| !Registry.TryGet(firstObjectId, out BoardObject? firstObject) || firstObject == null
			|| !Registry.TryGet(secondObjectId, out BoardObject? secondObject) || secondObject == null)
		{
			return;
		}

		Vector2 focusPosition = (GetBattleWorldPositionForCell(firstObject.Cell) + GetBattleWorldPositionForCell(secondObject.Cell)) * 0.5f;
		double holdDuration = Math.Max(
			AttackFocusHoldSeconds,
			Math.Max(
				BattleActionService.AttackPresentationDurationSeconds,
				_actionService?.LastImpactPresentationDurationSeconds ?? 0.0d));
		_ = PlayBattleCameraFocusAsync(focusPosition, holdDuration);
	}

	private Vector2 GetBattleWorldPositionForCell(Vector2I cell)
	{
		if (CurrentRoom == null)
		{
			return Vector2.Zero;
		}

		return CurrentRoom.ToGlobal(CurrentRoom.CellToLocalCenter(cell));
	}

	private async System.Threading.Tasks.Task PlayBattleCameraFocusAsync(Vector2 focusPosition, double holdDuration = -1.0d)
	{
		if (_battleCamera == null || _isCameraCinematicBusy)
		{
			return;
		}

		_isCameraCinematicBusy = true;
		_cameraResetTween?.Kill();
		_cameraCinematicTween?.Kill();

		Vector2 previousPosition = _battleCamera.Position;
		Vector2 previousZoom = _battleCamera.Zoom;
		Vector2 clampedFocus = ClampBattleCameraPosition(focusPosition);
		float zoomMultiplier = Mathf.Clamp(Mathf.Round(CameraFocusZoomMultiplier), 1.0f, 4.0f);
		Vector2 focusZoom = new(previousZoom.X * zoomMultiplier, previousZoom.Y * zoomMultiplier);

		_battleCamera.Position = clampedFocus;
		_battleCamera.Zoom = focusZoom;

		double resolvedHold = holdDuration >= 0.0d ? holdDuration : CameraFocusHoldSeconds;
		if (resolvedHold > 0.0d)
		{
			await ToSignal(GetTree().CreateTimer(resolvedHold), SceneTreeTimer.SignalName.Timeout);
		}

		_battleCamera.Position = ClampBattleCameraPosition(previousPosition);
		_battleCamera.Zoom = previousZoom;
		_isCameraCinematicBusy = false;
	}

	private BattleRoomTemplate InstantiateSelectedRoom()
	{
		PackedScene roomScene = SelectRoomScene();
		return roomScene.Instantiate<BattleRoomTemplate>();
	}

	private void ResolveEncounterConfiguration()
	{
		if (EncounterLibrary == null || string.IsNullOrWhiteSpace(EncounterId))
		{
			return;
		}

		BattleEncounterProfile? encounterProfile = EncounterLibrary.FindEntry(EncounterId);
		if (encounterProfile == null)
		{
			GD.PushWarning($"BattleSceneController: encounter '{EncounterId}' was not found in EncounterLibrary.");
			return;
		}

		if (!string.IsNullOrWhiteSpace(encounterProfile.PrimaryEnemyDefinitionId))
		{
			EncounterEnemyDefinitionId = encounterProfile.PrimaryEnemyDefinitionId;
		}

		string[] configuredEnemyTypeIds = encounterProfile.EnemyTypeIds
			.Where(enemyTypeId => !string.IsNullOrWhiteSpace(enemyTypeId))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToArray();

		if (configuredEnemyTypeIds.Length > 0)
		{
			EncounterEnemyTypeIds = configuredEnemyTypeIds;
		}

		EncounterPreferredRoomPoolId = !string.IsNullOrWhiteSpace(encounterProfile.PreferredRoomPoolId)
			? encounterProfile.PreferredRoomPoolId.Trim()
			: encounterProfile.EncounterId;
	}

	private PackedScene SelectRoomScene()
	{
		if (ForcedBattleRoomScene != null)
		{
			return ForcedBattleRoomScene;
		}

		List<PackedScene> exactMatches = new();
		List<PackedScene> partialMatches = new();
		List<PackedScene> fallbackMatches = new();

		foreach (PackedScene scene in ExpandRoomScenePool())
		{
			BattleRoomTemplate previewRoom = scene.Instantiate<BattleRoomTemplate>();

			if (previewRoom.SupportedEnemyTypeIds.Length == 0)
			{
				fallbackMatches.Add(scene);
				previewRoom.Free();
				continue;
			}

			bool matches = previewRoom.SupportsEnemyTypes(EncounterEnemyTypeIds, out bool exactMatch);
			previewRoom.Free();

			if (!matches)
			{
				continue;
			}

			if (exactMatch)
			{
				exactMatches.Add(scene);
			}
			else
			{
				partialMatches.Add(scene);
			}
		}

		List<PackedScene> candidatePool = exactMatches.Count > 0
			? exactMatches
			: partialMatches.Count > 0
				? partialMatches
				: fallbackMatches.Count > 0
					? fallbackMatches
					: BattleRoomScenes.ToList();

		if (candidatePool.Count == 0)
		{
			throw new InvalidOperationException("BattleSceneController: no battle room scenes are configured.");
		}

		return candidatePool[_rng.RandiRange(0, candidatePool.Count - 1)];
	}

	private IEnumerable<PackedScene> ExpandRoomScenePool()
	{
		HashSet<PackedScene> pooledScenes = new();

		if (BattleRoomPools != null)
		{
			if (!string.IsNullOrWhiteSpace(EncounterPreferredRoomPoolId))
			{
				foreach (BattleRoomPoolEntry entry in BattleRoomPools.Entries)
				{
					if (!string.Equals(entry.RoomPoolId, EncounterPreferredRoomPoolId, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					foreach (PackedScene scene in entry.RoomScenes)
					{
						if (scene != null)
						{
							pooledScenes.Add(scene);
						}
					}
				}
			}

			if (pooledScenes.Count > 0)
			{
				return pooledScenes;
			}

			foreach (BattleRoomPoolEntry entry in BattleRoomPools.Entries)
			{
				bool entryMatchesEncounter = string.IsNullOrWhiteSpace(entry.EnemyTypeId)
					|| EncounterEnemyTypeIds.Length == 0
					|| EncounterEnemyTypeIds.Any(enemyTypeId =>
						string.Equals(enemyTypeId, entry.EnemyTypeId, StringComparison.OrdinalIgnoreCase));

				if (!entryMatchesEncounter)
				{
					continue;
				}

				foreach (PackedScene scene in entry.RoomScenes)
				{
					if (scene != null)
					{
						pooledScenes.Add(scene);
					}
				}
			}
		}

		if (pooledScenes.Count > 0)
		{
			return pooledScenes;
		}

		HashSet<PackedScene> directScenes = new();
		foreach (PackedScene scene in BattleRoomScenes)
		{
			if (scene != null)
			{
				directScenes.Add(scene);
			}
		}

		return directScenes;
	}

	private List<Vector2I> BuildReachableCells(string objectId, Vector2I origin, int moveRange)
	{
		List<Vector2I> cells = new();
		if (Pathfinder == null)
		{
			return cells;
		}

		return Pathfinder.FindReachableCells(objectId, origin, moveRange).ToList();
	}

	private List<Vector2I> BuildArakawaWallTargetCells()
	{
		List<Vector2I> cells = new();
		if (BoardState == null || QueryService == null)
		{
			return cells;
		}

		foreach (BoardCellState cellState in BoardState.EnumerateCells())
		{
			if (QueryService.GetObjectsAtCell(cellState.Cell).Count == 0)
			{
				cells.Add(cellState.Cell);
			}
		}

		return cells;
	}

	private List<Vector2I> BuildAttackTargetCells(string objectId, Vector2I origin, int attackRange)
	{
		List<Vector2I> cells = new();
		if (CurrentRoom == null || attackRange <= 0)
		{
			return cells;
		}

		for (int y = origin.Y - attackRange; y <= origin.Y + attackRange; y++)
		{
			for (int x = origin.X - attackRange; x <= origin.X + attackRange; x++)
			{
				Vector2I cell = new(x, y);
				if (cell == origin || !CurrentRoom.Topology.IsInsideBoard(cell))
				{
					continue;
				}

				int distance = Mathf.Abs(cell.X - origin.X) + Mathf.Abs(cell.Y - origin.Y);
				if (distance <= attackRange)
				{
					cells.Add(cell);
				}
			}
		}

		return cells;
	}

	private List<Vector2I> BuildSelectedCardTargetCells(string sourceObjectId)
	{
		if (_playerDeck == null || TurnState == null)
		{
			return new List<Vector2I>();
		}

		if (!_playerDeck.TryGetHandCard(TurnState.SelectedCardInstanceId, out BattleCardInstance? selectedCard) || selectedCard == null)
		{
			return new List<Vector2I>();
		}

		return BuildCardTargetCells(sourceObjectId, selectedCard.Definition);
	}

	private async void TryExecuteArakawaBuildWall(Vector2I targetCell)
	{
		if (_actionService == null || GlobalSession == null || !CanUseArakawaThisTurn())
		{
			return;
		}

		if (!BuildArakawaWallTargetCells().Contains(targetCell))
		{
			return;
		}

		if (!GlobalSession.TrySpendArakawaEnergy(BuildWallAbility.EnergyCost))
		{
			return;
		}

		bool created = await _actionService.TryCreateArakawaBarrierAsync(targetCell);
		if (!created)
		{
			GlobalSession.RestoreArakawaEnergy(BuildWallAbility.EnergyCost);
			return;
		}
		TriggerBattleCameraFocusForCell(targetCell);

		AppendBattleActionLog($"\u8352\u5DDD->({targetCell.X},{targetCell.Y}) \u9020\u7269");

		CancelArakawaAbilityMode();
	}

	private void TryExecuteArakawaEnhanceCard(string cardInstanceId)
	{
		if (_playerDeck == null || GlobalSession == null || _hud == null || !CanUseArakawaThisTurn())
		{
			return;
		}

		if (!_playerDeck.TryGetHandCard(cardInstanceId, out BattleCardInstance? cardInstance) || cardInstance == null)
		{
			return;
		}

		if (cardInstance.IsEnhanced)
		{
			return;
		}

		BattleCardEnhancementDefinition enhancement = ResolveArakawaEnhancementDefinition(cardInstance.BaseDefinition);

		if (!GlobalSession.TrySpendArakawaEnergy(EnhanceCardAbility.EnergyCost))
		{
			return;
		}

		if (!cardInstance.TryApplyEnhancement(enhancement))
		{
			GlobalSession.RestoreArakawaEnergy(EnhanceCardAbility.EnergyCost);
			return;
		}

		_hud.PlayCardEnhancementEffect(cardInstanceId);
		AppendBattleActionLog($"\u8352\u5DDD->{cardInstance.Definition.DisplayName} \u5F3A\u5316");
		CancelArakawaAbilityMode();
	}

	private static BattleCardEnhancementDefinition ResolveArakawaEnhancementDefinition(BattleCardDefinition definition)
	{
		if (PrototypeCardEnhancements.TryGetValue(definition.CardId, out BattleCardEnhancementDefinition? configuredEnhancement))
		{
			return NormalizeEnhancementDefinition(configuredEnhancement);
		}

		int damageDelta = definition.Damage > 0 ? 2 : 0;
		int healingDelta = definition.HealingAmount > 0 ? 2 : 0;
		int drawDelta = definition.DrawCount > 0 ? 1 : 0;
		int energyDelta = definition.EnergyGain > 0 ? 1 : 0;
		int shieldDelta = definition.ShieldGain > 0 ? 2 : 0;
		int rangeDelta = definition.RequiresTarget && definition.Range > 0 ? 1 : 0;
		int costDelta = damageDelta == 0 && healingDelta == 0 && drawDelta == 0 && energyDelta == 0 && shieldDelta == 0 && rangeDelta == 0
			? -1
			: 0;

		BattleCardEnhancementDefinition fallback = new(
			"+",
			string.Empty,
			costDelta: costDelta,
			rangeDelta: rangeDelta,
			damageDelta: damageDelta,
			healingDelta: healingDelta,
			drawCountDelta: drawDelta,
			energyGainDelta: energyDelta,
			shieldGainDelta: shieldDelta);
		return NormalizeEnhancementDefinition(fallback);
	}

	public static BattleCardEnhancementDefinition ResolveArakawaEnhancementPreview(BattleCardDefinition definition)
	{
		return ResolveArakawaEnhancementDefinition(definition);
	}

	private static BattleCardEnhancementDefinition NormalizeEnhancementDefinition(BattleCardEnhancementDefinition enhancement)
	{
		return new BattleCardEnhancementDefinition(
			enhancement.DisplaySuffix,
			BuildReadableEnhancementDescription(enhancement),
			enhancement.CostDelta,
			enhancement.RangeDelta,
			enhancement.DamageDelta,
			enhancement.HealingDelta,
			enhancement.DrawCountDelta,
			enhancement.EnergyGainDelta,
			enhancement.ShieldGainDelta,
			enhancement.IsQuickOverride,
			enhancement.ExhaustsOnPlayOverride);
	}

	private static string BuildReadableEnhancementDescription(BattleCardEnhancementDefinition enhancement)
	{
		List<string> parts = new();
		if (enhancement.DamageDelta != 0)
		{
			parts.Add($"\u4F24\u5BB3 {(enhancement.DamageDelta > 0 ? "+" : string.Empty)}{enhancement.DamageDelta}");
		}

		if (enhancement.HealingDelta != 0)
		{
			parts.Add($"\u6CBB\u7597 {(enhancement.HealingDelta > 0 ? "+" : string.Empty)}{enhancement.HealingDelta}");
		}

		if (enhancement.DrawCountDelta != 0)
		{
			parts.Add($"\u62BD\u724C {(enhancement.DrawCountDelta > 0 ? "+" : string.Empty)}{enhancement.DrawCountDelta}");
		}

		if (enhancement.EnergyGainDelta != 0)
		{
			parts.Add($"\u56DE\u80FD {(enhancement.EnergyGainDelta > 0 ? "+" : string.Empty)}{enhancement.EnergyGainDelta}");
		}

		if (enhancement.ShieldGainDelta != 0)
		{
			parts.Add($"\u62A4\u76FE {(enhancement.ShieldGainDelta > 0 ? "+" : string.Empty)}{enhancement.ShieldGainDelta}");
		}

		if (enhancement.RangeDelta != 0)
		{
			parts.Add($"\u5C04\u7A0B {(enhancement.RangeDelta > 0 ? "+" : string.Empty)}{enhancement.RangeDelta}");
		}

		if (enhancement.CostDelta != 0)
		{
			parts.Add($"\u8D39\u7528 {(enhancement.CostDelta > 0 ? "+" : string.Empty)}{enhancement.CostDelta}");
		}

		if (parts.Count == 0)
		{
			return "\u5F3A\u5316";
		}

		return string.Join(" / ", parts);
	}

	private void TryExecuteArakawaEnhanceWeapon()
	{
		if (GlobalSession == null || StateManager == null || _pieceViewManager == null || !CanUseArakawaThisTurn())
		{
			return;
		}

		BattleObjectState? playerState = StateManager.GetPrimaryPlayerState();
		if (playerState == null)
		{
			return;
		}

		if (!GlobalSession.TrySpendArakawaEnergy(EnhanceWeaponAbility.EnergyCost))
		{
			return;
		}

		StateManager.AddPlayerAttackDamageBonus(1);
		_pieceViewManager.PlayTintPulse(playerState.ObjectId, new Color(0.30f, 0.76f, 1.0f, 1.0f));
		AppendBattleActionLog($"\u8352\u5DDD->{playerState.DisplayName} \u6B66\u5668\u5F3A\u5316");
		CancelArakawaAbilityMode();
	}

	private bool TryResolveSpecialCardEffect(string attackerId, BattleCardInstance cardInstance, BoardObject? targetObject, Vector2I? targetCell, out string failureReason)
	{
		failureReason = string.Empty;

		if (cardInstance.Definition.CardId == DrawRevolverCardId)
		{
			return TryApplyDrawRevolverCard(attackerId, cardInstance, out failureReason);
		}

		if (cardInstance.Definition.CardId == ArcLeakCardId)
		{
			return TryApplyArcLeakCard(attackerId, targetCell, out failureReason);
		}

		if (cardInstance.Definition.CardId == AimCardId)
		{
			return TryApplyAimCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == AlertCardId)
		{
			return TryApplyAlertCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == RollCallCardId)
		{
			return TryApplyRollCallCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == RamCardId)
		{
			return TryApplyRamCard(attackerId, targetObject, out failureReason);
		}

		if (cardInstance.Definition.CardId == StanceCardId)
		{
			return TryApplyStanceCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == HeavyBlowCardId)
		{
			return TryApplyHeavyBlowCard(attackerId, targetObject, out failureReason);
		}

		if (cardInstance.Definition.CardId == ConcussionShotCardId)
		{
			return TryApplyConcussionShotCard(attackerId, targetObject, out failureReason);
		}

		if (cardInstance.Definition.CardId == WeatheringCardId)
		{
			return TryApplyWeatheringCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == LearningCardId)
		{
			return TryApplyLearningCard(attackerId, targetObject, out failureReason);
		}

		if (cardInstance.Definition.CardId == RollCardId)
		{
			return TryApplyRollCard(attackerId, targetCell, out failureReason);
		}

		if (cardInstance.Definition.CardId == AlertGuardCardId)
		{
			return TryApplyAlertGuardCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == PlunderCardId)
		{
			return TryApplyPlunderCard(attackerId, targetObject, out failureReason);
		}

		if (cardInstance.Definition.CardId == StructuralBoostCardId)
		{
			return TryApplyStructuralBoostCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == TacticalShiftCardId)
		{
			return TryApplyTacticalShiftCard(attackerId, targetCell, out failureReason);
		}

		if (cardInstance.Definition.CardId == OptimizeCardId)
		{
			return TryApplyOptimizeCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == ContemplateCardId)
		{
			return TryApplyContemplateCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == RepairCardId)
		{
			return TryApplyRepairCard(attackerId, out failureReason);
		}

		if (cardInstance.Definition.CardId == FieldPatchPlusCardId)
		{
			return TryApplyFieldPatchPlusCard(attackerId, targetObject, out failureReason);
		}

		if (cardInstance.Definition.CardId == MagneticHuntCardId)
		{
			return TryApplyMagneticHuntCard(attackerId, targetObject, out failureReason);
		}

		return true;
	}

	private bool TryApplyLearningCard(string attackerId, BoardObject? targetObject, out string failureReason)
	{
		failureReason = string.Empty;
		if (targetObject == null || targetObject.ObjectType != BoardObjectType.Unit || targetObject.Faction != BoardObjectFaction.Enemy)
		{
			failureReason = "Learning card requires an enemy target.";
			return false;
		}

		EnemyLearnRewardProfile? rewardProfile = ResolveEnemyLearnRewardProfile(targetObject.DefinitionId);
		if (rewardProfile == null)
		{
			failureReason = "This enemy does not support learning rewards yet.";
			return false;
		}

		bool signatureAvailable = IsSignatureLearnStateAvailable(targetObject);
		string learnedCardId = signatureAvailable ? rewardProfile.SignatureCardId : rewardProfile.NormalCardId;
		if (string.IsNullOrWhiteSpace(learnedCardId))
		{
			failureReason = "Learning reward card is missing.";
			return false;
		}

		_pendingLearnedCardIds.Add(learnedCardId);
		int signatureEnergyBonus = signatureAvailable ? GetPlayerTalentScalarBonus("effect.learning_signature_energy_bonus.") : 0;
		if (signatureEnergyBonus > 0 && _playerDeck != null)
		{
			_playerDeck.GainEnergy(signatureEnergyBonus);
		}
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} \u5B66\u4E60{(signatureAvailable ? "\u00B7\u7279\u6280" : "\u00B7\u57FA\u7840")}");
		return true;
	}

	private bool TryApplyRollCard(string attackerId, Vector2I? targetCell, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || targetCell == null)
		{
			failureReason = "Roll target cell is missing.";
			return false;
		}

		if (!_actionService.TryMoveObject(attackerId, targetCell.Value, out failureReason, ignoreTerrainEffects: true))
		{
			return false;
		}

		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->({targetCell.Value.X},{targetCell.Value.Y}) 移动");
		_pieceViewManager?.PlayIdle(attackerId);
		return true;
	}

	private bool TryApplyAlertGuardCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || Registry == null || !Registry.TryGet(attackerId, out BoardObject? attackerObject) || attackerObject == null)
		{
			failureReason = "Alert guard systems are not initialized.";
			return false;
		}

		int enemyCount = Registry.AllObjects
			.Count(boardObject => boardObject.ObjectType == BoardObjectType.Unit
				&& boardObject.Faction == BoardObjectFaction.Enemy
				&& Mathf.Abs(boardObject.Cell.X - attackerObject.Cell.X) + Mathf.Abs(boardObject.Cell.Y - attackerObject.Cell.Y) <= 2);
		int shieldAmount = 2 + enemyCount * 2;
		DamageApplicationResult result = _actionService.ApplyShieldGainToTarget(attackerId, shieldAmount, out string shieldFailureReason);
		if (!string.IsNullOrWhiteSpace(shieldFailureReason))
		{
			failureReason = shieldFailureReason;
			return false;
		}

		int appliedShield = SumImpactAmount(result, CombatImpactType.ShieldGain);
		if (appliedShield > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(attackerId)} 鐠€锔藉劆+{appliedShield}");
		}

		return true;
	}

	private bool TryApplyPlunderCard(string attackerId, BoardObject? targetObject, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || targetObject == null)
		{
			failureReason = "Plunder target is missing.";
			return false;
		}

		Vector2 knockbackDirection = ResolveDirectionVector(attackerId, targetObject.ObjectId);
		DamageApplicationResult result = _actionService.ApplyDamageToTarget(
			targetObject.ObjectId,
			5,
			knockbackDirection,
			out bool wasDestroyed,
			out string damageFailureReason,
			allowKillKnockback: true);
		if (!string.IsNullOrWhiteSpace(damageFailureReason))
		{
			failureReason = damageFailureReason;
			return false;
		}

		int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (damageAmount > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} 伤害{damageAmount}");
		}

		if (!wasDestroyed || _playerDeck == null)
		{
			return true;
		}

		_playerDeck.DrawCards(1);
		_playerDeck.GainEnergy(1);
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(attackerId)} 閸戠粯娼冮幒鐘层仛");
		return true;
	}

	private bool TryApplyStructuralBoostCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (StateManager == null)
		{
			failureReason = "Structural boost systems are not initialized.";
			return false;
		}

		StateManager.AddPlayerAttackDamageBonus(1);
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->\u7ED3\u6784\u6027\u8865\u5F3A");
		return true;
	}

	private bool TryApplyTacticalShiftCard(string attackerId, Vector2I? targetCell, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || Registry == null || targetCell == null)
		{
			failureReason = "Tactical shift target cell is missing.";
			return false;
		}

		if (!_actionService.TryMoveObject(attackerId, targetCell.Value, out failureReason))
		{
			return false;
		}

		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->({targetCell.Value.X},{targetCell.Value.Y}) 战术转移");
		int adjacentObstacleCount = CountAdjacentObstacles(targetCell.Value);
		if (adjacentObstacleCount <= 0)
		{
			_pieceViewManager?.PlayIdle(attackerId);
			return true;
		}

		DamageApplicationResult shieldResult = _actionService.ApplyShieldGainToTarget(attackerId, 3, out string shieldFailureReason);
		if (!string.IsNullOrWhiteSpace(shieldFailureReason))
		{
			failureReason = shieldFailureReason;
			return false;
		}

		int shieldAmount = SumImpactAmount(shieldResult, CombatImpactType.ShieldGain);
		if (shieldAmount > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(attackerId)} 护盾+{shieldAmount}");
		}

		return true;
	}

	private bool TryApplyOptimizeCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (_playerDeck == null || TurnState == null)
		{
			failureReason = "Optimize systems are not initialized.";
			return false;
		}

		_playerDeck.ModifyEnergyRegenInterval(-1);
		TurnState.ConfigureEnergyRechargeInterval(_playerDeck.EnergyRegenIntervalTurns);
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->优化");
		return true;
	}

	private bool TryApplyContemplateCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (TurnState == null || StateManager?.GetPrimaryPlayerState() is not BattleObjectState playerState)
		{
			failureReason = "Contemplate systems are not initialized.";
			return false;
		}

		_pendingDelayedCardEffects.Add(new PendingDelayedCardEffect
		{
			Kind = PendingDelayedCardEffectKind.ContemplateEnergy,
			SourceObjectId = attackerId,
			TriggerTurnIndex = TurnState.TurnIndex + 1,
			Energy = 3,
			RequiredHp = playerState.CurrentHp,
		});
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->\u6C89\u601D\u5F85\u53D1");
		return true;
	}

	private bool TryApplyRepairCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || Registry == null || !Registry.TryGet(attackerId, out BoardObject? attackerObject) || attackerObject == null)
		{
			failureReason = "Repair systems are not initialized.";
			return false;
		}

		BoardObject[] targets = Registry.AllObjects
			.Where(boardObject => boardObject.ObjectType == BoardObjectType.Obstacle)
			.Where(boardObject => boardObject.Faction == BoardObjectFaction.World || boardObject.Faction == BoardObjectFaction.Player)
			.Where(boardObject => GetManhattanDistance(attackerObject.Cell, boardObject.Cell) <= 2)
			.ToArray();
		if (targets.Length == 0)
		{
			failureReason = "No friendly obstacle is in repair range.";
			return false;
		}

		bool applied = false;
		int repairAmount = 3 + GetPlayerTalentScalarBonus("effect.field_heal_bonus.");
		foreach (BoardObject obstacle in targets)
		{
			DamageApplicationResult result = _actionService.ApplyHealingToTarget(obstacle.ObjectId, repairAmount, out string healFailureReason);
			if (!string.IsNullOrWhiteSpace(healFailureReason))
			{
				continue;
			}

			int healAmount = SumImpactAmount(result, CombatImpactType.HealthHeal);
			if (healAmount > 0)
			{
				applied = true;
				AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(obstacle.ObjectId)} 治疗{healAmount}");
			}
		}

		if (!applied)
		{
			failureReason = "No obstacle needed repair.";
		}

		return applied;
	}

	private bool TryApplyFieldPatchPlusCard(string attackerId, BoardObject? targetObject, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || targetObject == null)
		{
			failureReason = "Emergency repair target is missing.";
			return false;
		}

		int healAmountBase = 3 + GetPlayerTalentScalarBonus("effect.field_heal_bonus.");
		int shieldAmountBase = 2 + GetPlayerTalentScalarBonus("effect.field_shield_bonus.");
		DamageApplicationResult healingResult = _actionService.ApplyHealingToTarget(targetObject.ObjectId, healAmountBase, out string healFailureReason);
		if (!string.IsNullOrWhiteSpace(healFailureReason))
		{
			failureReason = healFailureReason;
			return false;
		}

		DamageApplicationResult shieldResult = _actionService.ApplyShieldGainToTarget(targetObject.ObjectId, shieldAmountBase, out string shieldFailureReason);
		if (!string.IsNullOrWhiteSpace(shieldFailureReason))
		{
			failureReason = shieldFailureReason;
			return false;
		}

		int healAmount = SumImpactAmount(healingResult, CombatImpactType.HealthHeal);
		int shieldAmount = SumImpactAmount(shieldResult, CombatImpactType.ShieldGain);
		List<string> segments = new();
		if (healAmount > 0)
		{
			segments.Add($"治疗{healAmount}");
		}

		if (shieldAmount > 0)
		{
			segments.Add($"护盾+{shieldAmount}");
		}

		if (segments.Count > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} {string.Join("/", segments)}");
		}

		return segments.Count > 0;
	}

	private bool TryApplyMagneticHuntCard(string attackerId, BoardObject? targetObject, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || targetObject == null || Registry == null || QueryService == null || CurrentRoom == null)
		{
			failureReason = "Magnetic hunt systems are not initialized.";
			return false;
		}

		Vector2I direction = ResolveDirectionCell(attackerId, targetObject.ObjectId);
		DamageApplicationResult result = _actionService.ApplyDamageToTarget(
			targetObject.ObjectId,
			4,
			new Vector2(direction.X, direction.Y),
			out _,
			out string damageFailureReason,
			allowKillKnockback: false);
		if (!string.IsNullOrWhiteSpace(damageFailureReason))
		{
			failureReason = damageFailureReason;
			return false;
		}

		int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (damageAmount > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} 绾句胶鍌ㄩ幑鏇犲{damageAmount}");
		}

		if (targetObject.IsDestroyed || !Registry.TryGet(attackerId, out BoardObject? attackerObject) || attackerObject == null)
		{
			return true;
		}

		Vector2I pullDirection = new(
			Math.Sign(attackerObject.Cell.X - targetObject.Cell.X),
			Math.Sign(attackerObject.Cell.Y - targetObject.Cell.Y));
		if (!CurrentRoom.Topology.TryNormalizeCardinalDirection(pullDirection, out Vector2I normalizedPullDirection))
		{
			return true;
		}

		Vector2I originalCell = targetObject.Cell;
		Vector2I currentCell = targetObject.Cell;
		for (int step = 0; step < 2; step++)
		{
			Vector2I nextCell = currentCell + normalizedPullDirection;
			if (!CurrentRoom.Topology.IsInsideBoard(nextCell))
			{
				break;
			}

			if (!QueryService.TryMoveObject(targetObject.ObjectId, nextCell, out _))
			{
				break;
			}

			currentCell = nextCell;
		}

		if (currentCell != originalCell)
		{
			StateManager?.SyncAllFromRegistry();
			_pieceViewManager?.Sync(Registry, StateManager!, CurrentRoom);
			_pieceViewManager?.PlayMoveOnce(targetObject.ObjectId, BattleActionService.MovePresentationDurationSeconds);
		}

		return true;
	}

	private int CountAdjacentObstacles(Vector2I cell)
	{
		if (Registry == null)
		{
			return 0;
		}

		return BoardTopology.CardinalDirections
			.Select(direction => cell + direction)
			.Count(neighbor => Registry.AllObjects.Any(boardObject =>
				boardObject.Cell == neighbor && boardObject.ObjectType == BoardObjectType.Obstacle));
	}

	private bool HasSupportHealerTarget(string enemyObjectId)
	{
		if (Registry == null || StateManager == null || !Registry.TryGet(enemyObjectId, out BoardObject? healerObject) || healerObject == null)
		{
			return false;
		}

		return Registry.AllObjects
			.Where(boardObject => boardObject.ObjectId != enemyObjectId)
			.Where(boardObject => boardObject.Faction == healerObject.Faction)
			.Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit || boardObject.ObjectType == BoardObjectType.Obstacle)
			.Select(boardObject => StateManager.Get(boardObject.ObjectId))
			.Any(state => state != null && (state.CurrentHp < state.MaxHp || state.CurrentShield < state.MaxShield));
	}

	private bool TryApplyStanceCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (TurnState == null)
		{
			failureReason = "Turn state is not initialized.";
			return false;
		}

		_playerCounterStanceActive = true;
		_playerCounterStanceExpiresOnTurnIndex = TurnState.TurnIndex + 1;
		_playerCounterStanceDamage = 6 + GetPlayerTalentScalarBonus("effect.stance_counter_bonus.");
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->\u67B6\u52BF\u5F85\u53D1");
		return true;
	}

	private bool TryApplyHeavyBlowCard(string attackerId, BoardObject? targetObject, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || targetObject == null)
		{
			failureReason = "Heavy blow target is missing.";
			return false;
		}

		int shieldBonusDamage = targetObject.CurrentShield > 0
			? Mathf.CeilToInt(6.0f * 0.5f) + GetPlayerTalentScalarBonus("effect.heavy_blow_shield_bonus.")
			: 0;
		int totalDamage = 6 + shieldBonusDamage;
		Vector2 knockbackDirection = ResolveDirectionVector(attackerId, targetObject.ObjectId);
		DamageApplicationResult result = _actionService.ApplyDamageToTarget(
			targetObject.ObjectId,
			totalDamage,
			knockbackDirection,
			out bool wasDestroyed,
			out string damageFailureReason,
			allowKillKnockback: true);
		if (!string.IsNullOrWhiteSpace(damageFailureReason))
		{
			failureReason = damageFailureReason;
			return false;
		}

		int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (damageAmount > 0)
		{
			if (wasDestroyed && targetObject.ObjectType == BoardObjectType.Unit && targetObject.Faction == BoardObjectFaction.Enemy)
			{
				Vector2I attackerCell = Registry != null && Registry.TryGet(attackerId, out BoardObject? attackerObjectForFocus) && attackerObjectForFocus != null
					? attackerObjectForFocus.Cell
					: targetObject.Cell;
				TriggerBattleCameraFocusForCells(attackerCell, targetObject.Cell);
			}

			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} \u91CD\u51FB{damageAmount}");
		}

		return true;
	}

	private bool TryApplyConcussionShotCard(string attackerId, BoardObject? targetObject, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || targetObject == null || Registry == null || QueryService == null || CurrentRoom == null)
		{
			failureReason = "Concussion shot systems are not initialized.";
			return false;
		}

		Vector2I direction = ResolveDirectionCell(attackerId, targetObject.ObjectId);
		int shotDamage = 3 + GetPlayerTalentScalarBonus("effect.concussion_shot_damage_bonus.");
		DamageApplicationResult result = _actionService.ApplyDamageToTarget(
			targetObject.ObjectId,
			shotDamage,
			new Vector2(direction.X, direction.Y),
			out _,
			out string damageFailureReason,
			allowKillKnockback: false);
		if (!string.IsNullOrWhiteSpace(damageFailureReason))
		{
			failureReason = damageFailureReason;
			return false;
		}

		int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (damageAmount > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} \u9707\u8361{damageAmount}");
		}

		if (targetObject.IsDestroyed)
		{
			return true;
		}

		Vector2I nextCell = targetObject.Cell + direction;
		if (!CurrentRoom.Topology.IsInsideBoard(nextCell))
		{
			return true;
		}

		List<BoardObject> blockers = QueryService.GetObjectsAtCell(nextCell)
			.Where(boardObject => boardObject.ObjectId != targetObject.ObjectId)
			.Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit || boardObject.ObjectType == BoardObjectType.Obstacle || boardObject.BlocksMovement || !boardObject.StackableWithUnit)
			.ToList();
		if (blockers.Count == 0 && QueryService.TryMoveObject(targetObject.ObjectId, nextCell, out _))
		{
			StateManager?.SyncAllFromRegistry();
			_pieceViewManager?.Sync(Registry, StateManager!, CurrentRoom);
			_pieceViewManager?.PlayMoveOnce(targetObject.ObjectId, BattleActionService.MovePresentationDurationSeconds);
			return true;
		}

		BoardObject? collisionObject = blockers.FirstOrDefault();
		int collisionBonusDamage = GetPlayerTalentScalarBonus("effect.concussion_shot_collision_bonus.");
		DamageApplicationResult targetCollisionResult = _actionService.ApplyDamageToTarget(
			targetObject.ObjectId,
			2 + collisionBonusDamage,
			new Vector2(direction.X, direction.Y),
			out _,
			out string collisionFailureReason,
			allowKillKnockback: false);
		if (!string.IsNullOrWhiteSpace(collisionFailureReason))
		{
			failureReason = collisionFailureReason;
			return false;
		}

		int collisionDamage = SumImpactAmount(targetCollisionResult, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (collisionDamage > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(targetObject.ObjectId)}->\u649E\u51FB{collisionDamage}");
		}

		if (collisionObject != null)
		{
			DamageApplicationResult blockerResult = _actionService.ApplyDamageToTarget(
				collisionObject.ObjectId,
				2 + collisionBonusDamage,
				new Vector2(direction.X, direction.Y),
				out _,
				out string blockerFailureReason,
				allowKillKnockback: false);
			if (!string.IsNullOrWhiteSpace(blockerFailureReason))
			{
				failureReason = blockerFailureReason;
				return false;
			}

			int blockerDamage = SumImpactAmount(blockerResult, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
			if (blockerDamage > 0)
			{
				AppendBattleActionLog($"{ResolveObjectDisplayName(collisionObject.ObjectId)}->\u649E\u51FB{blockerDamage}");
			}
		}

		return true;
	}

	private bool TryApplyWeatheringCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || Registry == null || !Registry.TryGet(attackerId, out BoardObject? attackerObject) || attackerObject == null)
		{
			failureReason = "Weathering attacker was not found.";
			return false;
		}

		BoardObject[] targets = Registry.AllObjects
			.Where(boardObject => boardObject.ObjectType == BoardObjectType.Obstacle)
			.Where(boardObject => Mathf.Abs(boardObject.Cell.X - attackerObject.Cell.X) + Mathf.Abs(boardObject.Cell.Y - attackerObject.Cell.Y) <= 2)
			.ToArray();
		if (targets.Length == 0)
		{
			failureReason = "No obstacle is in weathering range.";
			return false;
		}

		foreach (BoardObject obstacle in targets)
		{
			int weatheringDamage = 3 + GetPlayerTalentScalarBonus("effect.weathering_damage_bonus.");
			DamageApplicationResult result = _actionService.ApplyDamageToTarget(
				obstacle.ObjectId,
				weatheringDamage,
				Vector2.Zero,
				out _,
				out string damageFailureReason,
				allowKillKnockback: false);
			if (!string.IsNullOrWhiteSpace(damageFailureReason))
			{
				continue;
			}

			int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
			if (damageAmount > 0)
			{
				AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(obstacle.ObjectId)} \u98CE\u5316{damageAmount}");
			}
		}

		return true;
	}

	private bool TryApplyDrawRevolverCard(string attackerId, BattleCardInstance cardInstance, out string failureReason)
	{
		failureReason = string.Empty;
		if (StateManager == null || GlobalSession == null || _pieceViewManager == null)
		{
			failureReason = "Battle systems are not initialized.";
			return false;
		}

		StateManager.ActivateTemporaryWeaponOverride(DrawnRevolverWeaponItemId, DrawnRevolverBasicAttackCharges);
		StateManager.SyncAllFromRegistry();

		if (Registry != null && Registry.TryGet(attackerId, out BoardObject? attackerObject) && attackerObject != null)
		{
			_pieceViewManager.PlayTintPulse(attackerId, new Color(0.94f, 0.82f, 0.34f, 1.0f));
			TriggerBattleCameraFocusForCell(attackerObject.Cell);
		}

		EquipmentDefinition? weaponDefinition = GlobalSession.FindEquipmentDefinition(DrawnRevolverWeaponItemId);
		string weaponName = string.IsNullOrWhiteSpace(weaponDefinition?.DisplayName) ? "Revolver" : weaponDefinition.DisplayName;
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{weaponName} \u88C5\u586B{DrawnRevolverBasicAttackCharges}\u51FB");
		return true;
	}

	private bool TryApplyArcLeakCard(string attackerId, Vector2I? targetCell, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || targetCell == null)
		{
			failureReason = "Arc terrain target cell is missing.";
			return false;
		}

		if (!_actionService.TryCreateArcTerrain(targetCell.Value, out failureReason))
		{
			return false;
		}

		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->({targetCell.Value.X},{targetCell.Value.Y}) \u7535\u5F27\u6CC4\u9732");
		return true;
	}

	private bool TryApplyAimCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (_playerDeck == null)
		{
			failureReason = "Player deck is not initialized.";
			return false;
		}

		BattleCardDefinition snipeDefinition = BuildAvailableCardCatalog()
			.FirstOrDefault(definition => string.Equals(definition.CardId, SnipeCardId, StringComparison.Ordinal))
			?? new BattleCardDefinition(SnipeCardId, "Snipe", "Range 4. Deal 7 damage to a single enemy.", 1, BattleCardCategory.Attack, BattleCardTargetingMode.StraightLineEnemy, range: 4, damage: 7, exhaustsOnPlay: true);
		_playerDeck.AddTemporaryCardToHand(snipeDefinition);
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->\u72D9\u51FB\u5F85\u53D1");
		return true;
	}

	private bool TryApplyAlertCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		if (TurnState == null)
		{
			failureReason = "Turn state is not initialized.";
			return false;
		}

		_pendingDelayedCardEffects.Add(new PendingDelayedCardEffect
		{
			Kind = PendingDelayedCardEffectKind.AlertStrike,
			SourceObjectId = attackerId,
			TriggerTurnIndex = TurnState.TurnIndex + 1,
			Radius = 2,
			Damage = 6,
		});
		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->\u6212\u5907\u5F85\u53D1");
		return true;
	}

	private bool TryApplyRollCallCard(string attackerId, out string failureReason)
	{
		failureReason = string.Empty;
		BoardObject? firstTarget = FindLowestHpEnemyInRange(attackerId, 2);
		if (firstTarget == null)
		{
			failureReason = "No valid roll-call target found.";
			return false;
		}

		if (!TryResolveRollCallHit(attackerId, firstTarget.ObjectId, out bool firstKilled, out failureReason))
		{
			return false;
		}

		if (!firstKilled)
		{
			return true;
		}

		BoardObject? secondTarget = FindLowestHpEnemyInRange(attackerId, 2);
		if (secondTarget == null)
		{
			return true;
		}

		return TryResolveRollCallHit(attackerId, secondTarget.ObjectId, out _, out failureReason);
	}

	private bool TryApplyRamCard(string attackerId, BoardObject? targetObject, out string failureReason)
	{
		failureReason = string.Empty;
		if (_actionService == null || Registry == null || QueryService == null || CurrentRoom == null || targetObject == null)
		{
			failureReason = "Ram card systems are not initialized.";
			return false;
		}

		if (!Registry.TryGet(attackerId, out BoardObject? attackerObject) || attackerObject == null)
		{
			failureReason = "Ram attacker was not found.";
			return false;
		}

		Vector2I direction = new(
			Math.Sign(targetObject.Cell.X - attackerObject.Cell.X),
			Math.Sign(targetObject.Cell.Y - attackerObject.Cell.Y));
		if (!CurrentRoom.Topology.TryNormalizeCardinalDirection(direction, out Vector2I normalizedDirection))
		{
			failureReason = "Ram requires a straight-line target.";
			return false;
		}

		Vector2I standCell = targetObject.Cell - normalizedDirection;
		if (standCell != attackerObject.Cell)
		{
			if (!CurrentRoom.Topology.IsInsideBoard(standCell))
			{
				failureReason = "No valid ram landing cell exists.";
				return false;
			}

			if (!_actionService.TryMoveObject(attackerId, standCell, out failureReason))
			{
				return false;
			}
		}

		Vector2 knockbackDirection = new(normalizedDirection.X, normalizedDirection.Y);
		_pieceViewManager?.PlayAttackExchange(attackerId, knockbackDirection, targetObject.ObjectId);
		DamageApplicationResult ramResult = _actionService.ApplyDamageToTarget(
			targetObject.ObjectId,
			3,
			knockbackDirection,
			out _,
			out string ramFailureReason,
			allowKillKnockback: false);
		if (!string.IsNullOrWhiteSpace(ramFailureReason))
		{
			failureReason = ramFailureReason;
			return false;
		}

		if (targetObject.IsDestroyed)
		{
			return true;
		}

		int ramDamage = SumImpactAmount(ramResult, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (ramDamage > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetObject.ObjectId)} \u51B2\u649E{ramDamage}");
		}

		List<Vector2I> knockbackPath = new() { targetObject.Cell };
		BoardObject? collisionObject = null;
		bool hitBoundary = false;
		Vector2I currentCell = targetObject.Cell;
		for (int step = 0; step < 2; step++)
		{
			Vector2I nextCell = currentCell + normalizedDirection;
			if (!CurrentRoom.Topology.IsInsideBoard(nextCell))
			{
				hitBoundary = true;
				break;
			}

			List<BoardObject> blockers = QueryService.GetObjectsAtCell(nextCell)
				.Where(boardObject => boardObject.ObjectId != targetObject.ObjectId)
				.Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit || boardObject.ObjectType == BoardObjectType.Obstacle || boardObject.BlocksMovement || !boardObject.StackableWithUnit)
				.ToList();
			if (blockers.Count > 0)
			{
				collisionObject = blockers[0];
				break;
			}

			if (!QueryService.TryMoveObject(targetObject.ObjectId, nextCell, out _))
			{
				collisionObject = QueryService.GetObjectsAtCell(nextCell).FirstOrDefault(boardObject => boardObject.ObjectId != targetObject.ObjectId);
				break;
			}

			currentCell = nextCell;
			knockbackPath.Add(currentCell);
		}

		if (knockbackPath.Count > 1)
		{
			StateManager?.SyncAllFromRegistry();
			_pieceViewManager?.Sync(Registry, StateManager!, CurrentRoom);
			_pieceViewManager?.PlayMoveOnce(targetObject.ObjectId, BattleActionService.MovePresentationDurationSeconds);
		}

		if (collisionObject == null && !hitBoundary)
		{
			return true;
		}

		DamageApplicationResult targetCollisionResult = _actionService.ApplyDamageToTarget(
			targetObject.ObjectId,
			4,
			knockbackDirection,
			out _,
			out string bonusFailureReason,
			allowKillKnockback: false);
		if (!string.IsNullOrWhiteSpace(bonusFailureReason))
		{
			failureReason = bonusFailureReason;
			return false;
		}

		int targetCollisionDamage = SumImpactAmount(targetCollisionResult, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (targetCollisionDamage > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(targetObject.ObjectId)}->\u649E\u51FB{targetCollisionDamage}");
		}

		if (collisionObject == null)
		{
			return true;
		}

		DamageApplicationResult blockerCollisionResult = _actionService.ApplyDamageToTarget(
			collisionObject.ObjectId,
			4,
			knockbackDirection,
			out _,
			out string blockerFailureReason,
			allowKillKnockback: false);
		if (!string.IsNullOrWhiteSpace(blockerFailureReason))
		{
			failureReason = blockerFailureReason;
			return false;
		}

		int blockerCollisionDamage = SumImpactAmount(blockerCollisionResult, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (blockerCollisionDamage > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(collisionObject.ObjectId)}->\u88AB\u649E{blockerCollisionDamage}");
		}

		return true;
	}

	private void ResolvePendingDelayedCardEffectsForTurnStart(int currentTurnIndex)
	{
		if (_actionService == null)
		{
			return;
		}

		ResetEnemyLearnStatesForTurnStart(currentTurnIndex);

		for (int index = _pendingDelayedCardEffects.Count - 1; index >= 0; index--)
		{
			PendingDelayedCardEffect effect = _pendingDelayedCardEffects[index];
			if (effect.TriggerTurnIndex != currentTurnIndex)
			{
				continue;
			}

			switch (effect.Kind)
			{
				case PendingDelayedCardEffectKind.AlertStrike:
					ResolveAlertStrike(effect);
					break;

				case PendingDelayedCardEffectKind.ContemplateEnergy:
					ResolveContemplateEnergy(effect);
					break;
			}

			_pendingDelayedCardEffects.RemoveAt(index);
		}
	}

	private void ResolveAlertStrike(PendingDelayedCardEffect effect)
	{
		if (_actionService == null || Registry == null || !Registry.TryGet(effect.SourceObjectId, out BoardObject? sourceObject) || sourceObject == null)
		{
			return;
		}

		foreach (BoardObject enemy in Registry.AllObjects
			.Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit && boardObject.Faction == BoardObjectFaction.Enemy)
			.Where(boardObject => GetManhattanDistance(sourceObject.Cell, boardObject.Cell) <= effect.Radius)
			.OrderBy(boardObject => boardObject.Cell.Y)
			.ThenBy(boardObject => boardObject.Cell.X)
			.ToArray())
		{
			DamageApplicationResult result = _actionService.ApplyDamageToTarget(
				enemy.ObjectId,
				effect.Damage,
				Vector2.Zero,
				out _,
				out string failureReason,
				allowKillKnockback: false);
			if (!string.IsNullOrWhiteSpace(failureReason))
			{
				continue;
			}

			int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
			if (damageAmount > 0)
			{
				AppendBattleActionLog($"{ResolveObjectDisplayName(effect.SourceObjectId)}->{ResolveObjectDisplayName(enemy.ObjectId)} \u6212\u5907{damageAmount}");
			}
		}
	}

	private void ResolveContemplateEnergy(PendingDelayedCardEffect effect)
	{
		if (_playerDeck == null || StateManager?.GetPrimaryPlayerState() is not BattleObjectState playerState)
		{
			return;
		}

		if (playerState.CurrentHp < effect.RequiredHp)
		{
			return;
		}

		_playerDeck.GainEnergy(effect.Energy);
		AppendBattleActionLog($"{ResolveObjectDisplayName(effect.SourceObjectId)}->{ResolveObjectDisplayName(effect.SourceObjectId)} \u6C89\u601D\u56DE\u80FD+{effect.Energy}");
	}

	private void SyncEnergyRechargeTrackingState()
	{
		if (_playerDeck == null || TurnState == null)
		{
			return;
		}

		TurnState.ConfigureEnergyRechargeInterval(_playerDeck.EnergyRegenIntervalTurns);
		if (_playerDeck.CurrentEnergy >= _playerDeck.MaxEnergyPerTurn)
		{
			TurnState.ResetEnergyRechargeProgress();
		}
	}

	private BoardObject? FindLowestHpEnemyInRange(string attackerId, int range)
	{
		if (Registry == null || StateManager == null || !Registry.TryGet(attackerId, out BoardObject? attackerObject) || attackerObject == null)
		{
			return null;
		}

		return Registry.AllObjects
			.Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit && boardObject.Faction == BoardObjectFaction.Enemy)
			.Where(boardObject => GetManhattanDistance(attackerObject.Cell, boardObject.Cell) <= range)
			.OrderBy(boardObject => StateManager.Get(boardObject.ObjectId)?.CurrentHp ?? int.MaxValue)
			.ThenBy(boardObject => StateManager.Get(boardObject.ObjectId)?.CurrentShield ?? int.MaxValue)
			.ThenBy(boardObject => boardObject.Cell.Y)
			.ThenBy(boardObject => boardObject.Cell.X)
			.FirstOrDefault();
	}

	private bool TryResolveRollCallHit(string attackerId, string targetId, out bool killed, out string failureReason)
	{
		killed = false;
		failureReason = string.Empty;
		if (_actionService == null)
		{
			failureReason = "Battle action service is not initialized.";
			return false;
		}

		DamageApplicationResult result = _actionService.ApplyDamageToTarget(
			targetId,
			3,
			Vector2.Zero,
			out bool wasDestroyed,
			out failureReason,
			allowKillKnockback: true);
		if (!string.IsNullOrWhiteSpace(failureReason))
		{
			return false;
		}

		int damageAmount = SumImpactAmount(result, CombatImpactType.HealthDamage, CombatImpactType.ShieldDamage);
		if (damageAmount > 0)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{ResolveObjectDisplayName(targetId)} \u70B9\u540D{damageAmount}");
		}

		killed = wasDestroyed;
		return true;
	}

	private void ConsumePlayerWeaponAttackCharge(string attackerId)
	{
		if (StateManager == null || !StateManager.ConsumeTemporaryWeaponAttackCharge(out bool expired, out int remainingCharges))
		{
			return;
		}

		if (GlobalSession == null)
		{
			return;
		}

		string weaponItemId = expired ? DrawnRevolverWeaponItemId : StateManager.GetActivePlayerWeaponItemId();
		EquipmentDefinition? weaponDefinition = GlobalSession.FindEquipmentDefinition(weaponItemId);
		string weaponName = string.IsNullOrWhiteSpace(weaponDefinition?.DisplayName) ? "Revolver" : weaponDefinition.DisplayName;

		if (expired)
		{
			AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{weaponName} \u5F39\u4ED3\u8017\u5C3D");
			return;
		}

		AppendBattleActionLog($"{ResolveObjectDisplayName(attackerId)}->{weaponName} \u5269\u4F59{remainingCharges}\u51FB");
	}

	private List<Vector2I> BuildCardTargetCells(string sourceObjectId, BattleCardDefinition cardDefinition)
	{
		if (Registry == null || !Registry.TryGet(sourceObjectId, out BoardObject? sourceObject) || sourceObject == null)
		{
			return new List<Vector2I>();
		}

		return cardDefinition.TargetingMode switch
		{
			BattleCardTargetingMode.EnemyUnit => BuildAttackTargetCells(sourceObjectId, sourceObject.Cell, cardDefinition.Range),
			BattleCardTargetingMode.StraightLineEnemy => BuildStraightLineTargetCells(sourceObject.Cell, cardDefinition.Range),
			BattleCardTargetingMode.FriendlyUnit => BuildFriendlyTargetCells(sourceObject.Cell, cardDefinition.Range),
			BattleCardTargetingMode.Cell => BuildCellTargetCells(sourceObject.Cell, cardDefinition.Range),
			_ => new List<Vector2I>(),
		};
	}

	private List<Vector2I> BuildCellTargetCells(Vector2I origin, int range)
	{
		List<Vector2I> cells = new();
		if (CurrentRoom == null)
		{
			return cells;
		}

		for (int y = origin.Y - range; y <= origin.Y + range; y++)
		{
			for (int x = origin.X - range; x <= origin.X + range; x++)
			{
				Vector2I cell = new(x, y);
				if (!CurrentRoom.Topology.IsInsideBoard(cell))
				{
					continue;
				}

				if (GetManhattanDistance(origin, cell) <= range)
				{
					cells.Add(cell);
				}
			}
		}

		return cells;
	}

	private List<Vector2I> BuildFriendlyTargetCells(Vector2I origin, int range)
	{
		List<Vector2I> cells = new();
		if (Registry == null || CurrentRoom == null)
		{
			return cells;
		}

		foreach (BoardObject boardObject in Registry.AllObjects)
		{
			if (boardObject.ObjectType != BoardObjectType.Unit || boardObject.Faction != BoardObjectFaction.Player)
			{
				continue;
			}

			int distance = Mathf.Abs(boardObject.Cell.X - origin.X) + Mathf.Abs(boardObject.Cell.Y - origin.Y);
			if (distance <= range)
			{
				cells.Add(boardObject.Cell);
			}
		}

		return cells;
	}

	private List<Vector2I> BuildStraightLineTargetCells(Vector2I origin, int range)
	{
		List<Vector2I> cells = new();
		if (CurrentRoom == null || QueryService == null || range <= 0)
		{
			return cells;
		}

		foreach (Vector2I direction in BoardTopology.CardinalDirections)
		{
			Vector2I currentCell = origin;
			for (int step = 0; step < range; step++)
			{
				currentCell += direction;
				if (!CurrentRoom.Topology.IsInsideBoard(currentCell))
				{
					break;
				}

				cells.Add(currentCell);

				bool shouldStop = false;
				foreach (BoardObject boardObject in QueryService.GetObjectsAtCell(currentCell))
				{
					if (boardObject.ObjectType == BoardObjectType.Unit || boardObject.BlocksLineOfSight)
					{
						shouldStop = true;
						break;
					}
				}

				if (shouldStop)
				{
					break;
				}
			}
		}

		return cells.Distinct().ToList();
	}

	private bool TryResolveCardTarget(
		string attackerId,
		string? targetId,
		Vector2I? targetCell,
		BattleCardDefinition cardDefinition,
		out BoardObject? targetObject,
		out string failureReason)
	{
		targetObject = null;
		failureReason = string.Empty;

		if (!cardDefinition.RequiresTarget)
		{
			return true;
		}

		if (Registry == null || !Registry.TryGet(attackerId, out BoardObject? attacker) || attacker == null)
		{
			failureReason = $"Attacker {attackerId} was not found.";
			return false;
		}

		if (cardDefinition.TargetingMode == BattleCardTargetingMode.Cell)
		{
			if (targetCell == null || CurrentRoom == null || !CurrentRoom.Topology.IsInsideBoard(targetCell.Value))
			{
				failureReason = "Target cell is invalid.";
				return false;
			}

			if (GetManhattanDistance(attacker.Cell, targetCell.Value) > cardDefinition.Range)
			{
				failureReason = $"Target cell is out of range. Range={cardDefinition.Range}.";
				return false;
			}

			return true;
		}

		if (string.IsNullOrWhiteSpace(targetId) || !Registry.TryGet(targetId, out targetObject) || targetObject == null)
		{
			failureReason = "Card target was not found.";
			return false;
		}

		switch (cardDefinition.TargetingMode)
		{
			case BattleCardTargetingMode.EnemyUnit:
				if (!BattleActionService.IsAttackable(attacker, targetObject))
				{
					failureReason = "This target cannot be targeted by an enemy card.";
					return false;
				}

				if (GetManhattanTarget(attacker, targetObject, cardDefinition.Range) == null)
				{
					failureReason = $"Target is out of range. Range={cardDefinition.Range}.";
					return false;
				}

				return true;

			case BattleCardTargetingMode.StraightLineEnemy:
				if (!BattleActionService.IsAttackable(attacker, targetObject))
				{
					failureReason = "This target cannot be targeted by a straight-line enemy card.";
					return false;
				}

				if (GetStraightLineTarget(attackerId, targetObject, cardDefinition.Range) == null)
				{
					failureReason = $"Target is not in a valid straight line. Range={cardDefinition.Range}.";
					return false;
				}

				return true;

			case BattleCardTargetingMode.FriendlyUnit:
				if (targetObject.ObjectType != BoardObjectType.Unit || attacker.Faction != targetObject.Faction)
				{
					failureReason = "Friendly unit target is invalid.";
					return false;
				}

				if (GetManhattanTarget(attacker, targetObject, cardDefinition.Range) == null)
				{
					failureReason = $"Friendly target is out of range. Range={cardDefinition.Range}.";
					return false;
				}

				return true;

			default:
				failureReason = "Unsupported card targeting mode.";
				return false;
		}
	}

	private static BoardObject? GetManhattanTarget(BoardObject attacker, BoardObject target, int range)
	{
		int distance = GetManhattanDistance(attacker.Cell, target.Cell);
		return distance <= range ? target : null;
	}

	private static int GetManhattanDistance(Vector2I a, Vector2I b)
	{
		return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
	}

	private BattleCardDefinition? GetSelectedCardDefinition()
	{
		if (_playerDeck == null || TurnState == null)
		{
			return null;
		}

		return _playerDeck.TryGetHandCard(TurnState.SelectedCardInstanceId, out BattleCardInstance? selectedCard) && selectedCard != null
			? selectedCard.Definition
			: null;
	}

	private BoardObject? GetStraightLineTarget(string attackerId, BoardObject target, int range)
	{
		if (TargetingService == null)
		{
			return null;
		}

		IReadOnlyDictionary<Vector2I, BoardObject> lineTargets = TargetingService.FindEnemiesInStraightLines(attackerId, range);
		foreach (BoardObject candidate in lineTargets.Values)
		{
			if (candidate.ObjectId == target.ObjectId)
			{
				return candidate;
			}
		}

		return null;
	}

	private List<Vector2I> BuildPreviewPath(string objectId, Vector2I start, Vector2I end, int moveRange)
	{
		if (Pathfinder == null)
		{
			return new List<Vector2I>();
		}

		IReadOnlyList<Vector2I> path;
		int totalCost;
		if (Pathfinder.TryFindPath(objectId, start, end, moveRange, out path, out totalCost))
		{
			return path.ToList();
		}

		return new List<Vector2I>();
	}

	private static IReadOnlyList<BattleCardDefinition> BuildPrototypePlayerDeck()
	{
		return new[]
		{
			new BattleCardDefinition(
				"debug_finisher",
				"\u5904\u51B3",
				"\u9020\u6210 99 \u4F24\u5BB3",
				1,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.EnemyUnit,
				range: 1,
				damage: 99,
				exhaustsOnPlay: true),
			new BattleCardDefinition(
				DrawRevolverCardId,
				"\u62D4\u67AA",
				"\u672C\u573A\u6218\u6597\u4E34\u65F6\u5207\u6362\u4E3A\u5DE6\u8F6E\uff1A\u666E\u901A\u653B\u51FB\u5C04\u7A0B\u6539\u4E3A 2\uff0C\u4F24\u5BB3\u6539\u4E3A 4\uff0C\u53EF\u653B\u51FB 6 \u6B21",
				2,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None),
			new BattleCardDefinition(
				ArcLeakCardId,
				"\u7535\u5F27\u6CC4\u9732",
				"\u5BF9 3 \u683C\u5185\u76EE\u6807\u683C\u53CA\u5176\u76F8\u90BB\u683C\u751F\u6210\u7535\u5F27\u5730\u5F62",
				1,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.Cell,
				range: 3),
			new BattleCardDefinition(
				RamCardId,
				"\u51B2\u649E",
				"\u76F4\u7EBF 2 \u683C\u5185\u51B2\u5230\u76EE\u6807\u9762\u524D\uff0c\u9020\u6210 3 \u4F24\u5BB3\uff1B\u82E5\u76EE\u6807\u88AB\u963B\u6321\uff0C\u518D\u8FFD\u52A0 4 \u70B9\u649E\u51FB\u4F24\u5BB3",
				1,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.StraightLineEnemy,
				range: 2),
			new BattleCardDefinition(
				"cross_slash",
				"\u4EA4\u65A9",
				"\u9020\u6210 3 \u4F24\u5BB3",
				1,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.EnemyUnit,
				range: 1,
				damage: 3),
			new BattleCardDefinition(
				"quick_cut",
				"\u75BE\u65A9",
				"\u9020\u6210 2 \u4F24\u5BB3\uff0c\u5FEB\u901F",
				0,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.EnemyUnit,
				range: 1,
				damage: 2,
				isQuick: true),
			new BattleCardDefinition(
				"line_shot",
				"\u8D2F\u5C04",
				"\u5BF9\u76F4\u7EBF\u654C\u4EBA\u9020\u6210 2 \u4F24\u5BB3",
				1,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.StraightLineEnemy,
				range: 4,
				damage: 2),
			new BattleCardDefinition(
				"heavy_shot",
				"\u91CD\u94F3",
				"\u5BF9\u76F4\u7EBF\u654C\u4EBA\u9020\u6210 5 \u4F24\u5BB3\uff0C\u6D88\u8017",
				2,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.StraightLineEnemy,
				range: 5,
				damage: 5,
				exhaustsOnPlay: true),
			new BattleCardDefinition(
				"battle_read",
				"\u6574\u5907",
				"\u62BD 2 \u5F20\u724C",
				1,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				drawCount: 2),
			new BattleCardDefinition(
				"meditate",
				"\u8C03\u606F",
				"\u62BD 1 \u5F20\u724C\uff0c\u56DE 1 \u80FD\u91CF\uff0c\u5FEB\u901F",
				0,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				drawCount: 1,
				energyGain: 1,
				isQuick: true),
			new BattleCardDefinition(
				"surge",
				"\u84C4\u80FD",
				"\u56DE 2 \u80FD\u91CF",
				1,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				energyGain: 2,
				exhaustsOnPlay: true),
			new BattleCardDefinition(
				"draw_spark",
				"\u7075\u611F",
				"\u62BD 1 \u5F20\u724C\uff0C\u56DE 1 \u80FD\u91CF",
				1,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				drawCount: 1,
				energyGain: 1),
			new BattleCardDefinition(
				"quick_plan",
				"\u5FEB\u8C0B",
				"\u62BD 2 \u5F20\u724C\uff0C\u5FEB\u901F",
				0,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				drawCount: 2,
				isQuick: true),
			new BattleCardDefinition(
				"burning_edge",
				"\u71C3\u5203",
				"\u9020\u6210 4 \u4F24\u5BB3",
				1,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.EnemyUnit,
				range: 1,
				damage: 4,
				exhaustsOnPlay: true),
			new BattleCardDefinition(
				"hook_shot",
				"\u94A9\u5C04",
				"\u5BF9\u76F4\u7EBF\u654C\u4EBA\u9020\u6210 3 \u4F24\u5BB3",
				1,
				BattleCardCategory.Attack,
				BattleCardTargetingMode.StraightLineEnemy,
				range: 4,
				damage: 3),
			new BattleCardDefinition(
				"deep_focus",
				"\u6C89\u5FF5",
				"\u62BD 3 \u5F20\u724C",
				2,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				drawCount: 3),
			new BattleCardDefinition(
				"spark_charge",
				"\u706B\u82B1",
				"\u56DE 1 \u80FD\u91CF\u5E76\u62BD 1 \u5F20\u724C",
				0,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				drawCount: 1,
				energyGain: 1,
				isQuick: true),
			new BattleCardDefinition(
				"burst_drive",
				"\u7206\u9A71",
				"\u56DE 2 \u80FD\u91CF",
				0,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				energyGain: 2,
				exhaustsOnPlay: true),
			new BattleCardDefinition(
				"guard_up",
				"\u4E3E\u76FE",
				"\u83B7\u5F97 3 \u62A4\u76FE",
				1,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				shieldGain: 3),
			new BattleCardDefinition(
				"brace",
				"\u67B6\u52BF",
				"\u83B7\u5F97 5 \u62A4\u76FE",
				2,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				shieldGain: 5),
			new BattleCardDefinition(
				"quick_guard",
				"\u77AC\u5B88",
				"\u83B7\u5F97 2 \u62A4\u76FE\uff0c\u5FEB\u901F",
				0,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.None,
				shieldGain: 2,
				isQuick: true),
			new BattleCardDefinition(
				"field_patch",
				"\u73B0\u573A\u5305\u624E",
				"\u5BF9 2 \u683C\u5185\u53CB\u65B9\u6062\u590D 3 \u751F\u547D",
				1,
				BattleCardCategory.Skill,
				BattleCardTargetingMode.FriendlyUnit,
				range: 2,
				healingAmount: 3),
		};
	}
}
