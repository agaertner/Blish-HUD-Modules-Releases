using Blish_HUD;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Vector3 = System.Numerics.Vector3;

namespace Nekres.Mumble_Info.Core.Services
{
    internal class MockService : IDisposable
    {
        private int _sizeLink;
        private int _sizeContext;
        private int _sizeDiscarded;
        private int _memFileLen;
        private string _filePath;

        private DateTime _lastAutoSave;
        private int _autoSaveMins;
        public MockService()
        {
            _autoSaveMins = 30;
            _lastAutoSave = DateTime.UtcNow.Subtract(new TimeSpan(_autoSaveMins, 0,0));
            _sizeLink = Marshal.SizeOf(typeof(Link));
            _sizeContext = Marshal.SizeOf(typeof(Context));
            _sizeDiscarded = 256 - _sizeContext + 4096; // Empty areas of context and description.
        
             // GW2 won't start sending data if memfile isn't big enough so we have to add discarded bits too.
             _memFileLen = _sizeLink + _sizeContext + _sizeDiscarded;

             _filePath = Path.Combine(MumbleInfoModule.Instance.DirectoriesManager.GetFullDirectoryPath("mumble_info"), "mockme.mumble");
        }

        public void Update()
        {
            if (DateTime.UtcNow.Subtract(_lastAutoSave).TotalMinutes < _autoSaveMins || !GameService.Gw2Mumble.IsAvailable) return;
            _lastAutoSave = DateTime.UtcNow;

            if (!this.GetData(out var data)) return;
            this.SaveFile(data);
        }

        public bool GetData(out byte[] data)
        {
            data = null;

            //var identity = JsonConvert.DeserializeObject<Identity>(GameService.Gw2Mumble.RawClient.RawIdentity);

            var link = new Link
            {
                uiVersion = 1,
                uiTick = (ulong)GameService.Gw2Mumble.RawClient.Tick,
                fAvatarPosition = GameService.Gw2Mumble.RawClient.AvatarPosition.ToVector3().ToArray(),
                fAvatarFront = GameService.Gw2Mumble.RawClient.AvatarFront.ToVector3().ToArray(),
                fAvatarTop = Vector3.Zero.ToArray(),
                name = "Guild Wars 2",
                fCameraPosition = GameService.Gw2Mumble.RawClient.CameraPosition.ToVector3().ToArray(),
                fCameraFront = GameService.Gw2Mumble.RawClient.CameraFront.ToVector3().ToArray(),
                fCameraTop = Vector3.Zero.ToArray(),
                identity = GameService.Gw2Mumble.RawClient.RawIdentity,
                context_len = 48
            };

            var ctx = new Context
            {
                serverAddress = GameService.Gw2Mumble.RawClient.ServerAddress.ToCharArray().Select(Convert.ToByte).ToArray(),
                mapId = (uint)GameService.Gw2Mumble.RawClient.MapId,
                mapType = (uint)GameService.Gw2Mumble.RawClient.MapType,
                shardId = GameService.Gw2Mumble.RawClient.ShardId,
                instance = GameService.Gw2Mumble.RawClient.Instance,
                buildId = (uint)GameService.Gw2Mumble.RawClient.BuildId,
                uiState = BitmaskUtil.GetBitmask(
                    GameService.Gw2Mumble.RawClient.IsMapOpen, 
                    GameService.Gw2Mumble.RawClient.IsCompassTopRight, 
                    GameService.Gw2Mumble.RawClient.IsCompassRotationEnabled, 
                    GameService.Gw2Mumble.RawClient.DoesGameHaveFocus, 
                    GameService.Gw2Mumble.RawClient.IsCompetitiveMode, 
                    GameService.Gw2Mumble.RawClient.DoesAnyInputHaveFocus, 
                    GameService.Gw2Mumble.RawClient.IsInCombat),
                compassWidth = (ushort)GameService.Gw2Mumble.RawClient.Compass.Width,
                compassHeight = (ushort)GameService.Gw2Mumble.RawClient.Compass.Height,
                compassRotation = (float)GameService.Gw2Mumble.RawClient.CompassRotation,
                playerX = (float)GameService.Gw2Mumble.RawClient.PlayerLocationMap.X,
                playerY = (float)GameService.Gw2Mumble.RawClient.PlayerLocationMap.Y,
                mapCenterX = (float)GameService.Gw2Mumble.RawClient.MapCenter.X,
                mapCenterY = (float)GameService.Gw2Mumble.RawClient.MapCenter.Y,
                mapScale = (float)GameService.Gw2Mumble.RawClient.MapScale,
                processId = GameService.Gw2Mumble.RawClient.ProcessId,
                mountIndex = (byte)GameService.Gw2Mumble.RawClient.Mount
            };

            var padd = new Padding();

            if (!link.GetBytes(out var linkbuf) || !ctx.GetBytes(out var ctxbuf) || !padd.GetBytes(out var padbuf)) return false;

            data = linkbuf.Concat(ctxbuf).Concat(padbuf).ToArray();

            return true;
        }

        public void SaveFile(byte[] data)
        {
            try
            {
                File.WriteAllBytes(_filePath, data);
            }
            catch (SystemException e)
            {
                MumbleInfoModule.Logger.Error(e, e.Message);
            }
        }

        public void Dispose()
        {
        }
    }

    public struct Link
    {
        public uint uiVersion;
        public ulong uiTick;
        public float[] fAvatarPosition;
        public float[] fAvatarFront;
        public float[] fAvatarTop;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string name; // char[256]
        public float[] fCameraPosition;
        public float[] fCameraFront;
        public float[] fCameraTop;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string identity; // char[256]
        public uint context_len;
        // ("context", ctypes.c_ubyte * 256),      # 256 bytes, see below
        // ("description", ctypes.c_wchar * 2048), # 4096 bytes, always empty
    }

    public struct Padding
    {
        public char[] description; // always empty
    }

    public struct Context
    {
        public byte[] serverAddress; // byte[28]
        public uint mapId;
        public uint mapType;
        public uint shardId;
        public uint instance;
        public uint buildId;
        public uint uiState;
        public ushort compassWidth;
        public ushort compassHeight;
        public float compassRotation;
        public float playerX;
        public float playerY;
        public float mapCenterX;
        public float mapCenterY;
        public float mapScale;
        public uint processId;
        public byte mountIndex;
    }
}
