using Blish_HUD;
using Blish_HUD.ArcDps;
using Gw2Sharp.Models;
using Nekres.Music_Mixer.Core.Player;
using Stateless;
using System;
using static Blish_HUD.GameService;
namespace Nekres.Music_Mixer.Core.Services
{
    internal class Gw2StateService : IDisposable
    {
        public event EventHandler<ValueEventArgs<TyrianTime>> TyrianTimeChanged;
        public event EventHandler<ValueChangedEventArgs<State>> StateChanged;
        public event EventHandler<ValueEventArgs<bool>> IsSubmergedChanged;
        public event EventHandler<ValueEventArgs<bool>> IsDownedChanged;

        public enum State
        {
            StandBy, // silence
            Ambient,
            Mounted,
            Battle,
            Competitive,
            StoryInstance, // silence
            Submerged,
            Defeated,
            Victory
        }

        private enum Trigger
        {
            StandBy,
            MapChanged,
            InCombat,
            OutOfCombat,
            Mounting,
            UnMounting,
            Submerging,
            Emerging,
            Victory,
            Death
        }

        #region Public Fields

        private TyrianTime _prevTyrianTime = TyrianTime.None;
        public TyrianTime TyrianTime { 
            get => _prevTyrianTime;
            private set {
                if (_prevTyrianTime == value) return;
                _prevTyrianTime = value;
                TyrianTimeChanged?.Invoke(this, new ValueEventArgs<TyrianTime>(value));
            }
        }

        private bool _prevIsSubmerged = Gw2Mumble.PlayerCamera.Position.Z <= 0; // for character pos: < -1.25f;
        public bool IsSubmerged {
            get => _prevIsSubmerged; 
            private set {
                if (_prevIsSubmerged == value) return;
                _prevIsSubmerged = value;
                IsSubmergedChanged?.Invoke(this, new ValueEventArgs<bool>(value));
            }
        }

        private bool _prevIsDowned = false;
        public bool IsDowned {
            get => _prevIsDowned; 
            private set {
                if (_prevIsDowned == value) return;
                _prevIsDowned = value;
                IsDownedChanged?.Invoke(this, new ValueEventArgs<bool>(value));
            }
        }

        public State CurrentState => _stateMachine?.State ?? State.StandBy;

        #endregion

        private StateMachine<State, Trigger> _stateMachine;
        private NTimer _inCombatTimer;
        private NTimer _outOfCombatTimer;
        private NTimer _outOfCombatTimerLong;
        public Gw2StateService() {
            _stateMachine = new StateMachine<State, Trigger>(GameModeStateSelector());
            _inCombatTimer = new NTimer(6500) { AutoReset = false };
            _inCombatTimer.Elapsed += InCombatTimerElapsed;
            _outOfCombatTimer = new NTimer(3250) { AutoReset = false };
            _outOfCombatTimer.Elapsed += OutOfCombatTimerElapsed;
            _outOfCombatTimerLong = new NTimer(20250) { AutoReset = false };
            _outOfCombatTimerLong.Elapsed += OutOfCombatTimerElapsed;
            Initialize();
        }

        private void InCombatTimerElapsed(object sender, EventArgs e)
        {
            _stateMachine.Fire(Trigger.InCombat);
        }

        private void OutOfCombatTimerElapsed(object sender, EventArgs e)
        {
            _stateMachine.Fire(Trigger.OutOfCombat);
        }

        public void Dispose() {
            _inCombatTimer.Elapsed -= InCombatTimerElapsed;
            _inCombatTimer?.Dispose();
            _outOfCombatTimer.Elapsed -= OutOfCombatTimerElapsed;
            _outOfCombatTimer?.Dispose();
            _outOfCombatTimerLong.Elapsed -= OutOfCombatTimerElapsed;
            _outOfCombatTimerLong?.Dispose();
            GameIntegration.Gw2Instance.Gw2Closed -= OnGw2Closed;
            ArcDps.RawCombatEvent -= CombatEventReceived;
            Gw2Mumble.PlayerCharacter.CurrentMountChanged -= OnMountChanged;
            Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            Gw2Mumble.PlayerCharacter.IsInCombatChanged -= OnIsInCombatChanged;
        }

        private void Initialize() {
            _stateMachine.OnUnhandledTrigger((s, t) => {
                switch (t) {
                    case Trigger.Mounting: 
                    case Trigger.UnMounting:
                        if (!MusicMixer.Instance.ToggleMountedPlaylistSetting.Value) return; break;
                    case Trigger.Submerging: 
                    case Trigger.Emerging:
                        return;
                    default: break;
                }
                MusicMixer.Logger.Info($"Trigger '{t}' was fired from state '{s}', but has no valid leaving transitions.");
            });
            _stateMachine.Configure(State.StandBy)
                        .Ignore(Trigger.StandBy)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                        .PermitDynamic(Trigger.OutOfCombat, GameModeStateSelector)
                        .PermitIf(Trigger.Mounting, State.Mounted, () => MusicMixer.Instance.ToggleMountedPlaylistSetting.Value)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.InCombat, State.Battle)
                        .Permit(Trigger.Death, State.Defeated)
                        .Ignore(Trigger.Submerging)
                        .Ignore(Trigger.Emerging);

            _stateMachine.Configure(State.Ambient)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .PermitIf(Trigger.Mounting, State.Mounted, () => MusicMixer.Instance.ToggleMountedPlaylistSetting.Value)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.InCombat, State.Battle)
                        .Permit(Trigger.Death, State.Defeated)
                        .Ignore(Trigger.Emerging)
                        .Ignore(Trigger.OutOfCombat);

