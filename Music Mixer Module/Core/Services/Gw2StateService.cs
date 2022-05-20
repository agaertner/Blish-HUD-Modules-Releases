using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.ArcDps;
using Stateless;
using static Blish_HUD.GameService;
using static Nekres.Music_Mixer.MusicMixerModule;
using Timer = System.Timers.Timer;
namespace Nekres.Music_Mixer.Core.Services
{
    public class Gw2StateService : IDisposable
    {
        public event EventHandler<ValueEventArgs<TyrianTime>> TyrianTimeChanged;
        public event EventHandler<ValueChangedEventArgs<State>> StateChanged;
        public event EventHandler<ValueEventArgs<bool>> IsSubmergedChanged;
        public event EventHandler<ValueEventArgs<bool>> IsDownedChanged;

        private List<ulong> _enemyIds;
        private ulong _playerId;

        public enum State
        {
            StandBy,
            Default,
            Mounted,
            Battle,
            SPvP,
            StoryInstance,
            Submerged,
            Defeated,
            Victory
        }

        private enum Trigger
        {
            StandBy,
            Default,
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

        private const int _enemyThreshold = 6;

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
        private Timer _arcDpsTimeOut;

        public Gw2StateService() {
            _playerId = 0;
            _enemyIds = new List<ulong>();
            _arcDpsTimeOut = new Timer(2500) { AutoReset = false };
            _arcDpsTimeOut.Elapsed += (o,e) =>
            {
                ArcDps.RawCombatEvent += CombatEventReceived;
            };

            InitializeStateMachine();
        }

        public void Dispose() {
            _arcDpsTimeOut?.Dispose();
            GameIntegration.Gw2Instance.Gw2Closed -= OnGw2Closed;
            ArcDps.RawCombatEvent -= CombatEventReceived;
            Gw2Mumble.PlayerCharacter.CurrentMountChanged -= OnMountChanged;
            Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            Gw2Mumble.PlayerCharacter.IsInCombatChanged -= OnIsInCombatChanged;
        }


        private async void InitializeStateMachine() {
            await Task.Run(() => {
                _stateMachine = new StateMachine<State, Trigger>(GameModeStateSelector());

                _stateMachine.OnUnhandledTrigger((s, t) => {
                    switch (t) {
                        case Trigger.Mounting: 
                        case Trigger.UnMounting:
                            if (!ModuleInstance.ToggleMountedPlaylistSetting.Value) return; break;
                        case Trigger.Submerging: 
                        case Trigger.Emerging:
                            if (!ModuleInstance.ToggleSubmergedPlaylistSetting.Value) return; break;
                        default: break;
                    }
                    MusicMixerModule.Logger.Info($"Warning: Trigger '{t}' was fired from state '{s}', but has no valid leaving transitions.");
                });
                _stateMachine.Configure(State.StandBy)
                            .Ignore(Trigger.StandBy)
                            .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                            .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                            .PermitDynamic(Trigger.OutOfCombat, GameModeStateSelector)
                            .PermitIf(Trigger.Submerging, State.Submerged, () => ModuleInstance.ToggleSubmergedPlaylistSetting.Value)
                            .PermitIf(Trigger.Mounting, State.Mounted, () => ModuleInstance.ToggleMountedPlaylistSetting.Value)
                            .Permit(Trigger.Victory, State.Victory)
                            .Permit(Trigger.InCombat, State.Battle)
                            .Permit(Trigger.Death, State.Defeated)
                            .Ignore(Trigger.Submerging)
                            .Ignore(Trigger.Emerging);

                _stateMachine.Configure(State.Default)
                            .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                            .PermitDynamic(Trigger.MapChanged, GameModeStateSelector)
                            .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
                            .PermitIf(Trigger.Submerging, State.Submerged, () => ModuleInstance.ToggleSubmergedPlaylistSetting.Value)
                            .PermitIf(Trigger.Mounting, State.Mounted, () => ModuleInstance.ToggleMountedPlaylistSetting.Value)
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

                _stateMachine.Configure(State.SPvP)
                            .OnEntry(t => StateChanged?.Invoke(this, new ValueChangedEventArgs<State>(t.Source, t.Destination)))
                            .PermitDynamic(Trigger.StandBy, GameModeStateSelector)
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
                            .PermitIf(Trigger.Submerging, State.Submerged, () => ModuleInstance.ToggleSubmergedPlaylistSetting.Value)
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
                            .PermitIf(Trigger.Mounting, State.Mounted, () => ModuleInstance.ToggleMountedPlaylistSetting.Value)
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
            });
        }
        private State GameModeStateSelector()
        {
            IsDowned = false;
            if (Gw2Mumble.PlayerCharacter.CurrentMount > 0)
                return State.Mounted;
            if (ModuleInstance.ToggleSubmergedPlaylistSetting.Value && _prevIsSubmerged)
                return State.Submerged;
            switch (Gw2Mumble.CurrentMap.Type)
            {
                case Gw2Sharp.Models.MapType.Pvp:
                case Gw2Sharp.Models.MapType.Gvg:
                case Gw2Sharp.Models.MapType.Tournament:
                case Gw2Sharp.Models.MapType.UserTournament:
                    return State.SPvP;
                case Gw2Sharp.Models.MapType.Instance:
                case Gw2Sharp.Models.MapType.FortunesVale:
                    return State.StoryInstance;
                case Gw2Sharp.Models.MapType.Tutorial:
                case Gw2Sharp.Models.MapType.Public:
                case Gw2Sharp.Models.MapType.PublicMini:
                    return State.Default;
                default:
                    return State.StandBy;
            }
        }

