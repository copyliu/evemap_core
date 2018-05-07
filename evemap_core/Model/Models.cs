using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace evemap_core.Model
{
    public class NPCInf
    {
        public int npcid;
        public int systemid;
        public string name;
        public double inf;

    }
    public class Alliance
    {
        public long id;
        public string name;
        private Color _color;
        public Color color => _color;
        private long x, y,count;
        public string colorString
        {
            get => _colorString;
            set
            {
                _colorString = value;
                int c;
                Int32.TryParse(value, NumberStyles.HexNumber,CultureInfo.CurrentCulture, out c);

                _color = Color.FromArgb(Convert.ToInt32(c));
            }
        }

        public bool isNPC;
        private string _colorString;

        public void translate(int x, int y)
        {
            this.x += x;
            this.y += y;
        }

        public void incrementCount()
        {
            count++;
        }
    }

    public class SolarSystem
    {
        public int systemid;
        public long allianceid;
        public int stations;
        public int constellationid;
        public int regionid;
        public int sovlevel;
        public double x;
        public double z;

    }

    public class SovChangeLog
    {
        public long fromalliid;
        public long toalliid;
        public long systemid;

    }

    public class SystemJump
    {
        public int fromsystem;
        public int tosystem;
        

    }

    public class Region
    {
        public int regionid;
        public string name;
        public double x;
        public double z;

    }
}
