using System;
using System.Linq;
using Gw2Sharp.WebApi.V2.Models;
using RBush;
namespace Nekres.Regions_Of_Tyria.Geometry
{
    public class Sector : ISpatialData, IComparable<Sector>, IEquatable<Sector>
    {
        private readonly Envelope _envelope;

        public readonly int Id;
        public readonly string Name;

        public Sector(ContinentFloorRegionMapSector sector)
        {
            Id = sector.Id;
            Name = sector.Name;

            _envelope = new Envelope(
                minX: sector.Bounds.Min(coord => coord.X),
                minY: sector.Bounds.Min(coord => coord.Y),
                maxX: sector.Bounds.Max(coord => coord.X),
                maxY: sector.Bounds.Max(coord => coord.Y));
        }

        public ref readonly Envelope Envelope => ref _envelope;

        public int CompareTo(Sector other)
        {
            if (this.Envelope.MinX != other.Envelope.MinX)
                return this.Envelope.MinX.CompareTo(other.Envelope.MinX);
            if (this.Envelope.MinY != other.Envelope.MinY)
                return this.Envelope.MinY.CompareTo(other.Envelope.MinY);
            if (this.Envelope.MaxX != other.Envelope.MaxX)
                return this.Envelope.MaxX.CompareTo(other.Envelope.MaxX);
            if (this.Envelope.MaxY != other.Envelope.MaxY)
                return this.Envelope.MaxY.CompareTo(other.Envelope.MaxY);
            return 0;
        }

        public bool Equals(Sector other) =>
            this._envelope == other._envelope;
    }
}
