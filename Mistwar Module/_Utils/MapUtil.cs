using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.Exceptions;
using File = System.IO.File;
using Rectangle = Gw2Sharp.WebApi.V2.Models.Rectangle;
namespace Nekres.Mistwar
{
    internal static class MapUtil
    {
        private static Point FromPixelToTileXY(Coordinates2 p, int zoom = 8)
        {
            var tileSize = zoom * 32;
            return new Point((int)(p.X / tileSize), (int)(p.Y / tileSize));
        }

        private static List<Point> GetAreaTileList(Rectangle rect)
        {
            var topLeft = FromPixelToTileXY(rect.TopLeft);
            var rightBottom = FromPixelToTileXY(rect.BottomRight);

            int x = Math.Max(0, topLeft.X);
            int toX = rightBottom.X;
            int y0 = Math.Max(0, topLeft.Y);
            int toY = rightBottom.Y;

            var list = new List<Point>((toX - x + 1) * (toY - y0 + 1));

            for (; x <= toX; x++)
            {
                for (int y = y0; y <= toY; y++)
                {
                    list.Add(new Point(x, y));
                }
            }

            return list;
        }

        private static async Task<Bitmap> GetTileImage(int dnsAlias, int continentId, int floor, int x, int y, int zoom = 6)
        {
            if (zoom < 0 || zoom > 7) return null;

            var dns = dnsAlias > 0 && dnsAlias < 5 ? dnsAlias.ToString() : string.Empty;

            System.Net.WebRequest request = System.Net.WebRequest.Create($"https://tiles{dns}.guildwars2.com/{continentId}/{floor}/{zoom}/{x}/{y}.jpg");
            System.Net.WebResponse response = await request.GetResponseAsync();
            System.IO.Stream responseStream = response.GetResponseStream();
            if (responseStream == null) return null;
            return new Bitmap(responseStream);
        }

        public static async Task<Stream> DrawMapImage(int mapId, bool removeBackground = false, string filePath = null)
        {
            var map = await RequestMap(mapId);
            if (map == null) return null;

            if (File.Exists(filePath))
                return new MemoryStream(File.ReadAllBytes(filePath));

            var area = map.ContinentRect;

            var zoom = 6;
            var padding = 0;

            var tileArea = GetAreaTileList(area);

            // area
            var topLeftPx = area.TopLeft;
            var rightBottomPx = area.BottomRight;
            var pxDelta = new Point((int)(rightBottomPx.X - topLeftPx.X), (int)(rightBottomPx.Y - topLeftPx.Y));

            var bmpDestination = new Bitmap(pxDelta.X + padding * 2, pxDelta.Y + padding * 2);
            using (var gfx = Graphics.FromImage(bmpDestination))
            {
                gfx.CompositingMode = CompositingMode.SourceOver;

                // get tiles & combine into one
                foreach (var p in tileArea)
                {
                    Console.WriteLine("Downloading[" + p + "]: " + tileArea.IndexOf(p) + " of " + tileArea.Count);

                    var tile = await GetTileImage(0, map.ContinentId, map.DefaultFloor, p.X, p.Y, zoom);
                    if (tile == null) continue;
                    using (tile)
                    {
                        var x = (long)(p.X * tile.Width - topLeftPx.X + padding);
                        var y = (long)(p.Y * tile.Height - topLeftPx.Y + padding);
                        gfx.DrawImage(tile, x, y, tile.Width, tile.Height);
                    }
                }
                gfx.Flush();

                if (removeBackground)
                {

                    var sectors = await RequestSectorsForFloor(map.ContinentId, map.DefaultFloor, map.RegionId, map.Id);

                    var polygonPath = new GraphicsPath();
                    polygonPath.FillMode = FillMode.Alternate;
                    foreach (var sector in sectors)
                    {
                        var bbox = sector.Bounds.Select(coord => Refit(coord, topLeftPx, padding)).ToArray();
                        polygonPath.AddPolygon(bbox);
                    }
                    // remove any pixels not inside a sector
                    var region = new Region();
                    region.MakeInfinite();
                    region.Exclude(polygonPath);
                    gfx.CompositingMode = CompositingMode.SourceCopy;
                    gfx.FillRegion(Brushes.Transparent, region);
                }
            }

            //bmpDestination.MakeGrayscale();

            if (!string.IsNullOrEmpty(filePath)) 
                bmpDestination.Save(filePath, ImageFormat.Png);

            var stream = new MemoryStream();
            bmpDestination.Save(stream, ImageFormat.Png);
            bmpDestination.Dispose();

            return stream;
        }

        public static Point Refit(Coordinates2 value, Coordinates2 destTopLeft, int padding = 0, int tileSize = 256)
        {
            var node = new Coordinates2(value.X / tileSize, value.Y / tileSize);
            var x = (int)(node.X * tileSize - destTopLeft.X + padding);
            var y = (int)(node.Y * tileSize - destTopLeft.Y + padding);
            return new Point(x, y);
        }

        private static async Task<List<List<Point>>> GetSectors(Map map)
        {
            var sectors = await RequestSectorsForFloor(map.ContinentId, map.DefaultFloor, map.RegionId, map.Id);
            var coords = sectors.Select(x => x.Bounds);
            var result = coords.Select(l => l.Select(coord => new Point((int)coord.X, (int)coord.Y)).ToList()).ToList();
            return result;
        }

        public static async Task<IEnumerable<ContinentFloorRegionMapSector>> RequestSectorsForFloor(int continentId, int floor, int regionId, int mapId)
        {
            try
            {
                return await MistwarModule.ModuleInstance.Gw2ApiManager.Gw2ApiClient.V2.Continents[continentId].Floors[floor].Regions[regionId].Maps[mapId].Sectors.AllAsync();
            }
            catch (Exception ex) when (ex is BadRequestException or NotFoundException)
            {
                return Enumerable.Empty<ContinentFloorRegionMapSector>();
            }
            catch (UnexpectedStatusException)
            {
                MistwarModule.Logger.Warn(CommonStrings.WebApiDown);
                return Enumerable.Empty<ContinentFloorRegionMapSector>();
            }
        }
        public static async Task<Map> RequestMap(int id)
        {
            try
            {
                return await MistwarModule.ModuleInstance.Gw2ApiManager.Gw2ApiClient.V2.Maps.GetAsync(id);
            }
            catch (Exception ex) when (ex is BadRequestException or NotFoundException)
            {
                return null;
            }
            catch (UnexpectedStatusException)
            {
                MistwarModule.Logger.Warn(CommonStrings.WebApiDown);
                return null;
            } 
        }
    }
}
