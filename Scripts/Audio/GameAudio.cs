using System;
using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Audio;

public partial class GameAudio : Node
{
	public const string MasterBusName = "Master";
	public const string MusicBusName = "Music";
	public const string SfxBusName = "Sfx";
	public const string UiBusName = "Ui";

	public const string MapMusicCueId = "music_map";
	public const string BattleMusicCueId = "music_battle";
	public const string BasicAttackCueId = "sfx_basic_attack";
	public const string CardSelectCueId = "sfx_card_select";
	public const string CardPlayCueId = "sfx_card_play";
	public const string EnemyHitCueId = "sfx_enemy_hit";
	public const string UnitDeathCueId = "sfx_unit_death";
	public const string UiConfirmCueId = "sfx_ui_confirm";
	public const string UiCancelCueId = "sfx_ui_cancel";
	public const string UiToggleOnCueId = "sfx_ui_toggle_on";
	public const string UiToggleOffCueId = "sfx_ui_toggle_off";
	public const string DialoguePopupCueId = "sfx_dialogue_popup";
	private const string SettingsConfigPath = "user://audio_settings.cfg";

	private const float SilentDb = -80.0f;
	private const int InitialSfxPlayerCount = 8;
	private const int InitialUiPlayerCount = 4;
	private const int MaxDynamicPlayerCount = 20;
	private const float DefaultImpactCutoffHz = 720.0f;
	private const float DefaultImpactResonance = 0.8f;
	private const float DefaultImpactBassBoostDb = 6.0f;

	private static readonly IReadOnlyDictionary<string, string[]> CueCandidatePaths =
		new Dictionary<string, string[]>(StringComparer.Ordinal)
		{
			[MapMusicCueId] = new[] { "res://Assets/Audio/BGM/map.mp3" },
			[BattleMusicCueId] = new[] { "res://Assets/Audio/BGM/battle.mp3" },
			[BasicAttackCueId] = new[] { "res://Assets/Audio/SFX/attack_normal.wav" },
			[CardSelectCueId] = new[] { "res://Assets/Audio/SFX/card_selected.wav" },
			[CardPlayCueId] = new[] { "res://Assets/Audio/SFX/card_played.wav" },
			[EnemyHitCueId] = new[] { "res://Assets/Audio/SFX/attack_metal.wav" },
			[UnitDeathCueId] = new[] { "res://Assets/Audio/SFX/something_died.wav" },
			[UiConfirmCueId] = new[] { "res://Assets/Audio/SFX/UI_clicked.wav" },
			[UiCancelCueId] = new[] { "res://Assets/Audio/SFX/UI_clicked.wav" },
			[UiToggleOnCueId] = new[] { "res://Assets/Audio/SFX/ui_on.wav" },
			[UiToggleOffCueId] = new[] { "res://Assets/Audio/SFX/ui_off.wav" },
			[DialoguePopupCueId] = new[] { "res://Assets/Audio/SFX/text_rolling.wav" },
		};

	public static GameAudio? Instance { get; private set; }

	private readonly Dictionary<string, AudioStream?> _streamCache = new(StringComparer.Ordinal);
	private readonly HashSet<string> _missingCueWarnings = new(StringComparer.Ordinal);
	private readonly Dictionary<string, ulong> _cueLastPlayMs = new(StringComparer.Ordinal);
	private readonly List<AudioStreamPlayer> _sfxPlayers = new();
	private readonly List<AudioStreamPlayer> _uiPlayers = new();