            _stateMachine.Configure(State.Mounted)
                        .Ignore(Trigger.Mounting)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.Death, State.Defeated)
                        .PermitDynamic(Trigger.UnMounting, GameModeStateSelector)
                        .Ignore(Trigger.Submerging)
                        .Ignore(Trigger.Emerging)
                        .Ignore(Trigger.OutOfCombat)
                        .Ignore(Trigger.MapChanged);

            _stateMachine.Configure(State.Battle)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.Death, State.Defeated)
                        .PermitDynamic(Trigger.OutOfCombat, GameModeStateSelector)
                        .Ignore(Trigger.Submerging)
                        .Ignore(Trigger.Emerging)
                        .Ignore(Trigger.InCombat);

            _stateMachine.Configure(State.Competitive)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .PermitIf(Trigger.Mounting, State.Mounted, () => MusicMixer.Instance.ToggleMountedPlaylistSetting.Value)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.Death, State.Defeated)
                        .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                        .Ignore(Trigger.Submerging)
                        .Ignore(Trigger.Emerging)
                        .Ignore(Trigger.OutOfCombat);

            _stateMachine.Configure(State.StoryInstance)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.Death, State.Defeated)
                        .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                        .Permit(Trigger.InCombat, State.Battle)
                        .Ignore(Trigger.Emerging)
                        .Ignore(Trigger.OutOfCombat);

            _stateMachine.Configure(State.Victory)
                        .Ignore(Trigger.Victory)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .Permit(Trigger.Death, State.Defeated);

            _stateMachine.Configure(State.Defeated)
                        .Ignore(Trigger.Death)
                        .OnEntry(t =>
                        {
                            IsDowned = false;
                            StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination));
                        })
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .Permit(Trigger.Victory, State.Victory);

            ArcDps.Common.Activate();
            ArcDps.RawCombatEvent += CombatEventReceived;
            Gw2Mumble.PlayerCharacter.CurrentMountChanged += OnMountChanged;
            Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            Gw2Mumble.PlayerCharacter.IsInCombatChanged += OnIsInCombatChanged;
            GameIntegration.Gw2Instance.Gw2Closed += OnGw2Closed;
        }
        private State GameModeStateSelector()
        {
            IsDowned = false;

            if (MusicMixer.Instance.ToggleMountedPlaylistSetting.Value && Gw2Mumble.PlayerCharacter.CurrentMount > 0)
                return State.Mounted;

            if (Gw2Mumble.CurrentMap.IsCompetitiveMode && !Gw2Mumble.CurrentMap.Type.IsWvW() && Gw2Mumble.CurrentMap.Id != 350)
                return State.Competitive;

            if (Gw2Mumble.CurrentMap.Type.IsPublic() || Gw2Mumble.CurrentMap.Type.IsInstance())
                return State.Ambient;

            return State.StandBy;
        }

        public void Update() {
            CheckTyrianTime();
            CheckWaterLevel();
        }

        private void CheckWaterLevel() => IsSubmerged = Gw2Mumble.PlayerCamera.Position.Z <= 0;
        private void CheckTyrianTime() => TyrianTime = TyrianTimeUtil.GetCurrentDayCycle();
        private void OnGw2Closed(object sender, EventArgs e) => _stateMachine.Fire(Trigger.StandBy);

        #region ArcDps Events

        private void CombatEventReceived(object o, RawCombatEventArgs e) {
            if (e.CombatEvent == null || 
                e.CombatEvent.Ev == null || 
                e.EventType == RawCombatEventArgs.CombatEventType.Local) return;

            // Check state changes of local player.
            if (e.CombatEvent.Src.Self > 0) {
                switch (e.CombatEvent.Ev.IsStateChange)
                {
                    case ArcDpsEnums.StateChange.ChangeDown:
                        IsDowned = true;
                        return;
                    case ArcDpsEnums.StateChange.ChangeUp:
                        IsDowned = false;
                        return;
                    case ArcDpsEnums.StateChange.Reward:
                        _stateMachine.Fire(Trigger.Victory);
                        return;
                    default: break;
                }
            }
        }

        #endregion

        #region Mumble Events

        private void OnMountChanged(object o, ValueEventArgs<MountType> e)
        {
            if (_outOfCombatTimer.IsRunning || _outOfCombatTimerLong.IsRunning) return;
            _stateMachine.Fire(e.Value > 0 ? Trigger.Mounting : Trigger.UnMounting);
        }

        private void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            _stateMachine.Fire(Trigger.MapChanged);
            _outOfCombatTimer.Stop();
            _outOfCombatTimerLong.Stop();
            _inCombatTimer.Stop();
        }

        private void OnIsInCombatChanged(object o, ValueEventArgs<bool> e) {
            if (e.Value)
            {
                _inCombatTimer.Restart();
            }
            else if (CurrentState == State.Battle)
            {
                if (Gw2Mumble.CurrentMap.Type.IsInstance() || Gw2Mumble.CurrentMap.Type.IsWvW() || Gw2Mumble.CurrentMap.Type == MapType.PublicMini)
                {
                    _outOfCombatTimerLong.Restart();
                }
                else
                {
                    _outOfCombatTimer.Restart();
                }
            }
            else
            {
                _inCombatTimer.Stop();
            }
        }

        #endregion
    }
}
