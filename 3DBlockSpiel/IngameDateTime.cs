using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _1st3DGame
{
    class IngameDateTime
    {
        public const uint TicksPerDay = 240000;
        
        public uint Day { get; private set; }
        public uint Time { get; private set; }

        public float RelativeTimeOfDay { get { return ((float)Time / TicksPerDay); } }
        public ulong AllTicks { get { return this.Time + this.Day * TicksPerDay; } }
        public float DateTime { get { return Day + RelativeTimeOfDay; } }

        public IngameDateTime(uint day, uint time)
        {
            this.Day = day;
            this.Time = time % TicksPerDay;
        }

        public void AddTime(uint ticks)
        {
            this.Time += ticks;
            if (this.Time >= TicksPerDay)
            {
                this.Time -= TicksPerDay;
                this.Day++;
            }
        }

        public override string ToString()
        {
            return "Day: " + this.Day + " Time: " + Math.Round(this.RelativeTimeOfDay,2);
        }

    }
}