	private AudioStreamPlayer _musicPlayerA = null!;
	private AudioStreamPlayer _musicPlayerB = null!;
	private AudioStreamPlayer _activeMusicPlayer = null!;
	private Tween? _musicTween;
	private Tween? _impactTween;
	private AudioEffectLowPassFilter? _masterLowPass;
	private AudioEffectEQ6? _masterEq;
	private float _impactCutoffHz = 20500.0f;
	private float _impactBassBoostDb;
	private string _activeMusicCueId = string.Empty;
	private float _masterVolumeLinear = 1.0f;
	private float _musicVolumeLinear = 1.0f;
	private float _sfxVolumeLinear = 1.0f;
	private bool _masterMuted;
	private bool _musicMuted;
	private bool _sfxMuted;

	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			QueueFree();
			return;
		}

		Instance = this;
		ProcessMode = ProcessModeEnum.Always;

		EnsureAudioBuses();
		EnsureMusicPlayers();
		EnsureSfxPools();
		EnsureMasterImpactEffects();
		LoadSettings();
		ApplyVolumeSettings(saveAfterApply: false);
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void PlayMapMusic(float fadeSeconds = 0.45f)
	{
		PlayMusic(MapMusicCueId, fadeSeconds);
	}

	public void PlayBattleMusic(float fadeSeconds = 0.35f)
	{
		PlayMusic(BattleMusicCueId, fadeSeconds);
	}

	public void PlayMusic(string cueId, float fadeSeconds = 0.4f)
	{
		if (string.IsNullOrWhiteSpace(cueId))
		{
			return;
		}

		if (string.Equals(_activeMusicCueId, cueId, StringComparison.Ordinal) && _activeMusicPlayer.Playing)
		{
			return;
		}

		AudioStream? stream = ResolveStream(cueId);
		if (stream == null)
		{
			StopMusic(fadeSeconds);
			return;
		}

		ConfigureMusicLoop(stream);

		AudioStreamPlayer nextPlayer = ReferenceEquals(_activeMusicPlayer, _musicPlayerA) ? _musicPlayerB : _musicPlayerA;
		AudioStreamPlayer previousPlayer = _activeMusicPlayer;
		_musicTween?.Kill();

		nextPlayer.Stream = stream;
		nextPlayer.VolumeDb = SilentDb;
		nextPlayer.Play();

		_musicTween = CreateTween();
		_musicTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_musicTween.SetParallel();
		_musicTween.SetEase(Tween.EaseType.Out);
		_musicTween.SetTrans(Tween.TransitionType.Cubic);
		_musicTween.TweenProperty(nextPlayer, "volume_db", 0.0f, Math.Max(fadeSeconds, 0.01f));
		if (previousPlayer.Playing)
		{
			_musicTween.TweenProperty(previousPlayer, "volume_db", SilentDb, Math.Max(fadeSeconds, 0.01f));
		}

		_activeMusicPlayer = nextPlayer;
		_activeMusicCueId = cueId;
		_ = FinalizeMusicSwapAsync(previousPlayer, Math.Max(fadeSeconds, 0.01f));
	}

	public void StopMusic(float fadeSeconds = 0.25f)
	{
		_activeMusicCueId = string.Empty;
		_musicTween?.Kill();

		if (!_musicPlayerA.Playing && !_musicPlayerB.Playing)
		{
			return;
		}

		float duration = Math.Max(fadeSeconds, 0.01f);
		_musicTween = CreateTween();
		_musicTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_musicTween.SetParallel();
		_musicTween.SetEase(Tween.EaseType.Out);
		_musicTween.SetTrans(Tween.TransitionType.Cubic);
		if (_musicPlayerA.Playing)
		{
			_musicTween.TweenProperty(_musicPlayerA, "volume_db", SilentDb, duration);
		}

		if (_musicPlayerB.Playing)
		{
			_musicTween.TweenProperty(_musicPlayerB, "volume_db", SilentDb, duration);
		}

		_ = StopMusicPlayersAsync(duration);
	}

	public void PlayBasicAttack() => PlaySfx(BasicAttackCueId, SfxBusName);
	public void PlayCardSelect() => PlaySfx(CardSelectCueId, UiBusName, minimumIntervalSeconds: 0.05f);
	public void PlayCardUse() => PlaySfx(CardPlayCueId, SfxBusName);
	public void PlayEnemyHit() => PlaySfx(EnemyHitCueId, SfxBusName, minimumIntervalSeconds: 0.04f);
	public void PlayUnitDeath() => PlaySfx(UnitDeathCueId, SfxBusName, minimumIntervalSeconds: 0.05f);
	public void PlayUiConfirm() => PlaySfx(UiConfirmCueId, UiBusName, minimumIntervalSeconds: 0.03f);
	public void PlayUiCancel() => PlaySfx(UiCancelCueId, UiBusName, minimumIntervalSeconds: 0.03f);
	public void PlayUiToggleOn() => PlaySfx(UiToggleOnCueId, MasterBusName, minimumIntervalSeconds: 0.03f);
	public void PlayUiToggleOff() => PlaySfx(UiToggleOffCueId, MasterBusName, minimumIntervalSeconds: 0.03f);
	public void PlayDialoguePopup() => PlaySfx(DialoguePopupCueId, UiBusName, minimumIntervalSeconds: 0.04f);

	public void TriggerDamageImpact(float durationSeconds = 0.42f, float cutoffHz = DefaultImpactCutoffHz, float bassBoostDb = DefaultImpactBassBoostDb)
	{
		if (_masterLowPass == null || _masterEq == null)
		{
			return;
		}

		_impactTween?.Kill();
		SetImpactCutoffHz(20500.0f);
		SetImpactBassBoostDb(0.0f);
		_masterLowPass.Resonance = DefaultImpactResonance;

		float clampedDuration = Mathf.Max(durationSeconds, 0.05f);
		float downDuration = clampedDuration * 0.18f;
		float holdDuration = clampedDuration * 0.32f;
		float upDuration = clampedDuration * 0.50f;

		_impactTween = CreateTween();
		_impactTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_impactTween.SetEase(Tween.EaseType.Out);
		_impactTween.SetTrans(Tween.TransitionType.Cubic);
		_impactTween.TweenMethod(Callable.From<float>(SetImpactCutoffHz), 20500.0f, cutoffHz, downDuration);
		_impactTween.TweenMethod(Callable.From<float>(SetImpactBassBoostDb), 0.0f, bassBoostDb, downDuration);
		_impactTween.TweenInterval(holdDuration);
		_impactTween.TweenMethod(Callable.From<float>(SetImpactCutoffHz), cutoffHz, 20500.0f, upDuration);
		_impactTween.TweenMethod(Callable.From<float>(SetImpactBassBoostDb), bassBoostDb, 0.0f, upDuration);
	}

	public void SetMasterVolumeLinear(float linear)
	{
		_masterVolumeLinear = Mathf.Clamp(linear, 0.0f, 1.0f);
		ApplyVolumeSettings();
	}

	public void SetMusicVolumeLinear(float linear)
	{
		_musicVolumeLinear = Mathf.Clamp(linear, 0.0f, 1.0f);
		ApplyVolumeSettings();
	}

	public void SetSfxVolumeLinear(float linear)
	{
		_sfxVolumeLinear = Mathf.Clamp(linear, 0.0f, 1.0f);
		ApplyVolumeSettings();
	}

	public float GetMasterVolumeLinear() => _masterVolumeLinear;
	public float GetMusicVolumeLinear() => _musicVolumeLinear;
	public float GetSfxVolumeLinear() => _sfxVolumeLinear;
	public float GetUiVolumeLinear() => _sfxVolumeLinear;

	public bool IsMasterMuted() => _masterMuted;
	public bool IsMusicMuted() => _musicMuted;
	public bool IsSfxMuted() => _sfxMuted;

	public void SetMasterMuted(bool muted)
	{
		_masterMuted = muted;
		ApplyVolumeSettings();
	}

	public void SetMusicMuted(bool muted)
	{
		_musicMuted = muted;
		ApplyVolumeSettings();
	}

	public void SetSfxMuted(bool muted)
	{
		_sfxMuted = muted;
		ApplyVolumeSettings();
	}

	private void EnsureAudioBuses()
	{
		EnsureBus(MusicBusName, MasterBusName);
		EnsureBus(SfxBusName, MasterBusName);
		EnsureBus(UiBusName, MasterBusName);
	}

	private static void EnsureBus(string busName, string sendBusName)
	{
		int busIndex = AudioServer.GetBusIndex(new StringName(busName));
		if (busIndex < 0)
		{
			AudioServer.AddBus(AudioServer.BusCount);
			busIndex = AudioServer.BusCount - 1;
			AudioServer.SetBusName(busIndex, busName);
		}

		AudioServer.SetBusSend(busIndex, new StringName(sendBusName));
	}

	private void EnsureMusicPlayers()
	{
		_musicPlayerA = CreatePlayer("MusicPlayerA", MusicBusName);
		_musicPlayerB = CreatePlayer("MusicPlayerB", MusicBusName);
		_musicPlayerA.VolumeDb = SilentDb;
		_musicPlayerB.VolumeDb = SilentDb;
		_activeMusicPlayer = _musicPlayerA;
	}

	private void EnsureSfxPools()
	{
		for (int index = 0; index < InitialSfxPlayerCount; index++)
		{
			_sfxPlayers.Add(CreatePlayer($"SfxPlayer{index}", SfxBusName));
		}

		for (int index = 0; index < InitialUiPlayerCount; index++)
		{
			_uiPlayers.Add(CreatePlayer($"UiPlayer{index}", UiBusName));
		}
	}

	private void EnsureMasterImpactEffects()
	{
		int masterBusIndex = AudioServer.GetBusIndex(new StringName(MasterBusName));
		if (masterBusIndex < 0)
		{
			return;
		}

		_masterLowPass = FindBusEffect<AudioEffectLowPassFilter>(masterBusIndex);
		if (_masterLowPass == null)
		{
			_masterLowPass = new AudioEffectLowPassFilter
			{
				CutoffHz = 20500.0f,
				Resonance = DefaultImpactResonance,
			};
			AudioServer.AddBusEffect(masterBusIndex, _masterLowPass, AudioServer.GetBusEffectCount(masterBusIndex));
		}

		_masterEq = FindBusEffect<AudioEffectEQ6>(masterBusIndex);
		if (_masterEq == null)
		{
			_masterEq = new AudioEffectEQ6();
			AudioServer.AddBusEffect(masterBusIndex, _masterEq, AudioServer.GetBusEffectCount(masterBusIndex));
		}

		SetImpactCutoffHz(20500.0f);
		SetImpactBassBoostDb(0.0f);
	}

	private static T? FindBusEffect<T>(int busIndex) where T : AudioEffect
	{
		int effectCount = AudioServer.GetBusEffectCount(busIndex);
		for (int index = 0; index < effectCount; index++)
		{
			if (AudioServer.GetBusEffect(busIndex, index) is T effect)
			{
				return effect;
			}
		}

		return null;
	}

	private AudioStreamPlayer CreatePlayer(string name, string busName)
	{
		AudioStreamPlayer player = new()
		{
			Name = name,
			Bus = busName,
			ProcessMode = ProcessModeEnum.Always,
		};
		AddChild(player);
		return player;
	}

	private async System.Threading.Tasks.Task FinalizeMusicSwapAsync(AudioStreamPlayer previousPlayer, float waitSeconds)
	{
		await ToSignal(GetTree().CreateTimer(waitSeconds, false, false, true), SceneTreeTimer.SignalName.Timeout);
		if (previousPlayer != _activeMusicPlayer)
		{
			previousPlayer.Stop();
			previousPlayer.VolumeDb = SilentDb;
		}

		_activeMusicPlayer.VolumeDb = 0.0f;
	}

	private async System.Threading.Tasks.Task StopMusicPlayersAsync(float waitSeconds)
	{
		await ToSignal(GetTree().CreateTimer(waitSeconds, false, false, true), SceneTreeTimer.SignalName.Timeout);
		_musicPlayerA.Stop();
		_musicPlayerB.Stop();
		_musicPlayerA.VolumeDb = SilentDb;
		_musicPlayerB.VolumeDb = SilentDb;
	}

	private void PlaySfx(string cueId, string busName, float volumeDb = 0.0f, float pitchScale = 1.0f, float minimumIntervalSeconds = 0.0f)
	{
		AudioStream? stream = ResolveStream(cueId);
		if (stream == null || !PassesCooldown(cueId, minimumIntervalSeconds))
		{
			return;
		}

		List<AudioStreamPlayer> pool = string.Equals(busName, UiBusName, StringComparison.Ordinal)
			? _uiPlayers
			: _sfxPlayers;
		AudioStreamPlayer player = AcquireFreePlayer(pool, busName);
		player.Bus = busName;
		player.Stream = stream;
		player.VolumeDb = volumeDb;
		player.PitchScale = pitchScale;
		player.Play();
	}

	private AudioStreamPlayer AcquireFreePlayer(List<AudioStreamPlayer> pool, string busName)
	{
		foreach (AudioStreamPlayer player in pool)
		{
			if (!player.Playing)
			{
				return player;
			}
		}

		if (pool.Count < MaxDynamicPlayerCount)
		{
			AudioStreamPlayer player = CreatePlayer($"{busName}Player{pool.Count}", busName);
			pool.Add(player);
			return player;
		}

		return pool[0];
	}

	private bool PassesCooldown(string cueId, float minimumIntervalSeconds)
	{
		if (minimumIntervalSeconds <= 0.0f)
		{
			return true;
		}

		ulong nowMs = Time.GetTicksMsec();
		if (_cueLastPlayMs.TryGetValue(cueId, out ulong lastMs))
		{
			float elapsedSeconds = (nowMs - lastMs) / 1000.0f;
			if (elapsedSeconds < minimumIntervalSeconds)
			{
				return false;
			}
		}

		_cueLastPlayMs[cueId] = nowMs;
		return true;
	}

	private AudioStream? ResolveStream(string cueId)
	{
		if (_streamCache.TryGetValue(cueId, out AudioStream? cached))
		{
			return cached;
		}

		if (!CueCandidatePaths.TryGetValue(cueId, out string[]? candidates))
		{
			_streamCache[cueId] = null;
			return null;
		}

		foreach (string candidate in candidates)
		{
			if (!FileAccess.FileExists(candidate))
			{
				continue;
			}

			AudioStream? stream = ResourceLoader.Load<AudioStream>(candidate);
			if (stream != null)
			{
				_streamCache[cueId] = stream;
				return stream;
			}
		}

		if (_missingCueWarnings.Add(cueId))
		{
			GD.PushWarning($"GameAudio: no audio asset found for cue '{cueId}'.");
		}

		_streamCache[cueId] = null;
		return null;
	}

	private static void ConfigureMusicLoop(AudioStream stream)
	{
		switch (stream)
		{
			case AudioStreamOggVorbis ogg:
				ogg.Loop = true;
				break;
			case AudioStreamMP3 mp3:
				mp3.Loop = true;
				break;
			case AudioStreamWav wav:
				wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
				break;
		}
	}

	private void SetImpactCutoffHz(float value)
	{
		_impactCutoffHz = value;
		if (_masterLowPass != null)
		{
			_masterLowPass.CutoffHz = _impactCutoffHz;
		}
	}

	private void SetImpactBassBoostDb(float value)
	{
		_impactBassBoostDb = value;
		if (_masterEq == null)
		{
			return;
		}

		_masterEq.SetBandGainDb(0, _impactBassBoostDb * 0.7f);
		_masterEq.SetBandGainDb(1, _impactBassBoostDb);
		_masterEq.SetBandGainDb(2, _impactBassBoostDb * 0.45f);
		_masterEq.SetBandGainDb(3, 0.0f);
		_masterEq.SetBandGainDb(4, 0.0f);
		_masterEq.SetBandGainDb(5, 0.0f);
	}

	private void ApplyVolumeSettings(bool saveAfterApply = true)
	{
		SetBusVolumeLinear(MasterBusName, _masterMuted ? 0.0f : _masterVolumeLinear);
		SetBusVolumeLinear(MusicBusName, _musicMuted ? 0.0f : _musicVolumeLinear);
		float sfxLinear = _sfxMuted ? 0.0f : _sfxVolumeLinear;
		SetBusVolumeLinear(SfxBusName, sfxLinear);
		SetBusVolumeLinear(UiBusName, sfxLinear);
		if (saveAfterApply)
		{
			SaveSettings();
		}
	}

	private static void SetBusVolumeLinear(string busName, float linear)
	{
		int busIndex = AudioServer.GetBusIndex(new StringName(busName));
		if (busIndex < 0)
		{
			return;
		}

		float clampedLinear = Mathf.Clamp(linear, 0.0f, 1.0f);
		AudioServer.SetBusVolumeDb(busIndex, clampedLinear <= 0.0001f ? SilentDb : Mathf.LinearToDb(clampedLinear));
	}

	private void LoadSettings()
	{
		ConfigFile config = new();
		Error error = config.Load(SettingsConfigPath);
		if (error != Error.Ok)
		{
			return;
		}

		_masterVolumeLinear = Mathf.Clamp(config.GetValue("audio", "master_volume", 1.0f).AsSingle(), 0.0f, 1.0f);
		_musicVolumeLinear = Mathf.Clamp(config.GetValue("audio", "music_volume", 1.0f).AsSingle(), 0.0f, 1.0f);
		_sfxVolumeLinear = Mathf.Clamp(config.GetValue("audio", "sfx_volume", 1.0f).AsSingle(), 0.0f, 1.0f);
		_masterMuted = config.GetValue("audio", "master_muted", false).AsBool();
		_musicMuted = config.GetValue("audio", "music_muted", false).AsBool();
		_sfxMuted = config.GetValue("audio", "sfx_muted", false).AsBool();
	}

	private void SaveSettings()
	{
		ConfigFile config = new();
		config.SetValue("audio", "master_volume", _masterVolumeLinear);
		config.SetValue("audio", "music_volume", _musicVolumeLinear);
		config.SetValue("audio", "sfx_volume", _sfxVolumeLinear);
		config.SetValue("audio", "master_muted", _masterMuted);
		config.SetValue("audio", "music_muted", _musicMuted);
		config.SetValue("audio", "sfx_muted", _sfxMuted);
		config.Save(SettingsConfigPath);
	}
}