        public void Update() {
            CheckTyrianTime();
            CheckWaterLevel();
        }

        private void CheckWaterLevel() => IsSubmerged = Gw2Mumble.PlayerCharacter.Position.Z < -1.25f;
        private void CheckTyrianTime() => TyrianTime = TyrianTimeUtil.GetCurrentDayCycle();
        private void OnGw2Closed(object sender, EventArgs e) => _stateMachine.Fire(Trigger.StandBy);

        #region ArcDps Events

        private void TimeOutCombatEvents() {
            ArcDps.RawCombatEvent -= CombatEventReceived;
            _arcDpsTimeOut.Restart();
        }

        private void CombatEventReceived(object o, RawCombatEventArgs e) {
            if (e.CombatEvent == null || 
                e.CombatEvent.Ev == null || 
                e.EventType == RawCombatEventArgs.CombatEventType.Local) return;

            var ev = e.CombatEvent.Ev;

            // Save id and check state changes of local player.
            if (e.CombatEvent.Src.Self > 0) {
                _playerId = e.CombatEvent.Src.Id;
                switch (ev.IsStateChange)
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
            } else if (e.CombatEvent.Dst.Self > 0)
                _playerId = e.CombatEvent.Dst.Id;

            if (ev.Iff == ArcDpsEnums.IFF.Foe) {
                
                ulong enemyId = e.CombatEvent.Src.Self > 0 ? e.CombatEvent.Dst.Id : e.CombatEvent.Src.Id;

                // allied minion/pet event
                if (_playerId != 0) {
                    if (ev.SrcMasterInstId == _playerId)
                        enemyId = e.CombatEvent.Dst.Id;
                    else if (ev.DstMasterInstId == _playerId)
                        enemyId = e.CombatEvent.Src.Id;
                }

                // track enemy and start combat if threshold is reached.
                if (_enemyIds.Any(x => x.Equals(enemyId))) {
                    if (enemyId.Equals(ev.SrcAgent) && ev.IsStateChange == ArcDpsEnums.StateChange.ChangeDead)
                        _enemyIds.Remove(enemyId);
                } else if (_enemyIds.Count() < _enemyThreshold) {
                    _enemyIds.Add(enemyId);
                } else {
                    _stateMachine.Fire(Trigger.InCombat);
                }

            }
        }

        #endregion

        #region Mumble Events

        private void OnMountChanged(object o, ValueEventArgs<Gw2Sharp.Models.MountType> e) => _stateMachine.Fire(e.Value > 0 ? Trigger.Mounting : Trigger.UnMounting);
        private void OnMapChanged(object o, ValueEventArgs<int> e) => _stateMachine.Fire(Trigger.MapChanged);
        private void OnIsInCombatChanged(object o, ValueEventArgs<bool> e) {
            if (!e.Value) {
                TimeOutCombatEvents(); 
                _enemyIds.Clear();
                _stateMachine.Fire(Trigger.OutOfCombat);
            } else if (!ArcDps.Loaded) 
                _stateMachine.Fire(Trigger.InCombat);
        }

        #endregion
    }
}
