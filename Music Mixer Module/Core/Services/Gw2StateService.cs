using Blish_HUD;
using Blish_HUD.ArcDps;
using Gw2Sharp.Models;
using Stateless;
using System;
using static Blish_HUD.GameService;
using static Nekres.Music_Mixer.MusicMixer;
using Timer = System.Timers.Timer;
namespace Nekres.Music_Mixer.Core.Services
{
    public class Gw2StateService : IDisposable
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

        private bool _prevIsSubmerged = Gw2Mumble.PlayerCharacter.Position.Z < -1.25f;
        public bool IsSubmerged {
            get => _prevIsSubmerged; 
            private set {
                if (_prevIsSubmerged == value) return;
                _prevIsSubmerged = value;
                IsSubmergedChanged?.Invoke(this, new ValueEventArgs<bool>(value));
                _stateMachine?.Fire(value ? Trigger.Submerging : Trigger.Emerging);
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
        private Timer _inCombatTimer;
        private Timer _outOfCombatTimer;

        public Gw2StateService() {
            _stateMachine = new StateMachine<State, Trigger>(GameModeStateSelector());
            _inCombatTimer = new Timer(6500) { AutoReset = false };
            _inCombatTimer.Elapsed += InCombatTimerElapsed;
            _outOfCombatTimer = new Timer(3250) { AutoReset = false };
            _outOfCombatTimer.Elapsed += OutOfCombatTimerElapsed;
            this.Initialize();
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
            _inCombatTimer?.Close();
            _outOfCombatTimer.Elapsed -= OutOfCombatTimerElapsed;
            _outOfCombatTimer?.Close();
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
                        if (!Instance.ToggleMountedPlaylistSetting.Value) return; break;
                    case Trigger.Submerging: 
                    case Trigger.Emerging:
                        if (!Instance.ToggleSubmergedPlaylistSetting.Value) return; break;
                    default: break;
                }
                MusicMixer.Logger.Info($"Trigger '{t}' was fired from state '{s}', but has no valid leaving transitions.");
            });
            _stateMachine.Configure(State.StandBy)
                        .Ignore(Trigger.StandBy)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                        .PermitDynamic(Trigger.OutOfCombat, GameModeStateSelector)
                        .PermitIf(Trigger.Submerging, State.Submerged, () => Instance.ToggleSubmergedPlaylistSetting.Value)
                        .PermitIf(Trigger.Mounting, State.Mounted, () => Instance.ToggleMountedPlaylistSetting.Value)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.InCombat, State.Battle)
                        .Permit(Trigger.Death, State.Defeated)
                        .Ignore(Trigger.Submerging)
                        .Ignore(Trigger.Emerging);

            _stateMachine.Configure(State.Ambient)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .PermitIf(Trigger.Submerging, State.Submerged, () => Instance.ToggleSubmergedPlaylistSetting.Value)
                        .PermitIf(Trigger.Mounting, State.Mounted, () => Instance.ToggleMountedPlaylistSetting.Value)
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
                        .PermitIf(Trigger.Mounting, State.Mounted, () => Instance.ToggleMountedPlaylistSetting.Value)
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
                        .PermitIf(Trigger.Submerging, State.Submerged, () => Instance.ToggleSubmergedPlaylistSetting.Value)
                        .Permit(Trigger.InCombat, State.Battle)
                        .Ignore(Trigger.Emerging)
                        .Ignore(Trigger.OutOfCombat);

            _stateMachine.Configure(State.Submerged)
                        .Ignore(Trigger.Submerging)
                        .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                        .PermitDynamic(Trigger.Emerging, GameModeStateSelector)
                        .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                        .Permit(Trigger.Victory, State.Victory)
                        .Permit(Trigger.Death, State.Defeated)
                        .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                        .PermitIf(Trigger.Mounting, State.Mounted, () => Instance.ToggleMountedPlaylistSetting.Value)
                        .Permit(Trigger.InCombat, State.Battle)
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

            if (Instance.ToggleMountedPlaylistSetting.Value && Gw2Mumble.PlayerCharacter.CurrentMount > 0)
                return State.Mounted;

            if (Instance.ToggleSubmergedPlaylistSetting.Value && _prevIsSubmerged)
                return State.Submerged;

            if (Gw2Mumble.CurrentMap.IsCompetitiveMode && !Gw2Mumble.CurrentMap.Type.IsWvW() && Gw2Mumble.CurrentMap.Id != 350)
                return State.Competitive;

            if (Gw2Mumble.CurrentMap.Type.IsInstance())
                return State.StoryInstance;

            if (Gw2Mumble.CurrentMap.Type.IsPublic())
                return State.Ambient;

            return State.StandBy;
        }

        public void Update() {
            CheckTyrianTime();
            CheckWaterLevel();
        }

        private void CheckWaterLevel() => IsSubmerged = Gw2Mumble.PlayerCharacter.Position.Z < -1.25f;
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

        private void OnMountChanged(object o, ValueEventArgs<MountType> e) => _stateMachine.Fire(e.Value > 0 ? Trigger.Mounting : Trigger.UnMounting);
        private void OnMapChanged(object o, ValueEventArgs<int> e) => _stateMachine.Fire(Trigger.MapChanged);
        private void OnIsInCombatChanged(object o, ValueEventArgs<bool> e) {
            if (e.Value)
            {
                _inCombatTimer.Restart();
            }
            else if (this.CurrentState == State.Battle)
            {
                _outOfCombatTimer.Restart();
            }
            else
            {
                _inCombatTimer.Stop();
            }
        }

        #endregion
    }
}
