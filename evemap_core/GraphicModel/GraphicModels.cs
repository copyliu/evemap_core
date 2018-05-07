using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using evemap_core.Model;

namespace evemap_core.GraphicModel
{

    public class Util
    {
        public static double ChinaRound(double value, int decimals=0)
        {
            if (value < 0)
            {
                return Math.Round(value + 5 / Math.Pow(10, decimals + 1), decimals, MidpointRounding.AwayFromZero);
            }
            else
            {
                return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            }
        }

    }
    public class SolarSystem
    {
        public Model.SolarSystem systemmodel;

        public SolarSystem(Model.SolarSystem systemmodel)
        {
            this.systemmodel = systemmodel;
            this.x = (int) Util.ChinaRound(systemmodel.x / MapConstants.SCALE + MapConstants.HORIZONTAL_SIZE / 2.0 +
                                           MapConstants.HORIZONTAL_OFFSET);
            this.y = (int)Util.ChinaRound(systemmodel.z / MapConstants.SCALE + MapConstants.VERTICAL_SIZE / 2.0 +
                                          MapConstants.HORIZONTAL_OFFSET);
        }

        public int constellationid => systemmodel.constellationid;
        public int regionid => systemmodel.regionid;
        public int systemid => systemmodel.systemid;
        public int x;
        public int y;
        public List<Alliance> alliances=new List<Alliance>();
        public List<double> influences=new List<double>();
        public bool hasstatsion => systemmodel.stations > 0;
        public long allianceid => systemmodel.allianceid;
        public int sovlevel => systemmodel.sovlevel;
        public void addInfluence(Alliance al, double value)
        {
            if (al == null)
            {
                alliances=new List<Alliance>(){al};
                influences=new List<double>(){value};
                return;
            }

            bool exists = false;
            var tempall = alliances.ToList();
            var tempinf = influences.ToList();
          
            for (int i = 0; i < alliances.Count; i++)
            {
                if (alliances[i].id == al.id)
                {
                    exists = true;
                    influences[i] += value;
                    break;
                }
            }

            if (!exists)
            {
                alliances.Add(al);
                influences.Add(value);
            }
        }

        public void draw()
        {
            throw new  NotImplementedException();
        }
    }

    public class Jump
    {
        private SolarSystem from, to;
        private static Color sJump = Color.FromArgb(0x30, 0, 0, 0xff);
        private static Color cJump = Color.FromArgb(0x30, 0xFF, 0, 0);
        private static Color rJump = Color.FromArgb(0x30, 0xFF, 0, 0xff);

        public override bool Equals(object obj)
        {
            if (obj is Jump o)
            {
                return Equals(o);
            }

            return false;
        }

        protected bool Equals(Jump other)
        {
            return (Equals(@from.systemid, other.@from.systemid) && Equals(to.systemid, other.to.systemid)) ||
                   (Equals(to.systemid, other.@from.systemid) && Equals(@from.systemid, other.to.systemid));
        }



        public Jump(SolarSystem from, SolarSystem to)
        {
            this.from = from;
            this.to = to;

        }

        public void Draw(Graphics g)
        {
            Pen p;
            if (@from.constellationid == to.constellationid)
            {
                p = new Pen(sJump);
            }
            else if (@from.regionid == to.regionid)
            {
                p = new Pen(cJump);
            }
            else
            {
                p = new Pen(rJump);
            }
            g.DrawLine(p, @from.x, @from.y,to.x,to.y);
        }
    }
}